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

using NUnit.Framework;

namespace StatePrinting.Tests.IntegrationTests
{
    /// <summary>
    /// An example of using the state printer as a generic ToString() implementation.
    /// </summary>
    [TestFixture]
    class ToStringMethodTest
    {
        [Test]
        public void TestToStringMethod()
        {
            var a = new AClassWithToString();
            string expected =
      @"new AClassWithToString()
{
    B = ""hello""
    C = new Int32[]()
    {
        [0] = 5
        [1] = 4
        [2] = 3
        [3] = 2
        [4] = 1
    }
}";
            Assert.AreEqual(expected, a.ToString());
        }
    }


    class AClassWithToString
    {
        string B = "hello";
        int[] C = { 5, 4, 3, 2, 1 };
        static readonly Stateprinter printer = new Stateprinter();

        public override string ToString()
        {
            return printer.PrintObject(this);
        }

        public void Dummy()
        {
            B = B + " ";
        }
    }
}
