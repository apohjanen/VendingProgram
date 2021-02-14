using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Text.RegularExpressions;

namespace VendingProgram {

    /// Handles reading .csv catalogue files from /catalogues/ and creating new items from catalogue values.
    /// New catalogues can be added by creating a .csv files in the /catalogues/ folder.
    
    class Catalogue {

        private string[] catalogueChoices;
        private string cataloguePath;
        private string path;
        public Dictionary<string, Item> ItemCatalogue;
        private IDebugLogger debugLogger;

        public Catalogue(IDebugLogger logger) {
            debugLogger = logger;
            cataloguePath = Directory.GetCurrentDirectory() + @"\catalogues\";
            catalogueChoices = Directory.GetFiles(cataloguePath);
            debugLogger.LogMessage("Found " + catalogueChoices.Length + " catalogue choices.");
            foreach(string s in catalogueChoices) debugLogger.LogMessage("Catalogue name " + s);
            GetNewCatalogue(catalogueChoices[0]);
        }

        public string[] GetAvailableCatalogues () {
            return catalogueChoices;
        }

        public string GetCurrentCataloguePath () {
            return path;
        }
        
        public Dictionary<string, Item> GetNewCatalogue (string catalogueName) {

            ItemCatalogue = new Dictionary<string, Item>();

            // Set current catalogue path here so we can resupply items from the chosen catalogue.
            path = catalogueName;

            if (FileExists(path)) {
                debugLogger.LogMessage("Generating a new catalogue from " + path);
                using (var reader = new StreamReader(path))
                {
                    while (!reader.EndOfStream)
                    {
                        // Read the items.csv file for items
                        var line = reader.ReadLine();
                        if (line == null) continue;
                        var values = line.Split(',');
                        
                        // Create item and add it to catalogue
                        KeyValuePair<string, Item> itemKeyValue = GenerateItem(values);

                        // Add key-value pair with ItemID as the key and the item as the value 
                        ItemCatalogue.Add(itemKeyValue.Key, itemKeyValue.Value);
                    }
                }
            }
            debugLogger.LogMessage("Catalogue from file '" + path + "' generated with " + ItemCatalogue.Count + " items.");
            return ItemCatalogue;
        }

        private KeyValuePair<string, Item> GenerateItem (string[] values) {
            if (values.Length == 5) {
                string itemID = values[0];
                string itemName = values[1];
                string itemDescription = values[2];
                decimal itemPrice = ConvertStringToDecimal(values[3]);
                string itemType = values[4];

                if (itemPrice <= 0) return GetErrorItem();

                // Randomize the amount of items in stock
                System.Random rnd = new System.Random();
                int randomAmount = rnd.Next(1, 5);

                Item itemToReturn;

                // Check item type specified in catalogue .csv file, create a derived item instance
                switch (itemType) {
                    case "DRINK":
                        itemToReturn = new Drink(itemName, itemDescription, randomAmount, itemPrice);
                        break;
                    case "FOOD":
                        itemToReturn = new Food(itemName, itemDescription, randomAmount, itemPrice);
                        break;
                    case "WEAPON":
                        itemToReturn = new Weapon(itemName, itemDescription, randomAmount, itemPrice);
                        break;
                    default:
                        itemToReturn = new Drink(itemName, itemDescription, randomAmount, itemPrice);
                        break;
                }

                // Create a key value pair to add to the dictionary
                KeyValuePair<string, Item> kvpToReturn = new KeyValuePair<string, Item>(itemID, itemToReturn);
                return kvpToReturn;
            } else {
                debugLogger.LogMessage("Error while creating item from catalogue values! Lenght of values was: " + values.Length + ", expected length is 5.");
                return GetErrorItem();
            }
        }

         public KeyValuePair<string, Item> GetRandomItem () {

            // Read all lines from the catalogue, choose a random one.
            if (FileExists(path)) {

                // Considering our catalogue won't probably be too huge for this program, we can read all lines.
                var linesInFile = File.ReadAllLines(path);
                int lineCount = linesInFile.Length;
                
                if (lineCount > 0) {
                    System.Random rnd = new System.Random();
                    int randomItem = rnd.Next(0, lineCount);
                    var itemLine = linesInFile[randomItem];
                    var values = itemLine.Split(',');
                    
                    return GenerateItem(values);
                } else {
                    debugLogger.LogMessage("Error while generating random item! Catalogue at path '" + path + "' seems to be empty!");
                    return GetErrorItem();
                }
            } else {
                debugLogger.LogMessage("Error while generating random item! Catalogue at path '" + path + "' doesn't exist!");
                return GetErrorItem();
            }
        }

        private decimal ConvertStringToDecimal (string value) {
            // Check decimal separator from culture info
            var cInfo = CultureInfo.InvariantCulture;

            // Use regex to match to US or DE cultures
            if (Regex.IsMatch(value, @"^(:?[\d,]+\.)*\d+$"))
            {
                // US Culture
                cInfo = new CultureInfo("en-US");
            }
            else if (Regex.IsMatch(value, @"^(:?[\d.]+,)*\d+$"))
            {
                // DE Culture
                cInfo = new CultureInfo("de-DE");
            }

            // Create numberstyles for decimal parsing
            NumberStyles styles = NumberStyles.Number;

            // If the price is somehow completely wrong, just make it 1 money.
            decimal finalPrice = 0;

            // Check for invalid price and return a 0 price item. This will be removed from the catalogue by the machine.
            // We don't give out freebies!
            if (!decimal.TryParse(value, styles, cInfo, out decimal decimalValue))
            {
                // Decimal parsing failed, set price to negative
                finalPrice = -1;
            } else
            {
                // Check if price is wrong
                if (decimalValue <= 0) decimalValue = -1;
                else finalPrice = decimalValue;
            }

            return finalPrice;
        }

        private KeyValuePair<string, Item> GetErrorItem () {
            debugLogger.LogMessage("Generating an error item.");
            Food errorFood = new Food("Error", "Vending Machine error, please report this!", -1, -1);
            KeyValuePair<string, Item> kvpToReturn = new KeyValuePair<string, Item>("00", errorFood);
            return kvpToReturn;
        }

        private bool FileExists (string fileName) {
            if (File.Exists(fileName)) {
                return true;
            } else {
                debugLogger.LogMessage("Couldn't find specified catalogue file " + fileName);
                Console.WriteLine("ERROR: Catalogue file {0} doesn't exist!", fileName);
                return false;
            }
        }

    }
}