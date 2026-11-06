# SPECIFICATIONS

## Skills

| Skill          | Category                             |
|----------------|--------------------------------------|
| Alchemy        | Production                           |
| Armoursmithing | Production                           |
| Bow            | Combat                               |
| Cartmaking     | Production                           |
| Catapult       | Combat                               |
| Crossbow       | Combat                               |
| Endurance      | Combat                               |
| Entertainment  | Economy                              |
| Espionage      | Reconnaissance and covert operations |
| Forestry       | Production                           |
| Herbalism      | Production                           |
| Magic          | Magic                                |
| Masonry        | Production                           |
| Melee          | Combat                               |
| Mining         | Production                           |
| Perception     | Reconnaissance and covert operations |
| Polearm        | Combat                               |
| Quarrying      | Production                           |
| Riding         | Movement                             |
| Roadwork       | Production                           |
| Sailing        | Movement                             |
| Shipcraft      | Production                           |
| Stealth        | Reconnaissance and covert operations |
| Tactics        | Combat                               |
| Taming         | Production                           |
| Taxation       | Economy                              |
| Trade          | Economy                              |
| Unarmed combat | Combat                               |
| Weaponsmithing | Production                           |

### Default skill category order

When category grouping is enabled, the default category order is:

1. Combat
2. Movement
3. Production
4. Economy
5. Magic
6. Reconnaissance and covert operations

Skills within each category follow the selected skill sorting mode.

## Map: building symbols

The building display style defaults to **Auto**: use a building icon when one is available,
otherwise draw a colored vector symbol. **Icons** draws only icons; **Symbols** always draws the
vector symbols described below. The fill colors apply to these vector symbols, not to the building
artwork. Symbol colors do not change with faction ownership.

### Shape and color by building type

| Building type  | Shape            | Fill color                       | Letter (EN) | Letter (FR) | Letter (DE) |
|----------------|------------------|----------------------------------|:-----------:|:-----------:|:-----------:|
| Tower          | Square           | Fortification brown `#8A5A44`    |      T      |      T      |      T      |
| Guard tower    | Square           | Fortification brown `#8A5A44`    |      G      |      T      |      W      |
| Guardhouse     | Square           | Fortification brown `#8A5A44`    |      G      |      C      |      W      |
| Scaffold       | Square           | Fortification brown `#8A5A44`    |      F      |      C      |      G      |
| Foundation     | Square           | Fortification brown `#8A5A44`    |      F      |      F      |      G      |
| Fortification  | Square           | Fortification brown `#8A5A44`    |      F      |      F      |      B      |
| Castle         | Square           | Fortification brown `#8A5A44`    |      C      |      C      |      B      |
| Fortress       | Square           | Fortification brown `#8A5A44`    |      F      |      F      |      F      |
| Citadel        | Square           | Fortification brown `#8A5A44`    |      C      |      C      |      Z      |
| Harbor         | Circle           | Water blue `#3F6FA8`             |      H      |      P      |      H      |
| Tavern         | Circle           | Default slate `#6E7C8C`          |      T      |      T      |      T      |
| Portal         | Circle           | Magic purple `#6A4FA8`           |      P      |      P      |      P      |
| Wormhole       | Circle           | Magic purple `#6A4FA8`           |      W      |      T      |      W      |
| Lighthouse     | Tall rectangle   | Water blue `#3F6FA8`             |      L      |      P      |      L      |
| Sawmill        | Square           | Forest green `#3F7A46`           |      S      |      S      |      S      |
| Stable         | Square           | Forest green `#3F7A46`           |      S      |      É      |      P      |
| Quarry         | Square           | Light gray `#B8B8B8`             |      Q      |      C      |      S      |
| Mine           | Square           | Anthracite `#42464A`             |      M      |      M      |      B      |
| Dam            | Rhombus          | Teal `#3E7C7C`                   |      D      |      B      |      D      |
| Caravanserai   | Square           | Sand gold `#B08A3E`              |      C      |      C      |      K      |
| Trading post   | Rhombus          | Sand gold `#B08A3E`              |      T      |      P      |      H      |
| Tunnel         | Rhombus          | Ice blue `#6FA8C8`               |      T      |      T      |      T      |
| Mage tower     | Tall rectangle   | Magic purple `#6A4FA8`           |      M      |      T      |      M      |
| Academy        | Square           | Default slate `#6E7C8C`          |      A      |      A      |      A      |
| Smithy         | Square           | Rust brown `#8A4A3A`             |      S      |      F      |      S      |
| Marketplace    | Circle           | Sand gold `#B08A3E`              |      M      |      M      |      M      |
| Monument       | Triangle         | Monument gray `#9A9A9A`          |      M      |      M      |      M      |
| Stone circle   | Circle           | Monument gray `#9A9A9A`          |      S      |      C      |      S      |
| Unknown type   | Square           | Default slate `#6E7C8C`          |      *      |      *      |      *      |

Vector symbols have a slightly emphasized dark outline (`#141414`). Contrast plates and shadows
from the building-contrast setting are drawn behind bitmap icons only; vector symbols are drawn
directly on the terrain.

### Symbol size and placement

The base marker size is relative to the rendered hexagon size:

