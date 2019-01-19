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
using System.Linq;
using NUnit.Framework;
using StatePrinting.Configurations;
using StatePrinting.FieldHarvesters;

namespace StatePrinting.Tests.FieldHarvesters
{
    /// <summary>
    /// Shows how the <see cref="ProjectionHarvester"/> can be utilized in unit tests
    /// </summary>
    [TestFixture]
    class ProjectionHarvesterTest
    {
        class A
        {
            public DateTime X;

            public DateTime Y { get; set; }

            public string Name;
        }


        class B : A
        {
            public int Age;
        }


        class C
        {
            public DateTime X;
        }

        [Test]
        public void Obsolete_TestFluintInterface_exclude()
        {
            var cfg = TestHelper.CreateTestConfiguration();
            cfg.Projectionharvester().Exclude<A>(x => x.X, x => x.Y);
            var printer = new Stateprinter(cfg);

            var state = printer.PrintObject(new A { X = DateTime.Now, Name = "Charly" });
            Assert.AreEqual(@"new A() { Name = ""Charly"" }", state);
        }

        [Test]
        public void TestFluintInterface_exclude()
        {
            var cfg = TestHelper.CreateTestConfiguration();
            cfg.Project.Exclude<A>(x => x.X, x => x.Y);
            var printer = new Stateprinter(cfg);

            var state = printer.PrintObject(new A { X = DateTime.Now, Name = "Charly" });
            Assert.AreEqual(@"new A() { Name = ""Charly"" }", state);

            state = printer.PrintObject(new B { X = DateTime.Now, Name = "Charly", Age = 43 });
            Assert.AreEqual(@"new B() { Name = ""Charly"" Age = 43 }", state);

            state = printer.PrintObject(new C { X = new DateTime(2010, 9, 8) });
            Assert.AreEqual(@"new C() { X = 08-09-2010 00:00:00 }", state);
        }

        [Test]
        public void TestFluintInterface_Include()
        {
            var cfg = TestHelper.CreateTestConfiguration();
            cfg.Project.Include<A>(x => x.Name);
            var printer = new Stateprinter(cfg);

            var state = printer.PrintObject(new A { X = DateTime.Now, Name = "Charly" });
            Assert.AreEqual(@"new A() { Name = ""Charly"" }", state);

            state = printer.PrintObject(new B { X = DateTime.Now, Name = "Charly", Age = 43 });
            Assert.AreEqual(@"new B() { Name = ""Charly"" }", state);

            state = printer.PrintObject(new C { X = new DateTime(2010, 9, 8) });
            Assert.AreEqual(@"new C() { X = 08-09-2010 00:00:00 }", state);
        }

        [Test]
        public void UserStory()
        {
            var cfg = TestHelper.CreateTestConfiguration();
            cfg.Project.AddFilter<A>(x => x.Where(y => y.SanitizedName != "X" && y.SanitizedName != "Y"));

            var printer = new Stateprinter(cfg);

            var state = printer.PrintObject(new A { X = DateTime.Now, Name = "Charly" });
            Assert.AreEqual(@"new A() { Name = ""Charly"" }", state);

            state =
                printer.PrintObject(new B { X = DateTime.Now, Name = "Charly", Age = 43 });
            Assert.AreEqual(@"new B() { Name = ""Charly"" Age = 43 }", state);

            state = printer.PrintObject(new C { X = new DateTime(2010, 9, 8) });
            Assert.AreEqual(@"new C() { X = 08-09-2010 00:00:00 }", state);
        }


        [TestFixture]
        class AddFilter
        {
            [Test]
            public void AddFilter_WorksForTypesAndSubtypes()
            {
                var selective = new ProjectionHarvester();
                selective.AddFilter<A>(x => x.Where(y => y.SanitizedName != "X" && y.SanitizedName != "Y"));

                Asserts(selective);
            }

            [Test]
            public void AddFilter_OnAlreadyInclude_Success()
            {
                var harvester = new ProjectionHarvester();
                harvester.AddFilter<A>(x => null);
                harvester.AddFilter<A>(x => null);
            }

            [Test]
            public void AddFilter_OnAlreadyExclude_Fail()
            {
                var harvester = new ProjectionHarvester();
                harvester.Exclude<A>(x => x.X);
                var ex = Assert.Throws<ArgumentException>(()=>harvester.AddFilter<A>(x => null));
                Assert.AreEqual("Type A has already been configured as an excluder.", ex.Message);
            }

            [Test]
            public void AddFilter_OnAlreadyInclude_Fail()
            {
                var harvester = new ProjectionHarvester();
                harvester.Include<A>(x => x.X);
                var ex = Assert.Throws<ArgumentException>(() => harvester.AddFilter<A>(x => null));
                Assert.AreEqual("Type A has already been configured as an includer.", ex.Message);
            }
        }




        [TestFixture]
        class Exclude
        {
            [Test]
            public void WrongSpec_Fail()
            {
                var harvester = new ProjectionHarvester();
                var ex = Assert.Throws<ArgumentException>(() => harvester.Exclude<A>(x => Math.Min(3, 3)));
                Assert.AreEqual("Field specification must refer to a field", ex.Message);

                ex = Assert.Throws<ArgumentException>(() => harvester.Exclude<A>(x => 1));
                Assert.AreEqual("Field specification must refer to a field", ex.Message);
            }

            [Test]
            public void AddExclude_FieldOnDifferentType_Fail()
            {
                var harvester = new ProjectionHarvester();
                var ex = Assert.Throws<ArgumentException>(() => harvester.Exclude<A>(x => x.X.Year));
                Assert.AreEqual("Field 'Year' is declared on type 'DateTime' not on argument: 'A'", ex.Message);
            }


