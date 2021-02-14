using System;

namespace VendingProgram
{
    public class Weapon : Item
    {
        public Weapon (string name, string description, int amount, decimal price) {
            base.ItemName = name;
            base.ItemDescription = description;
            base.AmountAvailable = amount;
            base.ItemPrice = price;
        }
    }
}