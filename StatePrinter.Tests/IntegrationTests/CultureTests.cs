using System;
using System.Globalization;
using NUnit.Framework;
using StatePrinter.Configurations;

namespace StatePrinter.Tests.IntegrationTests
{
  [TestFixture]
  class CultureTests
  {
    const decimal decimalNumber = 12345.343M;
    readonly DateTime dateTime = new DateTime(2010, 2, 28, 22, 10, 59);
    
    [Test]
    public void CultureDependentPrinting_us()
    {
      var cfg = ConfigurationHelper.GetStandardConfiguration();
      cfg.Culture = new CultureInfo("en-US");
      var usPrinter = new Stateprinter(cfg);

      Assert.AreEqual("12345.343\r\n", usPrinter.PrintObject(decimalNumber));
      Assert.AreEqual("12345.34\r\n", usPrinter.PrintObject((float)decimalNumber));
      Assert.AreEqual("2/28/2010 10:10:59 PM\r\n", usPrinter.PrintObject(dateTime));
    }

    [Test]
    public void CultureDependentPrinting_dk()
    {
      var cfg = ConfigurationHelper.GetStandardConfiguration();
      cfg.Culture = new CultureInfo("da-DK");
      var dkPrinter = new Stateprinter(cfg);

      Assert.AreEqual("12345,343\r\n", dkPrinter.PrintObject(decimalNumber));
      Assert.AreEqual("12345,34\r\n", dkPrinter.PrintObject((float)decimalNumber));
      Assert.AreEqual("28-02-2010 22:10:59\r\n", dkPrinter.PrintObject(dateTime));
    }
  }
}
