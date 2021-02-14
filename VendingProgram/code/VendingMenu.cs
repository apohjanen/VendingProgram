using System;
using System.Collections.Generic;
using System.IO;

namespace VendingProgram {

    /// Handles user interaction with the machine.
    /// Shows different menus and communicates with the machine depending on user choices.

    public class VendingMenu {

        private Machine machine;

        private bool inPurchasesMenu = false;

        private IDebugLogger logger;

        public VendingMenu (Machine machineMenuIsRunningOn, IDebugLogger debugLogger) {
            this.machine = machineMenuIsRunningOn;
            this.logger = debugLogger;
        }

        public void ShowMenu () {
            
            Console.Clear();
            // Show menu until user decides to quit
            while (true) {
                // Show interaction options
                Console.WriteLine("=== Welcome to the {0}, where our prices are low and quality matters! ====", machine.MachineName);
                Console.WriteLine("Please select an option:\n");
                Console.WriteLine("\t1. Show Available Items");
                Console.WriteLine("\t2. Buy An Item");
                Console.WriteLine("\t3. Change catalogue");
                Console.WriteLine("\t4. Exit\n");
              
                // Wait for input
                string input = Console.ReadLine();

                switch (input) {
                    case "1":
                        Console.WriteLine("\nShowing available items!");
                        ShowAvailableItems();
                        break;
                    case "2":
                        Console.WriteLine("\nMoving to purchases section!");
                        inPurchasesMenu = true;
                        ShowPurchasesMenu();
                        break;
                    case "3":
                        Console.WriteLine("\nWhich catalogue would you like to open?");
                        HandleCatalogueMenu();
                        break;
                    case "4":
                        Console.WriteLine("\nThank you for using {0}.", machine.MachineName);
                        Console.ReadLine();
                        ExitProgram();
                        break;
                    default:
                        Console.WriteLine("Please input a correct choice.");
                    break;
                }

                // Clear the console so we don't get spam
                Console.ReadLine();
                Console.Clear();
            }
        }

        private void ShowAvailableItems () {
            Dictionary<string, Item> currentItems = machine.GetCurrentItems();
            string columns = "{0,-5}| {1,-20}\t| {2,-50}\t |{3,5} |{4,9}|";
            Console.WriteLine(columns, "ID", "Name", "Description", "Price", "Available");
            Console.WriteLine("===========================================================================================================");
            foreach (KeyValuePair<string, Item> kvp in currentItems)
            {
                //Console.WriteLine(kvp.Key + "\t" + kvp.Value.ItemName + "\t" + kvp.Value.ItemDescription + "\t" + kvp.Value.ItemPrice + "\t" + kvp.Value.AmountAvailable);
                Console.WriteLine(columns, kvp.Key, kvp.Value.ItemName, kvp.Value.ItemDescription, kvp.Value.ItemPrice, kvp.Value.AmountAvailable);
            }
            Console.WriteLine("===========================================================================================================");
        }

        private void ShowPurchasesMenu () {
            Console.Clear();

            while (inPurchasesMenu) {
                // Show interaction options
                Console.WriteLine("==== Welcome to the purchases menu! ====");
                Console.WriteLine("This is what we currently have for sale:\n");

                ShowAvailableItems();

                Console.WriteLine("\nCURRENT MONEY: {0}", machine.CurrentMoney);
                Console.WriteLine("\nPlease select an option:\n");
                Console.WriteLine("\t1. Buy something");
                Console.WriteLine("\t2. Add money");
                Console.WriteLine("\t3. Refill inventory with new items");
                Console.WriteLine("\t4. Exit Purchases Menu\n");

                // Wait for input
                string input = Console.ReadLine();

                switch (input) {
                    case "1":
                        Console.WriteLine("\nPlease input an item code!");
                        string inputID = Console.ReadLine();
                        HandlePurchaseItem(inputID);
                        break;
                    case "2":
                        Console.WriteLine("\nHow much money would you like to add? Enter nothing to stop adding money.");
                        HandleAddMoney();
                        break;
                    case "3":
                        Console.WriteLine("\nHow many different items would you like to add?");
                        HandleRefill();
                        break;
                    case "4":
                        Console.WriteLine("\nReturning to main menu!");
                        inPurchasesMenu = false;
                        break;
                    default:
                        Console.WriteLine("Please input a choice from 1 to 4.");
                    break;
                }

                // Clear the console so we don't get spam
                if (inPurchasesMenu) {
                    Console.ReadLine();
                    Console.Clear();
                }
            }
        }

