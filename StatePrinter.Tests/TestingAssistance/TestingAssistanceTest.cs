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

using StatePrinter.Configurations;
using StatePrinter.TestAssistance;

namespace StatePrinter.Tests.TestingAssistance
{
    [TestFixture]
    class TestingAssistanceTest
    {
        const string ExpectedNonconfigured = 
                            "The configuration has no value for AreEqualsMethod which is to point to your testing framework, "
                            + "e.g. use the value: 'Assert.AreEqual' "
                            + "or the more long-winded: '(expected, actual, msg) => Assert.AreEqual(expected, actual, msg)'.\r\n"
                            + "Parameter name: Configuration.AreEqualsMethod"
                            +"\r\nParameter name: Configuration.AreEqualsMethod";

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


        [Test]
        public void AreEquals_WhenConfigured()
        {
            string called_expected = null, called_actual = null, called_msg = null;
            TestFrameworkAreEqualsMethod assertMth = (exp, act, msg) =>
            {
                called_expected = exp;
                called_actual = act;
                called_msg = msg;
            };

            var cfg = ConfigurationHelper.GetStandardConfiguration(assertMth);
            var printer = new Stateprinter(cfg);

            // without "
            printer.Assert.AreEqual("a", "b");
            Assert.AreEqual("a", called_expected);
            Assert.AreEqual("b", called_actual);
            Assert.AreEqual("\r\n\r\nProposed output for unit test:\r\n\r\nvar expected = \"b\";\r\n", called_msg);

            // with  "
            printer.Assert.AreEqual("c", "\"e\"");
            Assert.AreEqual("c", called_expected);
            Assert.AreEqual("\"e\"", called_actual);
            Assert.AreEqual("\r\n\r\nProposed output for unit test:\r\n\r\nvar expected = @\"\"\"e\"\"\";\r\n", called_msg);


            // without "
            printer.Assert.That("aa", Is.EqualTo("bb"));
            Assert.AreEqual("bb", called_expected);
            Assert.AreEqual("aa", called_actual);
            Assert.AreEqual("\r\n\r\nProposed output for unit test:\r\n\r\nvar expected = \"aa\";\r\n", called_msg);

            // with  "
            printer.Assert.That("\"cc\"", Is.EqualTo("ee"));
            Assert.AreEqual("ee", called_expected);
            Assert.AreEqual("\"cc\"", called_actual);
            Assert.AreEqual("\r\n\r\nProposed output for unit test:\r\n\r\nvar expected = @\"\"\"cc\"\"\";\r\n", called_msg);
        }

    }
}
