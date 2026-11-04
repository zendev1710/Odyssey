# Map

The type of display on the map and the overview map can be configured here. The map is drawn by different "renderers", which can be configured and switched off separately here.

![Options - Map](../../images/menu_extras_options_map.gif)

**Display of the navigation bar at the top edge of the map.
A number of setting options can be displayed above the main map. On the one hand, there is a slider for enlarging or zooming the map, a selection option for the level or layer displayed and finally, if hotspots have been defined, you can jump to these hotspots.

These options take up a little space and can be deactivated; the functionality can then only be used by right-clicking on the map. The pop-up menu (context menu) that appears contains the corresponding menu items.

**Display additional seasonal images** Snow-covered mountains in winter and ice floes on the coast are only displayed if this setting has been selected. If it is not activated, the standard graphics set is used for all seasons.

**Draw map immediately without loading graphics completely**
If this option is activated, the map is drawn immediately after loading the CR without first loading all region graphics into the cache. This means that the initial display of the map is slightly faster, but only the graphics of region types that occur on the current map section are loaded. This can lead to delays when moving the map section if region graphics still need to be loaded.

**Show tooltips**
If this option is activated, tooltips are displayed when the mouse hovers over regions on the map, showing the region name, type, farmers and pool silver.

## Main map

The rendering layer divides the display into logical sub-objects, such as regions, roads, buildings, etc., whose respective setting options are then displayed in the box below.

### Regions

&lt;inactive&gt;
    Regions are not displayed.
**Region renderer**
    This is the default setting in which the regions are displayed with graphics.
  **Region renderer (geometric/political)**
    This mode displays the regions as differently coloured areas. The following display modes are possible
  **Region type**
        Here you can assign a colour to each region type (mountain, swamp etc.).
  **Political**
        If this mode is active, the region is coloured in the colour assigned to the faction with the most people in the region.
  **All factions**
        Here you can assign a colour to each faction in the report. If there is more than one faction in a region, the regions are displayed by vertical subdivisions in the colours corresponding to the respective factions.
  Confidence level
        Display of the region colours according to the trust level of the factions there.
  **Trust level (guarding)**
        Display of the region colours according to the trust level of the guarding factions.
  **ARR (advanced region renderer)**
    This mode allows you to customise the display of the region depending on the conditions you create. You can find more information in the [Description of the replacement system](../../reference/atr_arr.md).

    For all settings, please note that the colour assignment of the factions is identical for all different display modes.

### Streets

&lt;inactive&gt;
    Roads are not displayed.
**Street renderer**
    This is the default setting in which the roads are displayed with graphics.

### Buildings

&lt;inactive&gt;
    Buildings are not displayed.
**Building renderer**
    This is the default setting in which the castles are displayed with graphics.

### Ships

&lt;inactive&gt;
    Ships are not displayed.
**Ship renderer**
    If this option is active, existing ships are displayed on the map. Docked ships are displayed on the respective docking coast, ships under construction and on the ocean in the centre of the region.

### Labelling

&lt;inactive&gt;
    Region names are not displayed.
**Region name renderer**
    If this option is active, the name is displayed above the respective region. It is also possible to set the font and font type here.

* Trade renderer** If this option is selected, the trade good and quantity are displayed.
* **ATR** Here you can define your own labelling. You can find more information in the [Description of the replacement system](../../reference/atr_arr.md).

### Paths

&lt;inactive&gt;
    Paths are not displayed.
**Path renderer**
    If this option is active, arrows are shown on the map if a currently active unit has a NEXT or ROUTE command.

### Markers

&lt;inactive&gt;
    Markers are not displayed.
**Marker renderer**
    If this option is active, the currently active region is highlighted with a white outline and marked regions are provided with a diffuse fog (differs depending on the loaded [graphics set](../../reference/graphicsets.md)).
**Marker renderer (geometric)**
    If this option is active, the currently active region is highlighted with a red outline and marked regions are outlined in white.

### Additional markings

&lt;inactive&gt;
    Additional markers are not displayed.
**Additional images**
    If this option is active, you can display your own images on the map. To do this, the tag _"image name";regionicon_ must be set within the region context, e.g. with template. The images themselves must be located below the magellan.jar directory in the path /res/images/map and correspond to the format described under [Create graphics set](../../reference/graphicsets_making.md).

## MiniMap

![Options - Overview map](../../images/menu_extras_options_map_minimap.gif)

The overview map supports all display modes of the region renderer (geometric/political). The colour assignment can therefore also be set for this. The scaling of the MiniMap can also be set here.
