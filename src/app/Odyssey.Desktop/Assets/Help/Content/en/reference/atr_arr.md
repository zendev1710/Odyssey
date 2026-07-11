# ATR, ARR, tooltips and region info

## Functionality

The so-called replacement system is ultimately a type of programming language that allows certain values to be calculated from the information about regions. The replacement system is used for the ATR and ARR, as well as for the tooltips on the map and the region info in the detailed display. The ATR is used for labelling the map. The ARR can be used to colour regions using a numerical value and a colour scale. Of course, you can also combine ATR, ARR and tooltip. Then you get coloured regions and matching texts on each region with optional additional information. The first lines of the detailed display can also be configured using this language. The following illustration shows a map that is coloured and labelled according to the trade goods.

![Map labelled according to trade](../images/atr_trade.gif)

What substitution options are there? The following words are important in connection with the tooltips. An example is given in brackets. There are

* variables (§herb).
* Compound variables (§item§Speer).
* conditions (§if§not§isOcean§text1§else§text2§end§).
* Compound switches (§faction§abcd). Restrict the function of the following substitutes according to definable criteria
* arithmetic operations (§+§3§2)
* Strings (§any text)

A definition string (line) now consists of several words written one after the other, each preceded by the separator §.

## Simple examples

### Example: Text with variable

§Herb §herb leads to the output Herb Elfenlieb in a region where the herb Elfenlieb grows. If Magellan recognises the word (here "herb"), it is replaced by the corresponding value in each region (here "Elfenlieb"). Otherwise, the text is simply output (here "herb").

### Example: Counting items

The composite variable §item§Item can be used to display the number of items. §item§Spear displays the number of spears in this region. §item§Spear§ §item§Crossbow gives the number of spears followed by a space followed by the number of crossbows. The item must be written exactly as it is named in Magellan.

### Example: Not displayed on oceans

You can restrict the output with if. For example, §if§not§isOcean§trees §trees ensures that the number of trees is only displayed in non-ocean regions. This makes things like trees clearer, as the word trees is not displayed in every ocean field. The §not can also be omitted. Then it is only displayed in ocean regions (which of course makes no sense with trees). The complete syntax is §if\[§not\]§{condition}§{if condition true}§else§{if condition false}§ending.  
Here, not is optional for the negation. The curly brackets including content should be replaced by the desired output.

### Example: Restrict counting to units from one faction

The compound switch §faction§faction number restricts the counting of items to the specified faction. In a region where the faction abcd has 3 spears, §spear §faction§abcd§item§spear leads to the output of spear 3

### Example: friendly and all persons

The composite switch §priv§confidence level restricts counting to factions with the specified confidence level. §priv§clear cancels this restriction. Factions for which the password is set have a trust level of 100. A HELP ALL corresponds to the value 60. If you want to count your own and other people, you can use the following: §priv§100§Own §count§priv§clear§ | All §count .

### Example: Add

The Polish notation is used, i.e. the arithmetic operators are written before the two (or more) operands. §+§item§spear§item§crossbow adds spears and crossbows together. This notation means that no brackets are required.  
a \* (b + c) in Polish notation is \* a + b c, i.e. §\*§a§+§b§c or also §\*§+§b§c§§a
a \* b + c is + \* a b c, i.e. §+§\*§a§b§c or also §+§c§\*§a§b.

## Hints and stumbling blocks

* Make sure that there are no spaces at the end of the line. These prevent the substitutes from being recognised correctly. It is best to put a § at the beginning and end of the line to prevent errors. However, the syntax also allows you to omit § at the beginning and end. Only the word herb also works.
* HTML can be used for tooltips and in the region information. To do this, the entire output must be enclosed in HTML tags, for example &lt;html&gt;&lt;body&gt;<b&gt;§rname§&lt;/b&gt; &lt;i&gt;§herb§&lt;/i&gt;&gt; &lt;/body>&lt;/html&gt;.
§newline creates a line break in the ATR. However, it is better to write a tooltip completely as HTML or use a space instead of the newline.
* In the ARR, the result of the entire expression must be a number. Any string prevents correct output. This also includes line breaks and spaces.
* If the syntax is incorrect, you will not receive an error message, but only incorrect output.
* Whether the output of the ARR is correct is best checked with a corresponding tooltip.
* A replacer that counts something (e.g. spears) does this for all units in a region. If you want to restrict this to your own units, for example, you must use restrictive substitutes such as priv or faction.
* A §clear after faction or priv cancels the restriction.
* A space is obtained by placing a space after §, e.g. §replacer§ §replacer
* If, for example, you do not want to call the replacer herb but output the character string 'herb', you can do this as follows: §\\herb§. A backslash '\\' can be output as follows: §\\§

