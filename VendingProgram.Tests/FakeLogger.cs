using System;
using System.Collections.Generic;
using System.Text;
using VendingProgram;

namespace VendingProgram.Tests
{
    class FakeLogger : IDebugLogger
    {
        public void LogMessage(string message)
        {
            // A fake logger for testing purposes that does nothing.
        }
    }
}
