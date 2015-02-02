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
        public static Configuration CreateTestConfiguration()
        {
            var res = ConfigurationHelper.GetStandardConfiguration("");
            res.OutputAsSingleLine = true;
            res.AreEqualsMethod = Assert.AreEqual;

            return res;
        }
    }
}
