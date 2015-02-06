using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;

using StatePrinter.Configurations;

namespace StatePrinter.Tests.TestingAssistance
{
    static class TestHelper
    {

        public static Stateprinter CreateTestPrinter()
        {
            var cfg = ConfigurationHelper.GetStandardConfiguration();
            cfg.SetAreEqualsMethod(Assert.AreEqual);
            
            return new Stateprinter(cfg);
        }

        public static Configuration CreateTestConfiguration()
        {
            var res = ConfigurationHelper
                .GetStandardConfiguration("")
                .SetNewlineDefinition(" ")
                .SetAreEqualsMethod(Assert.AreEqual);

            return res;
        }
    }
}
