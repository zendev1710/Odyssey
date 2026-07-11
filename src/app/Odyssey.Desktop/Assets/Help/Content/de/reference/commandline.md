# Kommandozeilen-Parameter

Magellan akzeptiert folgende beim [Starten](../faq.html#Kommandozeilenstart) angegebenen Parameter:

* \-d &lt;Verzeichnis&gt;: Gibt das Verzeichnis an, in dem Magellan nach seinen Ressourcen sucht, also Bildern, Übersetzungen etc.
* \-s &lt;Verzeichnis&gt;: Gibt das Verzeichnis an, in dem Magellan nach seinen Einstellungen sucht (profiles.ini). In dessen Unterverzeichnisse sind die _Profile_ mit den Konfigurationsdateien (magellan.ini, magellan\_desktop.ini) und der Datei errors.txt mit den Fehlermeldungen.
* \-p &lt;Profil&gt;: Startet Magellan mit einem bestimmten Profil, das in den Profileinstellungen definiert wurde.
* \-pm: Zeigt den Profilmanager beim Start.
* \-log &lt;X&gt;: Setzt die Detailstufe des Logs. Mögliche Werte für X sind: O - aus, E - nur Fehler, W - auch Warnungen, I - auch Infomeldungen.
* \--help: Zeigt nur die Magellan-Hilfe an.
* &lt;CR-Datei&gt;: Der angegebene CR wird direkt nach dem Start geladen.

Der gesamte Aufruf von Magellan sieht folgendermaßen aus: java -jar magellan.jar \[-d Verzeichnis\] \[-s Profile\] \[-pm\] \[-log \[O|E|W|I\]\] \[--help\] \[CR-Datei\]
