﻿# Disclaimer: You're using these tools at YOUR OWN risk. It's your own fault if you try to cheat and get banned. Always make sure you back up your files before changing them.

Words to live by: Don't be a jerk or rotten apple. Help your fellow modding community members. Play nice.

## PhxGui.exe

This tool is a drag-n-drop style desktop app.
1. Select which game you're messing with. The default is Definitive Edition.
2. Select the folder where files extracted from .ERAs will be put
3. Select the folder where re-built .ERAs, defined by .ERADEFs, will be put
4. Drag-n-drop files:
   A. One or more .ERA files, to extract all their files and generate a .ERADEF file which can be used to rebuild the archive
   - NOTE: when extracting files this tool will not automatically convert .XMB files to .XML, see the next step.

   B. One or more .XMB files, to convert them to .XML files which you can edit
   - If you convert an .XMB file to its editable form, you'll need to update the file reference in the .ERADEF to use the .XML based file, not the .XMB, or you'll never see your changes
   - Alternatively, you can use the "Always build with XML instead of XMB" option and the tool will swap XMB references with the XML file on disk if it finds one. NOT recommended if you mass convert all of your XMB files.

   C. One .ERADEF file, to build a .ERA file with the same name

   D. One .EXE file, which the app will try to patch (after making a backup, with the suffix "_UNTOUCHED") to support loading unofficial .ERA files
   - NOTE: The app tries to figure out how to patch new versions of the game. However, a future up may break the patterns it looks for.
5. Pay attention to the "Messages" UI after any of these processes finish for extra details or error info. The log file, PhxGui.log, can also contain additional diagnostic information.

### Known Issues
- Some errors don't present much details right now. For example, problems reading the .ERADEF while trying to build a new .ERA will just give a basic message saying that the build process failed. This should be addressed in future releases
- The "Halo Wars Alpha 093106 Feb 21 2009" build requires the "Skip Verification" flag to be checked. It was released using old tools that didn't properly update hashes used in the ERA, and all the ERAs are already decrypted.

## PhxTool

This is a CLI style app that resembles the original "KSoft.Tool" app from the 360 modding days. I'm currently leaving this undocumented since PhxGui should address 99% of user needs

## Thank Yous:
- Ensemble Studios. RIP.
- 343i/Creative Assembly for continuing the RTS side of the franchise
- 343i/Behaviour Interactive for working on Definitive Edition
- Halo Wars Modding Group Discord for additional testing (https://discord.gg/Qe6Ka2e)

## Change Log

### 2023.11.25

* 5 Long Years...I forget what all changes may be useful to note in a changelog. Sorry.
* Rebuilt code with Visual Studio 2022, targeting .NET 4.8. Your-Mileage-May-Vary.

### 2018.03.21

* PhxGui: Added support for expanding ECF files into individual chunk files and a .ecfdef (XML) file describing how to rebuild them again
* PhxGui: Added support for building ECF files from a .ecfdef (XML) file along with individual chunk files
* PhxGui: Added support for converting binary_data_tree->binary_data_tree_xml (binary_data_tree is a lot like XMB but is used by UGX files for the materials chunk). Converting from XML->BIN is not yet supported.
* Fixed XMB->XML conversion bug introduced from recent code refactoring and 64-bit support
* Fixed a bug where the ECF or ERA file adler checksum was not updated
* Made ERA build outputs a little more deterministic

### 2017.12.25

* Fixed PhxGui and PhxTool so they run in 64-bit mode instead of always 32-bit
* Fixed code which would otherwise use the current culture when parsing real numbers or compare strings, instead of using an invariant culture
* Added DDS utils to the build chain

### 2017.09.30

* Fixed compatability with the released "Halo Wars Alpha 093106 Feb 21 2009" build. Be sure to check "Skip Verification" to get around checksum errors.

### 2017.06.20

* PhxGui: Added UI for editing ModManifest.txt files
* Fixed Leader.xml support to support recent leader changes in the game to support more than 9 leader options and data drive more configuration values
* Fixed EXE patching to better support recent game updates

### 2017.04.30

* PhxGui: Improved formatting in PhxGui.log, to help interpret errors
* PhxGui: Renamed "Test data load" to "Validate Game Data"
* Fixed a bug in the "Validate Game Data" process where we'd try to load .tactics files before core files (eg, GameData.xml) were fully loaded, leading to false positive errors (eg, 'Supplies' being an undefined resource type)
* Fixed a bug where we would fail to report which file failed to get packed into an ERA and for what reason

### 2017.04.28

* PhxGui: Changed method of EXE patching to try and calculate the required patches instead of hard coding all the patches for each game version
* PhxGui: Added a feature to try and validate some of the core game data in the ERA Expand Path
* PhxGui: Enabled trace logging, information and additional errors can be found in PhxGui.log
* PhxGui: Fixed "Always build with XML instead of XMB" option

### 2017.02.22

* PhxGui: Added "Always build with XML instead of XMB" option
* Extracted files will have their creation/last-write dates set to the time stamp data in the ERA
* Included separate download for pre-built DirectXTex tools (https://github.com/Microsoft/DirectXTex).

### 2017.01.13

* Re-release with Definitive Edition support and GUI tool
