# Paramètres de la ligne de commande

Magellan accepte les paramètres suivants spécifiés dans [Start](../faq.html#Command line start) :

* \-d &lt;directory&gt; : Spécifie le répertoire dans lequel Magellan recherche ses ressources, c'est-à-dire les images, les traductions, etc.
\-s &lt;directory&gt; : Spécifie le répertoire dans lequel Magellan recherche ses paramètres (profiles.ini). Dans ses sous-répertoires se trouvent les _profiles_ avec les fichiers de configuration (magellan.ini, magellan\_desktop.ini) et le fichier errors.txt avec les messages d'erreur.
\-p &lt;profile&gt; : Démarre Magellan avec un profil spécifique défini dans les paramètres du profil.
\pm : Affiche le gestionnaire de profil au démarrage.
\N-log &lt;X&gt; : Définit le niveau de détail du journal. Les valeurs possibles pour X sont : O - désactivé, E - uniquement les erreurs, W - également les avertissements, I - également les messages d'information.
\--help : Affiche uniquement l'aide de Magellan.
&lt;CR file&gt; : Le fichier CR spécifié est chargé directement après le démarrage.

L'appel complet à Magellan ressemble à ceci : java -jar magellan.jar \N-[-d directory\N] \N-[-s profiles\N] \N-[-pm\N] \N-[-log\N-[O|E|W|I]\N] \N-[--help\N] \N-[CR-file\N]
