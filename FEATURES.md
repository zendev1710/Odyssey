# Features

## Keyboard shortcuts

## File management

- Ctrl+O: Open a file dialog to open a Report .cr file
- Ctrl+S: Save the current Report .cr file
- Ctlr+Shift+S: Save the current Report .cr file as a new file

- F11: toggle the fuullscreen mode
- Alf+F4: Close the apllication

## User interface


### Settings system

TO BE FINISHED.

- [x] Use [ConfigFactory.Avalonia](https://www.nuget.org/packages/ConfigFactory.Avalonia) for GUI settings pages
- [x] Restore/save settings by (de)serializing into an application settings JSON file (config.json)
- [ ] Restore/override settings by (de)serializing into an user settings JSON file (config.json)

### Translation system

- [x] Main view
- [x] Building details view
- [x] Ship details view
- [x] Unit details view
- [x] Explorer view
- [x] Region properties view

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

## Factions management

- [x] Multi active factions management
- [ ] Report owner taken into account when opening a Report .cr file

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
  - battles modifiers
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


## Known limitations

Here are main missing features compared to Magellan:

- minimap view
- statistics view
- search view
- information view
- search results view
- errors view
- profiles management
- islands management
- map other planes display (astral...)
- reports merging management

Here are main missing features compared to CsMapFx:

- minimap view
- region statistics view
- search view
- information view
- search results view
- errors view
- islands management
- map other planes display (astral...)
- map regions/islands multiselection
- map region borders/buildings/boats... display
- reports merging management

## Known issues

- Overview : tree badly sorted according to the content. In a region node, it should be :
  - Buildings
  - Ships
  - Active Factions
  - Allied Factions
  - Other Factions
