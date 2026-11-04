# Template

This is a graphical frontend for Template. Template is a powerful move template generator for Eressea and other compatible games, which also offers a meta language for automation. You can download Vorlage at [http://www.gulrak.de/etools.html](http://www.gulrak.de/etools.html). Documentation can also be found there.

After selecting the menu item, the following dialogue opens:

![menu_extras_template](../../images/menu_extras_template.gif)

In the **_source CR(s)_** field, enter the path to one or more CRs that are to be edited by the template. Normally this is the CR that was sent by the Eressea server.

The **_target_file_** field is used to specify the file to which the template should write its output.

The file specified in the **_Script file_** field uses Template to integrate external functions and procedures for processing the meta commands. More information on template scripts can be found in the template documentation.

Finally, the **_template_** field contains the path to the template.

In the **_Options_** block, you can specify command line options for template. The switch **_Output as computer report_** creates the -cr option. Further options can be entered in the corresponding field if required.

Click on **_Ok_** to call up the template with the set options. The generated CR or command file can now be loaded into Magellan and processed further.

The output of the template is displayed in the **_Output_** window so that you can see any error messages.
