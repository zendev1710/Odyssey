# Working with computer reports

In order to make your moves with Magellan, you must obtain the evaluation as a computer report (CR). This is normally obtained automatically. If not, you can give a unit the OPTION COMPUTER command.

The CR is usually contained in a zip file. You do not have to unpack this file, but can open it directly with Magellan. This is done via the menu [File -> Open...](../menus/file/open.md). The same also applies to the sometimes still existing reports with the extensions .cr.bz2 or cr.gz. If you are asked whether you want to accept the password for a file, you should answer in the affirmative. Afterwards, you can select your faction in the [Faction statistics](../menus/extras/factionstatistics.md) and use the **_Password and other properties_** button to tell Magellan your password, as it is only possible to write your commands once the password has been set.

When you receive your first evaluation, the password is also in the file with the extension .nr. It should be somewhere in the first few lines.

## Using a train CR

To be able to use all the functions of Magellan, it makes sense to always work with the "same" CR and add the new CR. It makes sense to start by saving your first CR under a different name, e.g. train.cr. If you now receive a new evaluation, load the old zug.cr and add the new report via [File -> Add...](../menus/file/add.md).

The functions that are only possible in this way include

* Change displays for the previous week
* Region information at the time of the last visit to a region from which no information was obtained with the current evaluation. This includes:
* * Streets
    * Buildings
    * Farmers
    * Silver
    * Resources

The information that is retained and therefore does not have to be re-entered (if this is possible at all) includes

* Assigned island names
* self-created region comments
* Information about growing herbs
* Information about potion recipes
* information about spells

## Exchange of CRs

When exchanging CRs with other players, you should first think about what you want to exchange. When exchanging map material, for example, you should avoid leaving your units and buildings in the CR as far as possible. After all, you don't have to reveal your strengths and weaknesses straight away. Magellan offers several options under the menu item [File -> Export CR...](../menus/file/crexport.md) as to what should be saved in a CR.

If you want to add external reports (maps, etc.) to your own, Magellan automatically tries to recognise the best match if the coordinate systems of the reports do not match. If, contrary to expectations, this should lead to an incorrect result, the report to be added must be loaded normally and then the [Adjust origin](../menus/map/origin.md), the report saved and then added to the main report.

## Working with several CRs of the same round

If you are working with several reports from the same round (e.g. reports from allies), it is recommended that you always load your own report first and add the other reports to it. This ensures that the coordinate system of your own report is retained.

When adding ally reports step by step, it can happen that, for example, change information is not recorded. To avoid this, you can first try to merge all ally reports into a single report and then add this alliance report to your own.
