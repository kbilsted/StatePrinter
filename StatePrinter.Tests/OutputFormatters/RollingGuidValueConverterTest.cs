// Copyright 2014-2020 Kasper B. Graversen
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
using StatePrinting.ValueConverters;

namespace StatePrinting.Tests.OutputFormatters
{
    [TestFixture]
    class RollingGuidValueConverterTest
    {
        [Test]
        public void TestRolling()
        {
            var sut = new RollingGuidValueConverter();
            Assert.AreEqual("00000000-0000-0000-0000-000000000001", sut.Convert(Guid.NewGuid()));
            Assert.AreEqual("00000000-0000-0000-0000-000000000002", sut.Convert(Guid.NewGuid()));
            Assert.AreEqual("00000000-0000-0000-0000-000000000003", sut.Convert(Guid.NewGuid()));
            Assert.AreEqual("00000000-0000-0000-0000-000000000004", sut.Convert(Guid.NewGuid()));
            Assert.AreEqual("00000000-0000-0000-0000-000000000005", sut.Convert(Guid.NewGuid()));
            Assert.AreEqual("00000000-0000-0000-0000-000000000006", sut.Convert(Guid.NewGuid()));
            Assert.AreEqual("00000000-0000-0000-0000-000000000007", sut.Convert(Guid.NewGuid()));
            Assert.AreEqual("00000000-0000-0000-0000-000000000008", sut.Convert(Guid.NewGuid()));
            Assert.AreEqual("00000000-0000-0000-0000-000000000009", sut.Convert(Guid.NewGuid()));
            Assert.AreEqual("00000000-0000-0000-0000-000000000010", sut.Convert(Guid.NewGuid()));
        }

        [Test]
        public void TestReuseRolledValues()
        {
            var sut = new RollingGuidValueConverter();
            Guid g1 = Guid.NewGuid();
            Guid g2 = Guid.NewGuid();
            Assert.AreEqual("00000000-0000-0000-0000-000000000001", sut.Convert(g1));
            Assert.AreEqual("00000000-0000-0000-0000-000000000002", sut.Convert(g2));
            Assert.AreEqual("00000000-0000-0000-0000-000000000001", sut.Convert(g1));
            Assert.AreEqual("00000000-0000-0000-0000-000000000002", sut.Convert(g2));
        }
    }
}
