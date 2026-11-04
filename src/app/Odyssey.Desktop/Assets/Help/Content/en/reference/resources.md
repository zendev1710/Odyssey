# Resources

Attention! This information is outdated and parts of it will no longer work.

This section explains how to add additional features to Magellan and how to replace the standard files supplied, for example to use alternative graphics or to adapt to a different PBeM.

## How do I set a resource path?

To integrate plug-ins, graphic sets etc. into Magellan, they must be included in a resource path. To do this, call up the options via the Extras menu, select the 'Resources' tab and click on 'New path...'. A file selection dialogue now appears in which only the relevant JAR or ZIP file (or the corresponding directory) needs to be specified.

Magellan must be restarted for the changes to take effect.

## Plug-ins

Some Magellan functions are not included in the standard version, but can be retrofitted. One example is the skinnable look & feel.

To be able to use the skins, Magellan requires additional Java classes that offer this functionality. They are contained in the skinlf.jar file, which Magellan is initially unaware of. Only by creating the skinlf.jar file as a new resource path in the options in Magellan can Magellan find and use the new classes (see also [Options/System](../menus/extras/options_system.md)).

## Modifiable objects

The following elements, so-called resources, can be customised in Magellan:

* All icons that appear in the tree-like displays (e.g. talents)
* The graphics used to display the regions and other objects on the map
* The rules file, which contains basic information about a game (e.g. the recruitment costs of each race)

## Directory structure

All resources are located in a specific directory depending on their intended use:

* about: concerns the info dialogue of Magellan
* images: contains icons and map graphics
* lang: contains the language-dependent translation tables
* rules: contains the rules files

If you create your own resources for Magellan, it is necessary that they are located in the correct directory so that Magellan can find them.

## Resource paths

Normally, Magellan finds all the necessary resources in the magellen.jar file itself. However, it would be very impractical if you had to change this file in order to use your own resources. It is therefore possible to specify resource paths that are searched for resources in sequence before the contents of the jar file.

The resource paths can be added, deleted or edited in the options in Magellan (Tools menu, Options, 'Resources' tab). Resource paths do not necessarily have to be directories, they can also refer to a URL on the Internet or point to a JAR file.

For example, if you want to replace the icon for the riding talent, place the file reiten.gif in the directory C:\\Ressourcen\\images\\icons and give Magellan the new resource path C:\\Ressourcen. As Magellan is now looking for the file 'images\\icons\\riding.gif', it starts with the resource paths, in this case C:\\Resources, and finds the file by appending the resource path and the file name with the subdirectories.

The exact loading sequence of resources is as follows

1. resource paths in sequence
2. in the current directory (usually the directory in which the magellan.ini file is also located) the subdirectory 'res'
3. loading via SystemClassLoader, which essentially means that the directories and JAR files specified in the CLASSPATH environment variable are searched
4. loading via SystemClassLoader with the res directory in front, which should find the resource, if available, in the JAR file from which Magellan is executed

This loading sequence shows that new resources can also be integrated without specifying a resource path by simply saving them in the same directory as Magellan itself (the directory structure for the respective resource, e.g. images/icons, must of course be observed). You can find out this directory structure by renaming the Magellan.jar to Magellan.zip and unpacking it.

## Graphic sets

On the page for [creating your own graphics sets](graphicsets_making.md) you will also find information on the resource paths.
