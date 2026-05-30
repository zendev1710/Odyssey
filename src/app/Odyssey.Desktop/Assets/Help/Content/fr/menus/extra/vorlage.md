# Gabarit

Il s'agit d'une interface graphique pour Template. Template est un puissant générateur de modèles de mouvements pour Eressea et d'autres jeux compatibles, qui offre également un méta-langage pour l'automatisation. Vous pouvez télécharger Vorlage à [http://www.gulrak.de/etools.html](http://www.gulrak.de/etools.html). La documentation est également disponible sur ce site.

Après avoir sélectionné l'élément de menu, la boîte de dialogue suivante s'ouvre :

![menu_extras_template](../../images/menu_extras_template.gif)

Dans le champ **_source CR(s)_**, entrez le chemin d'accès à un ou plusieurs CR qui doivent être édités par le modèle. Normalement, il s'agit du CR envoyé par le serveur Eressea.

Le champ **_fichier_cible_** est utilisé pour spécifier le fichier dans lequel le modèle doit écrire sa sortie.

Le fichier spécifié dans le champ **_Script file_** utilise Template pour intégrer des fonctions et des procédures externes pour le traitement des méta-commandes. Vous trouverez plus d'informations sur les scripts de modèle dans la documentation du modèle.

Enfin, le champ **_template_** contient le chemin d'accès au modèle.

Dans le bloc **_Options_**, vous pouvez spécifier des options de ligne de commande pour le modèle. Le commutateur **_Sortie sous forme de rapport informatique_** crée l'option -cr. D'autres options peuvent être saisies dans le champ correspondant si nécessaire.

Cliquez sur **_Ok_** pour appeler le modèle avec les options définies. Le CR généré ou le fichier de commande peut maintenant être chargé dans Magellan et traité ultérieurement.

La sortie du modèle est affichée dans la fenêtre **_Sortie_** afin que vous puissiez voir les éventuels messages d'erreur.
