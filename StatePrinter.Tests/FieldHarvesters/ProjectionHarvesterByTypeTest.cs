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
using StatePrinting.TestAssistance;

namespace StatePrinting.Tests.FieldHarvesters
{
    [TestFixture]
    public class TypeFilteringTests
    {
        interface IA
        {
            int A { get; set; }
        }

        interface IB
        {
            int B { get; set; }
        }

        interface IC
        {
            int C { get; set; }
        }

        interface ID
        {
            int D { get; set; }
        }

        class AtoD : IA, IB, IC, ID
        {
            public int A { get; set; }
            public int B { get; set; }
            public int C { get; set; }
            public int D { get; set; }

            public AtoD()
            {
                A = 1;
                B = 2;
                C = 3;
                D = 4;
            }
        }

        [Test]
        public void TestIncludeByType()
        {
            var sut = new AtoD();
            Asserter assert;

            assert = TestHelper.CreateShortAsserter();
            assert.PrintEquals("new AtoD() { A = 1 B = 2 C = 3 D = 4 }", sut);

            assert = TestHelper.CreateShortAsserter();
            assert.Project.IncludeByType<AtoD, IA>();
            assert.PrintEquals("new AtoD() { A = 1 }", sut);

            assert = TestHelper.CreateShortAsserter();
            assert.Project.IncludeByType<AtoD, IA, IB>();
            assert.PrintEquals("new AtoD() { A = 1 B = 2 }", sut);

            assert = TestHelper.CreateShortAsserter();
            assert.Project.IncludeByType<AtoD, IA, IB, IC>();
            assert.PrintEquals("new AtoD() { A = 1 B = 2 C = 3 }", sut);

            assert = TestHelper.CreateShortAsserter();
            assert.Project.IncludeByType<AtoD, IA, IB, IC, ID>();
            assert.PrintEquals("new AtoD() { A = 1 B = 2 C = 3 D = 4 }", sut);
        }

        [Test]
        public void TestExcludeByType()
        {
            var sut = new AtoD();
            Asserter assert;

            assert = TestHelper.CreateShortAsserter();
            assert.PrintEquals("new AtoD() { A = 1 B = 2 C = 3 D = 4 }", sut);

            assert = TestHelper.CreateShortAsserter();
            assert.Project.ExcludeByType<AtoD, IA>();
            assert.PrintEquals("new AtoD() { B = 2 C = 3 D = 4 }", sut);

            assert = TestHelper.CreateShortAsserter();
            assert.Project.ExcludeByType<AtoD, IA, IB>();
            assert.PrintEquals("new AtoD() { C = 3 D = 4 }", sut);

            assert = TestHelper.CreateShortAsserter();
            assert.Project.ExcludeByType<AtoD, IA, IB, IC>();
            assert.PrintEquals("new AtoD() { D = 4 }", sut);

            assert = TestHelper.CreateShortAsserter();
            assert.Project.ExcludeByType<AtoD, IA, IB, IC, ID>();
            assert.PrintEquals("new AtoD() { }", sut);
        }
    }
}
