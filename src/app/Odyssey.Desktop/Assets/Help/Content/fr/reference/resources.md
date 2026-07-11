# Ressources

Attention ! Ces informations sont obsolètes et certaines d'entre elles ne fonctionneront plus.

Cette section explique comment ajouter des fonctionnalités supplémentaires à Magellan et comment remplacer les fichiers standard fournis, par exemple pour utiliser d'autres graphiques ou pour s'adapter à un PBeM différent.

## Comment définir un chemin d'accès aux ressources ?

Pour intégrer des plug-ins, des jeux de graphiques, etc. dans Magellan, ils doivent être inclus dans un chemin de ressources. Pour ce faire, appelez les options via le menu Extras, sélectionnez l'onglet 'Ressources' et cliquez sur 'Nouveau chemin...'. Une boîte de dialogue de sélection de fichiers apparaît alors, dans laquelle seul le fichier JAR ou ZIP concerné (ou le répertoire correspondant) doit être spécifié.

Magellan doit être redémarré pour que les modifications soient prises en compte.

## Plug-ins

Certaines fonctions de Magellan ne sont pas incluses dans la version standard, mais peuvent être ajoutées ultérieurement. Un exemple est le look & feel skinnable.

Pour pouvoir utiliser les skins, Magellan a besoin de classes Java supplémentaires qui offrent cette fonctionnalité. Elles sont contenues dans le fichier skinlf.jar, dont Magellan n'a pas connaissance au départ. Ce n'est qu'en créant le fichier skinlf.jar comme nouveau chemin de ressources dans les options de Magellan que ce dernier peut trouver et utiliser les nouvelles classes (voir aussi [Options/Système](../menus/extras/options_system.md)).

## Objets modifiables

Les éléments suivants, appelés ressources, peuvent être personnalisés dans Magellan :

* Toutes les icônes qui apparaissent dans les affichages arborescents (par exemple les talents).
* Les graphiques utilisés pour afficher les régions et les autres objets sur la carte
* Le fichier de règles, qui contient des informations de base sur un jeu (par exemple, les coûts de recrutement de chaque race).

## Structure des répertoires

Toutes les ressources sont placées dans un répertoire spécifique en fonction de leur utilisation prévue :

* about : concerne le dialogue d'information de Magellan
* images : contient les icônes et les graphiques de la carte
* lang : contient les tables de traduction en fonction de la langue
* rules : contient les fichiers de règles

Si vous créez vos propres ressources pour Magellan, il est nécessaire qu'elles soient placées dans le bon répertoire afin que Magellan puisse les trouver.

## Chemins d'accès aux ressources

Normalement, Magellan trouve toutes les ressources nécessaires dans le fichier magellen.jar lui-même. Cependant, il serait très peu pratique de devoir modifier ce fichier pour utiliser ses propres ressources. Il est donc possible de spécifier des chemins d'accès aux ressources qui seront recherchés dans l'ordre avant le contenu du fichier jar.

Les chemins de ressources peuvent être ajoutés, supprimés ou modifiés dans les options de Magellan (menu Outils, Options, onglet 'Ressources'). Les chemins de ressources ne doivent pas nécessairement être des répertoires, ils peuvent également faire référence à une URL sur Internet ou pointer vers un fichier JAR.

Par exemple, si vous souhaitez remplacer l'icône du talent équestre, placez le fichier reiten.gif dans le répertoire C:\NRessourcen\Nimages\Nicons et donnez à Magellan le nouveau chemin de ressources C:\NRessourcen. Comme Magellan recherche maintenant le fichier "images", il commence par les chemins d'accès aux ressources, dans ce cas C:\NRessources, et trouve le fichier en ajoutant le chemin d'accès aux ressources et le nom du fichier aux sous-répertoires.

La séquence exacte de chargement des ressources est la suivante

1. chemins d'accès aux ressources dans l'ordre
2. dans le répertoire courant (généralement le répertoire dans lequel se trouve également le fichier magellan.ini), le sous-répertoire 'res'
3. chargement via SystemClassLoader, ce qui signifie essentiellement que les répertoires et les fichiers JAR spécifiés dans la variable d'environnement CLASSPATH sont recherchés
4. chargement via SystemClassLoader avec le répertoire res en tête, ce qui devrait permettre de trouver la ressource, si elle est disponible, dans le fichier JAR à partir duquel Magellan est exécuté.

Cette séquence de chargement montre que de nouvelles ressources peuvent également être intégrées sans spécifier de chemin d'accès, en les enregistrant simplement dans le même répertoire que Magellan (la structure du répertoire de la ressource concernée, par exemple images/icônes, doit bien sûr être respectée). Vous pouvez connaître cette structure de répertoire en renommant le fichier Magellan.jar en Magellan.zip et en le décompressant.

## Jeux graphiques

Sur la page de [création de vos propres jeux graphiques] (graphicsets_making.md), vous trouverez également des informations sur les chemins d'accès aux ressources.
