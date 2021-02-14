using System;

namespace VendingProgram
{
    class Program
    {
        private static IDebugLogger logger;

        static void Main(string[] args)
        {
            // Create a debug logger
            logger = new FileLogger();

            // Create vending machine and turn it on, showing the menu
            // Also inject logger to the machine
            Machine vMachine = new Machine("Vending Extravaganza", logger);
            vMachine.TurnMachineOn();
        }
    }
}