## What others use, downloads

Here are some further examples of substitutes used by Magellan users.

### Tooltip and ATR (Advanced Text Renderer)

| Name                                                    | Description                                                                                            | Author               |
|---------------------------------------------------------|--------------------------------------------------------------------------------------------------------|----------------------|
| [Trade map](atr/Trade.atr)                              | Use ATR together with ARR trade map                                                                    | Lars                 |
| [Overview trade and herbs, formatted](atr/fmzimmer.atr) | Provides all the data I want to know quickly without having to click on the province name in the list. | Frank-Michael Zimmer |

## AdvancedRegionShapeCellRenderer (ARR)

| Name                                                  | Description                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                  | Author       |
|-------------------------------------------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|--------------|
| [Crossbow Distribution](atr/CrossbowDistribution.arr) | The darker the red, the more crossbows are missing. The darker the green, the more crossbows are over in the region.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                         | Lars         |
| [Plague warning](atr/Pestwarnung.arr)                 | Shows how many jobs the farmers need (working units are not taken into account).  <br>yellow: many jobs available <br>green: some jobs available <br>red: critical, not enough jobs available, danger of plague                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                              | Jochen Schuh |
| [Shopping goods](atr/Einkaufsgut.arr)                 | Coloured map showing where to buy which shopping goods.  <br>Oil - brown <br>Incense - grey <br>Silk - white-blue <br>Myrrh - green <br>Jewel - red <br>Spice - yellow <br>Balm - blue                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                       | Lars         |
| [Herbs](atr/Kraeuter.arr)                             | Coloured herb map. Each terrain has a basic colour that varies in 3 brightness levels depending on the herb. White for ocean. Purple if the region has not yet been explored. Colours, herbs and returned values for the colours: <br>**Plain green tones** Spicy daring: 1, Owl's eye: 2, Flatroot: 3, **Desert yellow tones** Sand rot: 4, Water finder: 5, Cactus sweat:6, **Swamp orange tones** Gherkin: 7, Bubble morel:8, Knotty suckerwort: 9, **Highland red tones** Mandrake: 10, windbag: 11, fjord growth: 12, **mountains shades of grey** rift wax: 13, cave glimmer: 14, stonecrop: 15, **glaciers shades of turquoise** white rake: 16, snow crystal: 17, ice flower: 18, **forest shades of blue** elf love: 19, green spinneret: 20, blue tree ringlet: 21 | Lars         |

## Region short info

This can be configured under Options - Detailed display. Tip: You can also use Enter after each line for clarity

NameDescriptionStringAuthor[The original](atr/reginfoorig.txt)The default setting

|                |                                                                                                             |               |                                                                                              |
|----------------|-------------------------------------------------------------------------------------------------------------|---------------|----------------------------------------------------------------------------------------------|
| Peasants       | §peasants                                                                                                   | Mallorn/trees | §if§>§mallorn§0§mallorn§else§trees§end§                                                      |
| recruits:      | §recruit                                                                                                    | saplings:     | §sprouts                                                                                     |
| surplus:       | §if§<§peasants§maxWorkers§\*§peasants§-§peasantWage§10§else§-§\*§maxWorkers§peasantWage§\*§10§peasants§end§ | horses:       | §horses§                                                                                     |
| Entertainment: | §entertain§                                                                                                 | Iron/Laen     | §if§<§0§laen§if§<§0§iron§iron§ / §laen§else§laen§end§else§if§<§0§iron§iron§else§-?-§end§end§ |
| silver pool:   | §priv§100§item§silver§priv§clear§                                                                           | stones:       | §stones§                                                                                     |
| Trade:         | §maxtrade§                                                                                                  | Herb:         | §herb§                                                                                       |

[Region & Faction Info](atr/reginfo2.txt)Resource pool and earning opportunities in the region. The information is partially incomplete (e.g. no flaming swords)

