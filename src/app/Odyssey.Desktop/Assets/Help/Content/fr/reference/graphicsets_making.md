# Créer des jeux de graphiques

Cette fonctionnalité n'a pas été testée depuis longtemps. Si vous l'utilisez avec succès, si vous avez d'autres jeux graphiques ou si vous avez des idées sur la manière de l'utiliser, veuillez faire part de votre expérience à l'équipe de développement.

Pourquoi encore un nouveau format de jeu graphique ? _ C'est précisément la question qui s'est posée lors de la conception du nouveau module cartographique ; le résultat n'est pas en fait un nouveau format de jeu graphique, car le concept présenté ici permet de s'adapter à n'importe quel format pour autant que les informations nécessaires sur la géométrie de la cellule soient disponibles. Cela signifie, par exemple, que les jeux graphiques existants d'autres clients peuvent également être repris tels quels si les graphiques sont au format GIF ou PNG. Il suffit de créer un fichier cellgeometry.txt approprié qui contient les informations sur la géométrie du jeu de graphiques existant.

## Chemins de ressources

La page [resources](resources.md) contient des informations de base sur les ressources et les chemins d'accès aux ressources.

Magellan charge toujours les fichiers (par exemple les graphiques) à partir de répertoires spécifiques qui indiquent le but du fichier. Pour les fichiers graphiques de la carte, il s'agit de 'images/map/', un jeu de graphiques est donc toujours constitué d'un répertoire ou d'une archive (fichier ZIP ou JAR) contenant les graphiques de la carte dans ces sous-répertoires.

Par exemple, lors du chargement du fichier Ebene.png, le chemin de la ressource, le répertoire "images/map" et le nom du fichier "Ebene.png" sont simplement ajoutés ensemble pour accéder au fichier, par exemple "C:\Grafiksets\images\\map\Ebene.gif" si "C:\Grafiksets" est défini en tant que chemin de la ressource. Lors de l'accès, tous les chemins de ressources sont d'abord recherchés, puis, s'ils ne peuvent être trouvés dans aucun chemin de ressources, les graphiques standard du fichier Magellan Jar sont accédés.

Un jeu de graphiques peut également être transmis à d'autres sous forme d'archive (fichier ZIP ou JAR) pour simplifier la manipulation. Il suffit d'emballer l'ensemble, c'est-à-dire les fichiers graphiques et cellgeometry.txt, dans un fichier ZIP ou JAR. Toutefois, les fichiers doivent toujours se trouver dans cette archive dans un sous-répertoire 'images/map/'. Ensuite, chaque utilisateur de Magellan peut utiliser le jeu de graphiques en entrant cette archive comme chemin de ressource dans la boîte de dialogue des options de Magellan.

## Nom des fichiers

Les noms des fichiers utilisés par les moteurs de rendu existants pour afficher certains objets sont listés ici. L'extension ".png", ".gif" et/ou "-alpha.gif" doit être ajoutée au nom complet du fichier.

Les noms de fichiers doivent tous être en minuscules, sinon Magellan ne pourra pas y accéder !

