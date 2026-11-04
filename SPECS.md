# SPECIFICATIONS

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
