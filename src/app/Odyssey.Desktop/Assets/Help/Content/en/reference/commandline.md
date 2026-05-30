# Command line parameters

Magellan accepts the following parameters specified in [Start](../faq.html#Command line start):

* \-d &lt;directory&gt;: Specifies the directory in which Magellan searches for its resources, i.e. images, translations, etc.
\-s &lt;directory&gt;: Specifies the directory in which Magellan searches for its settings (profiles.ini). In its subdirectories are the _profiles_ with the configuration files (magellan.ini, magellan\_desktop.ini) and the errors.txt file with the error messages.
\-p &lt;profile&gt;: Starts Magellan with a specific profile defined in the profile settings.
\-pm: Shows the profile manager at startup.
\-log &lt;X&gt;: Sets the detail level of the log. Possible values for X are: O - off, E - only errors, W - also warnings, I - also info messages.
\--help: Displays only the Magellan help.
&lt;CR file&gt;: The specified CR is loaded directly after the start.

The entire Magellan call looks like this: java -jar magellan.jar \[-d directory\] \[-s profiles\] \[-pm\] \[-log \[O|E|W|I\]\] \[--help\] \[CR-file\]
