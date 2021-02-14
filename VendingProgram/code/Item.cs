using System;

namespace VendingProgram
{

    /// Base class for available items.

    public abstract class Item
    {
        public string ItemName {get; set;}
        public string ItemDescription {get; set;}
        public int AmountAvailable  {get; set;}
        public decimal ItemPrice  {get; set;}

        public Item () {}

        public Item (string name, string description, int amount, decimal price) {
            this.ItemName = name;
            this.ItemDescription = description;
            this.AmountAvailable = amount;
            this.ItemPrice = price;
        }

        public void AddToAmount (int amountToAdd) {
            // Simply adds to available amount
            AmountAvailable += amountToAdd;
        }

        public bool RemoveSingleItem () {
            AmountAvailable -= 1;
            if (AmountAvailable <= 0) {
                // This item was exhausted, return true to remove it from the catalogue
                return true;
            } else {
                // Items still available, return false to keep it in the catalogue
                return false;
            }
        }
    }
}
