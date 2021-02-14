# VendingProgram
A vending machine program with multiple .csv catalogues, simple injectable debug logging interface and NUnit tests.

# Classes
## VendingProgram.csproj
This is the main program. Opens in a console window by using RUN.bat file, or by running it through console with "dotnet run".
### Program.cs
Contains the main method of the program. Creates a debug logger and starts up the VendingMachine.
### IDebugLogger.cs
A simple interface for implementing debug logging.
### FileLogger.cs
A simple text file logger. Can be set to log to a new log using DateTime.Now or a continuous DebugLog.txt file.
### Machine.cs
Handles Machine functionality, such as transactions and catalogue updates.
### Catalogue.cs
Handles reading catalogue files & creating items & catalogues out of the data.
### Item.cs (Drink.cs, Food.cs, Weapon.cs)
An abstract item class that can be used to create new items. Items have a name, description, price and amount available.
### VendingMenu.cs
Handles user interaction and consequent calls to the vending machine functionalities.

## VendingProgram.Tests.csproj
NUnit tests project. Note that when using Visual Studio for Unit testing, the test /catalogues/ folder must be where the .exe file gets built!
### UnitTests.cs
Contains unit tests for the project.
### FakeLogger.cs
A debug logger that does nothing. Used for testing purposes.