|                    |                                                                                                                                                               |                 |                               |
|--------------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------|-----------------|-------------------------------|
| Farmers            | §peasants§                                                                                                                                                    | recruits        | §recruit§priv§100§            |
| max. taxes         | §if§<§peasants§maxWorkers§\*§peasants§§-§peasantWage§10§else§§-§\*§maxWorkers§peasantWage§\*§10§peasants§end§                                                 | pool silver     | §item§silver§                 |
| potential earnings | §\*§20§+§skillsum§entertainment§skillsum§tax collection§                                                                                                      | wood            | §item§wood§                   |
| Weapons            | §+§item§Spear§+§item§Hellard§+§item§Sword§+§item§War Axe§item§Bihänder§ / §+§item§Crossbow§+§item§Mallorn Crossbow§+§item§Bow§+§item§Catapult§item§Elven Bow§ | Chariot / Horse | §item§Chariot§ / §item§Horse§ |
| Armour / Shield    | §+§item§chainmail§item§plate armour§ / §item§shield§                                                                                                          | Iron / Stone    | §item§iron§ / §item§stone§    |
| Fighter            | §+§skill§Pole weapons§skill§Slashing weapons§ / §+§skill§Crossbow§+§skill§Archery§skill§Catapult use§                                                         |                 |                               |

Lars

## Replacement list and explanations with examples

