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
using NUnit.Framework;
using StatePrinting;
using StatePrinting.OutputFormatters;

namespace StatePrinter.Tests.IntegrationTests
{


    [TestFixture]
    class StatePrinterTest
    {
        [Test]
        public void Instantiate()
        {
            Assert.Throws<ArgumentNullException>(() => new Stateprinter(null));
            Assert.Throws<ArgumentNullException>(() => new StatePrinter(null));
            
        }
    }

    [TestFixture]
    class StandardConfigurationTests
    {
        Stateprinter curly;
        Stateprinter json;
        Stateprinter xml;

        [SetUp]
        public void Setup()
        {
            curly = TestHelper.CreateTestPrinter();
            json = TestHelper.CreateTestPrinter();
            json.Configuration.SetOutputFormatter(new JsonStyle(json.Configuration));
            xml = TestHelper.CreateTestPrinter();
            xml.Configuration.SetOutputFormatter(new XmlStyle(xml.Configuration));
        }

        [Test]
        public void NullContent()
        {
            curly.Assert.PrintEquals("null", null);
            json.Assert.PrintEquals("null", null);
            xml.Assert.PrintEquals("<Root>null</Root>", null);

            Assert.AreEqual("a = null", curly.PrintObject(null, "a"));
            Assert.AreEqual("\"a\": null", json.PrintObject(null, "a"));
            Assert.AreEqual("<a>null</a>", xml.PrintObject(null, "a"));
        }


        [Test]
        public void String_empty()
        {
            curly.Assert.PrintEquals("\"\"", "");
            json.Assert.PrintEquals("\"\"", "");
            xml.Assert.PrintEquals("<Root></Root>", "");
        }

        [Test]
        public void String()
        {
            curly.Assert.PrintEquals("\"Some string\"", "Some string");
            json.Assert.PrintEquals("\"Some string\"", "Some string");
            xml.Assert.PrintEquals("<Root>Some string</Root>", "Some string");
        }


        [Test]
        public void Bool_with_root()
        {
            Assert.AreEqual("Root = True", curly.PrintObject(true, "Root"));
            Assert.AreEqual("Ref = False", curly.PrintObject(false, "Ref"));
        }

        [Test]
        public void Bool()
        {
            Assert.AreEqual("True", curly.PrintObject(true));
            Assert.AreEqual("False", curly.PrintObject(false));
        }

        [Test]
        public void Decimal()
        {
            Assert.AreEqual("-1", curly.PrintObject(-1M));
            Assert.AreEqual("3,141592", curly.PrintObject(3.141592M));
            Assert.AreEqual("1,27E+23", curly.PrintObject(1.27E23));
        }

        [Test]
        public void Float()
        {
            Assert.AreEqual("-1", curly.PrintObject(-1f));
            Assert.AreEqual("3,141592", curly.PrintObject(3.141592f));
            Assert.AreEqual("1,27E+23", curly.PrintObject(1.27E23f));
        }

        [Test]
        public void Int()
        {
            Assert.AreEqual("-1", curly.PrintObject(-1));
            Assert.AreEqual("-1", json.PrintObject(-1));

            Assert.AreEqual("3", curly.PrintObject(3));
            Assert.AreEqual("3", json.PrintObject(3));

            Assert.AreEqual("1E+23", curly.PrintObject(1E23));
            Assert.AreEqual("1E+23", json.PrintObject(1E23));
        }


        [Test]
        public void Int_xml()
        {
            Assert.AreEqual("<Root>-1</Root>", xml.PrintObject(-1f));
            Assert.AreEqual("<Root>3</Root>", xml.PrintObject(3));
            Assert.AreEqual("<Root>1E+23</Root>", xml.PrintObject(1E23));
        }


        [Test]
        public void Long()
        {
            Assert.AreEqual("-1", curly.PrintObject(-1L));
            Assert.AreEqual("789328793", curly.PrintObject(789328793L));
            Assert.AreEqual("789389398328793", curly.PrintObject(789389398328793)); // outside int -range
        }

        [Test]
        public void GuidTest()
        {
            Assert.AreEqual("00000000-0000-0000-0000-000000000000", curly.PrintObject(Guid.Empty));
            Assert.AreEqual("00000000-0000-0000-0000-000000000000", json.PrintObject(Guid.Empty));
        }

        [Test]
        public void DateTime()
        {
            var dt = new DateTime(2010, 2, 3, 14, 15, 59);
            Assert.AreEqual("03-02-2010 14:15:59", curly.PrintObject(dt));
        }

        [Test]
        public void DateTimeOffset()
        {
            var dt = new DateTimeOffset(2010, 2, 3, 14, 15, 59, TimeSpan.FromMinutes(1));
            Assert.AreEqual("03-02-2010 14:15:59 +00:01", curly.PrintObject(dt));
        }

        [Test]
        public void Enum()
        {
            Assert.AreEqual("Hearts", curly.PrintObject(Suit.Hearts));
            Assert.AreEqual("Spades", curly.PrintObject(Suit.Spades));
        }

        enum Suit
        {
            Spades = 1, Hearts = 2
        }
    }
}
