# FAQ

Questions fréquemment posées :

## 1\. Questions sur l'installation

1. [Qu'est-ce qui est nécessaire pour démarrer Magellan ?] (#Installation)
2. [Bien que je dispose de suffisamment de mémoire vive, Magellan signale un manque de mémoire. Pourquoi ?](#Speichermangel)

## 2\. Questions sur Magellan

1. [Comment lancer ECheck ?](#ECheck)
2. [Que font ces § dans l'ARR, l'ATR ou les infobulles ?](#ARR)

## 2\. Questions sur Java

1. [Qu'est-ce que Java et pourquoi est-ce important ?
2. [Quelle version de Java dois-je télécharger exactement ? (#Javersion)
3. [JRE ou JDK ?](#jdk)
4. [Quelle version de Java ai-je ?](#myversion)
5. [Je reçois ce message d'erreur : 'Error : Une erreur JNI s'est produite, veuillez vérifier votre installation et réessayer.'](#JNI_ERROR)
6. [Mais la dernière version sur java.com est Java 8](#java8)
7. [Mais j'ai besoin de Java 8 pour un autre logiciel. Que puis-je faire ?](#needJava8)

## 4\. Questions sur les anciennes versions de Magellan

1. [Que signifie "Installer JAR" ?](#Installer)
2. [magellan-client.jar ? le fichier ne devrait-il pas s'appeler magellan.exe ?](#Dateiname)
3. [Pourquoi un autre programme (par exemple WinZip ou PowerArchiver) démarre-t-il lorsque j'essaie de lancer Magellan en double-cliquant ?](#Verknuepfung)
4. [Pourquoi Magellan ne démarre-t-il pas lorsque je clique sur le fichier magellan-client.jar ?
5. [Bien que j'aie suffisamment de RAM, Magellan signale un manque de mémoire. Pourquoi ?](#Speichermangel_legacy)
6. [Comment faire de Magellan le programme par défaut pour les fichiers CR sous Windows2000/XP ?](#CRVerknuepfung)
7. [Comment démarrer Magellan à partir de l'invite de commande ? (#Kommandozeilenstart)
8. [Comment faire fonctionner Magellan sur un Mac ?](#MacMagellan)
9. [Comment gérer les chemins de ressources (#Resourcenpfade)
10. [Pourquoi Magellan n'utilise-t-il soudainement plus les numéros de base 36 pour les unités ?](#base36nummern)
11. [Comment puis-je utiliser Vorlage pour confirmer automatiquement les ordres des unités ?

## Réponses

### Q : Que faut-il pour démarrer Magellan ?

**R:** Depuis la version 2.1, un seul fichier est nécessaire pour télécharger et installer Magellan. Vous trouverez la bonne version sur la [page d'accueil de Magellan] (https://magellan2.github.io) dans la section téléchargement. Vous trouverez des versions pour différents systèmes d'exploitation (Windows, Linux, MacOS). Vous aurez probablement besoin de droits d'administrateur pour installer Magellan sous Windows. Cela installera tout ce dont vous avez besoin, y compris une version [Java](#Java). Les mises à jour à partir de versions antérieures devraient fonctionner sans problème, mais il peut y avoir des problèmes avec les anciens plugins. Dans ce cas, vous devez désactiver ces plugins ou installer une version plus récente.

Si vous avez déjà installé Java, vous pouvez également télécharger et décompresser un fichier zip et lancer Magellan directement (de préférence en utilisant magellan.bat ou magellan.sh). Mais cette méthode n'est pas recommandée ou supportée officiellement.

### Q : Qu'est-ce que Java et pourquoi est-ce important ?

**A:** Java est le langage de programmation utilisé pour écrire Magellan. La particularité de ce langage est qu'il fonctionne sur de nombreuses plateformes (Windows, Linux, Mac, ...). Depuis la version 2.1, Magellan est livré avec sa propre version de Java. Il n'est donc pas nécessaire d'avoir recours à quoi que ce soit d'autre.

### Quelle version de Java, exactement, dois-je télécharger ?

**Les versions antérieures de Magellan nécessitaient l'installation préalable de Java. L'une des façons les plus simples d'installer Java est de télécharger un programme d'installation à partir de [AdoptOpenJDK] (https://adoptopenjdk.net/releases.html). Il existe plusieurs distributions Java. Nous recommandons, par exemple, [Open JDK](https://openjdk.java.net/) ou [Oracle Java SE](https://www.oracle.com/java/).

À l'heure actuelle (été 2021), nous recommandons la version suivante, qui devrait convenir à la plupart des utilisateurs :

* AdopterOpenJDK
* Votre système d'exploitation, naturellement (Windows pour la plupart)
* OpenJDK 11 (LTS), ou, à l'automne 2021, OpenJDK 17 (LTS)
* JVM HotSpot
* généralement x64

### JRE ou JDK ?

Le JDK est généralement destiné aux développeurs Java, le JRE aux utilisateurs finaux. Si vous avez l'intention d'utiliser les commandes étendues de Magellan, vous aurez besoin d'un JDK ! Le seul inconvénient du JDK est sa taille. Par conséquent, si vous ne manquez pas d'espace disque, il est recommandé d'utiliser un JDK.

### Quelle version de Java ai-je ?

1. Ouvrez un terminal (ligne de commande) :
    **Sous Windows:** Appuyez sur la touche Windows pour ouvrir le menu Démarrer. Tapez cmd pour ouvrir la ligne de commande.
    * **Sous MacOsX:** Ouvrez Spotlight (Commande + Espace ou cliquez sur la loupe en haut à droite), puis entrez 'terminal'.
    **Sous Linux:** Ouvrez un terminal (selon votre distribution Linux, ouvrez le menu des programmes, par exemple en appuyant sur la touche Windows et entrez "terminal").
2. Dans le terminal, tapez `java -version` et appuyez sur `Entrée`. Une sortie comme `'openjdk version "11.0.10" 2021-04-20'` signifie que vous utilisez la version 11. Une sortie comme "java is not recognized" ou "command not found" signifie que vous n'avez apparemment pas java.

### Q : Je reçois ce message d'erreur : 'Error : Une erreur JNI s'est produite, veuillez vérifier votre installation et réessayer.'

**A:**Vous avez probablement Java 8 au lieu de Java 11 ! Installez Java 11 (voir ci-dessus).

### Mais la dernière version sur java.com est Java 8.

Depuis qu'Oracle a racheté Java à Sun Microsystems, il existe un nouveau système de licence qui complique la publication des implémentations Java pour les utilisateurs finaux. Par conséquent, [java.com] (https://java.com) n'est plus le site de choix. Veuillez vous procurer Java auprès de l'une des sources mentionnées ci-dessus.

### J'ai besoin de Java 8 pour un autre logiciel

Vous pouvez utiliser plusieurs versions de Java simultanément, mais cela nécessite un certain travail.

**Sur Windows:**

1. Installez Java 11, puis Java 8. Vos autres logiciels devraient maintenant fonctionner normalement.
2. Localisez le chemin de votre installation Java 11 (par exemple C:\NProgram Files\Njdk-11.0.1).
3. Localisez le fichier magellan.bat (généralement dans C:\NProgram Files\NMagellan\Nmagellan.bat).
4. Modifiez ce fichier en tant qu'administrateur. Cela devrait fonctionner à peu près comme suit : Localisez l'entrée "notepad" dans le menu de démarrage. Cliquez dessus avec le bouton droit de la souris et choisissez "More ... Exécuter en tant qu'administrateur".
5. Ouvrez le fichier magellan.bat dans le bloc-notes.
6. Ajoutez cette ligne au début du fichier (adaptez-la à votre chemin d'accès de l'étape 2) :  
    `SET JAVA_HOME=C:\NProgram Files\Njdk-11.0.1`
7. Enregistrez le fichier magellan.bat.
8. Magellan devrait maintenant être lancé avec Java 11.

**Sur Linux:**

1. Installez à la fois Java 11 et Java 8.
2. Exécutez cette commande dans un terminal : `update-alternatives --config java`. Choisissez la version de Java dont vous avez besoin pour votre autre programme.
3. Souvenez-vous du chemin vers Java 11 affiché par la commande précédente (par exemple, `/usr/lib/jvm/java-11-openjkd-amd64`).
4. Localisez le fichier magellan.sh (habituellement dans $HOME/Magellan/magellan.sh).
5. Modifiez ce fichier comme suit : Ajoutez cette ligne au début du fichier (adaptée au chemin enregistré à l'étape 3 ci-dessus) :  
    `export JAVA_HOME=/usr/lib/jvm/java-11-openjdk-amd64`
6. Changez la dernière ligne de
    `java -Xmx1200m -jar "magellan-client.jar" "$@"` en
    `$JAVA_HOME/bin/java -Xmx1200m -jar "magellan-client.jar" "$@"`.
7. Enregistrez le fichier magellan.sh.
8. Magellan devrait maintenant être exécuté avec Java 11.

### Q : Bien que je dispose de suffisamment de RAM, Magellan signale un manque de mémoire. Pourquoi ?

**A:** En raison de l'architecture de Java, Magellan ne peut pas toujours allouer autant de mémoire qu'il en a besoin. Dans le répertoire d'installation de Magellan (sous Windows, il s'agit généralement de C:\NProgram Files\NMagellan) se trouve un fichier appelé magellan\Nlauncher.vmoptions . Vous devez éditer ce fichier (cela peut nécessiter des droits d'administrateur) et ajouter une ligne comme \-Xmx1G . Cela indique à Java d'allouer jusqu'à 1 gigaoctet de mémoire pour Magellan.

En général, il n'y a pas de problème à allouer jusqu'à la moitié de votre mémoire ou plus à Magellan. Ne vous inquiétez pas : Magellan n'allouera toujours que la quantité de mémoire dont il a besoin. Ainsi, si votre machine dispose de 4 gigaoctets de mémoire vive, un réglage de \-Xmx2G devrait suffire. Si votre système entier se bloque lors du chargement d'un rapport volumineux, vous pouvez réduire cette valeur et vous pouvez essayer de réduire la taille de votre rapport en ne chargeant pas toute la carte ou en n'ajoutant pas tous les rapports de vos alliés. Le plugin MemoryWatch de la page d'accueil de Magellan peut fournir des informations supplémentaires.

Pour d'autres façons d'allouer de la mémoire, en particulier pour les anciennes versions de Magellan, voir [cette section](#Speichermangel_legacy).

### Q : Comment lancer ECheck ?

**A:**Tout d'abord : Pourquoi pensez-vous avoir besoin d'ECheck ? Pratiquement toutes les fonctions d'ECheck sont remplies par Magellan lui-même. Les erreurs de syntaxe détectées par ECheck, et quelques autres, sont détectées par Magellan et sont mises en évidence dans les commandes. D'autres erreurs sont affichées dans le dock [open problems](docks/problems.html). Vous pouvez configurer les problèmes (potentiels) à afficher et ceux que vous préférez ignorer.

Si vous souhaitez vraiment exécuter ECheck, vous pouvez le faire à partir du [dock ECheck](docks/echeck.html). Magellan est livré avec sa propre version d'ECheck et est généralement configuré pour l'utiliser, de sorte que vous n'avez qu'à cliquer sur "Exécuter". Si cela ne fonctionne pas, peut-être parce que vous utilisez les paramètres d'une version antérieure, vous pouvez le configurer dans la [boîte de dialogue des options](menus/extras/options_resources.html) sous "Ressources". Il vous suffit de définir le chemin d'accès aux ressources de manière à ce qu'il pointe vers votre installation ECheck (quelque chose comme C:\NProgram Files\NMagellan\NCheck\Necheck.exe).

Si vous rencontrez des symboles bizarres ou des messages tels que "Unknown order : N?CHSTER", il y a probablement un problème avec l'encodage du texte. Vous pouvez ajuster le paramètre correspondant dans les [Options] (menus/extras/options_system.html#Textkodierung).

### Q : Que signifie "Installer JAR" ?

**A:** À partir de la version 2, Magellan n'est plus constitué d'un seul fichier. Nous l'avons divisé en plusieurs fichiers. Afin de simplifier l'installation de Magellan, nous avons regroupé le programme dans un fichier appelé "Installer JAR". Il s'agit d'un programme Java, tout comme Magellan lui-même. Il décompresse son contenu à un emplacement que vous pouvez spécifier lors de l'installation.  
À partir de la version 2, il est également possible de copier une nouvelle version sur une ancienne version sans désinstaller cette dernière au préalable. Les fichiers de configuration sont conservés et ajustés lors du prochain démarrage de Magellan.

### Q : magellan-client.jar ? Le fichier ne devrait-il pas s'appeler magellan.exe ?

**Pour être honnête, ce n'est (presque) rien d'autre qu'un fichier ZIP renommé, mais un fichier que Java peut gérer et que vous n'avez donc pas besoin de décompresser avec WinZip ou un programme similaire. Magellan consiste en fait en un grand nombre de fichiers qui sont simplement rassemblés dans magellan-client.jar, mais Magellan devrait démarrer lorsque l'on double-clique sur le fichier JAR.

### Q : Pourquoi un autre programme (par exemple WinZip ou PowerArchiver) démarre-t-il lorsque j'essaie de lancer Magellan en double-cliquant ?

**A:** Parce qu'il est configuré pour être le programme par défaut pour l'extension .jar au lieu de Java. Dans PowerArchiver et WinZip, vous pouvez annuler ce réglage dans les options de ces programmes, sinon une action manuelle est nécessaire :

1. Dans le menu Démarrer, cliquez sur "Exécuter
2. Tapez 'regedit' et cliquez sur OK
3. Dans l'arborescence de gauche, cliquez sur l'entrée HKEY_CLASSES\_ROOT et cliquez sur l'entrée '.jar'
4. Dans la fenêtre de droite, double-cliquez sur '(Default)'
5. Tapez jarfile et cliquez sur OK
6. Fermez le programme regedit - terminé.

### Q : Pourquoi Magellan ne démarre-t-il pas lorsque je clique sur le fichier magellan-client.jar ?

**A:** Une cause possible est que le fichier magellan-client.jar se trouve dans un dossier dont le nom contient des espaces. Il ne s'agit pas d'un problème causé par une erreur dans Magellan, mais d'un inconvénient dans le lien par défaut entre les fichiers .jar et java.

La solution simple est bien sûr de déplacer Magellan dans un autre dossier. La solution complexe est la suivante :

1. Dans le menu Démarrer, cliquez sur "Exécuter
2. Tapez 'regedit' et cliquez sur OK
3. Dans l'arborescence de gauche, cliquez sur l'entrée "HKEY_CLASSES\_ROOT" et cliquez sur l'entrée "jarfile".
4. Ouvrez les sous-entrées 'shell', 'open' et enfin 'command'
5. Dans la fenêtre de droite, double-cliquez sur "(Default)", et quelque chose comme (chemin d'accès à Java)\javaw.exe -jar %1 devrait apparaître.
6. Remplacez %1 par "%1"
7. Fermez le programme regedit - c'est fait.

Contexte : Si Magellan se trouve dans C:\NMy Documents\Nmagellan-client.jar, l'entrée de regedit essaierait d'inciter Java à appeler (Path to Java)\Njavaw.exe -jar C:\NMy Documents\Nmagellan-client.jar, ce qui signifie que Java essaierait d'exécuter le fichier C:\NMy avec les paramètres Documents\Nmagellan-client.jar, ce qui ne fonctionne pas tout à fait comme il le faudrait. Avec les guillemets, cela ressemble à ceci : (Chemin d'accès à java)\Njavaw.exe -jar "C:\NMy Documents\Nmagellan-client.jar" , et ici le fichier magellan-client.jar est lancé.

Si cela ne vous a pas aidé, vous pouvez essayer de [démarrer Magellan à partir de l'invite de commande](#4) pour pouvoir lire les éventuels messages d'erreur.

### Q : Comment faire de Magellan le programme par défaut pour les fichiers CR sous Windows2000/XP ?

**R:** Pour ce faire, vous devez établir un lien entre Magellan et le type de fichier "CR". Vous pouvez le faire dans l'Explorateur sous Extras, Folderoptions, Filetypes :

1. Choisissez le type de fichier "CR"
2. Cliquez sur le bouton "Avancé".
3. Dans la fenêtre "Modifier le type de fichier", cliquez sur le bouton "Nouveau".
4. Dans le champ "Action", entrez Magellan
5. Dans le champ "Programme pour cette action", entrez "(Chemin d'accès à java)\Njavaw.exe" -jar "(Chemin d'accès à magellan)\Nmagellan-client.jar" "%1". javaw.exe et magellan-client.jar doivent être entrés avec le chemin d'accès complet (par exemple, "c:\Ngames\Neressea\Nmagellan-client.jar"). Si le chemin contient des espaces, les guillemets sont obligatoires.
6. Cliquez sur "OK"
7. Dans la fenêtre "Edit filetype", choisissez l'entrée "Magellan" dans la liste "Actions".
8. Cliquez sur le bouton "As standard". L'entrée Magellan est maintenant affichée en caractères gras.
9. C'est fait :-)

### Q : Comment démarrer Magellan à partir de l'invite de commande ?

**A:** Magellan (ou Java) ne donne souvent que des messages d'erreur à l'invite de commande, il peut donc être utile de démarrer Magellan à partir de là.

Pour ce faire, lancez d'abord l'invite de commande (sous Windows ME, dans le menu Démarrer, ouvrez "Exécuter", tapez commande et cliquez sur OK, sous Windows 2000/XP, la commande est cmd). Saisissez ensuite la commande suivante : javaw -jar "(Chemin d'accès à Magellan)\Nmagellan-client.jar", c'est-à-dire par exemple javaw -jar "C:\NMy Documents\Nmagellan-client.jar".

Les paramètres de l'invite de commande pour Magellan sont listés dans la [Référence](reference/commandline.html).

### Q : Comment faire fonctionner Magellan sur un Mac ?

**A:** Malheureusement, Apple n'a publié qu'une version actuelle de Java pour Mac OS X qui supportera Magellan. Pour lancer Magellan, utilisez la console comme sous Windows à l'[invite de commande](#4).

### Q : Comment gérer les chemins d'accès aux ressources ?

**R:** Les chemins de ressources sont décrits sur [leur propre site] (reference/resources.html).

### Q : Bien que je dispose de suffisamment de RAM, Magellan signale un manque de mémoire. Pourquoi ?

**A:** Il suffit d'allouer un peu plus de mémoire à la machine virtuelle Java (VM). Magellan demande de la mémoire à la VM, et lorsque la VM n'en a pas à offrir, Magellan échoue tout simplement.

Vous pouvez allouer de la mémoire comme suit :

Modifiez le fichier texte magellan.bat (sous Windows) ou magellan.sh (Linux et Mac) et remplacez toutes les occurrences de la forme \-Xmx000m par une valeur plus grande, par exemple \-Xmx2000M , afin d'allouer environ 2 gigaoctets à Magellan.

Il n'y a pas d'autre solution que d'utiliser le système d'exploitation de Magellan.

Sous Windows :
Créez une icône pour Magellan sur votre bureau, cliquez dessus avec le bouton droit de la souris et allez dans "Propriétés". Sous "Cible", entrez ce qui suit (le chemin d'accès doit bien sûr correspondre à votre configuration particulière) :  
C:\NProgram Files\NJava\Njre6\Nbin\Njavaw.exe -Xms128M -Xmx512M -jar "c:\NProgram Files\NEressea\NMagellan\Nmagellan-client.jar" ou simplement
javaw -Xms128M -Xmx512M -jar "c:\NProgram Files\NEressea\NMagellan\Nmagellan-client.jar"

Sous Linux, tapez simplement (dans le shell, dans le répertoire magellan) :  
java -Xms128M -Xmx512M -jar magellan-client.jar

Cela indique au Java-VM d'allouer un minimum de 128 Mo et un maximum de 512 Mo. Vous pouvez modifier ces valeurs en fonction de votre propre configuration.

### Q : Pourquoi Magellan n'utilise-t-il soudainement plus les nombres de base 36 pour les unités ?

**A:**Il est très probable que la balise 36;Basis soit manquante dans le CR. Cette balise de base définit la base numérique utilisée par Magellan. Pour les CR Eressea, il s'agit généralement de la base 36, tandis que d'autres PBeM utilisant des versions plus anciennes du CR (par exemple Verdanon) utilisent la base décimale (base 10) et ne connaissent pas cette balise. C'est pourquoi Magellan utilise la base 10 lorsqu'il n'y a pas de mention d'une autre base numérique dans le CR.

### Q : Comment puis-je utiliser Vorlage pour confirmer automatiquement les ordres des unités ?

**A:** Avec // #tag EINHEIT ejcOrdersConfirmed 1

### Q : A quoi servent ces § dans l'ARR, l'ATR ou les infobulles ?

**A:** Une bonne aide sur l'ARR, l'ATR et les infobulles se trouve [dans l'aide de Magellan] (reference/atr_arr.html).