* Régions : Tous les noms de régions sans tréma (par exemple "wueste"), en plus de "nebel" pour le brouillard de guerre.
* Frontières : "strasse0" (NW) à "strasse5" (SW, dans le sens des aiguilles d'une montre) et "strasse_incomplete0" (NW) à "strasse_incomplete5" (SW, dans le sens des aiguilles d'une montre). S'il n'y a pas de graphiques pour les rues incomplètes, les graphiques des rues achevées sont utilisés.
* Bâtiments : Tous les noms de bâtiments sans tréma (par exemple "saegewerk").
* Navires : "schiff0" (pas de côte), "schiff1" (NW) à "schiff6" (SW, dans le sens des aiguilles d'une montre)
* Indicateurs de direction : "flèche0 (NW) à flèche5 (SW, sens des aiguilles d'une montre)
* Marqueurs de sélection : "actif", "sélectionné"

Tous les fichiers doivent être stockés avec le fichier cellgeometry.txt (voir "Géométrie des cellules") dans un répertoire commun "images/map/".

## Format du fichier

Les fichiers graphiques peuvent être au format PNG ou GIF ; les canaux alpha et la transparence sont utilisés de différentes manières :

PNG : si un fichier portant l'extension .png est trouvé, il est utilisé et les informations du canal alpha qu'il contient sont utilisées directement. Les fichiers PNG ont l'avantage de permettre une profondeur de couleur de 24 bits et un canal alpha intégré de 8 bits. Malheureusement, Java 1.2 ne prend pas encore en charge les graphiques PNG, de sorte que seuls les utilisateurs disposant d'un JRE >= 1.3 peuvent utiliser de tels jeux de graphiques.

GIF + canal alpha : Si aucun fichier portant l'extension ".png" n'est trouvé, le moteur de rendu recherche un fichier portant l'extension ".gif", qui contient les informations RVB de l'image et un fichier portant l'extension "-alpha.gif", qui est interprété comme une image en niveaux de gris et utilisé comme canal alpha pour l'autre GIF.

GIF : S'il n'y a pas de fichier avec l'extension "-alpha.gif", mais qu'il existe un fichier GIF standard, les informations RVB et de transparence qu'il contient sont utilisées.

## Taille des graphiques / géométrie des cellules

En principe, la taille des graphiques est arbitraire, mais elle doit être la même pour toutes les images d'un ensemble de graphiques. Le client a également besoin d'informations sur la géométrie de l'hexagone de la région, c'est-à-dire les coordonnées de ses coins, ainsi que sa position dans le graphique et la taille totale du graphique. Voici à quoi cela ressemble :

![cell_geometry](../images/reference_graphicsets_cellgeometry.gif)

x0=32 <- coordonnée x du point d'angle à 12 heures
x1=63 <- coordonnée x du point d'angle à 2 heures
x2=63
x3=32
x4=0 <- coordonnée x du point d'angle à 8 heures (coordonnées hexagonales, c'est-à-dire toujours 0 !)
x5=0 <- coordonnée x du point d'angle à 10 heures (coordonnées hexagonales, c'est-à-dire toujours 0 !)
y0=0 <- coordonnée y du point d'angle à 12 heures (coordonnées hexagonales, c'est-à-dire toujours 0 !)
y1=16 <- coordonnée y du point d'angle à 2 heures
y2=47
y3=63
y4=47
y5=16
imgOffsetx=8 <- distance entre le bord gauche de l'hexagone et le bord du graphique
imgOffsety=8 <- Distance entre le bord supérieur de l'hexagone et le bord du graphique
imgSizex=80 <- Largeur du fichier graphique
imgSizey=80 <- Hauteur du fichier graphique

Toutes les valeurs sont exprimées en pixels. Si la largeur et la hauteur de l'hexagone de la région correspondent à celles du fichier graphique, il n'y aura pas de chevauchement lors du dessin des différents graphiques. Toutefois, si la taille du graphique est supérieure à celle de l'hexagone de la région, il y aura des chevauchements lors du dessin des graphiques, en fonction de la valeur imgOffsetx/y au-dessus, au-dessous, à gauche ou à droite de l'hexagone de la région. L'utilisation d'informations de transparence dans les graphiques permet de créer toutes sortes d'effets, par exemple des transitions à peine perceptibles entre les régions. Il convient de noter que les régions sont dessinées ligne par ligne, du haut à gauche au bas à droite, sur toutes les couches de rendu.

Le fichier contenant ces informations doit s'appeler "cellgeometry.txt" et se trouver dans le même répertoire (images/map/) que les graphiques. Le contenu de ce fichier peut sembler quelque peu obscur, mais sa signification devrait rapidement devenir claire si vous regardez son contenu dans les jeux de graphiques existants.

## Renderer

Magellan prend en charge différents "moteurs de rendu" pour chaque sous-couche de la carte, c'est-à-dire des sous-modules qui affichent les fichiers graphiques à l'écran. Ils sont disposés en plusieurs couches afin d'avoir un ordre fixe pour le rendu, ce qui se reflète dans la disposition en profondeur des graphiques dessinés. L'ordre est actuellement le suivant :

1. régions
2. frontières (routes)
3. bâtiments
4. les navires
5. noms des régions
6. flèches de direction
7. marqueurs de sélection

Cela signifie que les couches antérieures sont couvertes par les couches ultérieures. Pour l'instant, il n'existe qu'un seul moteur de rendu standard pour chacune des couches, mais il devrait être suffisamment souple pour répondre à la plupart des besoins.

Lors de la conception des graphiques et de la définition des chevauchements entre les graphiques, il convient de noter que les régions sont dessinées ligne par ligne, du haut à gauche vers le bas à droite, sur toutes les couches de rendu.

Il est également très facile de programmer de nouveaux rendus, par exemple pour afficher des objets supplémentaires sur la carte ou certaines propriétés de la région.
