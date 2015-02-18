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

using System.Collections.Generic;
using NUnit.Framework;
using StatePrinter.Configurations;
using StatePrinter.OutputFormatters;

namespace StatePrinter.Tests.IntegrationTests
{
    [TestFixture]
    class IEnumeratbleTest
    {
        Stateprinter printer;

        [SetUp]
        public void Setup()
        {
            var cfg = ConfigurationHelper.GetStandardConfiguration();
            cfg.OutputFormatter = new CurlyBraceStyle(cfg);
            cfg.SetAreEqualsMethod(Assert.AreEqual);
            printer = new Stateprinter(cfg);
        }


        [Test]
        public void EmptyIntArray()
        {
            Assert.AreEqual("new Int32[]()\r\n", printer.PrintObject(new int[0]));
        }


        [Test]
        public void EmptyIntArray_json()
        {
            var cfg = ConfigurationHelper.GetStandardConfiguration();
            cfg.OutputFormatter = new JsonStyle(cfg);

            printer = new Stateprinter(cfg);
            Assert.AreEqual("[]\r\n", printer.PrintObject(new int[0]));
        }

        [Test]
        public void IntArray_oneline()
        {
            printer.Configuration.SetNewlineDefinition(" ");
            printer.Assert.AreEqual("new Int32[]() [0] = 1 [1] = 2 [2] = 3", printer.PrintObject(new[] { 1, 2, 3 }));
        }

        [Test]
        public void IntArray()
        {
            Assert.AreEqual(
      @"new Int32[]()
[0] = 1
[1] = 2
[2] = 3
", printer.PrintObject(new int[] { 1, 2, 3 }));
        }

        [Test]
        public void StringArrayWithNulls()
        {
            Assert.AreEqual(
      @"new String[]()
[0] = """"
[1] = null
[2] = ""42""
", printer.PrintObject(new[] { "", null, "42" }));
        }


        [Test]
        public void Dictionary_person_address()
        {
            var d = new Dictionary<Person, Address>
              {
                {
                  new Person {Age = 37, FirstName = "Klaus", LastName = "Meyer"},
                  new Address() {Street = "Fairway Dr.", StreetNumber = 50267, Country = Country.USA, Zip = "CA 91601"}
                },
              };

            var expected =
      @"new Dictionary<Person, Address>()
[0] = new KeyValuePair<Person, Address>()
{
    key = new Person()
    {
        Age = 37
        FirstName = ""Klaus""
        LastName = ""Meyer""
    }
    value = new Address()
    {
        Street = ""Fairway Dr.""
        StreetNumber = 50267
        Zip = ""CA 91601""
        Country = USA
    }
}
";
            Assert.AreEqual(expected, printer.PrintObject(d));
        }

        [Test]
        public void Xml_Dictionary_person_address()
        {
            printer = TestHelper.CreateTestPrinter();
            printer.Configuration.OutputFormatter = new XmlStyle(printer.Configuration);

            var d = new Dictionary<Person, Address>
              {
                {
                  new Person {Age = 37, FirstName = "Klaus", LastName = "Meyer"},
                  new Address() {Street = "Fairway Dr.", StreetNumber = 50267, Country = Country.USA, Zip = "CA 91601"}
                },
              };

            var expected = @"<ROOT type='Dictionary(Person, Address)'>
    <Enumeration>
    <ROOT type='KeyValuePair(Person, Address)'>
        <key type='Person'>
            <Age>37</Age>
            <FirstName>""Klaus""</FirstName>
            <LastName>""Meyer""</LastName>
        </key>
        <value type='Address'>
            <Street>""Fairway Dr.""</Street>
            <StreetNumber>50267</StreetNumber>
            <Zip>""CA 91601""</Zip>
            <Country>USA</Country>
        </value>
    </ROOT>
    </Enumeration>
</ROOT>
";

            printer.Assert.AreEqual(expected, printer.PrintObject(d));
        }


        class Person
        {
            public int Age;
            public string FirstName, LastName;
        }


        class Address
        {
            public string Street;
            public int StreetNumber;
            public string Zip;
            public Country Country;
        }


        enum Country
        {
            Denmark, USA,
        }
    }
}
