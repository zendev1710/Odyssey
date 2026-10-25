# Historique Complet de la Conversation : Optimisation C#, Formats Binaires et MVVM (Avalonia)

---

## 1. Transmission de paramètre sans allocation ni modification (`in`)

C# fournit le mot-clé **`in`**.

L'utilisation de `in` passe l'argument par **référence en lecture seule (`ref readonly`)** :
1. **Zéro allocation :** Pas de duplication d'objet ni de copie.
2. **Garantie d'immutabilité :** La méthode appelée ne peut ni réassigner la référence, ni modifier les champs d'une `struct`.

### 1. Si `MyType` est une `struct` (Value Type)
Le mot-clé `in` brille avec les types valeur (`struct`), car il évite la copie des données tout en empêchant la modification.

```csharp
public struct MyType
{
    public int X { get; set; }
}

public class Example
{
    // 'in' agit comme 'const ref' en C++
    public void ProcessData(in MyType myType)
    {
        // ❌ ERREUR DE COMPILATION : Impossible de modifier un paramètre 'in'
        // myType.X = 10; 

        // ❌ ERREUR DE COMPILATION : Impossible de réassigner un paramètre 'in'
        // myType = new MyType(); 

        // ✅ La lecture est autorisée
        int value = myType.X;
    }
}
```

### 2. Si `MyType` est une `class` (Reference Type)
Passer une `class` transmet déjà une référence (pas de second objet créé). `in` empêche de réassigner la référence elle-même. Pour garantir l'immutabilité des propriétés, la bonne pratique est d'utiliser une **interface en lecture seule** :

```csharp
public interface IReadOnlyMyType
{
    int X { get; } // Uniquement un getter
}

public class MyType : IReadOnlyMyType
{
    public int X { get; set; }
}

public class Example
{
    public void ProcessData(IReadOnlyMyType myType)
    {
        // ❌ ERREUR DE COMPILATION : Propriété en lecture seule
        // myType.X = 10; 

        int value = myType.X; // ✅ Autorisé
    }
}
```

---

## 2. Performances : Propriété entière vs `is B`

**Question :**  
J'ai un objet `myObject` (de classe `A`) avec une propriété `int theType` représentant son type. L'objet peut être une classe dérivée `B` héritant de `A`. Qu'est-ce qui est le plus performant : `myObject.theType == value` ou `myObject is B` ?

**Réponse :**  
Vérifier une propriété entière (`myObject.theType == value`) est **nettement plus rapide** que d'utiliser l'opérateur de vérification de type (`myObject is B`).

### Comparatif de performance :

| Opération | Instruction IL | Performance | Comportement Mémoire / CPU |
| :--- | :--- | :--- | :--- |
| **`myObject.theType == value`** | `ldfld` / `callvirt` + `ceq` | **~0.1 - 0.5 ns** | **Lecture mémoire directe :** Lit un entier en registre et compare. |
| **`myObject is B`** | `isinst` | **~1.5 - 4.0 ns** | **Inspection d'héritage :** Parcourt la table `RuntimeTypeHandle`. |

### Pourquoi `theType == value` est plus rapide :
1. **Instruction CPU directe :** Se traduit par une simple instruction d'assemblage `CMP`.
2. **Inlining JIT :** Le JIT transforme le getter en lecture directe de champ.
3. **Pas de recherche de type :** Aucune requête sur le runtime CLR ni inspection de vtable.

---

## 3. Utilisation de l'opérateur `as` (`var myObj = myObject as B`)

**Question :**  
Et qu'en est-il de faire `var myObj = myObject as B` ?

**Réponse :**  
Utiliser `as B` (puis vérifier `!= null`) est **équivalent en termes de performances** à `myObject is B`. Les deux utilisent l'instruction IL `isinst`.

- **Pour vérifier uniquement le type (sans accéder aux membres de B) :** `theType == value` reste ultra-rapide (~0.2 ns) par rapport à `as` ou `is` (~2 ns).
- **Pour accéder aux membres de B après vérification :** Le pattern matching est la syntaxe recommandée :

```csharp
// Pattern Matching (Standard C# 7+)
if (myObject is B bInstance)
{
    bInstance.FaireQuelqueChoseDeB();
}

// Opérateur 'as' (Ancienne syntaxe C# 2.0)
var bInstance = myObject as B;
if (bInstance != null)
{
    bInstance.FaireQuelqueChoseDeB();
}
```

En C# moderne, ces deux formes génèrent **exactement le même code assembleur JIT**.

