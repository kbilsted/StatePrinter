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
[0] = new Token()
{
    Tokenkind = StartEnumeration
    FieldType = null
    Field = null
    Value = null
    ReferenceNo = null
}
[1] = new Token()
{
    Tokenkind = EndEnumeration
    FieldType = null
    Field = null
    Value = null
    ReferenceNo = null
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
[0] = new Token()
{
    Tokenkind = StartEnumeration
    FieldType = null
    Field = null
    Value = null
    ReferenceNo = null
}
[1] = new Token()
{
    Tokenkind = SimpleFieldValue
    FieldType = null
    Field = new Field()
    {
        Name = null
        SimpleKeyInArrayOrDictionary = ""1""
    }
    Value = ""2""
    ReferenceNo = null
}
[2] = new Token()
{
    Tokenkind = SimpleFieldValue
    FieldType = null
    Field = new Field()
    {
        Name = null
        SimpleKeyInArrayOrDictionary = ""2""
    }
    Value = ""4""
    ReferenceNo = null
}
[3] = new Token()
{
    Tokenkind = SimpleFieldValue
    FieldType = null
    Field = new Field()
    {
        Name = null
        SimpleKeyInArrayOrDictionary = ""3""
    }
    Value = ""6""
    ReferenceNo = null
}
[4] = new Token()
{
    Tokenkind = EndEnumeration
    FieldType = null
    Field = null
    Value = null
    ReferenceNo = null
}";
                TestHelper.Assert().PrintEquals(expected, tokens);
            }




            [Test]
            public void Dictionary_insideOtherObject()
            {
                var expected = @"new List<Token>()
[0] = new Token()
{
    Tokenkind = FieldnameWithTypeAndReference
    FieldType = new RuntimeType()
    {
    }
    Field = new Field()
    {
        Name = null
        SimpleKeyInArrayOrDictionary = null
    }
    Value = null
    ReferenceNo = new Reference()
    {
        Number = 0
    }
}
[1] = new Token(), ref: 1
{
    Tokenkind = StartScope
    FieldType = null
    Field = null
    Value = null
    ReferenceNo = null
}
[2] = new Token(), ref: 0
{
    Tokenkind = StartEnumeration
    FieldType = null
    Field = null
    Value = null
    ReferenceNo = null
}
[3] = new Token()
{
    Tokenkind = SimpleFieldValue
    FieldType = null
    Field = new Field()
    {
        Name = ""int_int""
        SimpleKeyInArrayOrDictionary = ""1""
    }
    Value = ""2""
    ReferenceNo = null
}
[4] = new Token()
{
    Tokenkind = SimpleFieldValue
    FieldType = null
    Field = new Field()
    {
        Name = ""int_int""
        SimpleKeyInArrayOrDictionary = ""2""
    }
    Value = ""4""
    ReferenceNo = null
}
[5] = new Token()
{
    Tokenkind = SimpleFieldValue
    FieldType = null
    Field = new Field()
    {
        Name = ""int_int""
        SimpleKeyInArrayOrDictionary = ""3""
    }
    Value = ""6""
    ReferenceNo = null
}
[6] = new Token(), ref: 6
{
    Tokenkind = EndEnumeration
    FieldType = null
    Field = null
    Value = null
    ReferenceNo = null
}
[7] = new Token()
{
    Tokenkind = FieldnameWithTypeAndReference
    FieldType = new RuntimeType()
    {
    }
    Field = new Field()
    {
        Name = ""untyped_int_int""
        SimpleKeyInArrayOrDictionary = null
    }
    Value = null
    ReferenceNo = new Reference()
    {
        Number = 2
    }
}
[8] =  -> 0
[9] = new Token()
{
    Tokenkind = FieldnameWithTypeAndReference
    FieldType = new RuntimeType(), ref: 2
    {
    }
    Field = new Field()
    {
        Name = ""untyped_int_int""
        SimpleKeyInArrayOrDictionary = ""0""
    }
    Value = null
    ReferenceNo = new Reference()
    {
        Number = 3
    }
}
[10] =  -> 1
[11] = new Token()
{
    Tokenkind = SimpleFieldValue
    FieldType = null
    Field = new Field(), ref: 3
    {
        Name = ""_key""
        SimpleKeyInArrayOrDictionary = null
    }
    Value = ""3""
    ReferenceNo = null
}
[12] = new Token()
{
    Tokenkind = SimpleFieldValue
    FieldType = null
    Field = new Field(), ref: 4
    {
        Name = ""_value""
        SimpleKeyInArrayOrDictionary = null
    }
    Value = ""6""
    ReferenceNo = null
}
[13] = new Token(), ref: 5
{
    Tokenkind = EndScope
    FieldType = null
    Field = null
    Value = null
    ReferenceNo = null
}
[14] = new Token()
{
    Tokenkind = FieldnameWithTypeAndReference
    FieldType =  -> 2
    Field = new Field()
    {
        Name = ""untyped_int_int""
        SimpleKeyInArrayOrDictionary = ""1""
    }
    Value = null
    ReferenceNo = new Reference()
    {
        Number = 4
    }
}
[15] =  -> 1
[16] = new Token()
{
    Tokenkind = SimpleFieldValue
    FieldType = null
    Field =  -> 3
    Value = ""2""
    ReferenceNo = null
}
[17] = new Token()
{
    Tokenkind = SimpleFieldValue
    FieldType = null
    Field =  -> 4
    Value = ""4""
    ReferenceNo = null
}
[18] =  -> 5
[19] = new Token()
{
    Tokenkind = FieldnameWithTypeAndReference
    FieldType =  -> 2
    Field = new Field()
    {
        Name = ""untyped_int_int""
        SimpleKeyInArrayOrDictionary = ""2""
    }
    Value = null
    ReferenceNo = new Reference()
    {
        Number = 5
    }
}
[20] =  -> 1
[21] = new Token()
{
    Tokenkind = SimpleFieldValue
    FieldType = null
    Field =  -> 3
    Value = ""1""
    ReferenceNo = null
}
[22] = new Token()
{
    Tokenkind = SimpleFieldValue
    FieldType = null
    Field =  -> 4
    Value = ""2""
    ReferenceNo = null
}
[23] =  -> 5
[24] =  -> 6
[25] =  -> 5";
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
                var expected = @"[1] = 2
[2] = 4
[3] = 6";
                printer.Assert.PrintEquals(expected, int_int);
            }


            [Test]
            public void Dictionary_int_int_withRootName()
            {
                var expected = @"IntDict[1] = 2
IntDict[2] = 4
IntDict[3] = 6";

                printer.Assert.AreEqual(expected, printer.PrintObject(int_int, "IntDict"));
            }


            [Test]
            public void Dictionary_empty_int_int()
            {
                printer.Assert.PrintEquals(@"", empty_int_int);
            }

            [Test]
            public void Dictionary_empty_int_int_with_rootName()
            {
                printer.Assert.AreEqual("", printer.PrintObject(empty_int_int, "start"));
            }

            [Test]
            public void Dictionary_complexKeySimpleValue()
            {
                var expected = @"new Dictionary<Person, Int32>()
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
}";
                printer.Assert.PrintEquals(expected, complexKeySimpleValue);
            }


            [Test]
            public void IDictionary_untyped_int_int()
            {
                Assert.AreEqual(@"new Hashtable()
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
}", printer.PrintObject(untyped_int_int));
            }

            [Test]
            public void Dictionary_insideOtherObject()
            {
                var expected = @"new Holder()
{
    int_int[1] = 2
    int_int[2] = 4
    int_int[3] = 6
    untyped_int_int = new Hashtable()
    untyped_int_int[0] = new DictionaryEntry()
    {
        _key = 3
        _value = 6
    }
    untyped_int_int[1] = new DictionaryEntry()
    {
        _key = 2
        _value = 4
    }
    untyped_int_int[2] = new DictionaryEntry()
    {
        _key = 1
        _value = 2
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
                var expected = @"[
    { 1 : 2 },
    { 2 : 4 },
    { 3 : 6 }
]";
                printer.Assert.PrintEquals(expected, int_int);
            }
   
            [Test]
            public void Dictionary_int_int_withRootName()
            {

                var expected = @"""IntDict"" : [
    { 1 : 2 },
    { 2 : 4 },
    { 3 : 6 }
]";
                printer.Assert.AreEqual(expected, printer.PrintObject(int_int, "IntDict"));
            }


            [Test]
            public void Dictionary_empty_int_int()
            {
                var expected = @"[]";
                printer.Assert.PrintEquals(expected, empty_int_int);
            }

            [Test]
            public void Dictionary_empty_int_int_with_rootName()
            {
                printer.Assert.AreEqual("[]", printer.PrintObject(empty_int_int, "start"));
            }


            [Test]
            public void Dictionary_complexKeySimpleValue()
            {
                var expected = @"
[
    {
        ""key"" :
        {
            ""Name"" : ""Douglas""
        }
        ""value"" : 42
    }
    {
        ""key"" :
        {
            ""Name"" : ""Santa""
        }
        ""value"" : 100
    }
]";

                printer.Assert.PrintEquals(expected, complexKeySimpleValue);
            }

            [Test]
            public void IDictionary_untyped_int_int()
            {
                var expected = @"
[
    {
        ""_key"" : 3,
        ""_value"" : 6
    }
    {
        ""_key"" : 2,
        ""_value"" : 4
    }
    {
        ""_key"" : 1,
        ""_value"" : 2
    }
]";
                printer.Assert.PrintEquals(expected, untyped_int_int);
            }


            [Test]
            public void Dictionary_insideOtherObject()
            {
                var expected = @"
{
    ""int_int"" : [
        { 1 : 2 },
        { 2 : 4 },
        { 3 : 6 }
    ],
    ""untyped_int_int"" :
    [
        {
            ""_key"" : 3,
            ""_value"" : 6
        }
        {
            ""_key"" : 2,
            ""_value"" : 4
        }
        {
            ""_key"" : 1,
            ""_value"" : 2
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
                var expected = @"<ROOT>
    <Enumeration>
    <key>1</key><value>2</value>
    <key>2</key><value>4</value>
    <key>3</key><value>6</value>
    </Enumeration>
</ROOT>";
                printer.Assert.PrintEquals(expected, int_int);
            }

            [Test]
            public void Dictionary_int_int_withRootName()
            {
                var expected = @"<IntDict>
    <Enumeration>
    <key>1</key><value>2</value>
    <key>2</key><value>4</value>
    <key>3</key><value>6</value>
    </Enumeration>
</IntDict>";

                printer.Assert.AreEqual(expected, printer.PrintObject(int_int, "IntDict"));
            }


            [Test]
            public void Dictionary_empty_int_int()
            {
                var expected = "<Enumeration></Enumeration>";
                printer.Assert.PrintEquals(expected, empty_int_int);
            }

            [Test]
            public void Dictionary_empty_int_int_with_rootName() // TODO rootname mangler -se også andre test med rootname - meld dette som et issue og commit en anden dag
            {
                var expected = "<Enumeration></Enumeration>";
                printer.Assert.AreEqual(expected, printer.PrintObject(empty_int_int, "start"));
            }


            [Test]
            public void Dictionary_complexKeySimpleValue()
            {
                var expected = @"<ROOT type='Dictionary(Person, Int32)'>
    <Enumeration>
    <ROOT type='KeyValuePair(Person, Int32)'>
        <key type='Person'>
            <Name>""Douglas""</Name>
        </key>
        <value>42</value>
    </ROOT>
    <ROOT type='KeyValuePair(Person, Int32)'>
        <key type='Person'>
            <Name>""Santa""</Name>
        </key>
        <value>100</value>
    </ROOT>
    </Enumeration>
</ROOT>";
                printer.Assert.PrintEquals(expected, complexKeySimpleValue);
            }

            [Test]
            public void IDictionary_untyped_int_int()
            {
                var expected = @"<ROOT type='Hashtable'>
    <Enumeration>
    <ROOT type='DictionaryEntry'>
        <_key>3</_key>
        <_value>6</_value>
    </ROOT>
    <ROOT type='DictionaryEntry'>
        <_key>2</_key>
        <_value>4</_value>
    </ROOT>
    <ROOT type='DictionaryEntry'>
        <_key>1</_key>
        <_value>2</_value>
    </ROOT>
    </Enumeration>
</ROOT>";
                printer.Assert.PrintEquals(expected, untyped_int_int);
            }


            [Test]
            public void Dictionary_insideOtherObject()
            {
                var expected = @"<ROOT type='Holder'>
    <int_int>
        <Enumeration>
        <key>1</key><value>2</value>
        <key>2</key><value>4</value>
        <key>3</key><value>6</value>
        </Enumeration>
    </int_int>
    <untyped_int_int type='Hashtable'>
        <Enumeration>
        <untyped_int_int type='DictionaryEntry'>
            <_key>3</_key>
            <_value>6</_value>
        </untyped_int_int>
        <untyped_int_int type='DictionaryEntry'>
            <_key>2</_key>
            <_value>4</_value>
        </untyped_int_int>
        <untyped_int_int type='DictionaryEntry'>
            <_key>1</_key>
            <_value>2</_value>
        </untyped_int_int>
        </Enumeration>
    </untyped_int_int>
</ROOT>";
                printer.Assert.PrintEquals(expected, new Holder());
            }
        }
    }
}
