# Save commands

![Save commands](../../images/menu_file_saveorders.gif)

The following options (tabs) are available for saving or sending commands:

* **Email**
    Sends the commands directly by e-mail. Magellan can guess the necessary settings for some popular mail providers. Then you only have to enter the password. If this is not possible, you can fill in the remaining settings yourself. If you do not know the settings, a simple Internet search for "SMTP settings for provider name" often helps. With some providers, this function must first be activated in the e-mail settings. To do this, visit your provider's website. These settings have nothing to do with Eressea, but the Eressea community will be happy to help with such questions if you ask nicely.  
    ![Save commands](../../images/menu_file_saveorders_email.gif)
  * **Settings guess**
        Attempts to guess the server settings from the sender address or offers the option of selecting from a list of popular providers.
  * **Sender address**
        Your email address from which the commands are to be sent.
  * **SMTP server**
        Here you enter the mail server of your Internet provider (for example smtp.provider.de).
  * **Port**
        You must find out these settings from your provider. Common values are 25, 465 or 587.
  **Use SSL / Use TLS**
        These settings depend on the protocol used by your provider. If in doubt, try selecting both.
  * **Use authentication**
        Most providers nowadays require this box to be ticked.
  * **User name**
        Often identical to the sender address, but this also depends on the provider.
  * **Password**
        Magellan can save your password. This means a certain risk if someone with dishonest intentions gains access to your computer. You can tick the "Always ask" box, in which case you will have to re-enter the password each time, but it is more secure.
  **Recipient address**
        The email address of the Eressea server is entered here. Magellan can normally read it from the report, but if this does not work, you can enter it here (for example <eressea-server@eressea.kn-bremen.de>).
  * Subject
        The subject of the mail (e.g. Eressea commands).
  * **CC**
        Here you can enter one or more addresses (separated by commas) to which the report should also be sent.
* **File**
    Saves the commands in a file with the specified name. Select the Auto file name box to use special abbreviations as part of the file name. For example, 'commands-{round}.txt' causes the name to contain the current round, e.g. commands-123.txt.  

**Clipboard**
    Copies the command file to the clipboard. From there it can be easily copied into a mail programme, for example.  

* **Server upload**
    Uploads the commands directly to the server, without a diversion via email. The commands are not checked (by ECheck). The default address works for Eressea, but may not work for other games. If required, please ask the game management.

**Close**
    Closes the dialogue and saves all settings.
**Cancel**
    Closes the dialogue without saving the settings.

## Output options

Clicking on "Details" gives you access to further functions that determine the exact appearance of the exported commands.

![Output options](../../images/menu_file_saveorders_details.gif)

* **Automatic line break**
    Breaks the command file after _n_ characters. Longer lines (descriptions, messages, etc.) are automatically separated with " \\" in the process. Avoids problems with the automatic line break of mail programmes.
**ECheck comments**
    Inserts comments for the train checker programme ECheck (such as information about silver and persons) into the command file.
* **Remove comments beginning with ';'**
    Removes non-persistent comments from the command file. If possible, this option should be selected when sending the mail to the Eressea server in order to make the command file as small as possible. However, the information about the confirmation of the units' commands is also lost, as this is stored in ';' comments.
* **Remove comments beginning with '//'**
    Removes persistent comments from the command file. Template users should avoid using this option as far as possible, as all meta commands will also be deleted.
**Only units with confirmed commands**
    Only commands of confirmed units are written. Unconfirmed units are ignored. This option is very useful for players who share a faction.
* Selected regions**
    Here you can specify that commands are only sent for units that are currently selected on the map. This allows you to send commands to the server piece by piece.
**Insert unknown tags as template**
    This writes unknown tags to the command file. This option probably only makes sense for users of the train automation programme Template.
