# Carte

Le type d'affichage de la carte et de la carte d'ensemble peut être configuré ici. La carte est dessinée par différents "renderers", qui peuvent être configurés et désactivés séparément ici.

![Options - Carte](../../images/menu_extras_options_map.gif)

**Affichage de la barre de navigation sur le bord supérieur de la carte.
Un certain nombre d'options de réglage peuvent être affichées au-dessus de la carte principale. D'une part, il y a un curseur pour agrandir ou zoomer la carte, une option de sélection pour le niveau ou la couche affichée et enfin, si des hotspots ont été définis, vous pouvez sauter à ces hotspots.

Ces options prennent peu de place et peuvent être désactivées ; la fonctionnalité ne peut alors être utilisée qu'en cliquant avec le bouton droit de la souris sur la carte. Le menu contextuel qui s'affiche contient les éléments de menu correspondants.

**Afficher des images saisonnières supplémentaires** Les montagnes enneigées en hiver et les banquises sur la côte ne sont affichées que si ce paramètre a été sélectionné. S'il n'est pas activé, le jeu de graphiques standard est utilisé pour toutes les saisons.

**Dessiner la carte immédiatement sans charger complètement les graphiques**
Si cette option est activée, la carte est dessinée immédiatement après le chargement du CR, sans charger au préalable tous les graphiques de la région dans le cache. Cela signifie que l'affichage initial de la carte est légèrement plus rapide, mais que seuls les graphiques des types de régions présents sur la section de carte actuelle sont chargés. Cela peut entraîner des retards lors du déplacement de la section de carte si les graphiques de la région doivent encore être chargés.

**Afficher les infobulles**
Si cette option est activée, des infobulles s'affichent lorsque la souris survole des régions sur la carte, indiquant le nom de la région, le type, les agriculteurs et l'argent de la piscine.

## Carte principale

La couche de rendu divise l'affichage en sous-objets logiques, tels que les régions, les routes, les bâtiments, etc., dont les options de réglage respectives sont ensuite affichées dans la boîte ci-dessous.

### Régions

inactif&gt ;
    Les régions ne sont pas affichées.
**Region renderer**
    Il s'agit du paramètre par défaut dans lequel les régions sont affichées avec des graphiques.
  **Rendu des régions (géométrique/politique)**
    Ce mode affiche les régions sous forme de zones de couleurs différentes. Les modes d'affichage suivants sont possibles
  **Type de région**
        Vous pouvez ici attribuer une couleur à chaque type de région (montagne, marais, etc.).
  **Politique
        Si ce mode est activé, la région est colorée dans la couleur attribuée à la faction qui compte le plus d'habitants dans la région.
  **Toutes les factions
        Vous pouvez ici attribuer une couleur à chaque faction du rapport. S'il y a plus d'une faction dans une région, les régions sont affichées par des subdivisions verticales dans les couleurs correspondant aux factions respectives.
  Niveau de confiance
        Affichage des couleurs de la région en fonction du niveau de confiance des factions qui s'y trouvent.
  **Niveau de confiance (gardiennage)**
        Affichage des couleurs de la région en fonction du niveau de confiance des factions qui la gardent.
  **ARR (advanced region renderer)**
    Ce mode permet de personnaliser l'affichage de la région en fonction des conditions que vous créez. Vous trouverez plus d'informations dans la [Description du système de remplacement](../../reference/atr_arr.md).

    Pour tous les réglages, veuillez noter que l'attribution des couleurs des factions est identique pour tous les modes d'affichage.

### Rues

inactif&gt ;
    Les routes ne sont pas affichées.
**Rendu des rues**
    Il s'agit du paramètre par défaut dans lequel les routes sont affichées avec des graphiques.

### Bâtiments

inactif&gt ;
    Les bâtiments ne sont pas affichés.
**Rendu des bâtiments**
    Il s'agit du paramètre par défaut dans lequel les châteaux sont affichés avec des graphiques.

### Navires

&lt;inactive&gt ;
    Les navires ne sont pas affichés.
**Rendu des navires**
    Si cette option est activée, les navires existants sont affichés sur la carte. Les navires amarrés sont affichés sur la côte d'amarrage correspondante, les navires en construction et l'océan au centre de la région.

### Étiquetage

&lt;inactive&gt ;
    Les noms des régions ne sont pas affichés.
**Rendu du nom de la région**
    Si cette option est activée, le nom est affiché au-dessus de la région concernée. Il est également possible de définir la police et le type de police ici.

* Rendu du commerce** Si cette option est sélectionnée, la marchandise et la quantité sont affichées.
* **ATR** Vous pouvez définir ici votre propre étiquetage. Vous trouverez de plus amples informations dans la [Description du système de remplacement] (../../reference/atr_arr.md).

### Chemins d'accès

inactif&gt ;
    Les chemins ne sont pas affichés.
**Rendu des chemins**
    Si cette option est active, des flèches sont affichées sur la carte si une unité active dispose d'une commande NEXT ou ROUTE.

### Marqueurs

&lt;inactive&gt ;
    Les marqueurs ne sont pas affichés.
**Rendu des marqueurs**
    Si cette option est active, la région actuellement active est mise en évidence par un contour blanc et les régions marquées bénéficient d'un brouillard diffus (différent selon le [jeu de graphiques] chargé (.../../reference/graphicsets.md)).
**Rendu du marqueur (géométrique)**
    Si cette option est activée, la région active est mise en évidence par un contour rouge et les régions marquées sont soulignées en blanc.

### Marquages supplémentaires

&lt;inactive&gt ;
    Les marqueurs supplémentaires ne sont pas affichés.
**Images supplémentaires**
    Si cette option est activée, vous pouvez afficher vos propres images sur la carte. Pour ce faire, la balise _"nom de l'image";regionicon_ doit être définie dans le contexte de la région, par exemple avec le modèle. Les images elles-mêmes doivent être situées sous le répertoire magellan.jar dans le chemin /res/images/map et correspondre au format décrit sous [Create graphics set](../../reference/graphicsets_making.md).

## MiniMap

![Options - Aperçu de la carte](../../images/menu_extras_options_map_minimap.gif)

La carte d'ensemble supporte tous les modes d'affichage du moteur de rendu de région (géométrique/politique). L'attribution des couleurs peut donc également être définie pour ce mode. L'échelle de la mini-carte peut également être réglée ici.
