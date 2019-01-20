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
using NUnit.Framework.Internal;
using StatePrinting.TestAssistance;

namespace StatePrinting.Tests.TestingAssistance
{
    public class ParserTest
    {
        Parser sut;

        [SetUp]
        public void Setup()
        {
            sut = new Parser();
        }

        [Test]
        public void NoEscapingOfNewExpected()
        {
            var r = sut.ReplaceExpected("Assert.AreEqual(\"hello\", sut.Do());", 1, "hello", "boo");
            TestHelper.Assert().PrintAreAlike(@"""Assert.AreEqual(boo, sut.Do());""", r);
        }

        [Test]
        public void Somehow_content_could_not_be_found()
        {
            var ex = Assert.Throws<ArgumentException>(() => sut.ReplaceExpected("aaaaaaaa", 1, "someOtherString", "boo"));
            Assert.AreEqual("Did not find 'someOtherString'", ex.Message);
        }

        [Test]
        public void Somehow_content_containing_weird_symbols_could_not_be_found()
        {
            var ex = Assert.Throws<ArgumentException>(() => sut.ReplaceExpected("aaaaaaaa", 1, "some.OtherString()", "boo"));
            Assert.AreEqual("Did not find 'some.OtherString()'", ex.Message);
        }

        [Test]
        public void Simple_input()
        {
            string program =
@"  abc def
  qwe ert
  printer.Assert.Here(@""hello"", ...)
  iu of";
            var r = sut.ReplaceExpected(program, 3, "hello", "boo");

            var expected = @"""  abc def
  qwe ert
  printer.Assert.Here(boo, ...)
  iu of""";

            TestHelper.Assert().PrintAreAlike(expected, r);
        }

        [Test]
        public void Expected_variable_ContainsBackslashes_WillBeEscaped()
        {
            string program =
@"  abc def
  var expected = @""FilePath =Articles\Design\MalleableCodeUsingDecorators.md"";
";
            var r = sut.ReplaceExpected(program, 3, @"FilePath =Articles\Design\MalleableCodeUsingDecorators.md", "boo");

            var expected = @"""  abc def
  var expected = boo;
""";
            TestHelper.Assert().PrintAreAlike(expected, r);
        }




        [Test]
        public void Expected_variable()
        {
            string program =
@"  abc def
  var expected = @""hello"";
  qwe ert
  printer.Assert.Here(...)
  iu of";
            var r = sut.ReplaceExpected(program, 4, "hello", "boo");

            var expected = @"""  abc def
  var expected = boo;
  qwe ert
  printer.Assert.Here(...)
  iu of""";

            TestHelper.Assert().PrintAreAlike(expected, r);
        }

        [Test]
        public void Expected_variable_containsNewlines()
        {
            string program =
@"  abc def
  var expected = @""hello
"";
  qwe ert
  printer.Assert.Here(...)
  iu of";
            var r = sut.ReplaceExpected(program, 4, "hello\r\n", "boo");

            var expected = @"""  abc def
  var expected = boo;
  qwe ert
  printer.Assert.Here(...)
  iu of""";

            TestHelper.Assert().PrintAreAlike(expected, r);
        }

        [Test]
        public void Expected_variable_contains_brackets()
        {
            var r = sut.ReplaceExpected("Assert.AreEqual(\"[0]\", sut.Do());", 1, "[0]", "boo");
            TestHelper.Assert().PrintAreAlike(@"""Assert.AreEqual(boo, sut.Do());""", r);
        }
        
        [Test]
        public void Only_last_expected_changes_normal_string()
        {
            string program = @"abc def
var expected = @""b"";
var expected = ""b"";
qwe ert
printer.Assert.Here(...)
iu of";

            var r = sut.ReplaceExpected(program, 4, "b", "boo");

            string expected = @"""abc def
var expected = @""b"";
var expected = boo;
qwe ert
printer.Assert.Here(...)
iu of""";
            TestHelper.Assert().PrintAreAlike(expected, r);
        }



        [Test]
        public void Only_last_expected_changes_verbatim_string()
        {
            string program = @"abc def
var expected = ""b"";
var expected = @""b"";
qwe ert
printer.Assert.Here(...)
iu of";
            var r = sut.ReplaceExpected(program, 4, "b", "boo");
            string expected = @"""abc def
var expected = ""b"";
var expected = boo;
qwe ert
printer.Assert.Here(...)
iu of""";

            TestHelper.Assert().PrintAreAlike(expected, r);
        }




        /// <summary>
        /// Shows that we may further improve the matching to skip lines where lines start with "//"
        /// </summary>
        [Test]
        public void Bug_outcommented_string_is_matched_()
        {
            string program = @"abc def
var expected = @""a"";
// var expected = @""a"";
qwe ert
printer.Assert.Here(...)
iu of";
            var r = sut.ReplaceExpected(program, 4, "a", "boo");

            var expected = @"abc def
var expected = @""a"";
// var expected = boo;
qwe ert
printer.Assert.Here(...)
iu of";

           Assert.AreEqual(expected, r);
        }
    }
}
