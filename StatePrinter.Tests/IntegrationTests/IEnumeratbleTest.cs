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
using StatePrinting.OutputFormatters;

namespace StatePrinting.Tests.IntegrationTests
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
                var expected = @"new Int32[]()
{
}";
                printer.Assert.PrintEquals(expected, new int[0]);
            }



            [Test]
            public void IntArray_oneline()
            {
                printer.Configuration.SetNewlineDefinition(" ");
                printer.Configuration.SetIndentIncrement("");
                var expected = "new Int32[]() { [0] = 1 [1] = 2 [2] = 3 }";
                printer.Assert.PrintEquals(expected, new[] { 1, 2, 3 });
            }

            [Test]
            public void IntArray()
            {
                var expected = @"new Int32[]()
{
    [0] = 1
    [1] = 2
    [2] = 3
}";
                printer.Assert.PrintEquals(expected, new int[] { 1, 2, 3 });
            }

            [Test]
            public void StringArrayWithNulls()
            {
                var expected = @"new String[]()
{
    [0] = """"
    [1] = null
    [2] = ""42""
}";

                printer.Assert.PrintEquals(expected, new[] { "", null, "42" });
            }


            [Test]
            public void Dictionary_person_address()
            {
                var d = MakeDictionary();

                var expected = @"new Dictionary<Person, Address>()
{
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
}";
                printer.Assert.PrintEquals(expected, d);
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
                var expected = "[ 1, 2, 3 ]";
                printer.Assert.PrintEquals(expected, new[] { 1, 2, 3 });
            }

            [Test]
            public void IntArray()
            {
                var expected = @"[
    1,
    2,
    3
]";
                printer.Assert.PrintEquals(expected, new int[] { 1, 2, 3 });
            }

            [Test]
            public void StringArrayWithNulls()
            {
                var expected = @"[
    """",
    null,
    ""42""
]";

                printer.Assert.PrintEquals(expected, new[] { "", null, "42" });
            }


            [Test]
            public void Dictionary_person_address()
            {
                var expected = @"[
    {
        ""key"": {
            ""Age"": 37,
            ""FirstName"": ""Klaus"",
            ""LastName"": ""Meyer""
        },
        ""value"": {
            ""Street"": ""Fairway Dr."",
            ""StreetNumber"": 50267,
            ""Zip"": ""CA 91601"",
            ""Country"": USA
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

                var expected = @"<Root type='Int32[]'>
</Root>";
                printer.Assert.PrintEquals(expected, new int[0]);
            }



            [Test]
            public void IntArray_oneline()
            {
                printer.Configuration.SetNewlineDefinition(" ");
                printer.Configuration.SetIndentIncrement("");
                var expected = "<Root type='Int32[]'> <Element>1</Element> <Element>2</Element> <Element>3</Element> </Root>";
                printer.Assert.PrintEquals(expected, new[] { 1, 2, 3 });
            }

            [Test]
            public void IntArray()
            {
                var expected = @"<Root type='Int32[]'>
    <Element>1</Element>
    <Element>2</Element>
    <Element>3</Element>
</Root>";
                printer.Assert.PrintEquals(expected, new int[] { 1, 2, 3 });
            }

            [Test]
            public void StringArrayWithNulls()
            {
                var expected = @"<Root type='String[]'>
    <Element></Element>
    <Element>null</Element>
    <Element>42</Element>
</Root>";

                printer.Assert.PrintEquals(expected, new[] { "", null, "42" });
            }



            [Test]
            public void Dictionary_person_address()
            {
                var d = MakeDictionary();

                var expected = @"<Root type='Dictionary(Person, Address)'>
    <Element type='KeyValuePair(Person, Address)'>
        <key type='Person'>
            <Age>37</Age>
            <FirstName>Klaus</FirstName>
            <LastName>Meyer</LastName>
        </key>
        <value type='Address'>
            <Street>Fairway Dr.</Street>
            <StreetNumber>50267</StreetNumber>
            <Zip>CA 91601</Zip>
            <Country>USA</Country>
        </value>
    </Element>
</Root>";

                printer.Assert.PrintEquals(expected, d);
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
