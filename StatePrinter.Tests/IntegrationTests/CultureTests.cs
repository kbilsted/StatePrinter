// Copyright 2014-2015 Kasper B. Graversen
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
    class CultureTests
    {
        const decimal DecimalNumber = 12345.343M;
        readonly DateTime dateTime = new DateTime(2010, 2, 28, 22, 10, 59);

        [Test]
        public void CultureDependentPrinting_us()
        {
            var cfg = ConfigurationHelper.GetStandardConfiguration();
            cfg.Culture = new CultureInfo("en-US");
            var usPrinter = new Stateprinter(cfg);

            Assert.AreEqual("12345.343\r\n", usPrinter.PrintObject(DecimalNumber));
            Assert.AreEqual("12345.34\r\n", usPrinter.PrintObject((float)DecimalNumber));
            Assert.AreEqual("2/28/2010 10:10:59 PM\r\n", usPrinter.PrintObject(dateTime));
        }

        [Test]
        public void CultureDependentPrinting_dk()
        {
            var cfg = ConfigurationHelper.GetStandardConfiguration();
            cfg.Culture = new CultureInfo("da-DK");
            var dkPrinter = new Stateprinter(cfg);

            Assert.AreEqual("12345,343\r\n", dkPrinter.PrintObject(DecimalNumber));
            Assert.AreEqual("12345,34\r\n", dkPrinter.PrintObject((float)DecimalNumber));
            Assert.AreEqual("28-02-2010 22:10:59\r\n", dkPrinter.PrintObject(dateTime));
        }
    }
}
