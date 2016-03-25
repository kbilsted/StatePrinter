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
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;

using StatePrinter.OutputFormatters;
using StatePrinter.Tests.Introspection;

namespace StatePrinter.Tests.IntegrationTests
{
    [TestFixture]
    class DictionaryTest
    {
        static readonly Dictionary<int, int> empty_int_int = new Dictionary<int, int>();
        static readonly Dictionary<int, int> int_int = new Dictionary<int, int> { { 1, 2 }, { 2, 4 }, { 3, 6 } };
        static readonly Dictionary<string, string> string_string = new Dictionary<string, string> {
            { "abc", "ABC" }, { "xyz", "XYZ" } };

        static readonly IDictionary untyped_int_int = new Hashtable { { 1, 2 }, { 2, 4 }, { 3, 6 } };

        static readonly Dictionary<Person, int> complexKeySimpleValue = new Dictionary<Person, int>();

        static DictionaryTest() 
        {
            complexKeySimpleValue.Add(new Person() { Name = "Douglas" }, 42);
            complexKeySimpleValue.Add(new Person() { Name = "Santa" }, 100);
        }


        class Holder
        {
            public readonly Dictionary<int, int> int_int = new Dictionary<int, int> { { 1, 2 }, { 2, 4 }, { 3, 6 } };
            public readonly IDictionary untyped_int_int = new Hashtable { { 1, 2 }, { 2, 4 }, { 3, 6 } };
        }

        class Person
        {
            public string Name;
        }


        [TestFixture]
        class Tokens
        {
            Stateprinter printer;

            TokenOutputter tokenOutputter;

            [SetUp]
            public void Setup()
            {
                printer = TestHelper.CreateTestPrinter();
                tokenOutputter = new TokenOutputter();
                printer.Configuration.OutputFormatter = tokenOutputter;
            }


            [Test]
            public void Dictionary_empty_int_int_with_rootName()
            {
                var expected = @"new List<Token>()
{
    [0] = new Token()
    {
        Tokenkind = FieldnameWithTypeAndReference
        FieldType = new RuntimeType()
        {
        }
        Field = new Field()
        {
            Name = null
            Key = null
            Index = null
        }
        Value = null
        ReferenceNo = new Reference()
        {
            Number = 0
        }
    }
    [1] = new Token()
    {
        Tokenkind = StartDict
        FieldType = null
        Field = null
        Value = null
        ReferenceNo = null
    }
    [2] = new Token()
    {
        Tokenkind = EndDict
        FieldType = null
        Field = null
        Value = null
        ReferenceNo = null
    }
}";


                printer.PrintObject(empty_int_int);
                var tokens = tokenOutputter.IntrospectedTokens;
                TestHelper.Assert().PrintEquals(expected, tokens);
            }


            [Test]
            public void Dictionary_int_int()
            {
                printer.PrintObject(int_int);
                var tokens = tokenOutputter.IntrospectedTokens;

                var expected = @"new List<Token>()
{
    [0] = new Token()
    {
        Tokenkind = FieldnameWithTypeAndReference
        FieldType = new RuntimeType()
        {
        }
        Field = new Field()
        {
            Name = null
            Key = null
            Index = null
        }
        Value = null
        ReferenceNo = new Reference()
        {
            Number = 0
        }
    }
    [1] = new Token()
    {
        Tokenkind = StartDict
        FieldType = null
        Field = null
        Value = null
        ReferenceNo = null
    }
    [2] = new Token()
    {
        Tokenkind = SimpleFieldValue
        FieldType = null
        Field = new Field()
        {
            Name = null
            Key = ""1""
            Index = null
        }
        Value = ""2""
        ReferenceNo = null
    }
    [3] = new Token()
    {
        Tokenkind = SimpleFieldValue
        FieldType = null
        Field = new Field()
        {
            Name = null
            Key = ""2""
            Index = null
        }
        Value = ""4""
        ReferenceNo = null
    }
    [4] = new Token()
    {
        Tokenkind = SimpleFieldValue
        FieldType = null
        Field = new Field()
        {
            Name = null
            Key = ""3""
            Index = null
        }
        Value = ""6""
        ReferenceNo = null
    }
    [5] = new Token()
    {
        Tokenkind = EndDict
        FieldType = null
        Field = null
        Value = null
        ReferenceNo = null
    }
}";
                TestHelper.Assert().PrintEquals(expected, tokens);
            }




