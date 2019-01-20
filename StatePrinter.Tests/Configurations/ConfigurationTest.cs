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
using System.Collections.Generic;
using NUnit.Framework;
using NUnit.Framework.Internal;
using StatePrinting.Configurations;
using StatePrinting.FieldHarvesters;
using StatePrinting.TestAssistance;
using StatePrinting.ValueConverters;

namespace StatePrinting.Tests.Configurations
{
    [TestFixture]
    class ConfigurationTest
    {
        [Test]
        public void TryFind()
        {
            var config = new Configuration();
            config.Add(new StandardTypesConverter(null));

            IValueConverter h;
            Assert.IsTrue(config.TryGetValueConverter(typeof(decimal), out h));
            Assert.IsTrue(h is StandardTypesConverter);
        }

        [Test]
        public void SettingNullValues()
        {
            var sut = new Configuration();
            Assert.Throws<ArgumentNullException>(() => sut.SetCulture(null));
            Assert.Throws<ArgumentNullException>(() => sut.SetIndentIncrement(null));
            Assert.Throws<ArgumentNullException>(() => sut.SetNewlineDefinition(null));
            Assert.Throws<ArgumentNullException>(() => sut.SetOutputFormatter(null));
            Assert.Throws<ArgumentNullException>(() => sut.SetAreEqualsMethod(null));

            Assert.Throws<ArgumentNullException>(() => sut.Add((IFieldHarvester)null));
            Assert.Throws<ArgumentNullException>(() => sut.Add((IValueConverter)null));

            Assert.Throws<ArgumentNullException>(() => sut.AddHandler(null, t => new List<SanitizedFieldInfo>()));
            Assert.Throws<ArgumentNullException>(() => sut.AddHandler(t => true, null));
            Assert.Throws<ArgumentNullException>(() => sut.AddHandler(null, null));

            Assert.Throws<ArgumentNullException>(() => sut.Test.SetAreEqualsMethod((TestFrameworkAreEqualsMethod) null));
            Assert.Throws<ArgumentNullException>(() => sut.Test.SetAutomaticTestRewrite(null));
        }
    }
}