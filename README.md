# Fingerprint Module for Labview
A Fingerprint Module for Labview
# Log
## *[2019-04-22 Monday]*
- Got familiar with the two samples based on **C#**.
## *[2019-04-23 Tuesday]*
- Created a new solution based on **WPF App (.NET Framework 4)**, while the sample is based on **WinForm**.
- Got start with the WPF.
- Finished the ui of the Enrollment Window by using ***XAML*** language.
- The sample uses the methord of ***Form Inheritance***, while the window based on ***XAML*** cannot be inherited.
- Finished the Function of enrolling a fingerprint in the Enrollment Window.
- Finished the Function of saving ***.fpt*** file by using a ***Save-As Dialog*** in the MainWindow.
### Unsolved Problems:
- Cannot automatically save the ***.fpt*** file with the target name.
## *[2019-04-24 Wednesday]*
- Tried to automatically save the ***.fpt*** file with the target name when clicking the ***Close and Save*** button in the Enrollment Window, but meeting the problem of the System Exception.
### Unsolved Problems:
- Cannot automatically save the ***.fpt*** file with the target name when clicking the ***Close and Save*** button in the Enrollment Window.
## *[2019-04-25 Thursday]*
- Added the source control to the solution.
- Started writing log.
- The sample referred previously used a special Event handler to update event notifications, which would cause an unhandled exception when trying to change the save method.
- Fixed the problem of meeting an unhandled exception.
- Added ***Data.cs*** into the project.
- Finished a function to automatically create a ***data*** Folder and a ***dataLog.txt*** file under the current directory of the ***.exe*** file.
### Unsolved problems:
- More functions on the R/W of dataStream.
## *[2019-04-26 Friday]*
- Finished the function to automatically save ***.fpt*** file and use the username as the filename.
- Finished the whole basic function on the R/W of dataStream.
## *[2019-04-28 Sunday]*
- Finished the function of ***Verification***.
- Found and fixed a problem about the ***dataLog.txt***.
## *[2019-04-29 Monday]*
- Finished a simple communication between a ***C#*** **TCP Server** ***DEMO*** and a ***LabView*** **TCP Client** ***DEMO***.
## *[2019-04-30 Tuesday]*
- 
# To do
- ***TCP Communication*** with LabView.
