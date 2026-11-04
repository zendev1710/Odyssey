# Region Overview

In the region window all units and regions are displayed in a tree. The tree's structure is set up as follows, in descending order:

1. Islands
2. Regions
3. Factions / Buildings / Ships / Streets
4. Units

![windows_region](../images/windows_region.gif)

Nodes that have units with [unconfirmed orders](orders.html) are shown in bold. The display of the region window can be adjusted to your tastes in the [options](../menus/extras/options_region.html).

Units have a context menu that can be opened by right-clicking on them. This menu has the following options:

* **Copy ID** Copies the unit's ID to the clipboard.
* **Copy ID and name** Copies the unit's ID and name to the clipboard.
* **Copy ID and name and person count** Copies the unit's ID and name, as well as the number of persons in it to the clipboard.
* **Disguise unit** Creates the following orders to disguise the unit: NUMBER UNIT  
    NAME UNIT ""  
    DESCRIBE UNIT ""  
    HIDE FACTION  
    The old values are inserted as persistent comments, so that you can fairly easily undo this action.
* **Add to island/Remove from island**  
    This may be used to group regions.
* **Order ships**  
    You can give orders to the captains of one or more ships.
* **Ship route planner**  
    Used to create one-time or repeating route orders for one or more ships.

The faction node has an **alliance status icon** that shows your HELP status to this faction. Green squares mean this HELP status is set, red squares mean it's not. The squares have the following meaning: silver, combat, give, guard, faction stealth. Your own faction's squares are shown in blue.

When you select several units, the context menu (right click) contains the entry "Add orders". This opens a dialog to add the same orders to all units.
