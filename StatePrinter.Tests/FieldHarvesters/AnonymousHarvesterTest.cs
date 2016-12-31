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
using System.Collections.Generic;
using NUnit.Framework;
using StatePrinting.Configurations;
using StatePrinting.FieldHarvesters;

namespace StatePrinting.Tests.FieldHarvesters
{
    /// <summary>
    /// Shows how the <see cref="AnonymousHarvester"/> can be used
    /// </summary>
    [TestFixture]
    class AnonymousHarvesterTest
    {
        class A
        {
            public string Name;
        }

        class B
        {
            public int Age;
        }

        [Test]
        public void SpecializedClassHandlerHandledClass()
        {
            Configuration cfg = ConfigurationHelper.GetStandardConfiguration(" ");
            AddAnonymousHandler(cfg);
            var sut = new Stateprinter(cfg);

            var expected = @"new B()
{
 Age = ""Its age is 1""
}";

            var actual = sut.PrintObject(new B { Age = 1 });
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void SpecializedClassHandlerNotHandledClass()
        {
            var cfg = new Configuration(" ");
            AddAnonymousHandler(cfg);

            var sut = new Stateprinter(cfg);

            Assert.Throws<Exception>(() => sut.PrintObject(new A { Name = "MyName" }), "");
        }

        private void AddAnonymousHandler(Configuration cfg)
        {
            cfg.AddHandler(
                t => t == typeof(B),
                t => new List<SanitizedFieldInfo> { new SanitizedFieldInfo(null, "Age", o => "Its age is " + ((B)o).Age) });
        }
    }
}