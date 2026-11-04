# Migration des icônes Magellan 2 → `Odyssey.Desktop/Assets`

> Branche : `refactor/codebase-restructuring` — Date : 2026-10-25
>
> **Document historique.** L'arborescence intermédiaire `de/` / `en/` décrite ici a été
> remplacée par l'arborescence anglaise unique et le catalogue `Assets/i18n/icons.json`.
> Pour les conventions courantes, voir `src/tools/icons-generator/README.md` et
> `src/tools/icons-generator/data/icons.tsv`.
> Source : `C:\Program Files\Magellan\etc\images\icons` (catalogue Magellan 2)
> Cible : `src/app/Odyssey.Desktop/Assets`
> Sauvegarde avant intervention : `%TEMP%\opencode\assets-backup-20261025-0854` (1139 fichiers, 11 311 Ko)

## 1. Résultat en une page

| Indicateur | Valeur |
|---|---:|
| Icônes GIF dans le catalogue source | 472 |
| Concepts recensés (lignes du tableau de catégorisation) | 331 |
| Catégories | 13 |
| Fichiers dans `Assets` avant / après | 1139 → 1590 (**+451**) |
| Nouveaux chemins d'assets | 578 |
| Chemins disparus (déplacements + 2 doublons) | 127 |
| Conflits de contenu (cible conservée, source non copiée) | 5 |
| Dossiers renommés | 11 |
| Fichiers renommés (normalisation) | 20 |
| Concepts sans équivalent anglais officiel | 207 |
| Concepts avec dossier `en/` | 124 |
| Fichiers de code modifiés | 7 (~40 lignes) |
| Build | 0 erreur, 117 avertissements (préexistants) |

Règle appliquée partout : **aucun fichier préexistant n'a été écrasé**. En cas de contenu différent au même chemin,
la version d'Odyssey a été conservée et le cas consigné au §6.

## 2. Conventions retenues

```
Assets/<catégorie>/<de|en>/<slug>.gif          (icônes localisées)
Assets/misc/<nom technique>.gif                (icônes techniques, non localisées)
```

- `slug` : minuscules ASCII `[a-z0-9_]`, translittération `ä→ae`, `ö→oe`, `ü→ue`, `ß→ss`, espaces et tirets → `_`.
- Variante de détail : suffixe `_detail` (`berge_detail.gif`), toujours en `de/`.
- `lang` = langue **du nom de fichier** du catalogue Magellan, pas la langue de l'interface.
- `misc/` regroupe les icônes purement techniques (`alliancestate_*`, `border_*`, `shore_*`, `tobig`, `missing`…) : pas de découpage `de`/`en`.
- Les dossiers `de`/`en` et tous les fichiers copiés sont en minuscules, pour rester valides sur Windows, macOS et Linux.

## 3. Arborescence

### Avant

```
Assets/
├── FactionRace/      24   noms anglais
├── FactionStatus/     8
├── Flags/             3
├── Help/            515   Content/{de,en,fr}
├── Map/              89   Symbols 48, Terrains 41, Effects 0
├── Terrains/         18   noms anglais
├── buildings/        31   de/
├── effects/          15   de/
├── full/              3
├── images/           10
├── items/           269   de/
├── resources/        11   de/
├── ships/            67   de/ (png animés) + icons/ (18 gif)
├── skills/           59   de/ + en/
└── trade/             8   de/
    (9 fichiers à la racine)
```

### Après

```
Assets/
├── animals/          21 de  +  1 en
├── buildings/        60 de  +  4 en
├── effects/          15 de
├── factions/         19 de  + 12 en
├── factionstatus/     8            (racine, technique)
├── flags/             3            (racine, technique)
├── full/              3
├── help/            515            Content/{de,en,fr}
├── images/           10
├── items/           281 de  + 12 en
├── map/              89            effects 0, symbols 48, terrains 41
├── misc/             72            (technique, non localisé)
├── monsters/         43 de  + 10 en
├── races/            25 de  + 23 en
├── resources/        18 de  +  4 en
├── ships/
│   ├── icons/        21 de  +  3 en
│   └── visual/       49            (png animés, noms neutres)
├── skills/           52 de  + 30 en
├── spells/           19 de  +  9 en
├── status/           46 de  + 28 en
├── terrains/         50 de  + 18 en
└── trade/             8 de
    (9 fichiers à la racine)
```