            [Test]
            public void Fields_WorksForTypesAndSubtypes()
            {
                var harvester = new ProjectionHarvester();
                harvester
                    .Exclude<A>(x => x.X)
                    .Exclude<A>(x => x.Y);

                Asserts(harvester);
            }

            [Test]
            public void Fields_params_WorksForTypesAndSubtypes()
            {
                var harvester = new ProjectionHarvester();
                harvester.Exclude<A>(x => x.X, x => x.Y);

                Asserts(harvester);
            }

            [Test]
            public void Exclude_FieldInSuperclass_WorksForTypesAndSubtypes()
            {
                var harvester = new ProjectionHarvester();
                harvester.Exclude<B>(x => x.Name);

                IFieldHarvester fh = (IFieldHarvester)harvester;
                Assert.IsFalse(fh.CanHandleType(typeof(A)));

                Assert.IsTrue(fh.CanHandleType(typeof(B)));
                var fields = fh.GetFields(typeof(B)).Select(x => x.SanitizedName);
                CollectionAssert.AreEquivalent(new[] { "X", "Y", "Age" }, fields);

                Assert.IsFalse(fh.CanHandleType(typeof(C)));
            }


            [Test]
            public void Exclude_OnAlreadyFilter_Fail()
            {
                var harvester = new ProjectionHarvester();
                harvester.AddFilter<A>(x => null);
                var ex = Assert.Throws<ArgumentException>(() => harvester.Exclude<A>(x => x.X));
                Assert.AreEqual("Type A has already been configured as a filter.", ex.Message);
            }

            [Test]
            public void Exclude_OnAlreadyInclude_Fail()
            {
                var harvester = new ProjectionHarvester();
                harvester.Include<A>(x => x.X);
                var ex = Assert.Throws<ArgumentException>(() => harvester.Exclude<A>(x => x.X));
                Assert.AreEqual("Type A has already been configured as an includer.", ex.Message);
            }
        }


        [TestFixture]
        class Include
        {
            [Test]
            public void Include_Fieldsarr_WorksForTypesAndSubtypes()
            {
                var harvester = new ProjectionHarvester();
                harvester.Include<B>(x => x.Name, x => x.Age);

                IFieldHarvester fh = (IFieldHarvester)harvester;
                Assert.IsFalse(fh.CanHandleType(typeof(A)));
                Assert.IsTrue(fh.CanHandleType(typeof(B)));
                var fields = fh.GetFields(typeof(B)).Select(x => x.SanitizedName);
                CollectionAssert.AreEquivalent(new[] { "Name", "Age" }, fields);

                Assert.IsFalse(fh.CanHandleType(typeof(C)));
            }

            [Test]
            public void Include_OnAlreadyInclude_Fail()
            {
                var harvester = new ProjectionHarvester();
                harvester.Exclude<A>(x => x.X);
                var ex = Assert.Throws<ArgumentException>(() => harvester.Include<A>(x => x.X));
                Assert.AreEqual("Type A has already been configured as an excluder.", ex.Message);
            }

            interface IF
            {
                int I { get; set; } 
            }

            class F : IF
            {
                public int j, k;

                public int I { get; set; }
                public int Sum
                {
                    get
                    {
                        return I + j;
                    }
                }
            }

            [Test]
            public void Include_Getter()
            {
                F f = new F() { I = 1, j = 2, k = 4 };

                var assert = TestHelper.CreateShortAsserter();
                assert.Project.Include<F>(x => x.I, x => x.Sum);
                assert.PrintEquals(@"new F() { I = 1 Sum = 3 }", f);
            }

            [Test]
            public void Include_OnlyInterfaceType()
            {
                F f = new F() { I = 1, j = 2, k = 4 };

                var assert = TestHelper.CreateShortAsserter();
                assert.Project.IncludeByType<F, IF>();
                assert.PrintEquals(@"new F() { I = 1 }", f);
            }

            [Test]
            public void IncludeSubsetOfFields()
            {
                B b1 = new B() { Age = 1, Name = "m", X = DateTime.Now, Y = DateTime.Now };

                var stateprinter =
                    new Stateprinter(
                        ConfigurationHelper.GetStandardConfiguration()
                            .Add(
                                new ProjectionHarvester().Include<B>(
                                    x => x.Name,
                                    x => x.Age)));
                stateprinter.Configuration.Test.SetAreEqualsMethod(Assert.AreEqual);

                var expected = @"new B()
{
    Name = ""m""
    Age = 1
}";
                var actual = stateprinter.PrintObject(b1);
                stateprinter.Assert.AreEqual(expected, actual);

                // in array
                expected = @"new B[]()
{
    [0] = new B()
    {
        Name = ""m""
        Age = 1
    }
    [1] = new B()
    {
        Name = ""a""
        Age = 2
    }
}";
                B b2 = new B() { Age = 2, Name = "a", X = DateTime.Now, Y = DateTime.Now };
                actual = stateprinter.PrintObject(new[] { b1, b2 });
                stateprinter.Assert.AreEqual(expected, actual);
            }
        }

        static void Asserts(ProjectionHarvester harvester)
        {
            IFieldHarvester fh = (IFieldHarvester)harvester;
            Assert.IsTrue(fh.CanHandleType(typeof(A)));
            var fields = fh.GetFields(typeof(A)).Select(x => x.SanitizedName);
            CollectionAssert.AreEquivalent(new[] { "Name" }, fields);

            Assert.IsTrue(fh.CanHandleType(typeof(B)));
            fields = fh.GetFields(typeof(B)).Select(x => x.SanitizedName);
            CollectionAssert.AreEquivalent(new[] { "Name", "Age" }, fields);

            Assert.IsFalse(fh.CanHandleType(typeof(C)));
        }
    }
}
