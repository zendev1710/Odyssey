# FAQ

Frequently Asked Questions:

## 1\. Questions about the installation

1. [What is required to start Magellan?](#Installation)
2. [Even though I have enough RAM, Magellan reports a lack of memory. Why?](#Speichermangel)

## 2\. Questions about Magellan

1. [How to run ECheck?](#ECheck)
2. [What is this § stuff in the ARR, ATR, or the tooltips even doing!?](#ARR)

## 2\. Questions about Java

1. [What is Java and why is it important?](#Java)
2. [What Java version, exactly, should I download?](#Javaversion)
3. [JRE or JDK?](#jdk)
4. [What version of Java do I have?](#myversion)
5. [II get this error message: 'Error: A JNI Error has occured, please check your installation and try again.'](#JNI_ERROR)
6. [But the latest version on java.com is Java 8](#java8)
7. [But I need Java 8 for a different software. What can I do?](#needJava8)

## 4\. Questions about older Magellan versions

1. [What does "Installer JAR" mean?](#Installer)
2. [magellan-client.jar? Shouldn't the file be called magellan.exe?](#Dateiname)
3. [Why does another program (e.g. WinZip or PowerArchiver) start, when I try to start Magellan by double-clicking?](#Verknuepfung)
4. [Why doesn't Magellan start when I click the file magellan-client.jar?](#keinStart)
5. [Even though I have enough RAM, Magellan reports a lack of memory. Why?](#Speichermangel_legacy)
6. [How do I make Magellan the default program for CR-files under Windows2000/XP?](#CRVerknuepfung)
7. [How do I start Magellan from the command prompt?](#Kommandozeilenstart)
8. [How do I get Magellan to run on a Mac?](#MacMagellan)
9. [How do I handle resource paths?](#Resourcenpfade)
10. [Why does Magellan all of a sudden not use Base-36 numbers for the units anymore?](#base36nummern)
11. [How can I use Vorlage to automatically confirm the orders of units?](#VorlageBefehlsbestaetigung)

## Answers

### Q: What is required to start Magellan?

**A:** Since version 2.1, only a single file is required to download to install Magellan. You can find the right Version on the [Magellan homepage](https://magellan2.github.io) in the download section. You will find versions for different operation systems (Windows, Linux, MacOS). You will likely need administrator rights to install Magellan under Windows. This will install all you need including a [Java](#Java) version. Updates from older versions should work without issues, but there may be problems with older Plugins. In this case you must disable those plugins or install a newer version.

If you have installed Java already, you may also just download and unpack a zip file and start Magellan directly (preferably using magellan.bat or magellan.sh). But this method is not recommended or supported officially.

### Q: What is Java and why is it important?

**A:** Java is the programming language used to write Magellan. The special thing about it is that it runs on many platforms (Windows, Linux, Mac, ...). Since version 2.1, Magellan comes with its own version of java. No need to require any additional thing.

### What Java version, exactly, should I download?

**A:** Earlier versions of Magellan required you to install Java first. One of the easiest ways to install Java right now is to download an installer from [AdoptOpenJDK](https://adoptopenjdk.net/releases.html). There are several Java distributions. We recommend, for example, [Open JDK](https://openjdk.java.net/) oder [Oracle Java SE](https://www.oracle.com/java/).

At this moment (summer 2021) we recommend the following version, that should work for most users:

* AdoptOpenJDK
* You operating, naturally (Windows for most)
* OpenJDK 11 (LTS), or, by autumn 2021, OpenJDK 17 (LTS)
* HotSpot JVM
* usually x64

### JRE or JDK?

JDK is usually targeted at Java developers, JRE at end users. If you intend to use the ExtendedCommands of Magellan, you will need a JDK! The only disadvantage of getting a JDK is its size. So if you are not running short on disc space, a JDK is recommended.

### What version of Java do I have?

1. Open a terminal (command line):
    * **On Windows:** Press the windows key to open the start menu. Type cmd to open the command line.
    * **On MacOsX:** Open Spotlight (Command + Space or click on the magnifying glass in the upper right), then enter 'terminal'.
    * **On Linux:** Open a terminal (depending on your Linux distribution open the program menu, for example by pressing the windows key and enter "terminal").
2. In the terminal type `java -version` und press `Enter`. An output like `'openjdk version "11.0.10" 2021-04-20'` means that you are running version 11. An output like "java is not recognized" or "command not found" means that you apparently don't have java.

### Q: I get this error message: 'Error: A JNI Error has occured, please check your installation and try again.'

**A:**You probably have Java 8 instead of Java 11! Install Java 11 (see above).

### But the latest version on java.com is Java 8

Since Oracle acquired Java from Sun Microsystems, there is a new license system that complicated the release of Java implementations for end users. Therefore [java.com](https://java.com) is no longer the site of choice. Please get Java from one of the sources mentioned above.

### I need Java 8 for a different software

You can use several versions of java simultaneously, but this requires some work.

**On Windows:**

1. Install Java 11, then Java 8. Your other software should now work normally.
2. Locate the path of your Java 11 installation (for example C:\\Program Files\\jdk-11.0.1).
3. Locate the file magellan.bat (usually in C:\\Program Files\\Magellan\\magellan.bat).
4. Edit this file as administrator. This should work roughly as follows: Locate the entry for "notepad" in the start menu. Right click it and choose "More ... Run as administrator".
5. Open the file magellan.bat in the notepad.
6. Add this line at the start of the file (adapt it to your path from step 2):  
    `SET JAVA_HOME=C:\Program Files\jdk-11.0.1`
7. Save the file magellan.bat.
8. Magellan should now be started with Java 11.

**On Linux:**

1. Install sowohl 11 as well as Java 8.
2. Execute this command in a terminal: `update-alternatives --config java`. Choose the java version you need for your other program.
3. Remember the path to Java 11 displayed by the previous command (for instance, `/usr/lib/jvm/java-11-openjkd-amd64`).
4. Locate the file magellan.sh (usually at $HOME/Magellan/magellan.sh).
5. Edit this file as follows: Add this line at the start of the file (adapted to the path recorded in step 3 above):  
    `export JAVA_HOME=/usr/lib/jvm/java-11-openjdk-amd64`
6. Change the last line from  
    `java -Xmx1200m -jar "magellan-client.jar" "$@"` to  
    `$JAVA_HOME/bin/java -Xmx1200m -jar "magellan-client.jar" "$@"`.
7. Save the file magellan.sh.
8. Magellan should now be executed with Java 11.

### Q: Even though I have enough RAM, Magellan reports a lack of memory. Why?

**A:** Due to the architecture of Java, Magellan cannot always allocate as much memory as it needs. In the installation directory of Magellan (on Windows this is usually C:\\Program Files\\Magellan ) there is a file called magellan\_launcher.vmoptions . You should edit this file (this may require administrative rights) and add a line like \-Xmx1G . This tells Java to allocate up to 1 Gigabyte of memory for Magellan.

It should usually be no problem to give up to half of your memory or more to Magellan. Don't worry: Magellan will always only allocate as much memory as it requires. So if your machine has 4 gigabytes of RAM, a setting of \-Xmx2G should be fine. If your whole system freezes on loading a big report, you may want to reduce this value and you can try to reduce the size of your report by not loading the whole map or not adding all your allies' reports. The MemoryWatch plugin from the Magellan homepage may provide further insights.

For other ways to allocate memory, especially for older versions of Magellan, see [this section](#Speichermangel_legacy).

### Q: How do i run ECheck?

**A:**First of all: Why do you think you need ECheck? Virtually all functions of ECheck are fulfilled by Magellan itself. Syntax errors detected by ECheck, and some more, are detected by Magellan and are highlighted in the orders. Additional errors are shown in the [open problems](docks/problems.html) dock. You can configure which (potential) problems to show and which you would rather ignore.

If you really want to run ECheck, you can do this from the [ECheck dock](docks/echeck.html). Magellan comes with its own ECheck version and is usually configured to use it, so that you only have to click on "Run". If this should not work, maybe because you are using settings from an earlier version, you can configure this in the [options dialog](menus/extras/options_resources.html) under "Resources". Just set the resource path to point to your ECheck installation (something like C:\\Program Files\\Magellan\\echeck\\echeck.exe ).

Should you experience funny symbols or messages like "Unknown order: N?CHSTER", there is likely a problem with text encoding. You can adjust the according setting in the [Options](menus/extras/options_system.html#Textkodierung).

### Q: What does "Installer JAR" mean?

**A:** Starting from Version 2, Magellan does not consist of just one file any more. We have divided it into several files. In order to keep the installation of Magellan simple, we have packaged the program into one file called "Installer JAR". It's a Java program, just like Magellan itself. It unpacks its content to a location you can specify during installation.  
Starting from version 2 it is also possible to copy a new version over an old version without first de-installing the old version. The configuration files are conserved and adjusted when you start magellan the next time.

### Q: magellan-client.jar? Shouldn't the file be called magellan.exe?

**A:** No. To be honest, it is (almost) nothing more than a renamed ZIP-file, but one that Java can handle and that you therefore don't need to unpack with WinZip or a similar program. Magellan really consists of a lot of files that are simply gathered within magellan-client.jar, but Magellan should start when the JAR-file is double-clicked.

### Q: Why does another program (e.g. WinZip or PowerArchiver) start, when I try to start Magellan by double-clicking?

**A:** Because it is set to be the default program for the .jar extension instead of Java. In PowerArchiver and WinZip you can undo that in these programs' options, otherwise some manual action is needed:

1. In the Start menu click 'Run'
2. Type 'regedit' and click OK
3. In the tree on the left click the 'HKEY\_CLASSES\_ROOT' entry and click on the entry '.jar'
4. In the window on the right, doubleclick on '(Default)'
5. Type jarfile and click OK
6. Close the regedit program - done.

### Q: Why doesn't Magellan start when I click the file magellan-client.jar?

**A:** A possible cause for this is that the file magellan-client.jar is in a folder with a name that has spaces in it. This isn't a problem that is caused by an error in Magellan, but by an inconvenience in the default link between .jar -files with java.

The simple solution is of course to move Magellan to a different folder. The complex solution is as follows:

1. In the Start menu click 'Run'
2. Type 'regedit' and click OK
3. In the tree on the left click the 'HKEY\_CLASSES\_ROOT' entry and click on the entry 'jarfile'
4. Open the subentries 'shell', 'open' and finally 'command'
5. In the window on the right, doubleclick '(Default)', and something like (path to Java)\\javaw.exe -jar %1 should appear.
6. Replace %1 with "%1"
7. Close the regedit program - done.

Background: If Magellan is in C:\\My Documents\\magellan-client.jar , then the regedit entry would try to initiate java to call (Path to Java)\\javaw.exe -jar C:\\My Documents\\magellan-client.jar , which means that java would try to run the file C:\\My with parameters Documents\\magellan-client.jar , which doesn't quite work out right. With the quotes this looks like this: (Path to java)\\javaw.exe -jar "C:\\My Documents\\magellan-client.jar" , and here the actual magellan-client.jar file is started.

If this didn't help you can try to [start Magellan from the command prompt](#4) to be able to read possible error messages.

### Q: How do I make Magellan the default program for CR-files under Windows2000/XP?

**A:** To do this you need to set up a link between Magellan and the "CR" filetype. You can do this in Explorer under Extras, Folderoptions, Filetypes:

1. Choose filetype "CR"
2. Click the "Advanced" button
3. In the "Edit filetype" window click the "New" button
4. In the "Action" field, enter Magellan
5. In the "Program for this action" field enter "(Path to java)\\javaw.exe" -jar "(Path to magellan)\\magellan-client.jar" "%1". javaw.exe and magellan-client.jar have to be entered with the complete path (e.g. "c:\\games\\eressea\\magellan-client.jar"). If the path contains spaces, the surrounding quotes are mandatory.
6. Click "OK"
7. In the "Edit filetype" window choose the "Magellan" entry from the list "Actions".
8. Click the "As standard" button. The Magellan entry is now shown in bold font.
9. Done :-)

### Q: How do I start Magellan from the command prompt?

**A:** Magellan (or Java) often only gives error messages at the command prompt, therefore it can be useful to start Magellan from here.

To do this you first start the command prompt (in Windows ME under Start menu open 'Run', type command and click OK, under Windows 2000/XP the command is cmd ). Then enter the following command: javaw -jar "(Path to Magellan)\\magellan-client.jar" , so e.g. javaw -jar "C:\\My Documents\\magellan-client.jar" .

The command prompt parameters for Magellan are listed in the [Reference](reference/commandline.html).

### Q: How do I get Magellan to run on a Mac?

**A:** Unfortunately Apple has only with the Mac OS X just released a current Java-version that will support Magellan. To run Magellan use the console the same as under Windows at the [command prompt](#4).

### Q: How do I handle resource paths?

**A:** Resource paths are described on [their own site](reference/resources.html).

### Q: Even though I have enough RAM, Magellan reports a lack of memory. Why?

**A:** Simply allocate a bit more memory for the Java Virtual Machine (VM). Magellan requests memory from the VM, and when VM does not have any to give, Magellan simply fails.

You can allocate memory as follows:

Edit the text file magellan.bat (under Windows) or magellan.sh (Linux and Mac) and change all occurences of the form \-Xmx000m to a larger value, for example \-Xmx2000M , to allocate about 2 Gigabytes for Magellan.

_or_

Under Windows:  
Create an icon for Magellan on your desktop, right-click on it and go to "Properties". Under "Target" enter the following (the path should of course match your particular setup):  
C:\\Program Files\\Java\\jre6\\bin\\javaw.exe -Xms128M -Xmx512M -jar "c:\\Program Files\\Eressea\\Magellan\\magellan-client.jar" or simply  
javaw -Xms128M -Xmx512M -jar "c:\\Program Files\\Eressea\\Magellan\\magellan-client.jar"

Under Linux simply type (in the shell, in the magellan directory):  
java -Xms128M -Xmx512M -jar magellan-client.jar

This tells the Java-VM to allocate a minimum of 128MB and a maximum of 512MB. You can change these values according to your own setup.

### Q: Why does Magellan all of a sudden not use Base-36 numbers for the units anymore?

**A:**Most likely the 36;Basis tag is missing in the CR. This basic tag defines the number base that Magellan uses. For Eressea-CRs this usually is 36, other PBeMs using older CR-versions (e.g. Verdanon) use decimal (base 10) and don't know this tag. That's why Magellan assumes base 10 when there is no mention of another number base in the CR.

### Q: How can I use Vorlage to automatically confirm the orders of units?

**A:** With // #tag EINHEIT ejcOrdersConfirmed 1

### Q: What is this § stuff in the ARR, ATR, or the tooltips even doing!?

**A:** A good help to ARR, ATR and the Tooltips can be found [in the Magellan help.](reference/atr_arr.html)
