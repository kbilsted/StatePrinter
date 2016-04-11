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

using System.Globalization;
using NUnit.Framework;
using StatePrinting.TestAssistance;
using StatePrinting.ValueConverters;

namespace StatePrinting.Tests.ExamplesForDocumentation
{
    [TestFixture]
    class ExampleOnSimpleAsserts
    {
        #region hidden
        object a = 1, b = 2, c = 3, d = 4;
        #endregion

        public Stateprinter CreatePrinter()
        {
            var printer = new Stateprinter();
            printer.Configuration
                .SetCulture(CultureInfo.CreateSpecificCulture("en-US"))
                .Test.SetAreEqualsMethod(NUnit.Framework.Assert.AreEqual)
                .Test.SetAutomaticTestRewrite(filename => new EnvironmentReader().UseTestAutoRewrite())
                .Test.SetAutomaticTestRewrite(filename => true)
                .Add(new StringConverter(""));

            return printer;
        }

        [Test]
        public void TestName()
        {
            var printer = CreatePrinter();
            var sut = new AmountSplitter();
            var actual = sut.Split(100, 3);

            printer.Assert.AreEqual("3", printer.PrintObject(actual.Length));
            printer.Assert.AreEqual("33.333333333333333333333333333", printer.PrintObject(actual[0]));
            printer.Assert.AreEqual("33.333333333333333333333333333", printer.PrintObject(actual[1]));
            printer.Assert.AreEqual("33.333333333333333333333333333", printer.PrintObject(actual[2]));
        }

        [Test]
        public void TestImprovedSyntax()
        {
            var assert = CreatePrinter().Assert;
            var sut = new AmountSplitter();
            var actual = sut.Split(100, 3);

            assert.PrintEquals("3", actual.Length);
            assert.PrintEquals("33.333333333333333333333333333", actual[0]);
            assert.PrintEquals("33.333333333333333333333333333", actual[1]);
            assert.PrintEquals("33.333333333333333333333333333", actual[2]);
        }



        [Test]
        public void TestProcessOrder()
        {
            var printer = CreatePrinter();

            var sut = new OrderProcessor(a, b);
            var actual = sut.Process(c, d);

            printer.Assert.AreEqual("1", printer.PrintObject(actual.OrderNumber));
            printer.Assert.AreEqual("X-mas present", printer.PrintObject(actual.OrderDescription));
            printer.Assert.AreEqual("43", printer.PrintObject(actual.Total));
        }

        [Test]
        public void TestProcessOrderImproved()
        {
            var assert = CreatePrinter().Assert;

            var sut = new OrderProcessor(a, b);
            var actual = sut.Process(c, d);

            assert.PrintEquals("1", actual.OrderNumber);
            assert.PrintEquals("X-mas present", actual.OrderDescription);
            assert.PrintEquals("43", actual.Total);
        }


        class AmountSplitter
        {
            public decimal[] Split(decimal amount, int parts)
            {
                var moneyBags = new decimal[parts];
                for (int i = 0; i < parts; i++)
                    moneyBags[i] = amount / parts;
                return moneyBags;
            }
        }

        class OrderProcessor
        {
            public OrderProcessor(object a, object b)
            {
            }

            public Order Process(object c, object d)
            {
                return new Order(1, "X-mas present");
            }
        }
    }

    class Order
    {
        public int OrderNumber;
        public string OrderDescription;
        public decimal Total = 43;

        public Order(int orderNumber, string orderDescription)
        {
            this.OrderNumber = orderNumber;
            this.OrderDescription = orderDescription;
        }
    }
}
