# Travailler avec des rapports informatiques

Pour effectuer vos déplacements avec Magellan, vous devez obtenir l'évaluation sous la forme d'un rapport informatique (RI). Normalement, ce rapport est obtenu automatiquement. Si ce n'est pas le cas, vous pouvez donner à une unité la commande OPTION ORDINATEUR.

Le CR est généralement contenu dans un fichier zip. Vous ne devez pas décompresser ce fichier, mais vous pouvez l'ouvrir directement avec Magellan. Cela se fait via le menu [File -> Open...] (../menus/file/open.md). Il en va de même pour les rapports qui existent encore parfois avec les extensions .cr.bz2 ou cr.gz. Si l'on vous demande si vous voulez accepter le mot de passe d'un fichier, répondez par l'affirmative. Ensuite, vous pouvez sélectionner votre faction dans les [Statistiques de faction] (../menus/extras/factionstatistics.md) et utiliser le bouton **_Mot de passe et autres propriétés_** pour indiquer à Magellan votre mot de passe, car il n'est possible d'écrire vos commandes qu'une fois que le mot de passe a été défini.

Lorsque vous recevez votre première évaluation, le mot de passe se trouve également dans le fichier avec l'extension .nr. Il devrait se trouver quelque part dans les premières lignes.

## Utilisation d'un train CR

Pour pouvoir utiliser toutes les fonctions de Magellan, il est judicieux de toujours travailler avec le "même" CR et d'ajouter le nouveau CR. Il est judicieux de commencer par enregistrer votre premier CR sous un nom différent, par exemple train.cr. Si vous recevez maintenant une nouvelle évaluation, chargez l'ancien zug.cr et ajoutez le nouveau rapport via [File -> Add...] (.../menus/file/add.md).

Les fonctions qui ne sont possibles que de cette manière sont les suivantes

* Affichage des changements pour la semaine précédente
* Informations sur la région au moment de la dernière visite dans une région pour laquelle aucune information n'a été obtenue avec l'évaluation actuelle. Il s'agit notamment de :
* * Rues
    * Bâtiments
    * Les agriculteurs
    * Argent
    * Ressources

Les informations qui sont conservées et qu'il n'est donc pas nécessaire de saisir à nouveau (si cela est possible) sont les suivantes

* les noms d'îles attribués
* Commentaires sur les régions créées par l'utilisateur
* Informations sur la culture des herbes
* Informations sur les recettes de potions
* Informations sur les sorts

## Échange de CR

Lorsque vous échangez des CR avec d'autres joueurs, vous devez d'abord réfléchir à ce que vous voulez échanger. Lorsque vous échangez du matériel cartographique, par exemple, vous devez éviter autant que possible de laisser vos unités et vos bâtiments dans le CR. En effet, vous n'êtes pas obligé de révéler vos forces et vos faiblesses tout de suite. Magellan offre plusieurs options dans le menu [File -> Export CR...] (../menus/file/crexport.md) quant à ce qui doit être sauvegardé dans un CR.

Si vous souhaitez ajouter des rapports externes (cartes, etc.) aux vôtres, Magellan tente automatiquement de reconnaître la meilleure correspondance si les systèmes de coordonnées des rapports ne correspondent pas. Si, contrairement aux attentes, cela devait conduire à un résultat incorrect, le rapport à ajouter doit être chargé normalement et ensuite le [Ajuster l'origine] (../menus/map/origin.md), le rapport sauvegardé et ensuite ajouté au rapport principal.

## Travailler avec plusieurs CR de la même série

Si vous travaillez avec plusieurs rapports du même tour (par exemple des rapports d'alliés), il est recommandé de toujours charger d'abord votre propre rapport et d'y ajouter les autres rapports. Cela permet de conserver le système de coordonnées de votre propre rapport.

Lors de l'ajout progressif de rapports d'alliés, il peut arriver, par exemple, que des informations de changement ne soient pas enregistrées. Pour éviter cela, vous pouvez d'abord essayer de fusionner tous les rapports d'alliés en un seul rapport, puis ajouter ce rapport d'alliance au vôtre.
