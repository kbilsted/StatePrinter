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
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;

using StatePrinter.TestAssistance;

namespace StatePrinter.Tests.IntegrationTests
{
    [TestFixture]
    class ReflectorTest
    {
        [Test]
        public void TryGetInfo()
        {
            string expected = "";
            var res = new Reflector().TryGetLocation();

            Assert.IsTrue(res.Filepath.EndsWith("ReflectorTest.cs"));
            Assert.AreEqual(38, res.LineNumber);
            Assert.IsTrue(res.TestMethodHasAStringVariable);
        }

        [Test]
        public void TryGetInfo_with_no_local_variable_of_type_string()
        {
            var info = new Reflector().TryGetLocation();
            Assert.IsFalse(info.TestMethodHasAStringVariable);
        }

        const string key = "StatePrinter_auto_correct_expected";

        [Test]
        public void ENV()
        {
            Console.WriteLine("***");
            Console.WriteLine(Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.User));
        }
    }

    [TestFixture]
    class ParserTest
    {
        Parser sut;

        [TestFixtureSetUp]
        public void Setup()
        {
            sut = new Parser();
        }


        [Test]
        public void ReplaceExpected_simple_input()
        {
            string program = 
@"  abc def
  var expected = @""hello"";
  qwe ert
  printer.Assert.Here(...)
  iu of";
            var r = sut.ReplaceExpected(program, 4, "var expected = @\"boo\";");

            var expected = @"""  abc def
  var expected = @""boo"";
  qwe ert
  printer.Assert.Here(...)
  iu of""";

            TestHelper.CreateTestPrinter().Assert.PrintIsSame(expected, r);
        }


        [Test]
        public void Autocorrection_works()
        {
            var printer = TestHelper.CreateTestPrinter();
            printer.Configuration.SetAutomaticTestRewrite((x) => true);

            var expected = @"""test auto""";
            printer.Assert.PrintIsSame(expected, "test auto");
        }


        [Test]
        public void ReplaceExpected_only_last_expected_changes()
        {
            string program = @"abc def
var expected = @""a"";
var expected = @""b"";
qwe ert
printer.Assert.Here(...)
iu of";

            var r = sut.ReplaceExpected(program, 4, "var expected = @\"boo\";");

            var expected = @"""abc def
var expected = @""a"";
var expected = @""boo"";
qwe ert
printer.Assert.Here(...)
iu of""";
            TestHelper.CreateTestPrinter().Assert.PrintIsSame(expected, r);
        }



        static string nestedExpected = @"""  abc def
    var expected = @""boo"";
  qwe ert
  printer.Assert.Here(...)
  iu of""";


        [Test]
        public void ReplaceExpected_content_contains_expected_string()
        {
            string program =
@"  abc def
    var expected = @""a
 var expected = @""""aaa"""";
"";
  qwe ert
  printer.Assert.Here(...)
  iu of";

            var r = sut.ReplaceExpected(program, 5, "var expected = @\"boo\";");
            TestHelper.CreateTestPrinter().Assert.PrintIsSame(nestedExpected, r);
        }


        [Test]
        public void ReplaceExpected_content_contains_expected_string22()
        {
            string program =
@"  abc def
    var expected = @""a
 var expected = @""""aaa"""";
 var expected = @""""aaa"""";
 var expected = @""""aaa"""";
"";
  qwe ert
  printer.Assert.Here(...)
  iu of";

            var r = sut.ReplaceExpected(program, 8, "var expected = @\"boo\";");
            TestHelper.CreateTestPrinter().Assert.PrintIsSame(nestedExpected, r);
        }


        [Test]
        public void ReplaceExpected_outcommented_expected_string()
        {
            string program = @"abc def
var expected = @""a"";
// var expected = @""b"";
qwe ert
printer.Assert.Here(...)
iu of";
            Console.WriteLine("''program:" + program);
            var r = sut.ReplaceExpected(program, 4, "var expected = @\"boo\";");

            var expected = @"""abc def
var expected = @""boo"";
// var expected = @""b"";
qwe ert
printer.Assert.Here(...)
iu of""";

            TestHelper.CreateTestPrinter().Assert.PrintIsSame(expected, r);
        }


        [Test]
        public void ReplaceExpected_outcommented_expected_string2()
        {
            string program =
   @"  abc def
    var expected = @""a
 var expected = @""""aaa"""";
 var expected = @""""aaa"""";
 var expected = @""""aaa"""";
 var expected = @""""aaa"""";
 var expected = @""""aaa"""";
 var expected = @""""aaa"""";
 var expected = @""""aaa"""";
 var expected = @""""aaa"""";
 var expected = @""""aaa"""";
 var expected = @""""aaa"""";
 var expected = @""""aaa"""";
 var expected = @""""aaa"""";
 var expected = @""""aaa"""";
 var expected = @""""aaa"""";
 var expected = @""""aaa"""";
 var expected = @""""aaa"""";
"";
  qwe ert
  printer.Assert.Here(...)
  iu of";
            Console.WriteLine("''program:" + program);
            var r = sut.ReplaceExpected(program, 22, "var expected = @\"boo\";");

            var expected = @"""  abc def
    var expected = @""boo"";
  qwe ert
  printer.Assert.Here(...)
  iu of""";
            TestHelper.CreateTestPrinter().Assert.PrintIsSame(expected, r);
        }
    }
}
