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
using StatePrinting.Configurations;
using StatePrinting.Orderers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace StatePrinting.Tests.Orderers
{
    [TestFixture]
    class ComparableOrdererTest
    {
        [TestFixture]
        class UnitTests
        {
            [TestCase(typeof(IEnumerable<IComparable>), ExpectedResult = true)]
            [TestCase(typeof(int), ExpectedResult = false)]
            [TestCase(typeof(List<int>), ExpectedResult = true)]
            [TestCase(typeof(IEnumerable<int>), ExpectedResult = true)]
            [TestCase(typeof(Dictionary<int, int>), ExpectedResult = false)]
            [TestCase(typeof(Dictionary<int, ComparableOrdererTest>), ExpectedResult = false)]
            [TestCase(typeof(Dictionary<ComparableOrdererTest, ComparableOrdererTest>), ExpectedResult = false)]
            [TestCase(typeof(Dictionary<int, int>.KeyCollection), ExpectedResult = true)]
            [TestCase(typeof(NonGenericTypeImplementingIEnumerable), ExpectedResult = true)]
            public bool CanHandleTypesImplementingIEnumerableOfIComparables(Type type)
            {
                var comparableOrderer = new ComparableOrderer();

                return comparableOrderer.CanHandleType(type);
            }

            [Test]
            public void CanOrderArrayOfValueTypes()
            {
                var comparableOrderer = new ComparableOrderer();
                var ints = new object[] { 5, 1, 3, 4, 2 };

                var ordered = comparableOrderer.Order(ints).Cast<int>().ToArray();

                var expected = new[] { 1, 2, 3, 4, 5 };
                CollectionAssert.AreEqual(expected, ordered);
            }

            [Test]
            public void CanOrderArrayOfReferenceTypes()
            {
                var comparableOrderer = new ComparableOrderer();
                var strings = new[] { "c", "a", "b" };

                var ordered = comparableOrderer.Order(strings).Cast<string>().ToArray();

                var expected = new[] { "a", "b", "c" };
                CollectionAssert.AreEqual(expected, ordered);
            }

            [Test]
            public void CannotOrderArrayOfDifferentTypes()
            {
                var comparableOrderer = new ComparableOrderer();
                var array = new object[] { 1, "a" };

                Assert.Throws<ArgumentException>(() => comparableOrderer.Order(array).Cast<object>().ToArray());
            }

            class NonGenericTypeImplementingIEnumerable : IEnumerable<int>
            {
                public IEnumerator<int> GetEnumerator() => throw new NotImplementedException();

                IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();
            }
        }

        [TestFixture]
        public class IntegrationTests
        {
            [Test]
            public void IsUsedForEnumerables()
            {
                Stateprinter statePrinter = GetStatePrinterWithComparableOrderer();
                var list = new List<int>() { 3, 1, 2, 0 };

                var actual = statePrinter.PrintObject(list);

                var expected = @"new List<Int32>()
{
 [0] = 0
 [1] = 1
 [2] = 2
 [3] = 3
}";
                Assert.AreEqual(expected, actual);
            }

            [Test]
            public void IsUsedForDictionaries()
            {
                var statePrinter = GetStatePrinterWithComparableOrderer();
                var dictionary = new Dictionary<int, int>()
                {
                    { 3, 1 },
                    { 1, 2 },
                    { 2, 3 },
                };

                var actual = statePrinter.PrintObject(dictionary);

                var expected = @"new Dictionary<Int32, Int32>()
{
 [1] = 2
 [2] = 3
 [3] = 1
}";
                Assert.AreEqual(expected, actual);
            }

            private static Stateprinter GetStatePrinterWithComparableOrderer()
            {
                var cfg = ConfigurationHelper.GetStandardConfiguration(" ");
                cfg.Add(new ComparableOrderer());
                var statePrinter = new Stateprinter(cfg);
                return statePrinter;
            }
        }
    }
}