## 4. Détail par catégorie

« Fichiers source » = fichiers du catalogue Magellan rattachés à la catégorie (les 50 fichiers des jeux
`alliancestate_*` / `border_*` / `shore_*` sont comptés dans `technical`).
« Avant » = nombre de fichiers déjà présents dans le dossier avant migration (0 = dossier créé).

| Catégorie | Dossier | Concepts | Fichiers source | Avant | Après `de/` | Après `en/` |
|---|---|---:|---:|---:|---:|---:|
| Animaux | `animals` | 20 | 21 | 0 | 21 | 1 |
| Bâtiments | `buildings` | 46 | 55 | 31 | 60 | 4 |
| Factions | `factions` | 18 | 20 | 0 | 19 | 12 |
| Objets | `items` | 19 | 31 | 269 | 281 | 12 |
| Monstres | `monsters` | 40 | 45 | 0 | 43 | 10 |
| Races | `races` | 10 | 13 | 24 | 25 | 23 |
| Ressources | `resources` | 7 | 12 | 11 | 18 | 4 |
| Navires | `ships/icons` | 19 | 20 | 67 | 21 | 3 |
| Compétences | `skills` | 44 | 64 | 59 | 52 | 30 |
| Sorts | `spells` | 18 | 20 | 0 | 19 | 9 |
| Statuts / UI | `status` | 45 | 49 | 0 | 46 | 28 |
| Terrains | `terrains` | 28 | 50 | 18 | 50 | 18 |
| Technique | `misc` | 17 | 22 (+50 jeux) | 0 | 72 (racine) | — |

Les écarts `Après > Fichiers source` viennent des icônes préexistantes d'Odyssey (ex. `items/de` : 269 avant) ou
des doublons `de`/`en` légitimes (même mot dans les deux langues, ex. `wolf`, `hunger`, `aura`).

## 5. Actions réalisées

### 5.1 Renommage des dossiers (11)

| Avant | Après | Motif |
|---|---|---|
| `FactionRace/` | `races/{de,en}/` | nom de domaine, pas un nom de dossier ; contenu majoritairement anglais |
| `FactionStatus/` | `factionstatus/` | minuscules |
| `Flags/` | `flags/` | minuscules |
| `Help/` | `help/` | minuscules |
| `Map/` | `map/` | minuscules |
| `Map/Symbols/` | `map/symbols/` | minuscules |
| `Map/Terrains/` | `map/terrains/` | minuscules |
| `Map/Effects/` | `map/effects/` | minuscules |
| `Terrains/` | `terrains/{de,en}/` | minuscules + découpage de/en |
| `ships/de/` | `ships/visual/` | contenu = PNG animés à noms neutres (`boat0.png`…), pas de l'allemand |
| `ships/icons/` | `ships/icons/{de,en}/` | découpage par langue du nom |

Renommages *case-only* réalisés par double renommage temporaire (le système de fichiers Windows est insensible à la casse).

### 5.2 Création des dossiers de langue

`animals/{de,en}`, `monsters/{de,en}`, `spells/{de,en}`, `status/{de,en}`, `factions/{de,en}`, `misc/`,
`races/{de,en}`, `terrains/{de,en}`, `ships/icons/{de,en}`.

### 5.3 Copies

- 472 icônes source → 496 emplacements (une même image peut servir `de/` et `en/` quand le mot est identique).
- 355 contenus nouveaux ; 141 repositionnements d'images déjà présentes ; 108 fichiers déjà identiques (aucune action).
- Journal détaillé fichier par fichier : `docs/ICONS-ASSETS-MIGRATION-LOG.tsv`
  (`action | catégorie | source | cible | note` ; actions : `COPIE`, `DEPLACE`, `DEJA-IDENTIQUE`).

### 5.4 Normalisation des noms (20 fichiers)

| Emplacement | Avant | Après |
|---|---|---|
| `items/de` (10) | `arcanas segen.gif`, `astraler bann.gif`, `helm_der_sieben winde.gif`, `ring der …` (5), `schwarzer lotus.gif`, `tiegel_mit kroetenschleim.gif` | espaces → `_` |
| `skills/de` (1) | `rüstungen.gif` | `ruestungen.gif` |
| `skills/de` (1) | `steinbruch_abbau.gif` (alias généré) | `steinbruch.gif` |
| `map/symbols` (5) | `troop-ally.gif`, `troop-enemy.gif`, `troops-ally.gif`, `troops-enemy.gif`, `troops-unknown.gif` | tirets → `_` |
| `misc` (3) | `leere-detail.gif`, `magellan_leere-detail.gif`, `unbekannt-detail.gif` | `_detail` |