        private void HandleCatalogueMenu () {
            string[] catalogues = machine.GetCatalogueChoices();
            Console.WriteLine("Available catalogues: ");
            string curCatalogue = machine.GetCurrentCataloguePath();

            for (int i = 0; i < catalogues.Length; i++) {
                if (curCatalogue.Equals(catalogues[i])) {
                    Console.WriteLine(i+1 + "." + Path.GetFileNameWithoutExtension(catalogues[i]) + "\t[CURRENT]");
                } else {
                    Console.WriteLine(i+1 + "." + Path.GetFileNameWithoutExtension(catalogues[i]));
                }
            }

            bool success = false;
            while (!success) {
                // Get user input
                Console.WriteLine("\n");
                string choice = Console.ReadLine();

                if (!int.TryParse(choice, out int catalogueNo)) {
                    Console.WriteLine("ERROR: Please input a proper number!");
                } else {
                    catalogueNo -= 1;
                    if (catalogueNo < 0 || catalogueNo > catalogues.Length-1) {
                        Console.WriteLine("ERROR: Selection outside of possible catalogues!");
                    } else {
                        // All good, change catalogue!
                        logger.LogMessage("User switched catalogue from " + machine.GetCurrentCataloguePath() + "to" + catalogues[catalogueNo]);
                        Console.WriteLine("Switching catalogue to " + Path.GetFileNameWithoutExtension(catalogues[catalogueNo]) + "!");
                        machine.ChangeCatalogue(catalogues[catalogueNo]);
                        success = true;
                    }
                }
            }
        }

        private void HandlePurchaseItem (string ItemID) {

            bool success = machine.TryPurchase(ItemID.ToUpper());
            if (success) {
                Console.WriteLine("Item succesfully purchased!");
                logger.LogMessage("User purchased an item with ID " + ItemID.ToUpper());
            } else {
                Console.WriteLine("Item could not be purchased.");
            }
        }

        private void HandleAddMoney () {
            bool success = false;
            while (!success) {
                string amount = Console.ReadLine();
                if (amount == "") { 
                    success = true;
                    break;
                    //Console.WriteLine("Press again to stop adding money!");
                } 
                else if (!decimal.TryParse(amount, out decimal amountDecimalOut)) {
                    Console.WriteLine("ERROR: Please insert a proper decimal amount!");
                    string logMsg = "Couldn't add money because given input value " + amount + " was invalid.";
                    logger.LogMessage(logMsg);
                } else {
                    bool addMoneySuccess = machine.AddMoney(amountDecimalOut);
                    if (addMoneySuccess) {
                        Console.WriteLine("Current money in machine: {0}", machine.CurrentMoney);
                    } else {
                        Console.WriteLine("ERROR: Please add a positive amount of money!");
                        string logMsg = "Couldn't add money because given input value " + amount + " was a negative or zero number.";
                        logger.LogMessage(logMsg);
                    }
                }
            }
        }

        private void HandleRefill () {
            bool success = false;

            while (!success) {
                string input = Console.ReadLine();

                if (!int.TryParse(input, out int amount)) {
                    // Invalid or non-integer input!
                    Console.WriteLine("ERROR: Please input a proper number!");
                    logger.LogMessage("Couldn't refill inventory because the given item amount " + input + " was invalid.");
                } else {
                    // Proper integer input, check for amount errors
                    if (amount < 0) {
                        Console.WriteLine("\nPlease enter a non-zero & non-negative number!");
                    }
                    else if (amount == 0) {
                        Console.WriteLine("\nCanceling item supplies!");
                    } else {
                        Console.WriteLine("\nMagically refilling inventory with {0} new items!", amount);
                        machine.AddRandomItemsToCatalogue(amount);
                        success = true;
                    }
                }
            }
        }

        private void ExitProgram () {
            logger.LogMessage("User exited the program.");
            Environment.Exit(0);
        }
    }
}