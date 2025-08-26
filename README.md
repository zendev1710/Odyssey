# Odyssey

Modern desktop MDI docking-windows based application that can be used for [Eressea](https://wiki.eressea.de/index.php?title=Hauptseite/fr) game.

## Features

### File management

- Open and save .cr file(s)
- Open orders .txt file
- Save a modified orders document as an orders .txt file
- Save active faction orders from the active report document

### Views for the active report document

- Region properties view
- Region messages and information view
- Details view (for current unit, ship or building)
- Battles view (with messages)
- All messages report view
- Unit orders view

### Translation

- 3 languages available : english (used by default), german, french

Upcoming feature:

- dynamic language switch with combobox (having country flags)
- default GUI language in application settings
- user prefered GUI language in user settings
- use langugage defined in opened .cr report file
- use langugage defined in opened .txt orders file

### Theming

- Fluent theme
- Light and dark modes

Upcoming features:

- simple theme
- combobox to select a theme bet ween default, simple, light and dark themes
- default used theme in application settings
- user prefered theme in user settings

## Settings management

- settings pages view opened with `View/Settings` menu item
- settings categorized
- settings loaded from `config.json` file on program startup
- settings saved using the `Save` button on the main settings view left panel

## Known issues

- Close/Exit Windows does not work
- Layout management does not work
- Theme switching does not refresh explorer view tree items specific colors
- In fullscreen mode (F11), screen slightly blinks
- [AvaloniaUI issue](https://github.com/AvaloniaUI/Avalonia/issues/2441) : menu hotkey does not work until menu is displayed at lease onetime


## Useful links

* [Magellan](https://magellan.jaatho.de/index_en.php)
* [Eressea wiki](https://wiki.eressea.de/index.php?title=Hauptseite/fr)
* [Eressea GitHub repository](https://github.com/eressea)


