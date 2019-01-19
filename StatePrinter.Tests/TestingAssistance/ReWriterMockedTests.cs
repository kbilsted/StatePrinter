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
using StatePrinting.Tests.Mocks;

namespace StatePrinting.Tests.TestingAssistance
{
    public class UserStory_Integration_with_underlying_testing_framework
    {
        [Test]
        public void Rewriter_calls_to_testframework_autorewriting()
        {
            var printer = TestHelper.CreateTestPrinter();

            var fakeReadContent = new System.Text.UTF8Encoding(true).GetBytes(TestFileContent);
            var mock = new FileRepositoryMock(fakeReadContent);
            printer.Configuration.FactoryFileRepository = () => mock;
            printer.Configuration.Test.SetAutomaticTestRewrite(x => true);

            var assertMock = new AreEqualsMethodMock();
            printer.Configuration.Test.SetAreEqualsMethod(assertMock.AreEqualsMock);

            string expected = "boo";
            printer.Assert.AreAlike(expected, "actul");

            Assert.AreEqual("boo", assertMock.Expected);
            Assert.AreEqual("actul", assertMock.Actual);
            Assert.IsTrue(assertMock.Message.StartsWith("Rewritting test expectations in '"));
            Assert.IsTrue(assertMock.Message.EndsWith(@"'.
Compile and re-run to see green lights.
New expectations:
""actul"""));
            Assert.IsTrue(mock.WritePath.EndsWith("ReWriterMockedTests.cs"));
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
        public void Rewriter_calls_to_testframework_fileTooShort()
        {
            var printer = TestHelper.CreateTestPrinter();

            var fakeReadContent = new System.Text.UTF8Encoding(true).GetBytes(TestFileContent);
            printer.Configuration.FactoryFileRepository = () => new FileRepositoryMock(fakeReadContent);
            printer.Configuration.Test.SetAutomaticTestRewrite((x) => true);

            var assertMock = new AreEqualsMethodMock();
            printer.Configuration.Test.SetAreEqualsMethod(assertMock.AreEqualsMock);

            string expected = @"expect";

            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => printer.Assert.AreAlike(expected, "actul"));
            Assert.AreEqual("File does not have 121 lines. Only 47 lines.\r\nParameter name: content", ex.Message);
        }
    }
}