            [Test]
            public void Dictionary_insideOtherObject()
            {
                var expected = @"new List<Token>()
{
    [0] = new Token()
    {
        Tokenkind = FieldnameWithTypeAndReference
        FieldType = new RuntimeType()
        {
        }
        Field = new Field()
        {
            Name = null
            Key = null
            Index = null
        }
        Value = null
        ReferenceNo = new Reference()
        {
            Number = 0
        }
    }
    [1] = new Token(), ref: 0
    {
        Tokenkind = StartScope
        FieldType = null
        Field = null
        Value = null
        ReferenceNo = null
    }
    [2] = new Token()
    {
        Tokenkind = FieldnameWithTypeAndReference
        FieldType = new RuntimeType()
        {
        }
        Field = new Field()
        {
            Name = ""int_int""
            Key = null
            Index = null
        }
        Value = null
        ReferenceNo = new Reference()
        {
            Number = 1
        }
    }
    [3] = new Token()
    {
        Tokenkind = StartDict
        FieldType = null
        Field = null
        Value = null
        ReferenceNo = null
    }
    [4] = new Token()
    {
        Tokenkind = SimpleFieldValue
        FieldType = null
        Field = new Field()
        {
            Name = ""int_int""
            Key = ""1""
            Index = null
        }
        Value = ""2""
        ReferenceNo = null
    }
    [5] = new Token()
    {
        Tokenkind = SimpleFieldValue
        FieldType = null
        Field = new Field()
        {
            Name = ""int_int""
            Key = ""2""
            Index = null
        }
        Value = ""4""
        ReferenceNo = null
    }
    [6] = new Token()
    {
        Tokenkind = SimpleFieldValue
        FieldType = null
        Field = new Field()
        {
            Name = ""int_int""
            Key = ""3""
            Index = null
        }
        Value = ""6""
        ReferenceNo = null
    }
    [7] = new Token()
    {
        Tokenkind = EndDict
        FieldType = null
        Field = null
        Value = null
        ReferenceNo = null
    }
    [8] = new Token()
    {
        Tokenkind = FieldnameWithTypeAndReference
        FieldType = new RuntimeType()
        {
        }
        Field = new Field()
        {
            Name = ""untyped_int_int""
            Key = null
            Index = null
        }
        Value = null
        ReferenceNo = new Reference()
        {
            Number = 2
        }
    }
    [9] = new Token()
    {
        Tokenkind = StartList
        FieldType = null
        Field = null
        Value = null
        ReferenceNo = null
    }
    [10] = new Token()
    {
        Tokenkind = FieldnameWithTypeAndReference
        FieldType = new RuntimeType(), ref: 1
        {
        }
        Field = new Field()
        {
            Name = ""untyped_int_int""
            Key = null
            Index = 0
        }
        Value = null
        ReferenceNo = new Reference()
        {
            Number = 3
        }
    }
    [11] = -> 0
    [12] = new Token()
    {
        Tokenkind = SimpleFieldValue
        FieldType = null
        Field = new Field(), ref: 2
        {
            Name = ""_key""
            Key = null
            Index = null
        }
        Value = ""3""
        ReferenceNo = null
    }
    [13] = new Token()
    {
        Tokenkind = SimpleFieldValue
        FieldType = null
        Field = new Field(), ref: 3
        {
            Name = ""_value""
            Key = null
            Index = null
        }
        Value = ""6""
        ReferenceNo = null
    }
    [14] = new Token(), ref: 4
    {
        Tokenkind = EndScope
        FieldType = null
        Field = null
        Value = null
        ReferenceNo = null
    }
    [15] = new Token()
    {
        Tokenkind = FieldnameWithTypeAndReference
        FieldType = -> 1
        Field = new Field()
        {
            Name = ""untyped_int_int""
            Key = null
            Index = 1
        }
        Value = null
        ReferenceNo = new Reference()
        {
            Number = 4
        }
    }
    [16] = -> 0
    [17] = new Token()
    {
        Tokenkind = SimpleFieldValue
        FieldType = null
        Field = -> 2
        Value = ""2""
        ReferenceNo = null
    }
    [18] = new Token()
    {
        Tokenkind = SimpleFieldValue
        FieldType = null
        Field = -> 3
        Value = ""4""
        ReferenceNo = null
    }
    [19] = -> 4
    [20] = new Token()
    {
        Tokenkind = FieldnameWithTypeAndReference
        FieldType = -> 1
        Field = new Field()
        {
            Name = ""untyped_int_int""
            Key = null
            Index = 2
        }
        Value = null
        ReferenceNo = new Reference()
        {
            Number = 5
        }
    }
    [21] = -> 0
    [22] = new Token()
    {
        Tokenkind = SimpleFieldValue
        FieldType = null
        Field = -> 2
        Value = ""1""
        ReferenceNo = null
    }
    [23] = new Token()
    {
        Tokenkind = SimpleFieldValue
        FieldType = null
        Field = -> 3
        Value = ""2""
        ReferenceNo = null
    }
    [24] = -> 4
    [25] = new Token()
    {
        Tokenkind = EndList
        FieldType = null
        Field = null
        Value = null
        ReferenceNo = null
    }
    [26] = -> 4
}";
                printer.PrintObject(new Holder());
                var tokens = tokenOutputter.IntrospectedTokens;
                TestHelper.Assert().PrintEquals(expected, tokens);
            }
        }


