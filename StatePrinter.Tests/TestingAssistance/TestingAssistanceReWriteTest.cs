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
        [Explicit]
        public void Autocorrection_works_var()
        {
            var printer = TestHelper.CreateTestPrinter();
            printer.Configuration.Test.SetAutomaticTestRewrite((x) => true);

            var expected = @"""test auto""";
            printer.Assert.PrintIsSame(expected, "test auto");
        }

        [Test]
        [Explicit]
        public void Autocorrection_works_string()
        {
            var printer = TestHelper.CreateTestPrinter();
            printer.Configuration.Test.SetAutomaticTestRewrite((x) => true);

            string expected = @"""test auto""";
            printer.Assert.PrintIsSame(expected, "test auto");
        }
    }
}
