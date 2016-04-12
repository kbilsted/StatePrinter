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
using StatePrinting.TestAssistance;

namespace StatePrinting.Tests.IntegrationTests
{
    [TestFixture]
    class CallStackReflectorTest
    {
        [Test]
        public void TryGetInfo_inside_test_method()
        {
            var res = new CallStackReflector().TryGetLocation();

            Assert.IsTrue(res.Filepath.EndsWith("ReflectorTest.cs"));
            Assert.AreEqual(32, res.LineNumber);
        }

        [Test]
        public void TryGetInfo_inside_lambda_expected_outside_lambda()
        {
            UnitTestLocationInfo res = null;

            Action x = () => res = new CallStackReflector().TryGetLocation();
            x();

            Assert.IsTrue(res.Filepath.EndsWith("ReflectorTest.cs"));
            Assert.AreEqual(43, res.LineNumber);
        }

        [Test]
        public void TryGetInfo_inside_AssertThrowsLambda_expected_outside_lambda()
        {
            UnitTestLocationInfo res = null;

            Assert.DoesNotThrow(() => res = new CallStackReflector().TryGetLocation());

            Assert.IsTrue(res.Filepath.EndsWith("ReflectorTest.cs"));
            Assert.AreEqual(55, res.LineNumber);
        }
    }
}
