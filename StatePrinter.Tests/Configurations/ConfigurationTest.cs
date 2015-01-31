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

using System.Linq;
using NUnit.Framework;
using StatePrinter.Configurations;
using StatePrinter.ValueConverters;

namespace StatePrinter.Tests.Configurations
{
    [TestFixture]
    class ConfigurationTest
    {
        [Test]
        public void AddInReverse()
        {
            var config = new Configuration();

            var handler1 = new EnumConverter();
            config.Add(handler1);
            Assert.AreEqual(1, config.ValueConverters.Count);
            Assert.AreEqual(0, config.FieldHarvesters.Count);
            Assert.AreEqual(handler1, config.ValueConverters.First());

            var handler2 = new EnumConverter();
            config.Add(handler2);
            Assert.AreEqual(2, config.ValueConverters.Count);
            Assert.AreEqual(0, config.FieldHarvesters.Count);
            Assert.AreEqual(handler2, config.ValueConverters.First());
        }

        [Test]
        public void TryFind()
        {
            var config = new Configuration();
            config.Add(new StandardTypesConverter(null));

            IValueConverter h;
            Assert.IsTrue(config.TryGetValueConverter(typeof(decimal), out h));
            Assert.IsTrue(h is StandardTypesConverter);
        }
    }
}