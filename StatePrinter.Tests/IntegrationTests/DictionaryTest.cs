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

using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using StatePrinter.Configurations;
using StatePrinter.OutputFormatters;

namespace StatePrinter.Tests.IntegrationTests
{
    [TestFixture]
    class DictionaryTest
    {
        Stateprinter printer;

        [SetUp]
        public void Setup()
        {
            printer = new Stateprinter();
            printer.Configuration.OutputFormatter = new CurlyBraceStyle(printer.Configuration);
        }


        [Test]
        public void Dictionary_int_int()
        {
            var d = new Dictionary<int, int> { { 1, 2 }, { 2, 4 }, { 3, 6 } };
            Assert.AreEqual("[1] = 2\r\n[2] = 4\r\n[3] = 6\r\n", printer.PrintObject(d));
        }

        [Test]
        public void IDictionary_untyped_int_int()
        {
            IDictionary d = new Hashtable() { { 1, 2 }, { 2, 4 }, { 3, 6 } };
            Assert.AreEqual(
      @"new Hashtable()
[0] = new DictionaryEntry()
{
    _key = 3
    _value = 6
}
[1] = new DictionaryEntry()
{
    _key = 2
    _value = 4
}
[2] = new DictionaryEntry()
{
    _key = 1
    _value = 2
}
", printer.PrintObject(d));
        }

    }
}
