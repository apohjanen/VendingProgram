using System;
using System.Collections.Generic;
using System.IO;

namespace VendingProgram
{

    /// Handles machine related functionality, such as currently available catalogue and money interactions with the catalogue.
    
    public class Machine
    {

        public string MachineName {get; set;}

        // Amount of money currently inserted into the machine
        public decimal CurrentMoney {get; set;}

        // The current catalogue available in this machine
        public Dictionary<string, Item> currentCatalogue = new Dictionary<string, Item>();

        // Catalogue handler to get items to be sold
        private Catalogue catalogueHandler;

        // Logger injected in the constructor
        private IDebugLogger logger;

        public string lastMessage = "Last Message";

        public Machine (string name, IDebugLogger debugLogger) {
            MachineName = name;
            logger = debugLogger;
            
        }

        public void TurnMachineOn () {
            FillCatalogue();

            logger.LogMessage("====== Machine turned on " + DateTime.Now + "======");

            // Create and show menu for interaction
            VendingMenu menu = new VendingMenu(this, logger);
            menu.ShowMenu();
        }

        public void FillCatalogue ()
        {
            WriteToConsole("Filling Machine Catalogue...");
            // Fill the item catalogue for this machine
            catalogueHandler = new Catalogue(logger);
            currentCatalogue = catalogueHandler.ItemCatalogue;
            CheckCatalogueItems();
        }

        public bool AddMoney (decimal amount) {
            if (amount <= 0) {
                logger.LogMessage("Tried to input a zero or negative sum of money into the machine. Input: " + amount);
                return false;
            } else {
                logger.LogMessage("User inserted money into the machine. Input: " + amount + ", CurrentMoney: " + CurrentMoney);
                CurrentMoney += amount;
                return true;
            }
        }

        public void RemoveMoney (decimal amount) {
            if (amount < 0) amount *= -1;
            CurrentMoney -= amount;
            if (CurrentMoney <= 0) CurrentMoney = 0;
            logger.LogMessage("Removed " + amount + " money from the machine. Current money is now " + CurrentMoney);
        }

        public bool TryPurchase (string ItemID) {
            // Check if the item is available in the current catalogue
            
            Console.WriteLine("Processing item purchase...");
            if (currentCatalogue.ContainsKey(ItemID)) {
                decimal itemPrice = currentCatalogue[ItemID].ItemPrice;
                if (CurrentMoney >= itemPrice) {
                    RemoveMoney(itemPrice);

                    //string msg = String.Format("Purchased {0}", currentCatalogue[ItemID].ItemName + "!");
                    WriteToConsole(String.Format("Purchased {0}!", currentCatalogue[ItemID].ItemName));
                    logger.LogMessage("An item was bought: " + currentCatalogue[ItemID].ItemName);

                    // Check if item supply was exhausted
                    bool isExhausted = currentCatalogue[ItemID].RemoveSingleItem();
                    if (isExhausted) {
                        logger.LogMessage("An item was exhausted from the inventory: " + currentCatalogue[ItemID].ItemName);
                        currentCatalogue.Remove(ItemID);
                    }   

                    return true;
                } else {
                    // Didn't have enough money!
                    WriteToConsole("ERROR: Not enough money!");
                    logger.LogMessage("A purchase failed because not enough money was available. Money available: " + CurrentMoney + ", Money needed: " + itemPrice);
                    return false;
                }
            } else {
                // Wrong item code!
                WriteToConsole("ERROR: Invalid item code!");
                logger.LogMessage("A purchase failed because the ItemID was invalid. Input item ID: " + ItemID);

                return false;
            }
        }

        public Dictionary<string, Item> GetCurrentItems () {
            return currentCatalogue;
        }

        public void AddRandomItemsToCatalogue (int count) {
            // Get random items from the current catalogue and add them in.
            // Note these aren't saved anywhere, so loading the catalogue again will reset it!
            int logNewItemsAdded = 0;
            int logStockedItems = 0;
            for (int i = 0; i < count; i++) {
                KeyValuePair<string, Item> newItemKvp = catalogueHandler.GetRandomItem();

                if (currentCatalogue.ContainsKey(newItemKvp.Key)) {
                    // We already have this item on the list, add to the current amount available instead of adding a new item.
                    currentCatalogue[newItemKvp.Key].AddToAmount(newItemKvp.Value.AmountAvailable);
                    string msg = String.Format("Added {2} more stacks to {1}, {0}", newItemKvp.Value.ItemName, newItemKvp.Key, newItemKvp.Value.AmountAvailable);
                    WriteToConsole(msg);
                    logStockedItems++;
                } else {
                    // Add a new item
                    currentCatalogue.Add(newItemKvp.Key, newItemKvp.Value);
                    string msg = String.Format("Added new item {0} with key {1}", newItemKvp.Value.ItemName, newItemKvp.Key);
                    WriteToConsole(msg);
                    logNewItemsAdded++;
                }
            }

            logger.LogMessage("Machine refreshed with " + logNewItemsAdded + " new items. Increased current stock of " + logStockedItems + " existing items.");

        }

         public string[] GetCatalogueChoices () {
             return catalogueHandler.GetAvailableCatalogues();
        }

        public string GetCurrentCataloguePath () {
            return catalogueHandler.GetCurrentCataloguePath();
        }

        public void ChangeCatalogue (string catalogueName) {
            // Clear current catalogue and get a new one
            logger.LogMessage("Trying to get a new catalogue from file " + catalogueName);
            currentCatalogue.Clear();
            currentCatalogue = catalogueHandler.GetNewCatalogue(catalogueName);

            // Remove any errored items from the catalogue
            CheckCatalogueItems();
        } 

        public void CheckCatalogueItems ()
        {
            // Go through received catalogue and remove possible errored items.
            foreach (KeyValuePair<string, Item> kvp in currentCatalogue)
            {
                if (kvp.Value.AmountAvailable <= 0 || kvp.Value.ItemPrice <= 0 || kvp.Key == "00")
                {
                    logger.LogMessage("Removed a broken item (0 value or stock) from the machine with key " + kvp.Key + " and name " + kvp.Value.ItemName + ", price was: " + kvp.Value.ItemPrice + " and stock was " + kvp.Value.AmountAvailable);
                    currentCatalogue.Remove(kvp.Key);
                }
            }
        }

        private void WriteToConsole (string msg)
        {
            lastMessage = msg;
            Console.WriteLine(msg);
        }
    }
}