        [TestFixture]
        class CurlyStyle
        {
            Stateprinter printer;

            [SetUp]
            public void Setup()
            {
                printer = TestHelper.CreateTestPrinter();
                printer.Configuration.OutputFormatter = new CurlyBraceStyle(printer.Configuration);
            }


            [Test]
            public void Dictionary_int_int()
            {
                var expected = @"new Dictionary<Int32, Int32>()
{
    [1] = 2
    [2] = 4
    [3] = 6
}";
                printer.Assert.PrintEquals(expected, int_int);
            }

            [Test]
            public void Dictionary_string_string()
            {
                var expected = @"new Dictionary<String, String>()
{
    [""abc""] = ""ABC""
    [""xyz""] = ""XYZ""
}";
                printer.Assert.PrintEquals(expected, string_string);
            }


            [Test]
            public void Dictionary_int_int_withRootName()
            {
                var expected = @"IntDict = new Dictionary<Int32, Int32>()
{
    [1] = 2
    [2] = 4
    [3] = 6
}";

                printer.Assert.AreEqual(expected, printer.PrintObject(int_int, "IntDict"));
            }


            [Test]
            public void Dictionary_empty_int_int()
            {
                var expected = @"new Dictionary<Int32, Int32>()
{
}";
                printer.Assert.PrintEquals(expected, empty_int_int);
            }

            [Test]
            public void Dictionary_empty_int_int_with_rootName()
            {
                var expected = @"start = new Dictionary<Int32, Int32>()
{
}";
                printer.Assert.AreEqual(expected, printer.PrintObject(empty_int_int, "start"));
            }

            [Test]
            public void Dictionary_complexKeySimpleValue()
            {
                var expected = @"new Dictionary<Person, Int32>()
{
    [0] = new KeyValuePair<Person, Int32>()
    {
        key = new Person()
        {
            Name = ""Douglas""
        }
        value = 42
    }
    [1] = new KeyValuePair<Person, Int32>()
    {
        key = new Person()
        {
            Name = ""Santa""
        }
        value = 100
    }
}";
                printer.Assert.PrintEquals(expected, complexKeySimpleValue);
            }


            [Test]
            public void IDictionary_untyped_int_int()
            {
                var expected = @"new Hashtable()
{
    [0] = new DictionaryEntry()
    {
        _key = 3
        _value = 6
    }
    [1] = new DictionaryEntry()
    {
        _key = 2
        _value = 4
    }
    [2] = new DictionaryEntry()
    {
        _key = 1
        _value = 2
    }
}";
                printer.Assert.PrintEquals(expected, untyped_int_int);
            }

            [Test]
            public void Dictionary_insideOtherObject()
            {
                var expected = @"new Holder()
{
    int_int = new Dictionary<Int32, Int32>()
    {
        [1] = 2
        [2] = 4
        [3] = 6
    }
    untyped_int_int = new Hashtable()
    {
        [0] = new DictionaryEntry()
        {
            _key = 3
            _value = 6
        }
        [1] = new DictionaryEntry()
        {
            _key = 2
            _value = 4
        }
        [2] = new DictionaryEntry()
        {
            _key = 1
            _value = 2
        }
    }
}";
                printer.Assert.PrintEquals(expected, new Holder());
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
            public void Dictionary_int_int()
            {
                var expected = @"{
    ""1"": 2,
    ""2"": 4,
    ""3"": 6
}";
                printer.Assert.PrintEquals(expected, int_int);
            }

            [Test]
            public void Dictionary_string_string()
            {
                var expected = @"{
    ""abc"": ""ABC"",
    ""xyz"": ""XYZ""
}";
                printer.Assert.PrintEquals(expected, string_string);
            }

            [Test]
            public void Dictionary_int_int_withRootName()
            {

                var expected = @"""IntDict"": {
    ""1"": 2,
    ""2"": 4,
    ""3"": 6
}";
                printer.Assert.AreEqual(expected, printer.PrintObject(int_int, "IntDict"));
            }


            [Test]
            public void Dictionary_empty_int_int()
            {
                var expected = "{}";
                printer.Assert.PrintEquals(expected, empty_int_int);
            }

            [Test]
            public void Dictionary_empty_int_int_with_rootName()
            {
                var expected = @"""start"": {}";
                printer.Assert.AreEqual(expected, printer.PrintObject(empty_int_int, "start"));
            }


            [Test]
            public void Dictionary_complexKeySimpleValue()
            {
                var expected = @"[
    {
        ""key"": {
            ""Name"": ""Douglas""
        },
        ""value"": 42
    },
    {
        ""key"": {
            ""Name"": ""Santa""
        },
        ""value"": 100
    }
]";

