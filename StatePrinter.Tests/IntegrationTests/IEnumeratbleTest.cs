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

using StatePrinter.OutputFormatters;

namespace StatePrinter.Tests.IntegrationTests
{


    [TestFixture]
    class IEnumeratbleTest
    {


        [TestFixture]
        class Curly
        {
            Stateprinter printer;

            [SetUp]
            public void Setup()
            {
                printer = TestHelper.CreateTestPrinter();
            }


            [Test]
            public void EmptyIntArray()
            {
                Assert.AreEqual("new Int32[]()", printer.PrintObject(new int[0]));
            }



            [Test]
            public void IntArray_oneline()
            {
                printer.Configuration.SetNewlineDefinition(" ");
                printer.Configuration.SetIndentIncrement("");
                printer.Assert.AreEqual("new Int32[]() [0] = 1 [1] = 2 [2] = 3", printer.PrintObject(new[] { 1, 2, 3 }));
            }

            [Test]
            public void IntArray()
            {
                Assert.AreEqual(@"new Int32[]()
[0] = 1
[1] = 2
[2] = 3", printer.PrintObject(new int[] { 1, 2, 3 }));
            }

            [Test]
            public void StringArrayWithNulls()
            {
                Assert.AreEqual(@"new String[]()
[0] = """"
[1] = null
[2] = ""42""", printer.PrintObject(new[] { "", null, "42" }));
            }


            [Test]
            public void Dictionary_person_address()
            {
                var d = MakeDictionary();

                var expected = @"new Dictionary<Person, Address>()
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
}";
                Assert.AreEqual(expected, printer.PrintObject(d));
            }
        }



        [TestFixture]
        class Json
        {
            Stateprinter printer;

            [SetUp]
            public void Setup()
            {
                printer = TestHelper.CreateTestPrinter();
                printer.Configuration.SetOutputFormatter(new JsonStyle(printer.Configuration));
            }


            [Test]
            public void EmptyIntArray()
            {
                printer.Assert.PrintEquals("[]", new int[0]);
            }



            [Test]
            public void IntArray_oneline()
            {
                printer.Configuration.SetNewlineDefinition(" ");
                printer.Configuration.SetIndentIncrement("");
                var expected = " [     { 0 : 1 },     { 1 : 2 },     { 2 : 3 } ]";
                printer.Assert.AreEqual(expected, printer.PrintObject(new[] { 1, 2, 3 }));
            }

            [Test]
            public void IntArray()
            {
                var expected = @"
[
    { 0 : 1 },
    { 1 : 2 },
    { 2 : 3 }
]";
                printer.Assert.PrintEquals(expected, new int[] { 1, 2, 3 });
            }

            [Test]
            public void StringArrayWithNulls()
            {
                var expected = @"
[
    { 0 : """" },
    { 1 : null },
    { 2 : ""42"" }
]";

                printer.Assert.PrintEquals(expected, new[] { "", null, "42" });
            }


            [Test]
            public void Dictionary_person_address()
            {
                var expected = @"
[
    {
        ""key"" :
        {
            ""Age"" : 37,
            ""FirstName"" : ""Klaus"",
            ""LastName"" : ""Meyer""
        }
        ""value"" :
        {
            ""Street"" : ""Fairway Dr."",
            ""StreetNumber"" : 50267,
            ""Zip"" : ""CA 91601"",
            ""Country"" : USA
        }
    }
]";

                printer.Assert.PrintEquals(expected, MakeDictionary());
            }
        }





        [TestFixture]
        class Xml
        {
            Stateprinter printer;

            [SetUp]
            public void Setup()
            {
                printer = TestHelper.CreateTestPrinter();
                printer.Configuration.OutputFormatter = new XmlStyle(printer.Configuration);
            }


            [Test]
            public void EmptyIntArray()
            {

                var expected = @"<ROOT type='Int32[]'>
<Enumeration></Enumeration>"; 
                printer.Assert.PrintEquals(expected, new int[0]);
            }



            [Test]
            public void IntArray_oneline()
            {
                printer.Configuration.SetNewlineDefinition(" ");
                printer.Configuration.SetIndentIncrement("");
                var expected = "<ROOT type='Int32[]'> <ROOT>     <Enumeration>     <key>0</key><value>1</value>     <key>1</key><value>2</value>     <key>2</key><value>3</value>     </Enumeration> </ROOT>";
                printer.Assert.AreEqual(expected, printer.PrintObject(new[] { 1, 2, 3 }));
            }

            [Test]
            public void IntArray()
            {
                var expected = @"<ROOT type='Int32[]'>
<ROOT>
    <Enumeration>
    <key>0</key><value>1</value>
    <key>1</key><value>2</value>
    <key>2</key><value>3</value>
    </Enumeration>
</ROOT>";
                printer.Assert.PrintEquals(expected, new int[] { 1, 2, 3 });
            }

            [Test]
            public void StringArrayWithNulls()
            {
                var expected = @"<ROOT type='String[]'>
<ROOT>
    <Enumeration>
    <key>0</key><value>""""</value>
    <key>1</key><value>null</value>
    <key>2</key><value>""42""</value>
    </Enumeration>
</ROOT>";


                printer.Assert.PrintEquals(expected, new[] { "", null, "42" });
            }



            [Test]
            public void Dictionary_person_address()
            {
                var d = MakeDictionary();

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
</ROOT>";

                printer.Assert.AreEqual(expected, printer.PrintObject(d));
            }


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
            Denmark,

            USA,
        }


        static Dictionary<Person, Address> MakeDictionary()
        {
            return new Dictionary<Person, Address>
                       {
                           {
                               new Person { Age = 37, FirstName = "Klaus", LastName = "Meyer" },
                               new Address()
                                   {
                                       Street = "Fairway Dr.",
                                       StreetNumber = 50267,
                                       Country = Country.USA,
                                       Zip = "CA 91601"
                                   }
                           },
                       };
        }
    }
}
