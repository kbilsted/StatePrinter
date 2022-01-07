//// Copyright 2014 Kasper B. Graversen
//// 
//// Licensed to the Apache Software Foundation (ASF) under one
//// or more contributor license agreements.  See the NOTICE file
//// distributed with this work for additional information
//// regarding copyright ownership.  The ASF licenses this file
//// to you under the Apache License, Version 2.0 (the
//// "License"); you may not use this file except in compliance
//// with the License.  You may obtain a copy of the License at
//// 
////   http://www.apache.org/licenses/LICENSE-2.0
//// 
//// Unless required by applicable law or agreed to in writing,
//// software distributed under the License is distributed on an
//// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
//// KIND, either express or implied.  See the License for the
//// specific language governing permissions and limitations
//// under the License.

using NUnit.Framework;
using StatePrinting.Configurations;
using System.Collections.Generic;
using System.Linq;

namespace StatePrinting.Tests.Orderers
{
    [TestFixture]
    class AnonymousOrdererTest
    {
        [Test]
        public void AnonymousOrdererUsesOrderFuncForHandledType()
        {
            var cfg = ConfigurationHelper.GetStandardConfiguration(" ");
            cfg.AddOrderer(type => type == typeof(List<int>), enumerable => enumerable.Cast<int>().Reverse());
            var statePrinter = new Stateprinter(cfg);
            var list = new List<int>() { 0, 1, 2 };

            var actual = statePrinter.PrintObject(list);

            var expected = @"new List<Int32>()
{
 [0] = 2
 [1] = 1
 [2] = 0
}";
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void AnonymousOrdererDoesNotUseOrderFuncForUnhandledType()
        {
            var cfg = ConfigurationHelper.GetStandardConfiguration(" ");
            cfg.AddOrderer(type => false, enumerable => null);
            var statePrinter = new Stateprinter(cfg);
            var list = new List<int> { 1, 0 };

            var actual = statePrinter.PrintObject(list);

            var expected = @"new List<Int32>()
{
 [0] = 1
 [1] = 0
}";
            Assert.AreEqual(expected, actual);
        }
    }
}
