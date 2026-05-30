# Sauvegarde des commandes

![Sauvegarder les commandes](../../images/menu_file_saveorders.gif)

Les options suivantes (onglets) sont disponibles pour enregistrer ou envoyer des commandes :

* **Email**
    Envoie les commandes directement par e-mail. Magellan peut deviner les paramètres nécessaires pour certains fournisseurs de courrier électronique courants. Il suffit alors de saisir le mot de passe. Si ce n'est pas possible, vous pouvez remplir les autres paramètres vous-même. Si vous ne connaissez pas les paramètres, une simple recherche sur Internet pour "Paramètres SMTP pour le nom du fournisseur" est souvent utile. Chez certains fournisseurs, cette fonction doit d'abord être activée dans les paramètres de messagerie. Pour ce faire, visitez le site web de votre fournisseur. Ces paramètres n'ont rien à voir avec Eressea, mais la communauté Eressea se fera un plaisir de vous aider si vous le demandez gentiment.  
    ![Sauvegarder les commandes](../../images/menu_file_saveorders_email.gif)
  * **Suggestion des paramètres**
        Tente de deviner les paramètres du serveur à partir de l'adresse de l'expéditeur ou offre la possibilité de choisir parmi une liste de fournisseurs populaires.
  * **Adresse de l'expéditeur**
        Votre adresse électronique à partir de laquelle les commandes doivent être envoyées.
  **Serveur SMTP**
        Indiquez ici le serveur de messagerie de votre fournisseur d'accès à Internet (par exemple smtp.provider.de).
  **Port**
        Vous devez vous renseigner sur ces paramètres auprès de votre fournisseur d'accès. Les valeurs courantes sont 25, 465 ou 587.
  **Utiliser SSL / Utiliser TLS**
        Ces paramètres dépendent du protocole utilisé par votre fournisseur. En cas de doute, essayez de sélectionner les deux.
  **Utiliser l'authentification**
        La plupart des fournisseurs d'accès exigent aujourd'hui que cette case soit cochée.
  **Nom d'utilisateur**
        Souvent identique à l'adresse de l'expéditeur, mais cela dépend également du fournisseur.
  * **Mot de passe**
        Magellan peut enregistrer votre mot de passe. Cela représente un certain risque si une personne mal intentionnée accède à votre ordinateur. Vous pouvez cocher la case "Toujours demander", auquel cas vous devrez à chaque fois réintroduire le mot de passe, mais c'est plus sûr.
  **Adresse du destinataire**
        L'adresse électronique du serveur Eressea est saisie ici. Magellan peut normalement la lire à partir du rapport, mais si cela ne fonctionne pas, vous pouvez la saisir ici (par exemple <eressea-server@eressea.kn-bremen.de>).
  * Objet
        L'objet du courrier (par exemple, commandes Eressea).
  * **CC**
        Vous pouvez saisir ici une ou plusieurs adresses (séparées par des virgules) auxquelles le rapport doit également être envoyé.
**Fichier**
    Enregistre les commandes dans un fichier portant le nom spécifié. Cochez la case Nom de fichier automatique pour utiliser des abréviations spéciales dans le nom du fichier. Par exemple, "commands-{round}.txt" fait en sorte que le nom contienne le round en cours, par exemple commands-123.txt.  

**Clipboard**
    Copie le fichier de commandes dans le presse-papiers. De là, il peut être facilement copié dans un programme de messagerie, par exemple.  

**Téléchargement vers le serveur**
    Charge les commandes directement sur le serveur, sans passer par le courrier électronique. Les commandes ne sont pas vérifiées (par ECheck). L'adresse par défaut fonctionne pour Eressea, mais peut ne pas fonctionner pour d'autres jeux. Si nécessaire, demandez à la direction du jeu.

**Fermer
    Ferme le dialogue et enregistre tous les paramètres.
**Annuler
    Ferme la boîte de dialogue sans enregistrer les paramètres.

## Options de sortie

En cliquant sur "Détails", vous accédez à d'autres fonctions qui déterminent l'aspect exact des commandes exportées.

![Options de sortie](../../images/menu_file_saveorders_details.gif)

**Saut de ligne automatique**
    Interrompt le fichier de commandes après _n_ caractères. Les lignes plus longues (descriptions, messages, etc.) sont automatiquement séparées par " \\N" dans le processus. Évite les problèmes liés au saut de ligne automatique des programmes de messagerie.
**Commentaires de contrôle**
    Insère des commentaires pour le programme de contrôle des trains ECheck (tels que des informations sur l'argent et les personnes) dans le fichier de commandes.
**Suppression des commentaires commençant par ';'**
    Supprime les commentaires non persistants du fichier de commande. Dans la mesure du possible, cette option devrait être sélectionnée lors de l'envoi du courrier au serveur Eressea afin de réduire au maximum la taille du fichier de commandes. Cependant, les informations relatives à la confirmation des commandes des unités sont également perdues, car elles sont stockées dans les commentaires ';'.
**Supprimer les commentaires commençant par '//'**
    Supprime les commentaires persistants du fichier de commande. Les utilisateurs de modèles doivent éviter autant que possible d'utiliser cette option, car toutes les méta-commandes seront également supprimées.
**Uniquement les unités avec des commandes confirmées**
    Seules les commandes des unités confirmées sont écrites. Les unités non confirmées sont ignorées. Cette option est très utile pour les joueurs qui partagent une faction.
* Régions sélectionnées
    Cette option permet de spécifier que les commandes ne sont envoyées qu'aux unités actuellement sélectionnées sur la carte. Cela vous permet d'envoyer des commandes au serveur morceau par morceau.
**Insérer les balises inconnues comme modèle**
    Cette option permet d'écrire les balises inconnues dans le fichier de commandes. Cette option n'a probablement de sens que pour les utilisateurs du programme d'automatisation des trains Template.
