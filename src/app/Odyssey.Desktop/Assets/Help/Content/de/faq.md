# FAQ

Frequently asked questions (oft gestellte Fragen):

## 1\. Fragen zur Installation

1. [Was brauche ich, um Magellan starten zu können?](#Installation)
2. [Obwohl ich genug RAM habe, meldet Magellan Speichermangel. Wieso?](#Speichermangel)

## 2\. Fragen zu Magellan

1. [Wie rufe ich ECheck auf?](#echeck)
2. [Was ist eigentlich dieses §-Zeug in ARR, ATR, den Tooltips oder der Regions-Kurzinfo der Detailanzeige?](#ARR)

## 3\. Fragen zur Java

1. [Was ist Java und wozu ist es gut?](#Java)
2. [Welche Version sollte ich genau herunterladen?](#Javaversion)
3. [JRE oder JDK?](#jdk)
4. [Welche Java-Version habe ich?](#myversion)
5. [Ich bekomme folgende Fehlermeldung: 'Error: A JNI Error has occured, please check your installation and try again.'](#JNI_ERROR)
6. [Die neueste Version auf java.com ist aber Version 8!](#java8)
7. [F: Ich brauche aber Java 8 für ein anderes Program. Was kann ich tun?](#needJava8)

## 4\. Fragen zu früheren Magellanversionen

1. [Was bedeutet Installer JAR](#Installer)
2. [magellan-client.jar? Müsste die Datei nicht magellan.exe heißen?](#Dateiname)
3. [Warum startet ein anderes Programm (z.B. WinZip oder PowerArchiver), wenn ich Magellan per Doppelklick starte?](#Verknuepfung)
4. [Warum startet Magellan nicht, wenn ich auf die Datei magellan-client.jar klicke?](#keinStart)
5. [Obwohl ich genug RAM habe, meldet Magellan Speichermangel. Wieso?](#Speichermangel_legacy)
6. [Wie verknüpfe ich unter Windows2000/XP CR-Dateien mit Magellan?](#CRVerknuepfung)
7. [Wie starte ich Magellan von der Kommandozeile?](#Kommandozeilenstart)
8. [Wie bekomme ich Magellan auf einem Mac zum laufen?](#MacMagellan)
9. [Wie gehe ich mit Ressourcenpfaden um?](#Ressourcenpfade)
10. [Warum verwendet Magellan plötzlich keine Base-36 Nummern mehr für die Einheiten?](#base36nummern)
11. [Wie kann ich mit Vorlage die Befehle von Einheiten automatisch bestätigen?](#VorlageBefehlsbestaetigung)

## Antworten

### F: Was brauche ich, um Magellan starten zu können?

**A:** Seit der Version 2.1 brauchst du nur noch eine einzige Datei zur Installation herunterladen. Die richtige Version findest du auf der [Magellan-Homepage](https://magellan2.github.io) in der Downloadsektion. Dort bekommst du die Version für dein Betriebssystem (Windows, Linux oder MacOS). Diese Datei führst du aus (unter Windows benötigst du dafür normalerweise Administratorrechte). Dadurch wird alles Nötige installiert inklusive [Java](#Java). Updates von älteren Versionen funktionieren normalerweise problemlos, aber es kann Probleme mit alten Plugins geben. Diese musst du in diesem Fall entweder deinstallieren oder eine neue Version installieren.

Falls du Java schon installiert hast, kannst du auch nur die Zip-Datei entpacken und Magellan direkt (am besten über die Datei magellan.bat oder magellan.sh) starten. Dieser Weg wird aber nicht mehr empfohlen oder offiziell unterstützt.

### F: Was ist Java und wozu ist es gut?

**A:** Java ist die Programmiersprache mit der Magellan geschrieben ist. Das Besondere an Java ist, dass es auf verschiedenen Plattformen läuft (Windows, Linux, Mac, ...). Seit Version 2.1 bringt das Magellan-Installationsprogramm eine eigene Version von Java bereits mit. Für frühere Versionen von Magellan, muss Java vorher installiert sein. Eine der einfachsten Möglichkeiten, Java zu installieren ist der Download von [AdoptOpenJDK](https://adoptopenjdk.net/releases.html). Wir empfehlen zum Beispiel [Open JDK](https://openjdk.java.net/) oder [Oracle Java SE](https://www.oracle.com/java/).

### F: Welche Version sollte ich genau herunterladen?

**A:** Es gibt verschiedene Herausgeber von Java. Eine der einfachsten Möglichkeit, Java zu installieren ist der Download von [AdoptOpenJDK](https://adoptopenjdk.net/releases.html). Wir empfehlen zum Beispiel [Open JDK](https://openjdk.java.net/) oder [Oracle Java SE](https://www.oracle.com/java/).

Im Moment (Sommer 2021) haben wir die folgende Empfehlung, die für die meisten passen sollte:

* AdoptOpenJDK
* Natürlich die für dein Betriebssystem (Windows für die meisten)
* OpenJDK 11 (LTS), ab Herbst OpenJDK 17 (LTS)
* HotSpot JVM
* in der Regel x64

### F: JRE oder JDK?

**A:** JDK ist normalerweise nur für Entwickler, JRE eher für Endbenutzer. Falls du die ExtendedCommands von Magellan benutzen willst, brauchst du ein JDK! Der einzige Nachteil des JDK ist die Größe. Wenn du also nicht besonders knapp an Festplattenplatz bist, nimm lieber ein JDK.

### F: Welche Java-Version habe ich?

**A:** Du kannst die Javaversion auf folgende Art ermitteln:

1. Öffne die Kommandozeile
    * **Unter Windows:** Drücke die Windowstaste um das Startmenü aufzumachen. Tippe dann 'cmd' um die 'Kommandozeile zu öffnen
    * **Unter MacOsX:** Öffne Spotlight (Command + Leertaste oder klick auf die Lupe rechts oben), dann gibt 'terminal' ein.
    * **Unter Linux:** Öffne ein Terminal (je nach Linuxdistribution drücke zum Beispiel die Windowstaste und gib Terminal ein).
2. Tippe `java -version` und drücke `Enter`. Eine Ausgabe wie `'openjdk version "11.0.10" 2021-04-20'` heißt zum Beispiel, dass du Version 11 hast. "1.8.0" entspricht Java 8, ist also zu alt. Eine Ausgabe wie "Befehl java nicht erkannt" heißt, dass du vermutlich gar kein Java hast.

### F: Ich bekomme folgende Fehlermeldung: 'Error: A JNI Error has occured, please check your installation and try again.'

**A:** Du hast wahrscheinlich nicht Java 11 sondern Java 8! Installiere Java 11 (siehe oben).

### F: Die neueste Version auf java.com ist aber Version 8

**A:** Seit dem Übergang von Java von Sun Microsystems an Oracle haben diese ein neues Lizenzmodell etabliert. Das hat die Herausgabe und Benutzung neuer Java-Implementationen verkompliziert. Deshalb ist für aktuelle Versionen [java.com](https://java.com) nicht mehr der Ort der Wahl. Stattdessen gibt es die oben genannten Alternativen.

### F: Ich brauche aber Java 8 für ein anderes Program

**A:** Du kannst Magellan trotzdem benutzen, aber du musst ein bisschen tricksen.

**Unter Windows:**

1. Installiere erst Java 11, dann wieder Java 8. Dein anderes Programm sollte jetzt weiterhin funktionieren.
2. Finde den Installationspfad von Java 11 (zum Beispiel: C:\\Programme\\jdk-11.0.1).
3. Finde die Datei magellan.bat (in der Regel in C:\\Programme\\Magellan\\magellan.bat).
4. Ändere diese Datei als Administrator. Gehe dazu etwa wie folgt vor: Finde im Startmenü den Eintrag für "notepad". Mache einen Rechstklick darauf und wähle "Mehr ... Als Administrator ausführen".
5. Öffne die Datei magellan.bat im Notepad.
6. Füge folgende Zeile am Anfang ein (angepasst an deinen Pfad in Schritt 2):  
    `SET JAVA_HOME=C:\Programme\jdk-11.0.1`
7. Speichere die Datei magellan.bat.
8. Jetzt solltest du Magellan mit Java 11 ausführen können.

**Unter Linux:**

1. Installiere sowohl Java 8 als auch Java 11.
2. Führe in der Kommandozeile folgendes Kommando aus: `update-alternatives --config java`. Wähle die Version aus, die du für das andere Programm benötigst.
3. Notiere dir den Pfad zu Java 11 (zum Beispiel `/usr/lib/jvm/java-11-openjkd-amd64`).
4. Finde die Datei magellan.sh (in der Regel $HOME/Magellan/magellan.sh).
5. Ändere die Datei wie folgt: Füge die Zeile `export JAVA_HOME=/usr/lib/jvm/java-11-openjdk-amd64` am Anfang ein (angepasst an deine Installation).
6. Ändere die letzte Zeile von `java -Xmx1200m -jar "magellan-client.jar" "$@"` auf `$JAVA_HOME/bin/java -Xmx1200m -jar "magellan-client.jar" "$@"` ab.
7. Speichere die Datei magellan.sh.
8. Jetzt sollte Magellan mit Java 11 ausgeführt werden.

### F: Obwohl ich genug RAM habe, meldet Magellan Speichermangel. Wieso?

**A:** Aufgrund der Besonderheiten von Java kann Magellan sich nicht immer so viel Speicher holen, wie es braucht. Im Installationsverzeichnis von Magellan (unter Windows meist C:\\Programme\\Magellan ) gibt es eine Datei magellan\_launcher.vmoptions . Diese Datei musst du (womöglich als Administrator) editieren und eine Zeile wie \-Xmx1G hinzufügen. Dies weist Java an, bis zu 1 Gigabyte Speicher zur Verfügung zu stellen.

Normalerweise sollte es kein Problem sein, die Hälfte des Arbeitsspeichers deines Rechners oder mehr zur Verfügung zu stellen. Keine Angst: Magellan nimmt sich davon immer nur so viel Speicher, wie es braucht. Wenn du also 4 Gigabyte RAM hast, ist eine Einstellung von \-Xmx2G in Ordnung. Sollte dein ganzes System nach dem Laden eines großen Reports sehr langsam sein, solltest du den Wert wieder verringern und kannst eventuell die Reportgröße verringern, indem du zum Beispiel nur einen Kartenausschnitt lädst oder Reports von Verbündeten nicht hinzufügst. Eventuell hilft dir auch das MemoryWatch Plugin von der Magellan-Hompeage weiter um deinen Speicherverbrauch zu beobachten.

Weitere Möglichkeiten, die Speichermenge zu verändern, die vor allem für ältere Magellanversionen interessant sind, siehe [weiter unten](#Speichermangel_legacy).

### F: Wie rufe ich ECheck auf?

**A:** Zunächst einmal: Warum brauchst du ECheck? Praktisch alle Funktionen von ECheck kann Magellan selbst erledigen. Alle Syntaxfehler in den Befehlen, die ECheck erkennt, und noch einige mehr, erkennt Magellan und markiert sie entsprechend farbig. Weitere Fehler werden in der [Offene Probleme](docks/problems.md)\-Ansicht dargestellt. Hier kannst du auch konfigurieren, welche Art von (potenziellen) Problemen du anzeigen und welche du lieber ignorieren willst.

Falls du aber wirklich ECheck ausführen willst, kannst du das aus dem [ECheck-Dock](docks/echeck.md) tun. Magellan kommt mit einer eigenen Version von ECheck. Diese wird normalerweise automatisch konfiguriert, so dass du nur noch auf "Ausführen" klicken musst. Falls das nicht klappt, oder falls deine Einstellungen noch von einer älteren Version stammen, kannst du dies unter [Optionen - Ressourcen](menus/extra/opt menus/extras/options_resources.md) einstellen. Dort gibst du den Pfad zu deiner ECheck-Installation (zum Beispiel C:\\Programme\\Magellan\\echeck\\echeck.exe ) an. Falls beim Aufruf "komische" Zeichen angezeigt werden, kann es ein Problem mit der [Textkodierung](menus/extras/options_system.html#Textkodierung) geben. In den Optionen unter "System" kannst du Einstellungen für ECheck auf _UTF-8_ oder _ISO-8859-1_ ändern. Eine der beiden Einstellungen (wahrscheinlich UTF-8) sollte funktionieren.

### F: Was bedeutet Installer JAR?

**A:** Ab Version 2 von Magellan ist das Programm nicht mehr nur eine Datei. Wir haben Magellan in viele Dateien aufgeteilt. So sieht man jetzt im Dateisystem alle Bilder und Icons, die Magellan verwendet oder auch alle Sprachdateien für die Internationalisierung (sogenannte Resources). Damit die Installation von Magellan nicht so aufwendig wird, liefern wir das Programm jetzt in einem Paket aus, dass man als Installer JAR bezeichnet. Es ist ein Java Programm, genauso wie Magellan. Es entpackt sich selbst an einen beliebigen Ort, den du während der Installation festlegen kannst.  
Ab Version 2.0 ist es übrigens einfach möglich, eine neue Version über eine alte Version zu installieren ohne vorher die alte Version zu deinstallieren. Die Konfiguration bleibt erhalten und wird während des ersten Starts angepasst.

### F: magellan-client.jar? Müsste die Datei nicht magellan.exe heißen?

**A:** Nein. Um ehrlich zu sein, ist das (fast) nur eine umbenannte ZIP-Datei, aber eine, mit der Java etwas anfangen kann und die du deshalb auch nicht mit WinZip oder ähnlichen Programmen dekomprimieren musst. Magellan besteht eigentlich aus sehr vielen Programmdateien, die einfach nur in magellan-client.jar zusammengefasst sind, trotzdem sollte Magellan nach einem Doppelklick auf die JAR-Datei starten.

### F: Warum startet ein anderes Programm (z.B. WinZip oder PowerArchiver), wenn ich Magellan per Doppelklick starte?

**A:** Weil es sich statt Java mit der Dateiendung .jar verknüpft hat. Bei PowerArchiver und WinZip kann man das in den Optionen wieder rückgängig machen, ansonsten ist Handarbeit angesagt:

1. Wähle im Startmenü den Punkt 'Ausführen'
2. Gib regedit ein und klicke auf OK
3. Öffne links im Baum den Eintrag 'HKEY\_CLASSES\_ROOT' und dann klicke auf den Eintrag '.jar'
4. Mache nun im Fenster rechts einen Doppelklick auf '(Default)'
5. Gib jarfile ein und klicke OK
6. Schließe das regedit Programm - fertig

### F: Warum startet Magellan nicht, wenn ich auf die Datei magellan-client.jar klicke?

**A:** Eine mögliche Ursache dafür ist, dass sich die Datei magellan-client.jar in einem Ordner befindet, dessen Name Leerzeichen enthält. Dieses Problem wird nicht durch einen Fehler in Magellan verursacht sondern durch eine ungeschickte Verknüpfung von .jar -Dateien mit java.

Die einfache Lösung ist natürlich, Magellan in einen anderen Ordner zu verschieben. Die komplizierte Lösung sieht folgendermaßen aus:

1. Wähle im Startmenü den Punkt 'Ausführen'
2. Gib 'regedit' ein und klicke auf OK
3. Öffne links im Baum den Eintrag 'HKEY\_CLASSES\_ROOT' und dann klicke auf den Eintrag 'jarfile'
4. Öffne die Untereinträge 'shell', 'open' und schließlich 'command'
5. Mache nun im Fenster rechts einen Doppelklick auf '(Default)', dort sollte nun etwas in der Form (Pfad zu Java)\\javaw.exe -jar %1 stehen.
6. Ersetze nun %1 durch "%1"
7. Schließe das regedit Programm - fertig

Hintergrund: Liegt Magellan im Ordner C:\\Eigene Dateien\\magellan-client.jar bewirkt der regedit-Eintrag einen Aufruf von java in der Form (Pfad zu Java)\\javaw.exe -jar C:\\Eigene Dateien\\magellan-client.jar , das heißt, java würde versuchen, die Datei C:\\Eigene mit dem Parameter Dateien\\magellan-client.jar zu starten, was nicht so richtig gut klappen kann. Mit den Anführungszeichen sieht es dann so aus: (Pfad zu java)\\javaw.exe -jar "C:\\Eigene Dateien\\magellan-client.jar" , es wird hier also wirklich die Datei magellan-client.jar gestartet.

Sollte das nicht geholfen haben, kann man versuchen, [Magellan von der Kommandozeile aus zu starten](#4), um eventuelle Fehlermeldungen sehen zu können.

### F: Wie verknüpfe ich unter Windows2000/XP CR-Dateien mit Magellan?

Dafür muss man für den Dateityp "CR" einen entsprechenden Vorgang definieren. Das geht im Dateimanager (Explorer) unter Extras, Ordneroptionen, Dateitypen:

1. Wähle den Dateityp "CR".
2. Wähle den Button "Erweitert".
3. Wähle im Fenster "Dateityp bearbeiten" den Button "Neu".
4. Trage im Feld "Vorgang" den Wert Magellan ein.
5. Trage im Feld "Anwendung für diesen Vorgang" den Wert "(Pfad zu Java)\\javaw.exe" -jar "(Pfad zu Magellan)\\magellan-client.jar" "%1" ein. javaw.exe und magellan-client.jar müssen dabei mit vollständigem Pfad eingegeben werden (z.B. "c:\\spiele\\eressea\\magellan-client.jar"). Enthält dieser Pfad Leerzeichen sind die umschließenden Anführungszeichen unbedingt notwendig.
6. Wähle den Button "OK"
7. Wähle im Fenster "Dateityp bearbeiten" den Eintrag "Magellan" aus der Liste "Vorgänge" aus.
8. Wähle den Button "Als Standard". Der Eintrag Magellan ist nun fett dargestellt.
9. Fertig :-)

### F: Wie starte ich Magellan von der Kommandozeile?

**A:** Magellan (bzw. Java) gibt Fehlermeldungen oft nur in der Eingabeaufforderung aus, deshalb kann es nützlich sein, Magellan von der Kommandozeile aus zu starten, um solche Fehlermeldungen zu sehen.

Dazu startet man zunächst die MS-DOS-Eingabeaufforderung (unter Windows ME im Startmenü den Punkt 'Ausführen' öffnen, command eingeben und auf OK klicken, bei Windows 2000/XP lautet das Kommando cmd ). Danach gibt man folgenden Befehl ein: javaw -jar "(Pfad zu Magellan)\\magellan-client.jar" also z.B. javaw -jar "C:\\Eigene Dateien\\magellan-client.jar" .

Die Kommandozeilen-Parameter von Magellan sind in der [Referenz](reference/commandline.md) beschrieben.

### F: Wie bekomme ich Magellan auf einem Mac zum laufen?

**A:** Leider hat Apple erst mit dem Mac OS X eine aktuelle Java-Version veröffentlicht, unter der auch Magellan funktioniert. Zum Laden von Magellan geht man an der Konsole so vor, wie unter Windows an der [Kommandozeile](#Kommandozeilenstart).

### F: Wie gehe ich mit Ressourcenpfaden um?

**A:** Die Ressourcenpfade sind auf einer [eigenen Seite](reference/resources.md) beschrieben.

### F: Obwohl ich genug RAM habe, meldet Magellan Speichermangel. Wieso?

**A:** Du musst der Java-VM einfach etwas mehr Speicher zubilligen. Magellan fordert den Speicher von der VM an, wenn die ihm den Speicher nicht geben will, guckt Magellan in die Röhre.

Die Speicherzuteilung kannst Du folgendermaßen machen:

Editiere die Datei magellan.bat (unter Windows) oder magellan.sh (Linux und Mac) und ändere alle Vorkommen der Form \-Xmx000m auf einen anderen Wert, zum Beispiel \-Xmx2000M , um etwa 2 Gigabyte zur Verfügung zu stellen.

_oder_

Unter Windows:  
Erstelle eine Verknüpfung von Magellan, klicke mit der rechten Maustaste drauf und geh auf "Eigenschaften". Dort trägst Du unter "Ziel" folgende Zeile ein (die Pfade müssen natürlich an Deine Installation angepasst werden):  
"C:\\Programme\\Java\\jre6/bin/javaw.exe" -Xms128M -Xmx512M -jar "c:\\Programme\\Eressea\\Magellan\\magellan-client.jar" oder einfach  
javaw.exe -Xms128M -Xmx512M -jar "c:\\Programme\\Eressea\\Magellan\\magellan-client.jar"

Unter Linux einfach (in der Shell, im Magellan-Verzeichnis):  
java -Xms128M -Xmx512M -jar magellan-client.jar

Dies weist die Java-VM an, sich mindestens 128MB und maximal 512MB RAM zu sichern. Je nach Speicherausbau kannst Du die Werte auch verändern.

### F: Warum verwendet Magellan plötzlich keine Base-36 Nummern mehr für die Einheiten?

**A:**Im CR fehlt vermutlich das Tag 36;Basis . Das Basis-Tag definiert die Zahlenbasis mit der Magellan arbeitet. Bei Eressea-CRs ist das normalerweise 36, andere PBeMs, die ältere CR-Versionen benutzen (z.B. Verdanon), arbeiten mit Dezimalzahlen (Basis 10) und kennen dieses Tag nicht. Daher nimmt Magellan Basis 10 an, wenn im CR nichts anderes definiert ist.

### F: Wie kann ich mit Vorlage die Befehle von Einheiten automatisch bestätigen?

**A:** Mit // #tag EINHEIT ejcOrdersConfirmed 1

### F: Was ist eigentlich dieses §-Zeug in ARR, ATR, den Tooltips oder der Regions-Kurzinfo der Detailanzeige?

**A:** Eine gute Hilfe zum ATR, ARR und den Tooltips findet sich [in der Magellanhilfe.](reference/atr_arr.md)