| Replacer                  | Explanation - if nothing else is specified, the replacer applies to a region. Parameters must be specified exactly as they are displayed by Magellan. So item§stone for stones of units. Not item§stone and also not item§stones | Explanatory example. If none is specified, the name of the replacer is sufficient to obtain an output |
|---------------------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|-------------------------------------------------------------------------------------------------------|
| \+ - \* /                 | Addition, subtraction, multiplication or division of numbers. Uses Polish notation, i.e. first the operator, then the arguments.                                                                                                 | 3 + (peasants \* 3) = §+§3§\*§wage§peasants                                                           |
| <                         | Returns _true_ if the value of the first parameter (if possible as a number, otherwise as a string) is less than the second.                                                                                                     | Returns _true_ if the number of pawns is greater than 5: §<§5§peasants                                |
| cmd                       | Returns the § character.                                                                                                                                                                                                         | §cmd§ 1 of the Basic Law                                                                              |
| contains                  | Checks whether the second argument (as a string) occurs in the first (as a string). It is case-sensitive.                                                                                                                        | Returns _true_ for the region Lummerland: §contains§rname§land§                                       |
| containsIgnoreCase        | As above, but case-sensitive.                                                                                                                                                                                                    | Returns _true_ for the region Lummerland: §contains§rname§Land§                                       |
| coordinate                | Returns the coordinates of the region. "x, y" in level 0, "x, y, z" in other levels                                                                                                                                              |                                                                                                       |
| count                     | Number of people in all units in the region. Can be narrowed down using filters.                                                                                                                                                 | number of persons: §count                                                                             |
| countUnits                | Number of units                                                                                                                                                                                                                  | Number of units: §countUnits                                                                          |
| description               | Provides the description of describable objects such as regions or units                                                                                                                                                         | Counts "soldiers" in the region: §filter§contains§description§Soldat§count                            |
| entertain                 | Maximum possible entertainment as specified in the CR                                                                                                                                                                            | UnterhaltMax: §entertain                                                                              |
| equals / equalsIgnoreCase | Returns _true_ if the two arguments are the same. The second variant ignores upper/lower case. Works for numerical values and character strings, but may not work if they are mixed.                                             | if§equals§herb§Iceflower§Here grows iceflower§No iceflower§end <br>§equals§5§+§2§3 returns _false_!   |
| faction                   | Restricts the following substitutes to the specified faction. Specify faction number. faction§clear cancels the restriction.                                                                                                     | Counts persons for faction abcd and all: §faction§abcd§abcd: §count§faction§clear§ All: §count        |
| Filter | filter | Filters units based on a replacer that is passed as the first argument. This filter is applied to substitutes such as count. Is cancelled with §end.                                                                                                                                                                                                                                                                                                                                                                  | Counts "soldiers" in the region: §filter§contains§description§soldier§count§end |
| herb | Returns the herb growing in the region.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                               | |
| horses | number of horses | |
| if | §if§condition§replacer1§end or §if§condition§replacer1§else§replacer2§end. If the condition is _true_, execute Replacer1. Can be extended with else, then Replacement2 is executed if the condition is _false_. Nesting is possible | If less than 100 horses, write "less than 100", otherwise write "more than or equal to 100": if§<§horses§§100§less than 100§else§more than or equal to 100§end |
| iron | iron not yet mined | |
| ironlevel | the current level at which iron can be mined | |
| isActiveVolcano, isMountains etc.   | returns _true_ if the terrain of the region corresponds to the specified type | if§isLevel§Region is level§else§Region is no level§end |
| item | Number of an item across all units. Item must be specified exactly as written in the report. Can be restricted with filters.                                                                                                                                                                                                                                                                                                                                                                                | item§Spear |
| laen | not yet mined laen | |
| laenlevel | the current level at which Laen can be mined | |
| mallorn | Returns the amount of mallorn available as a resource in the region.                                                                                                                                                                                                                                                                                                                                                                                                                                                            | |
| mallornregion | returns true if the region is a mallorn region, false otherwise.                                                                                                                                                                                                                                                                                                                                                                                                                                                                      | |
| maxWorkers | max. available workplaces, trees taken into account.                                                                                                                                                                                                                                                                                                                                                                                                                                                                         | |
| maxtrade | trade volume before prices change | |
| morale | farmer morale (E3) | |
| name | Returns the name of nameable objects. These are currently units, regions, buildings, ships, islands, spells and potions. In "normal" use, this replacer will return the name of the current region. In connection with unit filters, however, it returns the name of a unit, which can be used for filtering using string comparison/content.                                                                                                                                                    | Counts persons from units A-J: §filter§<§name§K§count§ |
| newline | Inserts line break. Does not work with tooltip; use HTML there.                                                                                                                                                                                                                                                                                                                                                                                                                                                             | first line§newline§second line |
| not | Negates replacer. _true_ becomes _false_ and _false_ becomes _true_ | Often used to exclude ocean: <br>if§not§isOcean§no ocean |
| null | Returns _true_ if the argument is _null_, otherwise _false_.                                                                                                                                                                                                                                                                                                                                                                                                                                                                        | §if§null§iron§-§else§iron§end |
| oldHorses and other old... values | Returns the value from the previous round | oldHorses |
| op | op is a parametric replacer that can process _true_ or _false_ as a parameter. It stands for OperationSwitch - you switch the mode of operation of operators. If the value behind "op" is _true_, _null_\ values (e.g. errors in previous calculations or unknown values) are interpreted as 0, otherwise as incorrect (and the calculation is cancelled).  <br>Example: A neighbouring region has an unknown number of trees. This means that /§trees§2 results in "-?-". But §op§true§/§trees§2 returns 0. | §op§true§+§iron§laen§op§false§ |
| peasantWage | Labour wage for peasants taking into account the castle bonus | |
| peasants | number of peasants |
| posX, posY, posZ | Returns the x-, y- or z-coordinate of the region.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                     | coordinates: §posX§,§posY |
| price | Price for a luxury good. Specify luxury good as in Magellan | price§oil |
| priv | Restricts the following substitutes to factions with a minimum trust level. Specify trust level. Trust level is displayed in the faction statistics. §priv§clear cancels the restriction.                                                                                                                                                                                                                                                                                                                                | All persons to whom HELP ALL is set: priv§60§count |
| privminmax | Restricts the following substitutes to factions whose trust level lies between the two entries | Number of persons to whom any HELFE is set (but not their own): privminmax§1§60§count |
| recruit | Maximum recruits of the region | |
| rname | region name | |
| rtype | terrain, e.g. plain | |
| silver | silver of the farmers | |
| skill | Number of people who have the specified skill.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                     | skill§entertainment |
| skillmin | Number of people who have a skill with the specified minimum level. Specify talent and level | skillmin§entertainment§3 |
| skillminsum | Talent levels added together. A minimum talent level must be present in order to be counted. E.g. to determine possible production quantity; specify talent and level.                                                                                                                                                                                                                                                                                                                                                            | skillminsum§Woodcutting§2 |
| skillsum | Added talent levels; specify talent.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   | skillsum§trade |
| soldchar1 | Purchasable luxury good, first letter | |
| soldchar2 | Purchasable luxury good, first two letters | |
| soldname | Purchasable luxury good, full name | |
| soldprice | Purchasable luxury good, purchase price; positive value.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    | |
| sprouts | number of sprouts | | stones |
| stones | stones not yet mined | |
| stoneslevel | | The level at which stones can currently be mined | |
| substr | Returns part of a character string. The first two arguments are start (inclusive) and end (exclusive), the third argument is the character string. Negative values are calculated from the end of the string.                                                                                                                                                                                                                                                                                                           | Last and first two letters of the region name: §substr§0§2§rname§...§substr§-2§-1§rname |
| tag | Specifies the content of a tag. Tags can be set by tools or via the unit and region context menu.                                                                                                                                                                                                                                                                                                                                                                                                                | If the regionicon tag exists, it can be displayed as follows: tag§regionicon |
| tagblank | Specifies the content of a tag. Specify tag name. If the tag name does not exist, an empty string is returned instead of the usual -?- | tagblank§regionicon |
| trees | number of trees | |
| wage | Wage of a person for player units | |