                printer.Assert.PrintEquals(expected, complexKeySimpleValue);
            }

            [Test]
            public void IDictionary_untyped_int_int()
            {
                var expected = @"[
    {
        ""_key"": 3,
        ""_value"": 6
    },
    {
        ""_key"": 2,
        ""_value"": 4
    },
    {
        ""_key"": 1,
        ""_value"": 2
    }
]";
                printer.Assert.PrintEquals(expected, untyped_int_int);
            }


            [Test]
            public void Dictionary_insideOtherObject()
            {
                var expected = @"{
    ""int_int"": {
        ""1"": 2,
        ""2"": 4,
        ""3"": 6
    },
    ""untyped_int_int"": [
        {
            ""_key"": 3,
            ""_value"": 6
        },
        {
            ""_key"": 2,
            ""_value"": 4
        },
        {
            ""_key"": 1,
            ""_value"": 2
        }
    ]
}";
                printer.Assert.PrintEquals(expected, new Holder());
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
                printer.Configuration.SetOutputFormatter(new XmlStyle(printer.Configuration));
            }



            [Test]
            public void Dictionary_int_int()
            {
                var expected = @"<Root type='Dictionary(Int32, Int32)'>
    <Element key='1'>2</Element>
    <Element key='2'>4</Element>
    <Element key='3'>6</Element>
</Root>";
                printer.Assert.PrintEquals(expected, int_int);
            }

            [Test]
            public void Dictionary_string_string()
            {
                var expected = @"<Root type='Dictionary(String, String)'>
    <Element key='abc'>ABC</Element>
    <Element key='xyz'>XYZ</Element>
</Root>";
                printer.Assert.PrintEquals(expected, string_string);
            }

            [Test]
            public void Dictionary_int_int_withRootName()
            {
                var expected = @"<IntDict type='Dictionary(Int32, Int32)'>
    <Element key='1'>2</Element>
    <Element key='2'>4</Element>
    <Element key='3'>6</Element>
</IntDict>";

                printer.Assert.AreEqual(expected, printer.PrintObject(int_int, "IntDict"));
            }


            [Test]
            public void Dictionary_empty_int_int()
            {
                var expected = @"<Root type='Dictionary(Int32, Int32)'>
</Root>";
                printer.Assert.PrintEquals(expected, empty_int_int);
            }

            [Test]
            public void Dictionary_empty_int_int_with_rootName() // TODO rootname mangler -se også andre test med rootname - meld dette som et issue og commit en anden dag
            {
                var expected = @"<start type='Dictionary(Int32, Int32)'>
</start>";
                printer.Assert.AreEqual(expected, printer.PrintObject(empty_int_int, "start"));
            }


            [Test]
            public void Dictionary_complexKeySimpleValue()
            {
                var expected = @"<Root type='Dictionary(Person, Int32)'>
    <Element type='KeyValuePair(Person, Int32)'>
        <key type='Person'>
            <Name>Douglas</Name>
        </key>
        <value>42</value>
    </Element>
    <Element type='KeyValuePair(Person, Int32)'>
        <key type='Person'>
            <Name>Santa</Name>
        </key>
        <value>100</value>
    </Element>
</Root>";
                printer.Assert.PrintEquals(expected, complexKeySimpleValue);
            }

            [Test]
            public void IDictionary_untyped_int_int()
            {
                var expected = @"<Root type='Hashtable'>
    <Element type='DictionaryEntry'>
        <_key>3</_key>
        <_value>6</_value>
    </Element>
    <Element type='DictionaryEntry'>
        <_key>2</_key>
        <_value>4</_value>
    </Element>
    <Element type='DictionaryEntry'>
        <_key>1</_key>
        <_value>2</_value>
    </Element>
</Root>";
                printer.Assert.PrintEquals(expected, untyped_int_int);
            }


            [Test]
            public void Dictionary_insideOtherObject()
            {
                var expected = @"<Root type='Holder'>
    <int_int type='Dictionary(Int32, Int32)'>
        <Element key='1'>2</Element>
        <Element key='2'>4</Element>
        <Element key='3'>6</Element>
    </int_int>
    <untyped_int_int type='Hashtable'>
        <Element type='DictionaryEntry'>
            <_key>3</_key>
            <_value>6</_value>
        </Element>
        <Element type='DictionaryEntry'>
            <_key>2</_key>
            <_value>4</_value>
        </Element>
        <Element type='DictionaryEntry'>
            <_key>1</_key>
            <_value>2</_value>
        </Element>
    </untyped_int_int>
</Root>";
                printer.Assert.PrintEquals(expected, new Holder());
            }
        }
    }
}
