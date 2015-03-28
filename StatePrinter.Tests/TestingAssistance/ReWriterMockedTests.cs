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
    class UserStory_Integration_with_underlying_testing_framework
    {
        [Test]
        public void Rewriter_calls_to_testframework_autorewriting()
        {
            try
            {
                var printer = TestHelper.CreateTestPrinter();
                printer.Configuration.SetAutomaticTestRewrite((x) => true);

                FileRepository.UnitTestFakeReadContent = new System.Text.UTF8Encoding(true).GetBytes(TestFileContent);

                var assertMock = new AreEqualsMethodMock();
                printer.Configuration.SetAreEqualsMethod(assertMock.AreEqualsMock);

                string expected = "boo";
                printer.Assert.IsSame(expected, "actul");

                Assert.AreEqual("boo", assertMock.Expected);
                Assert.AreEqual("actul", assertMock.Actual);
                Assert.AreEqual("AUTOMATICALLY rewritting test expectations. Compile and re-run to see green lights.\nNew expectation\n:var expected = \"actul\";", assertMock.Message);
            }
            finally
            {
                FileRepository.UnitTestFakeReadContent = null;
            }
        }

        const string TestFileContent = @"
must
contain
more
lines than the
test above
such that the 
file contains as many lines as the line number
reported by the callstackreflector
0
1
2
3
4
5
6
7
8
9
0
1
2
3
4
5
6
7
8
9
0
1
2
3
4
5
6
7
8
9
      string expected = @""boo"";
0
1
2
3
4
5
";

        /// <summary>
        /// By running again against the same file, the line number has now increased to something larger than the input file
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException), ExpectedMessage = "File does not have 123 lines. Only 47 lines.\r\nParameter name: content")]
        public void Rewriter_calls_to_testframework_fileTooShort()
        {
            try
            {
                var printer = TestHelper.CreateTestPrinter();
                printer.Configuration.SetAutomaticTestRewrite((x) => true);

                FileRepository.UnitTestFakeReadContent = new System.Text.UTF8Encoding(true).GetBytes(TestFileContent);

                var assertMock = new AreEqualsMethodMock();
                printer.Configuration.SetAreEqualsMethod(assertMock.AreEqualsMock);
                
                string expected = @"expect";

                printer.Assert.IsSame(expected, "actul");
            }
            finally
            {
                FileRepository.UnitTestFakeReadContent = null;
            }
        }

        [Test]
        public void Rewriter_calls_to_testframework_fileTooShort2()
        {
            try
            {
                var printer = TestHelper.CreateTestPrinter();
                printer.Configuration.SetAutomaticTestRewrite((x) => true);

                FileRepository.UnitTestFakeReadContent = new System.Text.UTF8Encoding(true).GetBytes(TestFileContent);

                var assertMock = new AreEqualsMethodMock();
                printer.Configuration.SetAreEqualsMethod(assertMock.AreEqualsMock);

                string expected = @"expect";

                var ex = Assert.Throws<ArgumentOutOfRangeException>(() => printer.Assert.IsSame(expected, "actul"));
                Assert.AreEqual("File does not have 146 lines. Only 47 lines.\r\nParameter name: content", ex.Message);
            }
            finally
            {
                FileRepository.UnitTestFakeReadContent = null;
            }
        }
    }
}
