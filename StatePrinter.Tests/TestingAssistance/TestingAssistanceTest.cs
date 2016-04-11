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
using NUnit.Framework;

using Is = StatePrinting.TestAssistance.Is;
using StatePrinter.Tests.Mocks;
using StatePrinting;
using StatePrinting.TestAssistance;

namespace StatePrinter.Tests.TestingAssistance
{
    [TestFixture]
    class TestingAssistanceTest
    {
        const string ExpectedNonconfigured =
                            "The configuration has no value for AreEqualsMethod which is to point to your testing framework, "
                            + "e.g. use the value: 'Nunit.Framework.Assert.AreEqual' "
                            + "or the more long-winded: '(expected, actual, msg) => Assert.AreEqual(expected, actual, msg)'.\r\n"
                            + "Parameter name: Configuration.AreEqualsMethod"
                            + "\r\nParameter name: Configuration.AreEqualsMethod";

        [Test]
        public void AreEquals_WhenNotConfigured()
        {
            var printer = new Stateprinter();
            var ex = Assert.Throws<ArgumentNullException>(() => printer.Assert.AreEqual("a", "b"));
            Assert.AreEqual(ExpectedNonconfigured, ex.Message);
        }

        [Test]
        public void That_WhenNotConfigured()
        {
            var printer = new Stateprinter();
            var ex = Assert.Throws<ArgumentNullException>(() => printer.Assert.That("a", Is.EqualTo("b")));
            Assert.AreEqual(ExpectedNonconfigured, ex.Message);
        }

        const string AreAlikeNotice = @"Info: Expected value and Actual value are not equal, but they are alike. Use 'Asserter.AreAlike()' if you expected the values to be alike.
";



        class  Tuple
        {
            public Tuple(string item1, string item2)
            {
                Item1 = item1;
                Item2 = item2;
            }

            public string Item1, Item2;
        }

        readonly Tuple[] alikeStrings =
            {
                new Tuple("a\r", "a\r\n"), 
                new Tuple("a\r\n", "a\r"), 
                new Tuple("a\n", "a\r\n"),
                new Tuple("a\n", "a\r"), 
                new Tuple("a\r", "a\n")
            };

        [Test]
        public void AreEquals_WhenValues_AreAlike_Then_Suggest_change()
        {
            var assertMock = new AreEqualsMethodMock();
            Asserter assert = TestHelper.Assert();
            assert.Configuration.Test.SetAreEqualsMethod(assertMock.AreEqualsMock);

            foreach (Tuple t in alikeStrings)
            {
                assert.AreEqual(t.Item1, t.Item2);
                Assert.IsTrue(assertMock.Message.StartsWith(AreAlikeNotice));
            }
        }

        [Test]
        public void AreEquals_Escaping_To_Csharp_format_when_required()
        {
            var assertMock = new AreEqualsMethodMock();
            Asserter assert = TestHelper.Assert();
            assert.Configuration.Test.SetAreEqualsMethod(assertMock.AreEqualsMock);

            // without "
            assert.AreEqual("a", "b");
            Assert.AreEqual("a", assertMock.Expected);
            Assert.AreEqual("b", assertMock.Actual);
            Assert.AreEqual("\r\n\r\nProposed output for unit test:\r\n\r\nvar expected = \"b\";\r\n", assertMock.Message);

            // with  "
            assert.AreEqual("c", "\"e\"");
            Assert.AreEqual("c", assertMock.Expected);
            Assert.AreEqual("\"e\"", assertMock.Actual);
            Assert.AreEqual("\r\n\r\nProposed output for unit test:\r\n\r\nvar expected = @\"\"\"e\"\"\";\r\n", assertMock.Message);


            // without "
            assert.That("aa", Is.EqualTo("bb"));
            Assert.AreEqual("bb", assertMock.Expected);
            Assert.AreEqual("aa", assertMock.Actual);
            Assert.AreEqual("\r\n\r\nProposed output for unit test:\r\n\r\nvar expected = \"aa\";\r\n", assertMock.Message);

        
            // with  "
            assert.That("\"cc\"", Is.EqualTo("ee"));
            Assert.AreEqual("ee", assertMock.Expected);
            Assert.AreEqual("\"cc\"", assertMock.Actual);
            Assert.AreEqual("\r\n\r\nProposed output for unit test:\r\n\r\nvar expected = @\"\"\"cc\"\"\";\r\n", assertMock.Message);
        }

        [Test]
        public void AreAlike()
        {
            var assertMock = new AreEqualsMethodMock();
            Asserter assert = TestHelper.Assert();
            assert.Configuration.Test.SetAreEqualsMethod(assertMock.AreEqualsMock);

            foreach (Tuple t in alikeStrings)
            {
                assert.AreAlike(t.Item1, t.Item2);
                assert.That(t.Item1, Is.AlikeTo(t.Item2));
            }
        }
    }
}
