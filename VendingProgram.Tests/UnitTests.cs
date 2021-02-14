using NUnit.Framework;
using System.Collections.Generic;

namespace VendingProgram.Tests
{
    /// <summary>
    ///  NUnit tests for VendingProgram.
    ///  Note that when run from Visual Studio, the /catalogues/ folder needs to be where the built .exe is created.
    ///  In my test cases this was in '\VendingProgram.Tests\bin\Debug\netcoreapp3.1\' 
    /// </summary>

    public class UnitTests
    {
        private Machine m;
        private IDebugLogger fl;

        [SetUp]
        public void Setup () {
            // Setup machine with a dummy logger for each test
            fl = new FakeLogger();
            m = new Machine("TestMachine", fl);
        }

        #region MoneyTests
        // MONEY TESTS
        [Test]
        public void TestAddMoneyToMachine()
        {
            m.AddMoney(5);

            // Assert if added money is set to decimal and the right amount
            Assert.AreEqual(5.00M, m.CurrentMoney);
        }

        [Test]
        public void TestRemoveMoneyFromMachine()
        {
            m.RemoveMoney(5);

            // Assert if removing money from empty machine results in 0 money being left in the machine.
            Assert.AreEqual(0, m.CurrentMoney);
        }

        [Test]
        public void TestAddRemoveMoney()
        {
            m.AddMoney(10);
            m.RemoveMoney(8.00M);

            // Assert that money is decimal and addition/substraction work
            Assert.That(m.CurrentMoney.Equals(2.00M));
        }

        [Test]
        public void TestAddNegativeMoney()
        {
            m.AddMoney(-5);

            // Assert that machine still has 0 money
            Assert.AreEqual(0, m.CurrentMoney);
        }

        [Test]
        public void TestIfCanRemoveNegativeMoney ()
        {
            m.RemoveMoney(-5);

            // Assert that machine still has 0 money
            Assert.AreEqual(0, m.CurrentMoney);
        }

        #endregion

        #region Purchase Tests
        // PURCHASE TESTS

        [Test]
        public void TestIfPurchaseFailsOnEmptyCatalogue()
        {
            // This should error due to empty catalogue
            m.TryPurchase("D1");

            string expected = "ERROR: Invalid item code!";
            string result = m.lastMessage;

            Assert.AreEqual(result, expected);
        }

        [Test]
        public void TestIfPurchaseSuccessful ()
        {
            m.FillCatalogue();
            m.AddMoney(2);
            m.TryPurchase("D1");
            string expected = "Purchased Silly Cat Juice box!";
            string result = m.lastMessage;

            Assert.AreEqual(expected, result);
        }

        [Test]
        public void TestIfPurchaseFailedInvalidItemID ()
        {
            m.FillCatalogue();  // By default the machine fills with "Drinks" catalogue with only D1-D4 itemIDs
            m.AddMoney(10);
            m.TryPurchase("W1"); // Try to purchase a weapon instead

            string expected = "ERROR: Invalid item code!";
            string result = m.lastMessage;

            Assert.AreEqual(result, expected);
        }
        
        [Test]
        public void TestIfPurchaseFailedNotEnoughMoney ()
        {
            m.FillCatalogue();
            m.TryPurchase("D1");    // Drinks catalogue has D1

            string expected = "ERROR: Not enough money!";
            string result = m.lastMessage;

            Assert.AreEqual(result, expected);
        }

        #endregion

        #region Catalogue Tests
        // CATALOGUE RELATED TESTS

        [Test]
        public void TestIfMachineIsEmptyAtStart ()
        {

            Assert.IsEmpty(m.currentCatalogue);
        } 

        [Test]
        public void TestIfMachineFillsCatalogue()
        {
            m.FillCatalogue();  // By default machine should fill with "Drinks" catalogue, so code D1 should be available.
            Item i = m.GetCurrentItems()["D1"]; // Should be a Silly Cat Juice box

            Assert.IsNotNull(i);
        }

        [Test]
        public void TestIfCatalogueChanges ()
        {
            m.FillCatalogue();  // By default the machine fills with "Drinks" catalogue with only D1-D4 itemIDs
            m.ChangeCatalogue(m.GetCatalogueChoices()[2]);  // Should swap to Mixed catalogue

            Assert.That(m.currentCatalogue.ContainsKey("W1"));  // Should have a weapon available
        }

        [Test]
        public void TestIfItemAmountIsUpdatedOnPurchase()
        {
            m.FillCatalogue();
            m.AddMoney(2);
            int amountBeforePurchase = m.currentCatalogue["D1"].AmountAvailable;
            m.TryPurchase("D1");
            int amountAfterPurchase = m.currentCatalogue["D1"].AmountAvailable;

            Assert.That(amountBeforePurchase == (amountAfterPurchase + 1));
        }

        [Test]
        public void TestIfItemAmountIsUpdated ()
        {
            m.FillCatalogue();
            m.AddMoney(100);

            // Set amount available to 3
            m.currentCatalogue["D1"].AmountAvailable = 3;
            int itemAmount = m.currentCatalogue["D1"].AmountAvailable;

            Assert.AreEqual(3, itemAmount);
        }

        [Test]
        public void TestIfExhaustedItemIsRemoved ()
        {
            m.FillCatalogue();
            m.AddMoney(100);

            // Set amount of item available to 3, then purchase all of the items
            m.currentCatalogue["D1"].AmountAvailable = 3;
            int itemAmount = m.currentCatalogue["D1"].AmountAvailable;

            for (int i = 0; i < itemAmount; i++)
            {
                m.TryPurchase("D1");
            }

            Assert.That(!m.currentCatalogue.ContainsKey("D1"));
        }

        [Test]
        public void TestIfFaultyItemIsRemoved ()
        {
            m.FillCatalogue();
            // Use a catalogue with a single negative price (faulty) item to test if the machine removes it.
            m.ChangeCatalogue(m.GetCatalogueChoices()[3]);

            Assert.IsEmpty(m.currentCatalogue);
        }
        #endregion
        #region Item Tests

        [Test]
        public void TestIfAmountCanBeIncreased ()
        {
            m.FillCatalogue();
            // Amount is normally randomized so we'll save it here
            int beforeAdd = m.currentCatalogue["D1"].AmountAvailable;
            m.currentCatalogue["D1"].AddToAmount(5);
            // Amount should be updated
            int afterAdd = m.currentCatalogue["D1"].AmountAvailable;

            Assert.That(beforeAdd < afterAdd);
        }

        [Test]
        public void TestIfAmountCanBeDecreased ()
        {
            m.FillCatalogue();
            // Amount is normally randomized so we'll save it here
            int beforeDecrease = m.currentCatalogue["D1"].AmountAvailable;
            m.currentCatalogue["D1"].RemoveSingleItem();
            // Amount should be updated
            int afterDecrease = m.currentCatalogue["D1"].AmountAvailable;

            Assert.That(beforeDecrease == afterDecrease + 1);
        }

        #endregion
    }
}