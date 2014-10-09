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
using System.Linq;
using NUnit.Framework;
using StatePrinter.Configurations;
using StatePrinter.FieldHarvesters;

namespace StatePrinter.Tests.FieldHarvesters
{
  /// <summary>
  /// Shows how the <see cref="SelectiveHarvester"/> can be utilized in unit tests
  /// </summary>
  [TestFixture]
  class SelectiveHarvesterTest
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

    [SetCulture("da-DK")]
    [Test]
    public void TestFluintInterface()
    {
      var cfg = ConfigurationHelper.GetStandardConfiguration(" ");
      cfg.SelectiveHarvester().Exclude<A>(x => x.X, x => x.Y);
      var printer = new StatePrinter(cfg);

      var state = printer.PrintObject(new A {X = DateTime.Now, Name = "Charly"});
      Assert.AreEqual(@"new A(){ Name = ""Charly""}", state.Replace("\r\n", ""));

      state = printer.PrintObject(new B {X = DateTime.Now, Name = "Charly", Age = 43});
      Assert.AreEqual(@"new B(){ Name = ""Charly"" Age = 43}", state.Replace("\r\n", ""));

      state = printer.PrintObject(new C {X = new DateTime(2010, 9, 8)});
      Assert.AreEqual(@"new C(){ X = 08-09-2010 00:00:00}", state.Replace("\r\n", ""));
    }

    [SetCulture("da-DK")]
    [Test]
    public void UserStory()
    {
      var cfg = ConfigurationHelper.GetStandardConfiguration(" ");
      cfg.SelectiveHarvester()
        .AddFilter<A>(x => x.Where(y => y.SanitizedName != "X" && y.SanitizedName != "Y"));

      var printer = new StatePrinter(cfg);

      var state = printer.PrintObject(new A {X = DateTime.Now, Name = "Charly"});
      Assert.AreEqual(@"new A(){ Name = ""Charly""}", state.Replace("\r\n", ""));

      state = printer.PrintObject(new B {X = DateTime.Now, Name = "Charly", Age = 43});
      Assert.AreEqual(@"new B(){ Name = ""Charly"" Age = 43}", state.Replace("\r\n", ""));

      state = printer.PrintObject(new C {X = new DateTime(2010, 9, 8)});
      Assert.AreEqual(@"new C(){ X = 08-09-2010 00:00:00}", state.Replace("\r\n", ""));
    }


    [TestFixture]
    class AddFilter
    {
      [Test]
      public void ExcludeFilter_ForTypesAndSubtypes()
      {
        var selective = new SelectiveHarvester();
        selective.AddFilter<A>(
          x => x.Where(y => y.SanitizedName != "X" && y.SanitizedName != "Y"));

        Asserts(selective);
      }

      [Test]
      [ExpectedException(typeof (ArgumentException),
        ExpectedMessage = "Type A has already been configured as an excluder.")]
      public void AddFilter_OnAlreadyExclude_Fail()
      {
        var harvester = new SelectiveHarvester();
        harvester.Exclude<A>(x => x.X);
        harvester.AddFilter<A>(x => null);
      }

      [Test]
      [ExpectedException(typeof (ArgumentException),
        ExpectedMessage = "Type A has already been configured as an includer.")]
      public void AddFilter_OnAlreadyInclude_Fail()
      {
        var harvester = new SelectiveHarvester();
        harvester.Include<A>(x => x.X);
        harvester.AddFilter<A>(x => null);
      }
    }




    [TestFixture]
    class Exclude
    {
      [Test]
      public void WrongSpec_Fail()
      {
        var harvester = new SelectiveHarvester();
        var ex =
          Assert.Throws<ArgumentException>(() => harvester.Exclude<A>(x => Math.Min(3, 3)));
        Assert.AreEqual("Field specification must refer to a field", ex.Message);

        ex = Assert.Throws<ArgumentException>(() => harvester.Exclude<A>(x => 1));
        Assert.AreEqual("Field specification must refer to a field", ex.Message);
      }

