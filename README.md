# PatientEditor - A solution to the MindLinc challenge

This repo contains a solution to the [MinLinc/Holmusk challenge](https://github.com/Holmusk/.NET-Challenge).

To run the code (demo at https://youtu.be/LPD0d_77pGc).
* Download [PatientEditor.zip](https://github.com/razvanvoicu/PatientEditor/blob/master/BinaryRelease/PatientEditor.zip);
* Unzip it in a fresh folder on the local drive of a Windows 10 machine;
* In the configuration file PatientEditor.exe.config, the connection string needs to be tweaked:
 * change `localhost` to point to the local instance of SQL Server (if not running on `localhost`)
 * change `mindlinc` in `initial catalog=mindlinc` to point to a database on that server
 * change `mindlinc` in `user id=mindlinc` to your user name on that server
 * change `mindlinc123` in `password=mindlinc123` to your password for that user on that server.
 * At this point you may want to use the `PopulateDatabase` app (details below) to initialize the database with some randomly generated test data, so you can have a more thorough testing experience.
* Now double-click on the executable, and the app should start.

To set up the database (you may want to do this before starting the app, as in the instructions given in the [video](https://youtu.be/LPD0d_77pGc) )
* Download the companion app [PopulateDatabase](https://github.com/razvanvoicu/PatientEditor/blob/master/BinaryRelease/PopulateDatabase.zip);
* Unzip it in a fresh folder;
* Tweak the connection string in configuration file `PopulateDatabase.exe.config` to point to your database, and have your credentials;
* Double-click on the executable, which will create the `Patients` table, and populate it with randomly generated test data.

## Building the app

The app was developed with [Visual Studio 2015 Community Edition](https://www.visualstudio.com/en-us/downloads/download-visual-studio-vs.aspx), and can be built by simply loading the solution file into the IDE, and then selecting Build -> Build Solution in the menu.

The solution has 3 projects:
* `PatientEditor`, the app proper;
* `PatientEditorTests`, unit tests for the app proper;
* `PopulateDatabase`, companion app to set up the database with test data.
 
The code is thoroughly commented. It has an event-based architecture, where event brokers are simulated with reactive components.

## Environment

The app was built and tested on:
* OS: Windows 10
* IDE: Visual Studio Community Edition
* .NET target version: 4.5
* SQL Sever Express version: 13.0.1708