### 5.5 Suppressions (2)

| Fichier | Raison |
|---|---|
| `skills/de/holzfällen.gif` | doublon binaire de `holzfaellen.gif` (hash identique) |
| `skills/de/straßenbau.gif` | doublon binaire de `strassenbau.gif` (hash identique) |

> Remarque de diagnostic : ces deux fichiers n'étaient **pas** corrompus. Ils portaient des caractères Latin-1
> valides (`ä`, `ü`, `ß`) que la console affichait mal. Ils ont été remplacés par leurs équivalents ASCII.

## 6. Conflits — source ≠ cible (5)

Aucun écrasement : la version d'Odyssey est conservée. À arbitrer manuellement si le visuel Magellan est préféré.

| Chemin cible | Taille source | Taille cible | Écart |
|---|---:|---:|---:|
| `terrains/de/sumpf.gif` | 615 o | 191 o | +222 % |
| `terrains/de/mahlstrom.gif` | 1 015 o | 196 o | +418 % |
| `items/de/ruestungen.gif` | 672 o | 402 o | +67 % |
| `items/de/traenke.gif` | 1 068 o | 402 o | +166 % |
| `resources/de/kraeuter.gif` | 133 o | 163 o | −18 % |

Les quatre premiers sont des icônes Magellan plus détaillées que les icônes historiques d'Odyssey ; le dernier est
légèrement plus petit. Rien ne permet de trancher automatiquement : le choix est laissé au mainteneur.

## 7. Références de code mises à jour (7 fichiers)

