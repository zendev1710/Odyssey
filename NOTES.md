# Notes

## Dev information

ScrollViewer:

- Extent => total size of the scrollable content
- Viewport => size of the visible area
- IsDeferredScrollingEnabled: if true, scroll only after mouse button released (use for performance reason)

### Dev app packaging

self contained-app = recommended approach <= .NET runtime included
dotnet publish -r win-x64 --self-contained true
on a MSI per .NET target
dotnet publish app/Odyssey.Desktop/Odyssey.csproj -c Release -r win-x64 -f net8.0 --self-contained true -o publish/net8.0
dotnet publish app/Odyssey.Desktop/Odyssey.csproj -c Release -r win-x64 -f net9.0 --self-contained true -o publish/net9.0

### Dev doc generation

docfx could be used.

## Menus

Help
    Interface Help
	About Odyssey

## Keyboards shortcuts

I would like to handle shortcuts with two letters pressed (but not possible in Avalonia at this moment) :

- ctrl+W+I => region info
- ctrl+W+B => battles tab
- ctrl+W+F => search results tab
- ctrl+W+R => report tab
- ctrl+W+E => errors tab
- ctrl+W+D => details tab
- ctrl+W+S => statistics tab
- ctrl+W+O => unit orders tab
- ctrl+W+P => region properties
- ctrl+W+M => map
- ctrl+W+X => regions explorer

## Translations

Workspace tip in FR :basculer l'affichage vers la vue principale de l'application.

## Reports information

### Global CR

- Astral with 34 regions, bounds = -5 -2 -> 7 6
- Standard : 5573 regions, bounds = -77 -77 -> 70'; 230 visible regions

- Ruka has laen
- Gobur has mallorn
- Tudwokul => desert visible because traveled on it, but no people (only peasants) on it
- Wall of fire -7 15 visible because lighthouse on it, , but no people (only peasants) on it


## CR content

### caution when saving

German special characters (Schößlinge...)

### What is modified each time it's saved

- DATE: `1745694818;date`
- 

### What is modified in a CR with Magellan

- `1;aktiveRegion` for the selected region when CR saved
- `1;bookmark` and `"Šuma";bookmarkname` as last region datablock for a new region bookmark

**What was modified after saving original global_map.cr  with magellan Version 2.1.0**.

0;SHOWSKCHANGE => 0;TALENTVERSCHIEBUNGEN
1;SHOWSKCHANGE => 1;TALENTVERSCHIEBUNGEN

Dans les ZAUBER, name est traduit d'allemand en anglais
ZAUBER 1619726087
"Magischer Pfad";name => "Magic Path";name

Les blocs TRANK (potions) sont pas dans le même ordre, avec : 
- traduction du nom d'allemenad en anglais

0;Kampfstatus avant demonen et pas après
"Dämonen";wahrerTyp après ...;Typ

les SPRUCHE sont traduits en anglais

557;Runde ajouté aux RESOURCE

Propriétés avec valeurs 0 ou "" ajoutés aux terrains spéciaux (ozean, Gletscher...) :
ozean :
"";Beschr
0;Baeume
0;Schoesslinge
0;Bauern
0;Pferde
0;Silber

nnnnn;letzteunterh ajouté à certaines régions, avec même valeur  que Unterh
ajout de pas mal de traductions dans TRANSLATION
=> 25000 lignes de plus sur global_map !

### What is modified in a CR with CsMapFx

## Magellan changelogs

### Magellanb 2.1.0 

- Order detection and processing was improved in many parts. ECheck is practically unnecessary and replaced by the "Open Tasks" dock.
- Support for convoys (GIVE xyz 1 SHIP).
- Further rules updates implemented (galleons, dragon ships, !@ orders, PAY NOT, PROMOTE, no more SIEGE, heroes, LEARN AUTO, QUIT, mixing skills, trolls and horses, ...).
- Improved quick find dialog (CTRL+G)

* Magellan now requires Java 11. The installer now comes with its own Java version, so no need for most users to worry about Java. New versions can be downloaded automatically.
* Better MacOS compatibility.
* The ARR and ARR properties were polished. An updated explanation can be found in the Help dialog.
* Order detection and processing was improved in many parts. ECheck ist practically unnecessary and replaced by the "Open Tasks" dock.
* Support for convoys (GIVE xyz 1 SHIP).
* Further rules updates implemented (galleons, dragon ships, !@ orders, PAY NOT, PROMOTE, no more SIEGE, heroes, LEARN AUTO, QUIT, mixing skills, trolls and horses, ...).
* Improved quick find dialog (CTRL+G)
* ExtendedCommands use JShell now, making it much faster. ExtendedCommands allow you to automate your orders.
* The "BattleAnalyzer" plugin displays battles a bit more detailed.
* The skill statistics of the faction statistics has been improved.
* Map zooming and panning with the mouse.
* Improved Help, including the Eressea Help (updated from the Wiki). English help is, unfortunately, still a TODO.
* Support for other games (E3, Atlantis, Drachensgrab, ...)
* Many, many further bug fixes and improvements ...

## Miscellaneous

vie locale/gestion/administration/activité ??? existe sur un firewall !

