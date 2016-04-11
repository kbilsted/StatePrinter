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


using NUnit.Framework;
using Is = StatePrinting.TestAssistance.Is;

namespace StatePrinting.Tests.TestingAssistance
{
    [TestFixture]
    class Userstory_nonexplicit
    {
        [Test]
        public void IsSame_differentNewlines()
        {
            TestHelper.Assert().IsSame("a\n", "a\r\n");
            TestHelper.Assert().IsSame("a\r\n", "a\n");
            TestHelper.Assert().IsSame("a\r", "a\n");
            TestHelper.Assert().IsSame("a\r", "a\r\n");

            TestHelper.Assert().PrintIsSame("\"a\r\"", "a\r\n");
            TestHelper.Assert().PrintIsSame("\"a\r\"", "a\r");
            TestHelper.Assert().PrintEquals("\"a\r\"", "a\r");
        }

        [Test]
        public void AreAlike_differentNewlines()
        {
            TestHelper.Assert().AreAlike("a\n", "a\r\n");
            TestHelper.Assert().AreAlike("a\r\n", "a\n");
            TestHelper.Assert().AreAlike("a\r", "a\n");
            TestHelper.Assert().AreAlike("a\r", "a\r\n");

            TestHelper.Assert().PrintAreAlike("\"a\r\"", "a\r\n");
            TestHelper.Assert().PrintAreAlike("\"a\r\"", "a\r");
            TestHelper.Assert().PrintEquals("\"a\r\"", "a\r");
        }
    }

    [Ignore("Run these in order to see how Nunit integrates with the testing assistance")]
    [TestFixture]
    class Userstory
    {
        [Test]
        public void AreEquals_without()
        {
            TestHelper.Assert().AreEqual("a", "b");
        }

        [Test]
        public void That_without()
        {
            TestHelper.Assert().That("a", Is.EqualTo("b"));
        }


        [Test]
        public void AreEquals_with()
        {
            TestHelper.Assert().AreEqual("a", "\"b\"");
        }

    }
}
