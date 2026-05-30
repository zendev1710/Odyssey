# Add

CTRL-D

Cette fonction permet d'ajouter des CR à celui qui se trouve dans la mémoire. Le CR de la mémoire est complété ou mis à jour avec les nouvelles informations. La balise ronde dans le CR est particulièrement importante. Une distinction est faite entre les cas suivants :

1. le CR à ajouter est plus récent que celui qui se trouve dans la mémoire (nouvelle série) :  
    Toutes les données relatives aux unités et aux navires sont effacées et les données plus récentes sont transférées. Les données relatives aux régions, châteaux, routes, etc. sont conservées.  

2. le CR à ajouter est du même tour que celui en mémoire :
    Les données d'unité et de région existantes sont conservées et peuvent être complétées.  

3. le CR à ajouter est plus ancien que le CR en mémoire :
    Seules les données de la région sont transférées - bien sûr seulement si des données plus récentes ne sont pas déjà disponibles. Malheureusement, je dois dire que cette fonction ne fonctionne pas toujours parfaitement à 100 %. En particulier, la fusion de cartes qui ne se chevauchent que dans une mesure très limitée conduit généralement à une carte fusionnée complètement incorrecte. Il existe toutefois une astuce : si le rapport à fusionner est plus ancien que celui qui est actuellement chargé, il suffit d'inverser les rôles et de charger le nouveau rapport dans l'ancien. Vous pouvez également modifier l'étiquette lap dans le rapport CR lui-même.

Magellan tente automatiquement d'assembler les parties de carte correspondantes. Magellan compare la séquence des différents types de régions selon des modèles identiques. Si deux cartes se chevauchent, il y a de bonnes chances que Magellan les ajuste correctement. Des problèmes peuvent survenir surtout lorsque les régions du rapport ajouté ne sont pas clairement identifiables. De nombreuses régions astrales ou océaniques, par exemple, se ressemblent comme deux gouttes d'eau et sont donc difficiles à identifier.

De plus amples informations sur cette fonction sont disponibles dans la section "Référence" sous [Rapports informatiques](../../reference/cr.md).