| Number of buildings in the region | Base marker size  |
|-----------------------------------|-------------------|
| 1                                 | `0.65 ×` hex size |
| 2–4                               | `0.50 ×` hex size |
| 5 or more                         | `0.40 ×` hex size |

Each vector symbol contains the uppercase first letter of the building type localized to the UI
language. A map setting, enabled by default, forces the English initial; when disabled, the initial
follows the UI language. The table lists the English, French, and German initials. Unknown types
(`*`) use the initial of the source type name. The letter is black or white, whichever has the
stronger contrast with the symbol fill.
The vector shape itself uses 78% of the base size. Fortification markers are enlarged by a further
10%. Tall rectangles use that same size as their height and are 78% as wide. Up to seven buildings
use the center and six perimeter positions; when there are more, six
buildings are placed around the perimeter and a `+n` badge is shown at the center for the remainder.
Fortifications prefer the center, while buildings with a terrain affinity prefer the perimeter slot
facing a matching neighboring region.

## Keyboards shortcuts

- F11: toggle fullscreen

## Bookmarks system

### Toggle bookmark action

* Key Ctrl+F2
* Bookmarks menu with "toggle bookmark" item
* an icon/image in the status bar that can be clicked to toggle bookmark (only if selected objet is bookmarkable; otherwise it's grayed)

The corresponding command toggles bookmark on the current selected object from :

* hex map control selected region (hex)
* tree selected object from tree view (tree in Explorer view model), except faction items (so every other node can be bookmarked, i.e. region, building, ship, unit)

Bookmarking means adding (at first position)/removing the object to/from bookmarks list (BookmarksViewModel Items colleciton).
Items collection has a SelectedItem, binded to a ListBox SelectedItem in the bookmarks view.

In the bookmarks view, or even better in the main window status bar (after the toggle bookmark icon) a combobox can be used to :

* select a bookmarks file to load it (and then reset and update bookmarks list)
* select a 'new...' item to save current bookmarks list to naother bookmarks file (and then reset and update bookmarks list)

### Bookmarks file handling

Bookmarks can be saved (Save... item in Bookmarks menu) in a bookmarsks file (xml) and loaded from it.
Bookmarks can be loaded at game (cr file report = CRDocument) load time.
Auto-loading is done like this: for a `<turn> - <cr short name>`.cr loaded file, all `<cr short name>-*.xml` files are searched for (and loaded if exist).
if exists, the `<cr short name>-default.xml` is laded and the current one used, otherwise the first found file is loaded.

File example:

```xml
<?xml version="1.0" encoding="utf-8"?>
<Bookmarks>
  <Bookmark Type="REGION" Id="1" Name="Erebor" />
  <Bookmark Type="BUILDING" Id="45" Name="The Lonely Mountain" />
  <Bookmark Type="SHIP" Id="23" Name="The Black Pearl" />
  <Bookmark Type="UNIT" Id="78" Name="Gandalf the Grey" />
</Bookmarks>
```

Bookmarks files are saved in the same folder as main save file, with the same name but with `bookmarks-<pattern>.xml` extension.
Global settings have a default bookmarks location.
Global settings have a bookmarks auto-save option (if enabled, bookmarks are saved automatically each time bookmarks list is modified).

## Minimap

### CsMapFx

the full map zoomed-in to 20%, with a rectangle representing the visible main map area and that can be moved by maintaining the mouse button.
A rgion is displayed using its terrain image. 

### Magellan

the full map zoomed-in to 20%.
A region is displayed in a color depending on a predefined property/criteria.

## Statistics system

### CsMapFx

Statistics are displayed for the selected region(s). Information displayed are, by faction (or for all factions) :

- for each race : `<Race>: <people>`
- for each item type : `<Item Type>: <count>`
- for each skill type and level (decreasing order): `<Skill Type> <level>: <number>`
- for each building : `<Building Type>: <count> (size)` - size is displayed only if building is not fully built
- for each ship : `<Ship Type>: <count> (size)` - size is displayed only if shipcraft is not completed

Five checkboxes allow to filter the displayed information (races, items, skills...).
Right clicking on information show a context menu with details (a menu item per unit, or building, or ship...) .
A combobow allows to select a faction (or all factions) to display information for.

A multi-selection mode to select sevral regions on the map is available (Ctrl+click or Shift+click).
In this case, information displayed are the sum of all selected regions.

## Astral

TODO: obtain a map with astral having people.

A region in the astral has the following properties:

- "Nebel";Terrain
- "<island id>";Insel 
- "neighbour";visibility : for unseen astral regions
- a list of SCHEMEN Properties :
  - SCHEMEN X Y
  - "region name>";Name

## Additional views

### Information view

Information view (Ctrl + I in CsMapFx) - display information about the world data (11 tabs as an alternate view) :

- alchemy
- buildings
- resources
- calendar (winter...)
- batlles modifiers
- Races
- Regions
- Production
- Navigation
- Weapons
- Waren ?


## Data & models

Caractéristiques d'une faction.

infos statiques :

- Nom (id)
- Coût de recrutement
- Type de magie
- langue
- Email

infos évolutives :

- number of people
- age en nombre de tours
