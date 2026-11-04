# Synthèse de la conversation : Optimisation C#, Formats Binaires et Architecture MVVM (Avalonia)

## 1. Vitesse d'exécution et allocations sur de gros volumes (200 000+ lignes)

- **Impact du Garbage Collector (GC) :** Sur 200 000 lignes, la création récurrente d'objets `string` (métadonnées C# de 20-32 octets par instance) génère une forte pression sur le Heap et déclenche des cycles GC (Gen0/Gen1) qui gèlent brièvement le thread.
- **Inlining et `ReadOnlySpan<char>` :** Le passage aux `Span` permet de découper et traiter les données directement sans réallouer de chaînes de caractères.
- **Gain estimé :** Passage de **~80-150 ms** (allocations chaînes + GC) à **~10-25 ms** (zero allocation).

---

## 2. Passage de paramètres sans allocation ni modification (`in` / `ref readonly`)

- En C#, le mot-clé `const` ne s'applique pas aux arguments de méthodes. Le modificateur approprié est **`in`** (`ref readonly`).
- **Pour une `struct` (Value Type) :** `in MyType myType` évite toute copie de la structure et interdit la modification de ses champs par la méthode appelée.
- **Pour une `class` (Reference Type) :** L'argument est déjà transmis sous forme de référence (pas de duplication d'objet).
  - `in` empêche de réassigner la référence elle-même à une nouvelle instance (`myType = new MyType()`).
  - Pour interdire la modification des propriétés de l'objet, il convient d'exposer une interface en lecture seule (`IReadOnlyMyType`).

---

## 3. Comparatif de performance des vérifications de type et casts

| Opération / Syntaxe | Instruction IL | Temps d'exécution | Description / Comportement |
| :--- | :--- | :--- | :--- |
| `myObject.theType == value` | `ceq` (direct) | **~0.1 - 0.5 ns** | Lecture directe d'un entier/enum en registre CPU. Ultra-rapide. |
| `((B)myObject)` *(après check entier)* | `castclass` | **~0.5 - 0.7 ns** | Cast direct. Sûr si l'enum est fiable, évitant l'inspection d'héritage. |
| `myObject is B` / `as B` / Pattern Matching | `isinst` | **~1.5 - 2.0 ns** | Parcourt la table des types (MethodTable/RuntimeTypeHandle). |
| Cast direct aveugle `((B)myObject)` *(si échec)* | Exception | **> 10 000 ns** | Le levée d'une `InvalidCastException` détruit les performances. |

---

## 4. Polymorphisme : Méthodes virtuelles vs Interfaces vs Dispatch Enum

 Sur des collections massives (500 000+ objets) :

1. **Enum Check + Cast direct :** **~0.5 - 0.7 ns.** Conserve une excellente localisation mémoire dans les lignes de cache CPU (L1/L2).
2. **Méthodes `virtual` / `override` :** **~1.0 - 1.5 ns.** Appel via la `vtable` avec un offset fixe connu à la compilation. Prédiction de branchement efficace.
3. **Appels d'Interface (`ILocation`) :** **~2.0 - 4.5 ns.** Utilise le *Virtual Stub Dispatch* (VSD). Si la collection contient un mélange hétérogène de classes, la recherche dynamique dans l'ITable ralentit l'exécution.

---

## 5. Empreinte mémoire du runtime .NET (`class` vs `struct`)

### Surcharge des objets (`class`) :
- **Au niveau de la classe :** Métadonnées (`MethodTable`), vtable, reflection (~quelques Ko par type, négligeable).
- **Au niveau de chaque instance (64 bits) :** 
  - **Object Header :** 8 octets
  - **TypeHandle (Pointeur MethodTable) :** 8 octets
  - **Overhead fixe = 16 octets / objet**, avant même d'ajouter le moindre champ de données.
  - 500 000 instances de classe = **~8 Mo de pure surcharge mémoire**.

### Avantages de la `struct` (Data-Oriented Design) :
- **0 octet d'overhead par élément.**
- Stockage contigu en mémoire (dans un tableau `DataBlock[]`).
- Pression GC nulle (1 seul grand tableau à analyser au lieu de 500 000 références).
- Accès par `Span<T>` / `ReadOnlySpan<T>` ultra-rapides tout en conservant la sécurité du runtime C#.

---

## 6. Architecture pour 500 000 DataBlocks + Application Avalonia MVVM

### Problématique :
Un fichier texte de 500 000 lignes (`.cr`) contient la topologie (régions, unités, océans, événements). L'utilisateur saisit des ordres (`DataBlock.Command`) pour ses unités. Les données doivent être chargées/affichées dans un arbre (TreeView) Avalonia, et réexportables au format texte `.cr` de façon 100% identique au fichier source (à l'exception des ordres mis à jour).

### Solutions architecturales validées :

1. **Compilation Binaire `.crx` :**
   - Conversion du fichier texte `.cr` en binaire `.crx` (tableau contigu de `struct DataBlock`).
   - Temps de chargement réduit de plusieurs secondes à **< 20 ms**.
   - Empreinte RAM divisée par 10 (~15-30 Mo au lieu de 400+ Mo).

2. **Séparation / Fusion des Ordres (`Commands`) :**
   - En cours de session : stockage des nouveaux ordres dans une structure légère séparée (`Dictionary<int, string>`).
   - À la réexportation : relecture du `.crx` et injection des nouveaux ordres à la volée vers le nouveau fichier texte `.cr`.

3. **Optimisation de la hiérarchie UI (25 000 items utiles) :**
   - Sur 500 000 blocs, la majorité sont des régions vides/océans ou des historiques. Seules ~500 régions et ~25 000 unités sont interactives.
   - **Filtrage DAL :** Ne créer des ViewModels que pour les éléments utiles.
   - **Raccourci clavier "Tout déplier" & Virtualisation UI :** Utiliser `TreeDataGrid` d'Avalonia (au lieu d'un `TreeView` classique). Grâce à la virtualisation visuelle, seuls les 30 à 40 contrôles actuellement visibles à l'écran sont instanciés, rendant le dépliage de 25 000 éléments instantané et fluide (60 FPS).