---

## 4. Cast direct `((B)myObject).MyPropertyFromB`

**Question :**  
Et le cast direct `((B)myObject).MyPropertyFromB` ?

**Réponse :**  
Faire un cast direct est la solution la plus rapide **si et seulement si tu es certain à 100% que `myObject` est un `B`**.

- **Performance :** **~0.5 à 1.5 ns** (instruction IL `castclass`).
- **Risque :** Si l'objet n'est pas un `B`, le CLR lève une `InvalidCastException` dont le coût dépasse les 10 000 ns.

### Synthèse des approches :

| Approche | Code | Temps | Sécurité |
| :--- | :--- | :--- | :--- |
| **1. Propriété / Enum int** | `if (myObject.theType == TypeB)` | **~0.2 ns** | 100% Sûr |
| **2. Type vérifié + Cast direct** | `if (myObject.theType == TypeB) { var x = ((B)myObject).Prop; }` | **~0.7 ns** | Sûr si l'enum est fiable |
| **3. Pattern Matching** | `if (myObject is B b) { var x = b.Prop; }` | **~2.0 ns** | 100% Sûr |
| **4. Opérateur `as`** | `var b = myObject as B; if (b != null) { var x = b.Prop; }` | **~2.0 ns** | 100% Sûr |
| **5. Double contrôle (Bad practice)** | `if (myObject is B) { var x = ((B)myObject).Prop; }` | **~4.0 ns** | Inutilement doublé |
| **6. Cast aveugle avec Exception** | `var x = ((B)myObject).Prop;` *(en cas d'erreur)* | **> 10 000 ns** | Catastrophique |

En combinant le filtre par entier et le cast direct (`Option 2`), l'économie est d'environ **1,3 ns par opération** (2,8× à 3× plus rapide que le pattern matching).

---

## 5. Polymorphisme : Interfaces vs Méthodes Virtual/Abstract sur 500 000 objets

**Question :**  
Qu'en est-il des performances des interfaces et des méthodes virtuelles/abstracts sur mes classes A et B pour plusieurs centaines de milliers d'objets ?

**Réponse :**  

### Synthèse des performances :

| Méthode d'appel | Mécanisme | Temps relatif | Coût mémoire |
| :--- | :--- | :--- | :--- |
| **1. Check Enum + Cast direct** | `CMP` + `call` direct | **~0.5 - 0.7 ns** | 0 octet de pointeur vtable |
| **2. Appels `virtual` / `override`** | Direct VTable Lookup (`callvirt`) | **~1.0 - 1.5 ns** | 8 octets / objet (pointeur vtable) |
| **3. Appels d'Interface (`ILocation`)** | Interface Table Lookup (VSD) | **~2.0 - 4.5 ns** | Dispatch dynamique |

- **Méthodes `virtual` / `override` :** L'offset dans la vtable est fixe et connu à la compilation. La prédiction de branchement du CPU est très efficace.
- **Interfaces (`ILocation`) :** Le .NET Runtime utilise le *Virtual Stub Dispatch* (VSD). Si la collection contient un mélange hétérogène de classes, l'accès dynamique ralentit l'exécution.

---

## 6. Optimisation de l'empreinte mémoire pour la topologie (DataBlock, Region, Unit)

**Question :**  
J'ai une classe `DataBlock`. Je la spécialise en `RegionDataBlock : DataBlock, ILocation`. Pour une `Unit` rattachée à une région, dois-je la spécialiser en `Unit` (`ILocation`), stocker la région (`_region`), ou retrouver la région via un lien `PreviousBlock` et la propriété `Depth` ?

**Réponse :**  
Pour réduire au maximum la mémoire consommée, **remonter la chaîne via `PreviousBlock` et `Depth` est la meilleure option (0 octet ajouté par instance)**.

### Implémentation :

```csharp
public class UnitDataBlock : DataBlock, ILocation
{
    // Pas de champ X, Y, Plane ou _region dupliqué !

    public int X => GetRegion()?.X ?? 0;
    public int Y => GetRegion()?.Y ?? 0;
    public PlaneType Plane => GetRegion()?.Plane ?? PlaneType.Default;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private RegionDataBlock? GetRegion()
    {
        DataBlock? current = PreviousBlock;
        int targetDepth = Depth - 1;

        while (current != null)
        {
            if (current.Depth <= targetDepth)
            {
                return current as RegionDataBlock; 
            }
            current = current.PreviousBlock;
        }

        return null;
    }
}
```

---

## 7. Empreinte mémoire des métadonnées C# (`class` vs `struct`)

**Question :**  
L'allocation de métadonnées de classes supplémentaires pour le runtime C# est-elle volumineuse ? Par instance ou par classe ?

**Réponse :**  

1. **Au niveau de la CLASSE (Unique) :** `MethodTable`, vtable, reflection (~quelques Ko par classe, négligeable).
2. **Au niveau de CHAQUE INSTANCE (Répété 300 000 fois) :**
   - **Object Header :** 8 octets
   - **TypeHandle :** 8 octets
   - **Overhead fixe = 16 octets / objet** (64-bit).
   - 300 000 instances de classe = **4,8 Mo de pure surcharge mémoire**.

---

## 8. Utilisation des `struct` (Data-Oriented Design)

**Question :**  
Tu suggères d'utiliser des `struct` plutôt que des `classes` ?

**Réponse :**  
**Oui**, si la mémoire et les performances sont prioritaires.

- **`class` :** 300 000 objets disséminés sur le Heap (4,8 Mo d'en-têtes, fragmentation, pression GC).
- **`struct` :** 300 000 éléments contigus dans un seul tableau `DataBlock[]` (0 Mo d'overhead, locacité cache CPU maximale, 0 pression GC).

```csharp
public readonly struct DataBlock
{
    public readonly int Id;
    public readonly BlockType Type;
    public readonly int PreviousBlockIndex; // Index dans le tableau au lieu d'un pointeur
    public readonly int Depth;
}
```

---

## 9. Pourquoi utiliser des `struct` en C# plutôt qu'en C/C++ ?

**Question :**  
Du coup, autant faire du C ou du C++ si je dois utiliser des `struct DataBlock` !?

**Réponse :**  
Non, car C# moderne (C# 7 à 12) offre une syntaxe expressive et sûre sans la complexité de la gestion manuelle de la mémoire du C/C++ :

1. **Pattern Matching & Switch Expressions :**
```csharp
public string GetDescription(in DataBlock block) => block switch
{
    { Type: BlockType.Region } r => $"Région à la profondeur {r.Depth}",
    { Type: BlockType.Unit, Depth: > 2 } u => $"Unité à l'index {u.PreviousIndex}",
    _ => "Bloc standard"
};
```
2. **Garantie d'immutabilité :** via `readonly struct`.
3. **`Span<T>` et `ReadOnlySpan<T>` :** Manipulation de tranches de mémoire contiguës sans allocation ni pointeurs bruts.
4. **Sécurité du GC :** Le tableau est libéré d'un coup sans `free()` manuel.

---

## 10. Architecture complète : Fichier Binaire `.crx`, Ordres et Avalonia MVVM

**Question :**  
Mes DataBlock sont lus depuis un fichier texte (500 000 lignes `.cr`). Toutes les données sont en lecture seule, sauf les ordres des unités (`DataBlock.Command`). Comment résoudre mes problématiques de mémoire ? Faut-il convertir le texte `.cr` en binaire `.crx` et enregistrer les ordres à côté ? Le réexport en `.cr` doit être identique à l'original.

**Réponse :**  

### Architecture recommandée :

```
 [ Fichier Texte .cr ] 
         │ (Conversion / Compilation initiale)
         ▼
 [ Fichier Binaire .crx ] ────────┐
         │                        │
  Charge 500k structs             │ (Format Binaire ultra-rapide)
  en mémoire (~20 Mo)             │
         │                        │
         ▼                        ▼
 [ Layer DAL (Structs) ] ◄── [ Fichier Ordres .cro ] (Saisie utilisateur)
         │
  Génération à la demande (Lazy / Virtualisée)
         │
         ▼
 [ Modèles / ViewModels MVVM (Avalonia) ] (Seulement les éléments à l'écran)
         │
         ▼
 [ Export final / Sauvegarde ] ──► [ NOUVEAU Fichier .cr ] (100% conforme)
```

### Export texte fidèle :

```csharp
public void ExportToTextCr(string crxFilePath, Dictionary<int, string> updatedCommands, string outputCrPath)
{
    using var writer = new StreamWriter(outputCrPath, false, Encoding.UTF8);

    foreach (var block in ReadBlocksFromCrx(crxFilePath))
    {
        if (block.Type == BlockType.Command)
        {
            if (updatedCommands.TryGetValue(block.TargetUnitId, out var newCommandText))
            {
                writer.WriteLine(FormatCommandBlock(block.TargetUnitId, newCommandText));
                continue;
            }
        }
        writer.WriteLine(FormatOriginalBlock(block));
    }
}
```

---

## 11. Gestion du TreeView dans Avalonia (Lazy Loading)

**Question :**  
J'ai un explorateur (TreeView) qui affiche 1 item par région et par unité/bâtiment. Cela fait quand même beaucoup de modèles/viewmodels.

**Réponse :**  
Utiliser le **Lazy Loading** pour ne charger les enfants qu'au dépliage d'un nœud :

```csharp
public class RegionViewModel : ViewModelBase
{
    private readonly int _regionBlockIndex;
    private bool _isExpanded;
    private bool _childrenLoaded;

    public ObservableCollection<ViewModelBase> Children { get; } = new();

    public bool IsExpanded
    {
        get => _isExpanded;
        set
        {
            if (SetProperty(ref _isExpanded, value) && value)
            {
                LoadChildrenLazy();
            }
        }
    }

    private void LoadChildrenLazy()
    {
        if (_childrenLoaded) return;

        ReadOnlySpan<DataBlock> units = DalService.GetChildrenForRegion(_regionBlockIndex);
        foreach (ref readonly var unitBlock in units)
        {
            Children.Add(new UnitViewModel(unitBlock));
        }
        _childrenLoaded = true;
    }
}
```

---

## 12. Raccourci "Tout déplier" et Virtualisation UI avec `TreeDataGrid`

**Question :**  
Si l'utilisateur propose un raccourci clavier qui déplie toutes les régions, le Lazy Loading ne convient pas. Sur 500 000 blocs, seuls ~25 000 sont des régions/unités utiles (le reste = métadonnées/océans). Est-ce jouable sans Lazy Loading ?

**Réponse :**  
**Oui, 25 000 items sont parfaitement gérables** (seulement ~5 à 10 Mo de RAM pour les ViewModels), à condition d'utiliser la **Virtualisation UI**.

Au lieu du `TreeView` classique, utilise le **`TreeDataGrid`** d'Avalonia :

```xml
<TreeDataGrid Source="{Binding HierarchicalSource}" />
```

### Pourquoi c'est fluide :
1. **Filtrage DAL :** Ignorer les notifications et océans vides dès la relecture du binaire `.crx`.
2. **Virtualisation visuelle :** Même si 25 000 nœuds sont dépliés, Avalonia n'instancie **que les 30 à 40 contrôles visuels affichés à l'écran**.
3. Le défilement reste à 60 FPS constants.


## 13.

**Question :**  
Du coup, mon DataBlock est acutellement une classe. Je uiss obligé, ou c'est recommandé, d'avour une struct pour pouvoir enregistrer mes données en format binaire  crx ?

**Réponse :** 
Non, tu n'es absolument pas obligé d'utiliser une struct pour lire ou écrire tes données au format binaire .crx. Tu peux tout à fait conserver ta class DataBlock.

Cependant, il y a une différence fondamentale entre la structure du fichier sur le disque et la représentation des données en mémoire RAM.

1. Pourquoi tu peux garder ta class DataBlock
Quand tu écris un fichier binaire .crx, tu n'enregistres pas un objet C# brut tel quel ; tu écris une suite de octets (des entiers int, des octets byte, des chaînes UTF-8, etc.).

Tu peux parfaitement lire ce fichier binaire et instancier des class DataBlock au fur et à mesure :

```csharp
// Exemple de lecture du binaire .crx vers des objets class DataBlock
public List<DataBlock> ReadCrxFile(string filePath)
{
    using var reader = new BinaryReader(File.OpenRead(filePath));
    int count = reader.ReadInt32(); // Nombre de blocs
    var blocks = new List<DataBlock>(count);

    for (int i = 0; i < count; i++)
    {
        // On instancie la classe C# classique à partir des octets lus
        var block = new DataBlock
        {
            Id = reader.ReadInt32(),
            Type = (BlockType)reader.ReadByte(),
            Depth = reader.ReadInt32(),
            PreviousIndex = reader.ReadInt32()
        };
        blocks.Add(block);
    }

    return blocks;
}
```

Avantage : Le chargement à partir du binaire .crx restera 10 à 20 fois plus rapide que le parsing du fichier texte .cr, même en utilisant des class.

2. Alors pourquoi la struct était-elle recommandée ?
La différence n'est pas au niveau du fichier .crx, mais au niveau de la mémoire (RAM) après le chargement de tes 500 000 objets :

Option A : Tu gardes class DataBlock
Vitesse de chargement binaire : Très rapide (~100 ms).

Consommation RAM : Plus élevée (~30 à 50 Mo pour les 500 000 objets en mémoire en raison des 16 octets d'en-tête par objet).

Garbage Collector : Le GC doit surveiller 500 000 références.

Option B : Tu t'appuies sur struct DataBlock
Vitesse de chargement binaire : Instantané / Binaire brut (File.ReadAllBytes ou MemoryMappedFile en < 5 ms).

Consommation RAM : Minimale (~10 à 15 Mo).

Garbage Collector : 0 pression (1 seul tableau en mémoire).

3. Recommandation pour ton projet
Si passer ton DataBlock en struct demande de refactoriser trop de code existant (héritage, méthodes, etc.) :

Garde ta class DataBlock !

Implémente le format binaire .crx avec BinaryWriter / BinaryReader.

Tu obtiendras déjà 90% du gain de performance que tu cherches : le temps d'ouverture de ton fichier passera de plusieurs secondes de parsing texte à un chargement binaire quasi-instantané, sans avoir à réécrire la logique de tes classes métier.

## 14.

**Question :**  
et le fichier .crx obtenu resterait compatible multi-plateforme (Windows, Linux, MacOS) ?

**Réponse :**
Oui, le fichier binaire .crx restera 100 % compatible multi-plateforme, à condition de respecter deux règles d'or lors de la lecture et de l'écriture en C#.

1. Règle n°1 : L'Endianness (Ordre des octets)
Les processeurs modernes (x86, x64, ARM64 utilisés sur Windows, Mac x64/M1/M2/M3 et Linux) fonctionnent tous en Little-Endian.

Par chance, les classes natives de .NET (BinaryWriter et BinaryReader) écrivent et lisent par défaut en Little-Endian, indépendamment du système d'exploitation.

```csharp
// Écriture (Windows, Linux ou macOS = exactement le même résultat sur disque)
using var stream = File.Create("data.crx");
using var writer = new BinaryWriter(stream, Encoding.UTF8);

writer.Write(block.Id);        // int (4 octets - Little-Endian)
writer.Write((byte)block.Type); // byte (1 octet)
writer.Write(block.Name);      // string UTF-8 (préfixée par sa longueur)
```

Un fichier .crx généré sur Windows se lira sans aucun problème sous Linux ou macOS, et inversement.

2. Règle n°2 : L'encodage des chaînes de caractères (UTF-8)
Les séparateurs de ligne (\r\n sur Windows vs \n sur Linux/Mac) pose souvent problème sur les fichiers texte (.cr).

En binaire (.crx) :

Les chaînes de caractères écrites avec BinaryWriter.Write(string) sont sérialisées en UTF-8.

La longueur de la chaîne est stockée sous forme d'entier avant les octets du texte.

Résultat : Aucun problème de saut de ligne, de caractère spécial ou d'encodage OS (CRLF vs LF).

3. Le piège à ÉVITER pour garder la compatibilité : Le Cast Brut de mémoire (Unsafe.As / MemoryMarshal)
Si un jour tu décides de passer en struct pour charger le binaire en faisant un "copier-coller" direct de la mémoire du fichier vers un tableau C# via du code unsafe :

```
// ⚠️ PIÈGE : Fonctionne très bien, MAIS...
ReadOnlySpan<DataBlockStruct> blocks = MemoryMarshal.Cast<byte, DataBlockStruct>(bytes);
```csharp

Ce type d'écriture brute n'est compatible multi-plateforme que si ta struct a un layout mémoire fixe et explicite :
```
[StructLayout(LayoutKind.Sequential, Pack = 1)] // Alignement garanti sur tous les OS
public readonly struct DataBlockStruct
{
    public readonly int Id;
    public readonly byte Type;
    public readonly int PreviousIndex;
}
```

En résumé
Si tu utilises BinaryReader et BinaryWriter avec ta class DataBlock actuelle, ton fichier .crx est nativement et parfaitement portable sur Windows, Linux et macOS. Vous n'avez aucun code spécifique à ajouter pour gérer l'OS.

## 15.
**Question :**  
je veux aussi pouvoir fournir un outil en ligne de commande/console de conversion cr -> crx et crx -> cr. Il faudra donc que mes classes de conversion soient dans une dll à part ? une lib ?

**Réponse :**  
Oui, absolument. Placer la logique de parsing, de conversion et d'E/S binaire dans une bibliothèque de classes dédiée (Class Library / .dll) est la meilleure pratique d'architecture logicielle.

Cela vous permettra de partager 100 % du code métier entre votre application graphique Avalonia et votre outil en ligne de commande (CLI), sans jamais dupliquer de code.

1. Organisation de la solution .NET
Voici la structure recommandée pour votre projet C# :

```cosnole
MyGameEngine/
│
├── src/
│   ├── MyGameEngine.Core/          [Class Library (.dll)]
│   │   ├── DAL/
│   │   │   ├── DataBlock.cs         (Classes/Structs métier)
│   │   │   ├── CrxBinaryReader.cs   (Lecture binaire)
│   │   │   ├── CrxBinaryWriter.cs   (Écriture binaire)
│   │   │   └── CrConverter.cs       (Logique de conversion .cr <-> .crx)
│   │   └── Models/
│   │
│   ├── MyGameEngine.Cli/           [Console App (.exe / binaire CLI)]
│   │   ├── Program.cs               (Gestion des arguments CLI)
│   │   └── (Référence -> MyGameEngine.Core)
│   │
│   └── MyGameEngine.Desktop/       [Avalonia UI App]
│       ├── ViewModels/
│       ├── Views/
│       └── (Référence -> MyGameEngine.Core)
```

2. Contenu de la bibliothèque MyGameEngine.Core.dll
Dans votre bibliothèque de classes, vous centralisez le moteur de conversion :

```csharp
namespace MyGameEngine.Core.DAL;

public class CrConverter
{
    // Conversion Fichier Texte (.cr) -> Fichier Binaire (.crx)
    public static void ConvertCrToCrx(string crPath, string crxPath)
    {
        // 1. Lire et parser le fichier texte .cr
        // 2. Écrire les blocs et propriétés dans le fichier binaire .crx via BinaryWriter
    }

    // Conversion Fichier Binaire (.crx) -> Fichier Texte (.cr)
    public static void ConvertCrxToCr(string crxPath, string crPath, Dictionary<int, string>? updatedCommands = null)
    {
        // 1. Lire le binaire .crx
        // 2. Réinjecter les nouveaux ordres s'il y en a
        // 3. Réécrire le fichier texte .cr d'origine
    }
}
```

3. Exemple d'implémentation de l'outil CLI (MyGameEngine.Cli)
Grâce à la DLL séparée, le code de votre outil en ligne de commande devient extrêmement simple et léger :

```csharp
using MyGameEngine.Core.DAL;

// Utilisation standard du CLI :
//  crtools convert -i world.cr -o world.crx
//  crtools export  -i world.crx -o world.cr -c orders.cro

if (args.Length < 3)
{
    Console.WriteLine("Usage:");
    Console.WriteLine("  crtools -to-crx input.cr output.crx");
    Console.WriteLine("  crtools -to-cr  input.crx output.cr");
    return;
}

string mode = args[0];
string inputFile = args[1];
string outputFile = args[2];

try
{
    if (mode == "-to-crx")
    {
        Console.WriteLine($"Conversion de {inputFile} vers {outputFile}...");
        CrConverter.ConvertCrToCrx(inputFile, outputFile);
        Console.WriteLine("Conversion terminée avec succès !");
    }
    else if (mode == "-to-cr")
    {
        Console.WriteLine($"Exportation de {inputFile} vers {outputFile}...");
        CrConverter.ConvertCrxToCr(inputFile, outputFile);
        Console.WriteLine("Exportation terminée avec succès !");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Erreur lors de la conversion : {ex.Message}");
}
```

4. Les avantages majeurs de cette approche
Aucune duplication de code : Si le format .cr évolue ou si vous optimisez le format .crx, la mise à jour se fait dans MyGameEngine.Core et bénéficie instantanément à l'application Avalonia et à l'outil CLI.

Automatisation / Scripts : Vous (ou vos utilisateurs) pourrez facilement intégrer l'outil CLI dans des scripts de compilation, des pipelines de CI/CD ou des batchs pour convertir automatiquement des lots de cartes/fichiers.

Publication autonome (Native AOT / Single File) : L'outil CLI peut être publié avec .NET en un seul binaire exécutable autonome (ex: crtools.exe sur Windows ou crtools sur Linux/Mac) sans nécessiter l'installation du runtime .NET sur la machine cible :

```bash
dotnet publish -c Release -r win-x64 --self-contained /p:PublishSingleFile=true
```
