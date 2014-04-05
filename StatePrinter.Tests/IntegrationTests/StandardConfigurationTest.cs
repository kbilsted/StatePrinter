// Copyright 2014 Kasper B. Graversen
// 
// Licensed to the Apache Software Foundation (ASF) under one
// or more contributor license agreements.  See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership.  The ASF licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License.  You may obtain a copy of the License at
// 
//   http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied.  See the License for the
// specific language governing permissions and limitations
// under the License.

using System;
using System.Globalization;
using NUnit.Framework;
using StatePrinter.Configurations;

namespace StatePrinter.Tests.IntegrationTests
{
  [TestFixture]
  class StandardConfigurationTests
  {
    readonly StatePrinter printer = new StatePrinter();

    [Test]
    public void NullContent()
    {
      Assert.AreEqual("ROOT = null\r\n", printer.PrintObject(null));
    }

    [Test]
    public void String_empty()
    {
      Assert.AreEqual("ROOT = \"\"\r\n", printer.PrintObject(""));
      Assert.AreEqual("ROOT = \"Some string\"\r\n", printer.PrintObject("Some string"));
    }

    [Test]
    public void Bool()
    {
      Assert.AreEqual("ROOT = True\r\n", printer.PrintObject(true));
      Assert.AreEqual("ROOT = False\r\n", printer.PrintObject(false));
    }
    
    [Test]
    public void Decimal()
    {
      Assert.AreEqual("ROOT = -1\r\n", printer.PrintObject(-1M));
      Assert.AreEqual("ROOT = 3,141592\r\n", printer.PrintObject(3.141592M));
      Assert.AreEqual("ROOT = 1,27E+23\r\n", printer.PrintObject(1.27E23));
    }

    [Test]
    public void Float()
    {
      Assert.AreEqual("ROOT = -1\r\n", printer.PrintObject(-1f));
      Assert.AreEqual("ROOT = 3,141592\r\n", printer.PrintObject(3.141592f));
      Assert.AreEqual("ROOT = 1,27E+23\r\n", printer.PrintObject(1.27E23f));
    }

    [Test]
    public void Int()
    {
      Assert.AreEqual("ROOT = -1\r\n", printer.PrintObject(-1f));
      Assert.AreEqual("ROOT = 3\r\n", printer.PrintObject(3));
      Assert.AreEqual("ROOT = 1E+23\r\n", printer.PrintObject(1E23));
    }

    [Test]
    public void Long()
    {
      Assert.AreEqual("ROOT = -1\r\n", printer.PrintObject(-1L));
      Assert.AreEqual("ROOT = 789328793\r\n", printer.PrintObject(789328793L));
      Assert.AreEqual("ROOT = 789389398328793\r\n", printer.PrintObject(789389398328793)); // outside int -range
    }

    [Test]
    public void GuidTest()
    {
      Assert.AreEqual("ROOT = 00000000-0000-0000-0000-000000000000\r\n", printer.PrintObject(Guid.Empty));
    }

    [Test]
    public void DateTime()
    {
      var dt = new DateTime(2010, 2, 3, 14, 15, 59);
      Assert.AreEqual("ROOT = 03-02-2010 14:15:59\r\n", printer.PrintObject(dt));
    }

    [Test]
    public void DateTimeOffset()
    {
      var dt = new DateTimeOffset(2010, 2, 3, 14, 15, 59, TimeSpan.FromMinutes(1));
      Assert.AreEqual("ROOT = 03-02-2010 14:15:59 +00:01\r\n", printer.PrintObject(dt));
    }

    [Test]
    public void Enum()
    {
      Assert.AreEqual("ROOT = Hearts\r\n", printer.PrintObject(Suit.Hearts));
      Assert.AreEqual("ROOT = Spades\r\n", printer.PrintObject(Suit.Spades));
    }

    enum Suit
    {
      Spades = 1, Hearts = 2
    }

    [Test]
    public void CultureDependentPrinting()
    {
      const decimal decimalNumber = 12345.343M;
      var dateTime = new DateTime(2010, 2, 28, 22, 10, 59);

      var cfg = ConfigurationHelper.GetStandardConfiguration(new CultureInfo("en-US"));
      var printer = new StatePrinter(cfg);

      Assert.AreEqual("ROOT = 12345.343\r\n", printer.PrintObject(decimalNumber));
      Assert.AreEqual("ROOT = 12345.34\r\n", printer.PrintObject((float)decimalNumber));
      Assert.AreEqual("ROOT = 2/28/2010 10:10:59 PM\r\n", printer.PrintObject(dateTime));

      cfg = ConfigurationHelper.GetStandardConfiguration(new CultureInfo("da-DK"));
      printer = new StatePrinter(cfg);
      Assert.AreEqual("ROOT = 12345,343\r\n", printer.PrintObject(decimalNumber));
      Assert.AreEqual("ROOT = 12345,34\r\n", printer.PrintObject((float)decimalNumber));
      Assert.AreEqual("ROOT = 28-02-2010 22:10:59\r\n", printer.PrintObject(dateTime));
    }
  }
}
