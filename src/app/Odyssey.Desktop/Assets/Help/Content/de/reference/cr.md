# Arbeiten mit Computerreporten

Um mit Magellan seine Züge machen zu können, muss man die Auswertung als Computerreport (CR) beziehen. Diesen bekommt man normalerweise automatisch. Falls doch nicht, kann man einer Einheit den Befehl OPTION COMPUTER geben.

Meist ist der CR in einer Zipdatei enthalten. Diese muss man nicht extra entpacken, sondern kann sie direkt mit Magellan öffnen. Dies geschieht über das Menü [Datei -> Öffnen...](../menus/file/open.md). Dasselbe gilt auch für die manchmal noch vorhandenen Reports mit den Endungen .cr.bz2 oder cr.gz. Die eventuelle Frage, ob man das Passwort für eine Partei übernehmen will, sollte man bejahen. Im Nachhinein kann man in der [Parteistatistik](../menus/extras/factionstatistics.md) seine Partei auswählen und mittels der Schaltfläche **_Passwort und andere Eigenschaften_** Magellan sein Passwort mitteilen, da es erst mit gesetztem Passwort möglich ist, seine Befehle zu schreiben.

Bekommt man seine erste Auswertung, so befindet sich das Passwort auch in der Datei mit der Endung .nr. Dort sollte es irgendwo in den ersten Zeilen stehen.

## Benutzen eines Zug-CRs

Um alle Funktionen von Magellan nutzen zu können, ist es sinnvoll, immer mit dem "gleichen" CR zu arbeiten und den neuen CR hinzuzuladen. Man beginnt damit sinnvollerweise, indem man seinen ersten CR unter einem anderen Namen speichert, z.B. zug.cr. Erhält man nun eine neue Auswertung, so lädt man den alten zug.cr und fügt den neuen Report per [Datei -> Hinzufügen...](../menus/file/add.md) hinzu.

Zu den Funktionen, die so erst möglich sind, gehören:

* Veränderungsanzeigen zur Vorwoche
* Regionsinformationen zur Zeit des letzten Besuchs einer Region, von der man mit der aktuellen Auswertung keine Informationen bekommen hat. Dazu gehören:
* * Straßen
    * Gebäude
    * Bauern
    * Silber
    * Resourcen

Zu den Informationen, die dadurch erhalten bleiben und somit nicht neu eingegeben werden müssen (so das überhaupt möglich ist), gehören:

* zugewiesene Inselnamen
* selbsterstellte Regionskommentare
* Informationen über wachsende Kräuter
* Informationen über Trankrezepte
* Informationen über Zaubersprüche

## Austausch von CRs

Beim Austausch von CRs mit anderen Spielern sollte man sich zuerst Gedanken machen, was man überhaupt tauschen will. So sollte man beim Tauschen von Kartenmaterial es tunlichst vermeiden, seine Einheiten und Gebäude im CR zu lassen, schließlich muss man ja nicht gleich seine Stärken und Schwächen offen legen. Magellan bietet hierzu unter dem Menüpunkt [Datei -> CR exportieren...](../menus/file/crexport.md) mehrere Optionen an, was in einem CR gespeichert werden soll.

Will man fremde Reporte (Karten o.a.) dem eigenen hinzufügen, so versucht Magellan automatisch, die beste Übereinstimmung zu erkennen, falls das Koordinatensysteme der Reporte nicht übereinstimmen. Falls dies allerdings wider Erwarten zu einem unkorrekten Ergebnis führen sollte, so muss man den hinzuzufügenden Report normal laden und dann selbst den [Ursprung anpassen](../menus/map/origin.md), den Report speichern und ihn dann zum Hauptreport hinzufügen.

## Arbeiten mit mehreren CRs der gleichen Runde

Arbeitet man mit mehreren Reporten aus der gleichen Runde (z.B. Reporte von Verbündeten), empfiehlt es sich, immer seinen eigenen Report zuerst zu laden und diesem die fremden Reporte hinzuzufügen. Dadurch ist sichergestellt, dass das Koordinatensystem des eigenen Reports beibehalten wird.

Beim schrittweisen Hinzufügen von Verbündetenreporten kann es passieren, dass z.B. Veränderungsinformationen nicht erfasst werden. Um das zu vermeiden, kann man versuchen, zunächst alle Verbündetenreporte zu einem einzigen Report zusammenzufügen und dann diesen Bündnisreport dem eigenen hinzufügen.
