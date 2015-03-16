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
    class TestingAssistanceRewriteTest
    {

        [Test]
        public void Rewriter_requires_local_variable_of_type_string()
        {
            var printer = TestHelper.CreateTestPrinter();
            printer.Configuration.SetAutomaticTestRewrite((x) => true);

            var ex = Assert.Throws<System.ArgumentException>(() => printer.Assert.IsSame("", "a"));
            Assert.AreEqual("Cannot find a local variable of type string. Expecting the test to contain the variable 'expected' of type string.", ex.Message);
        }


        [Test]
        public void Rewriter_requires_an_expected()
        {
            var printer = TestHelper.CreateTestPrinter();
            printer.Configuration.SetAutomaticTestRewrite((x) => true);

            string unusued = " ";
            var ex = Assert.Throws<System.ArgumentException>(() => printer.Assert.IsSame(unusued, "a"));
            Assert.AreEqual("Cannot find a local variable of type string. Expecting the test to contain the variable 'expected' of type string.", ex.Message);
        }


        [Test]
        public void Rewriter_requires_verbatim_string()
        {
            var printer = TestHelper.CreateTestPrinter();
            printer.Configuration.SetAutomaticTestRewrite((x) => true);

            string expected = " ";
            var ex = Assert.Throws<System.ArgumentException>(() => printer.Assert.IsSame(expected, "a"));
            Assert.AreEqual("Cannot find a local variable of type string. Expecting the test to contain the variable 'expected' of type string.", ex.Message);
        }

        [Test]
        public void Rewriter_when_expectedIsOutCommented_fail()
        {
            var printer = TestHelper.CreateTestPrinter();
            printer.Configuration.SetAutomaticTestRewrite((x) => true);

            // string expected = @" ";
            var ex = Assert.Throws<System.ArgumentException>(() => printer.Assert.IsSame("b", "a"));
            Assert.AreEqual("Cannot find a local variable of type string. Expecting the test to contain the variable 'expected' of type string.", ex.Message);
        }



        [Test]
        [Explicit]
        public void Autocorrection_works_var()
        {
            var printer = TestHelper.CreateTestPrinter();
            printer.Configuration.SetAutomaticTestRewrite((x) => true);

            var expected = @"""test auto""";
            printer.Assert.PrintIsSame(expected, "test auto");
        }

        [Test]
        [Explicit]
        public void Autocorrection_works_string()
        {
            var printer = TestHelper.CreateTestPrinter();
            printer.Configuration.SetAutomaticTestRewrite((x) => true);

            string expected = @"""test auto""";
            printer.Assert.PrintIsSame(expected, "test auto");
        }
    }
}