      [Test]
      [ExpectedException(typeof (ArgumentException),
        ExpectedMessage =
          "Field 'Year' is declared on type 'DateTime' not on argument: 'A'")]
      public void AddExclude_FieldOnDifferentType_Fail()
      {
        var harvester = new SelectiveHarvester();
        harvester.Exclude<A>(x => x.X.Year);
      }


      [Test]
      public void Fields_WorksForTypesAndSubtypes()
      {
        var harvester = new SelectiveHarvester();
        harvester.Exclude<A>(x => x.X)
          .Exclude<A>(x => x.Y);

        Asserts(harvester);
      }

      [Test]
      public void Fieldsarr_WorksForTypesAndSubtypes()
      {
        var harvester = new SelectiveHarvester();
        harvester.Exclude<A>(x => x.X, x => x.Y);

        Asserts(harvester);
      }

      [Test]
      public void Exclude_FieldInSuperclass_WorksForTypesAndSubtypes()
      {
        var harvester = new SelectiveHarvester();
        harvester.Exclude<B>(x => x.Name);

        IFieldHarvester fh = (IFieldHarvester) harvester;
        Assert.IsFalse(fh.CanHandleType(typeof (A)));

        Assert.IsTrue(fh.CanHandleType(typeof (B)));
        var fields = fh.GetFields(typeof (B)).Select(x => x.SanitizedName);
        CollectionAssert.AreEquivalent(new[] {"X", "Y", "Age"}, fields);

        Assert.IsFalse(fh.CanHandleType(typeof (C)));
      }


      [Test]
      [ExpectedException(typeof (ArgumentException),
        ExpectedMessage = "Type A has already been configured as a filter.")]
      public void Exclude_OnAlreadyFilter_Fail()
      {
        var harvester = new SelectiveHarvester();
        harvester.AddFilter<A>(x => null);
        harvester.Exclude<A>(x => x.X);
      }

      [Test]
      [ExpectedException(typeof (ArgumentException),
        ExpectedMessage = "Type A has already been configured as an includer.")]
      public void Exclude_OnAlreadyInclude_Fail()
      {
        var harvester = new SelectiveHarvester();
        harvester.Include<A>(x => x.X);
        harvester.Exclude<A>(x => x.X);
      }
    }



    [TestFixture]
    class Include
    {
      [Test]
      public void Include_Fieldsarr_WorksForTypesAndSubtypes()
      {
        var harvester = new SelectiveHarvester();
        harvester.Include<B>(x => x.Name, x => x.Age);


        IFieldHarvester fh = (IFieldHarvester) harvester;
        Assert.IsFalse(fh.CanHandleType(typeof (A)));
        Assert.IsTrue(fh.CanHandleType(typeof (B)));
        var fields = fh.GetFields(typeof (B)).Select(x => x.SanitizedName);
        CollectionAssert.AreEquivalent(new[] {"Name", "Age"}, fields);

        Assert.IsFalse(fh.CanHandleType(typeof (C)));
      }

      [Test]
      [ExpectedException(typeof (ArgumentException),
        ExpectedMessage = "Type A has already been configured as an excluder.")]
      public void Include_OnAlreadyInclude_Fail()
      {
        var harvester = new SelectiveHarvester();
        harvester.Exclude<A>(x => x.X);
        harvester.Include<A>(x => x.X);
      }
    }

    static void Asserts(SelectiveHarvester harvester)
    {
      IFieldHarvester fh = (IFieldHarvester) harvester;
      Assert.IsTrue(fh.CanHandleType(typeof (A)));
      var fields = fh.GetFields(typeof (A)).Select(x => x.SanitizedName);
      CollectionAssert.AreEquivalent(new[] {"Name"}, fields);

      Assert.IsTrue(fh.CanHandleType(typeof (B)));
      fields = fh.GetFields(typeof (B)).Select(x => x.SanitizedName);
      CollectionAssert.AreEquivalent(new[] {"Name", "Age"}, fields);

      Assert.IsFalse(fh.CanHandleType(typeof (C)));
    }

  }
}