| Fichier | Lignes | Modification |
|---|---|---|
| `ViewModels/Tools/ExplorerViewModel.cs` | 665-686 | `/Assets/FactionRace/` → `/Assets/races/de/` (22) ; `demons.gif` → `daemonen.gif` ; `race.gif` → `rasse.gif` |
| `Converters/RegionItemAdditionalInfoToResourceUriConverter.cs` | 23-27 | `FactionStatus/` → `factionstatus/` (5) |
| `Converters/RegionItemAdditionalInfoToResourceUriConverter.cs` | 108 | `/Assets/Terrains/` → `/Assets/terrains/de/` |
| `Controls/HexMapControl.cs` | 260-261 | commentaire `Assets/Map/Effects` → `Assets/map/effects` |
| `Controls/HexMapControl.cs` | 3432 | `/Assets/Map/Symbols/castle_owned.gif` → `/Assets/map/symbols/castle_owned.gif` |
| `Controls/HexMapControl.cs` | 3511 | `/Assets/Map/Terrains/` → `/Assets/map/terrains/` |
| `ViewModels/HelpViewModel.cs` | 151 | `avares://Odyssey/Assets/Help/Content/…` → `…/Assets/help/Content/…` |
| `Odyssey.csproj` | 48, 262 | `Assets\Help\` → `Assets\help\` |
| `Controls/Ships/ShipVisual.cs` | 74 | `IconsFolder` : `/Assets/ships/de` → `/Assets/ships/visual` |
| `Controls/Ships/ShipVisual.cs` | 75 | `ExplorerIconsFolder` : `/Assets/ships/icons` → `/Assets/ships/icons/de` |
| `Controls/Buildings/BuildingContrast.cs` | 35 | commentaire `Assets/Map/Terrains` → `Assets/map/terrains` |

Aucune logique métier n'a été modifiée : uniquement des constantes et chemins de ressources.

## 8. Vérifications

| Contrôle | Résultat |
|---|---|
| Chemins `/Assets/...` statiques du code (47) pointent vers un fichier existant | OK |
| Références résiduelles aux anciens dossiers (`FactionRace/`, `Assets/Help`, `ships/de`…) | aucune (hors noms d'énumérations C#) |
| Dossiers `de/` ou `en/` vides | aucun |
| `dotnet build` | 0 erreur, 117 avertissements préexistants |
| Noms d'icônes hors convention `[a-z0-9_.]` | 4 exceptions documentées (§9.7) |

## 9. Points d'attention

1. **Allowlist des bâtiments non mise à jour.** `Controls/Buildings/BuildingVisual.cs` contient `ExplorerIconFiles`
   (32 noms) et `IconFileOverrides` : les 27 nouveaux bâtiments sont copiés mais **ne s'afficheront pas** tant que
   leurs noms ne sont pas ajoutés à cette liste. Décision volontaire de ne pas toucher à la logique dans cette passe.
2. **`UnitItemConverter` code en dur `skills/de` et `items/de`** et ignore `Status`, `Spell`, `Building`, `Ship`.
   Avec `en/` désormais présent, c'est le prochain point à paramétrer par langue — mais `items/en` ne contient que
   12 fichiers, donc un basculement en anglais produirait beaucoup d'icônes manquantes.
3. **`en/` est volontairement incomplet** : 207 concepts n'ont aucun terme anglais dans le catalogue
   (voir `docs/ICONS-EN-MISSING.md`). Aucune traduction n'a été inventée, conformément à la consigne.
   Fichiers `en/` par catégorie : status 28, skills 30, races 23, terrains 18, factions 12, items 12, monsters 10,
   spells 9, buildings 4, resources 4, ships 3, animals 1 — aucun pour `effects`, `misc`, `trade`, `map`.
4. **`items/de` contient 281 fichiers pour 19 concepts Magellan** : la bibliothèque d'objets d'Odyssey (269 fichiers)
   coexistait déjà et a été conservée. Les deux jeux de nommage cohabitent ; `items/de/armour.gif` (nom anglais dans
   un dossier `de/`) est un vestige antérieur, non renommé car non catalogué.
5. **Icônes de races dupliquées** `races/de` (25) et `races/en` (23) : les mêmes visuels sous deux noms. Le code
   utilise `de/` ; `en/` sert de référence. C'est le prix de la séparation par langue pour des images non traduisibles.
6. **`map/effects` est vide** : `boat_wake.png` est référencé uniquement dans du code commenté
   (`HexMapControl.cs:260-261`). Le dossier a été conservé pour ne pas casser la convention.
7. **Exceptions de nommage assumées** : `misc/border_-1.gif`, `misc/shore_-1.gif` (tiret structurel des jeux de
   variantes), `csmap.PNG` (extension majuscule, hors catalogue), `help-icon.png` (hors catalogue).
8. **Git et renommages de casse.** Les dossiers `Map→map`, `Help→help`, `Flags→flags`, `FactionStatus→factionstatus`,
   `Terrains→terrains` n'ont été renommés que sur le disque (double renommage), pas via `git mv`.
   L'index Git contient encore l'ancienne casse : lancer `git add -A` puis vérifier avec
   `git status --short` / `git diff --cached --stat` avant de committer. Sur un clone Linux, l'ancienne casse
   provoquerait des ressources introuvables.
9. **`Assets/help` = 515 fichiers déplacés** (contenu documentaire `Content/{de,en,fr}`). Les liens internes des
   fichiers `.md` sont relatifs et n'ont pas été touchés ; une vérification visuelle de la fenêtre d'aide reste
   conseillée (les URIs sont construites dans `HelpViewModel`).
10. **`LanguageUtils.ReplaceEnglishDigraphsWithGermanCharacterss` n'est pas inversible** (`Steuer` → `Stüer`).
    Elle ne doit jamais servir à construire un chemin d'asset : les chemins reposent uniquement sur le slug
    unidirectionnel décrit au §2.
11. **Sauvegarde** : `%TEMP%\opencode\assets-backup-20261025-0854`. Restauration = supprimer `Assets` et recopier
    ce dossier. Les fichiers de code modifiés (7) sont listés au §7 pour un retour arrière ciblé.

## 10. Annexes

| Fichier | Contenu |
|---|---|
| `docs/ICONS-ASSETS-MIGRATION-LOG.tsv` | journal fichier par fichier (`action | catégorie | source | cible | note`) |
| `docs/ICONS-EN-MISSING.md` | 207 concepts sans équivalent anglais officiel, par catégorie |
| `ICONS-CATEGORIZATION.md` (racine) | tableau de catégorisation d'origine : fichier(s), noms EN/DE/FR, catégorie |
