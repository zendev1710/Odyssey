# TODO

Content moved in [zendev monday Odyssey workspace](https://zendev-team.monday.com/).

## Features in progress

### Bookmarks system

- [ ] 'New bookmarks set' item in the status bar's combobox
- [ ] Manage bookmarks (dialog box)
- [ ] Combobox control content to up (not bottom), using flyout ?
- [ ] Bad design preview for starcontrol

### Statistics view

TO BE FINISHED.

### Search/Find system

TO BE DONE.

### Settings system

- [x] Use [ConfigFactory.Avalonia](https://www.nuget.org/packages/ConfigFactory.Avalonia) for GUI settings pages
- [x] Restore/save settings by (de)serializing into an application settings JSON file (config.json)
- [x] Restore/override settings by (de)serializing into an user settings JSON file (config.json)

### Translation system

- [ ] Achieve missing translations displayed -- xxx -- in the UI and stored in to-translate bookmarks
- [ ] Achieve missing german translations marked [de]

### Tooltips system

TO BE FINISHED.

- [ ] Region properties views

## File system

- [x] Drag and drop to open a Report .cr file
- [ ] Drag and drop to open an Orders .txt file
- [x] Drag and drop to open a Report .cr file coming embedded in a zip file
- [x] Drag and drop to open several Report .cr files
- [ ] Drag and drop to open several Orders .txt files
- [x] Drag and drop to open several Report .cr files, each one embedded in a zip file
- [ ] Drag and drop to open mixed types files (.txt; .cr, .zip coming)
- [ ] Extract report .cr file from the active opened zip => extract and transform zip document as CR document
- [ ] Extract report .cr file from each opened zip
- [ ] Merge and save CR files

## Features upcoming

- Multi-CR file open system using drag and drop
- Add the default theme based on system theme (theming)
- Add a combobox in a settings panel to select a theme among the 4 themes: default simple, dark, light (theming)
- Closed dock windows display handling
- Copy/Paste from/to clipboard for data
- Persisten layout system (save/load layout)
- App-wide persistent settings system, with :
  - group of active factions (`ActiveFactionGroup`) - Regions explrer
  - Toobar switch status (visible or not)
  - `Show only map` option (maximize and hide other dock windows / restore layout) - Ctrl + Shift + K
  - Map/minimap display options
  - Theme selection
- Recently opened CR system
- Map simple display (regions/islands/oceans...) - using OpenGL ?
- Orders check system
- Toolbar with theme icons
- Translations system (language selection, resources files...)
- Remove the switch theme icon from the top bar (when themes combox is available)
- Add the simple theme for a lighter app (theming)
- Overview map (minimap)
- Send Orders

### Information view

Information view (Ctrl + I in CsMapFx) - display information about the world data (11 tabs in a navigation view ... on central area ?) :

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

### Orders view additional features

- Set Unit orders view or text editor as readonly when other than an orders-editable unit is selected
- [Define the language grammar](https://benparizek.com/notebook/notes-on-how-to-create-a-language-grammar-and-custom-theme-for-a-textmate-bundle) for orders instructions, with [TextMateSharp](https://github.com/danipen/TextMateSharp)
- Implement syntax color using [TextMateSharp](https://github.com/danipen/TextMateSharp), see [AvaloniaEdit](https://github.com/AvaloniaUI/AvaloniaEdit/)
- Implement auto-completion using [TextMateSharp](https://github.com/danipen/TextMateSharp), see [AvaloniaEdit](https://github.com/AvaloniaUI/AvaloniaEdit/)
- Add a "navigate between unconfirmed only/editable units" toggle fluent icon button

## Improvements

- [x] Load then 'save as' a CR file without any change create a CR file with exactly the same content as the original one
- 'Expand all' action in explorer view
- 'Collapse all' action in explorer view
- Icon for Ships node and each ship node
- Icon for Buildings node and each building node
- Icon for Factions node and each faction node (race)
- Display all messages in report view as a list view (not as a 1-depth tree view)
- Avoid data view update (tree rebuild and so on) when it's not necessary
- Unit orders view: prev/next buttons as images to the right side of the status bar
- Unit orders view: confirmed checkbox as a fluent toggle icon
- Optimize the file loading service
- Translate GUI in the current language for each view
- Translate tab contextual menu items
