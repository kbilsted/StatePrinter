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

using System.Collections;
using System.Collections.Generic;

using NUnit.Framework;

using StatePrinter.Configurations;
using StatePrinter.FieldHarvesters;

namespace StatePrinter.Tests.IntegrationTests
{
    [TestFixture]
    class PublicPropertiesTest
    {
        class GetterOnly
        {
            public int i;

            int j = 22;

            public int Sum
            {
                get
                {
                    return i + j;
                }
            }

            internal int Sum2
            {
                get
                {
                    return i + j;
                }
            }
        }

        class SetterOnly
        {
            public int i, j;

            public int Sum
            {
                set
                {
                    i = value;
                }
            }
        }

        class GetterSetter
        {
            public int I { get; set; }
            int J { get; set; }
            int K { get; set; }
        }

        class GetterSetterExplicitBackingField
        {
            int i, j;

            public int I
            {
                get
                {
                    return i;
                }
                set
                {
                    i = value;
                }
            }

            internal int J
            {
                get
                {
                    return j;
                }
                set
                {
                    j = value;
                }
            }
        }

        class IndexedProperty
        {
            public int i, j;

            public int this[int index]
            {
                get
                {
                    if (index == 1) return i;
                    if (index == 2) return j;
                    return -1;
                }
            }
        }

        [Test]
        public void GetterOnly_IsIncluded()
        {
            var sut = new GetterOnly() { i = 1 };
            var printer = CreatePrinter();
            Assert.AreEqual(@"new GetterOnly()
{
    Sum = 23
    i = 1
}", printer.PrintObject(sut, ""));
        }


        [Test]
        public void SetterOnly_NotIncluded()
        {
            var sut = new SetterOnly() { i = 1, j = 2 };
            var printer = CreatePrinter();
            Assert.AreEqual(@"new SetterOnly()
{
    i = 1
    j = 2
}", printer.PrintObject(sut, ""));
        }


        [Test]
        public void GetterSetterOnly_IsIncluded()
        {
            var sut = new GetterSetter() { I = 1 };
            var printer = CreatePrinter();

            Assert.AreEqual(@"new GetterSetter()
{
    I = 1
}", printer.PrintObject(sut, ""));
        }


        /// <summary>
        /// unfortunately both are printed. A <see cref="ProjectionHarvester"/> is needed in order to reduce the number of fields.
        /// 
        /// We see this kind of implementation in 
        /// <see cref="KeyValuePair{TKey,TValue}"/> 
        /// and <see cref="DictionaryEntry"/>
        /// </summary>
        [Test]
        public void GetterSetter_WithExplicitBackingField_BothAreIncluded()
        {
            var sut = new GetterSetterExplicitBackingField() { I = 1, J = 2 };
            var printer = CreatePrinter();

            Assert.AreEqual(@"new GetterSetterExplicitBackingField()
{
    I = 1
}", printer.PrintObject(sut, ""));
        }


        [Test]
        public void GetterIndexedProperty_not_included()
        {
            var sut = new IndexedProperty() { i = 1, j = 2 };
            var printer = CreatePrinter();

            Assert.AreEqual(@"new IndexedProperty()
{
    i = 1
    j = 2
}", printer.PrintObject(sut, ""));
        }


        Stateprinter CreatePrinter()
        {
            return
                new Stateprinter(
                    ConfigurationHelper.GetStandardConfiguration()
                        .Add(new PublicFieldsAndPropertiesHarvester()));
        }
    }
}