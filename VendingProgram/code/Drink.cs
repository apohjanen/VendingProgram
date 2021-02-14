using System;

namespace VendingProgram
{
    class Drink : Item
    {
        public Drink (string name, string description, int amount, decimal price) {
            base.ItemName = name;
            base.ItemDescription = description;
            base.AmountAvailable = amount;
            base.ItemPrice = price;
        }
    }
}