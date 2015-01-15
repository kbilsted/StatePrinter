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

using NUnit.Framework;
using StatePrinter.Configurations;
using StatePrinter.FieldHarvesters;

namespace StatePrinter.Tests.FieldHarvesters
{
  /// <summary>
  /// Shows how the <see cref="ProjectionHarvester"/> can be utilized in unit tests
  /// </summary>
  [TestFixture]
  class ToStringAwareHarvesterTest
  {
    class A
    {
      public int X;
      public B b = new B() { Age = 2 };
    }

    class B 
    {
      public int Age;

      public override string ToString()
      {
        return "My age is " + Age;
      }
    }

    [Test]
    public void Userstory_PrintUseToString_WhenDirectlyAvailable()
    {
      var sut = GetPrinter();
      var expected = @"new B()
{
 ToString() = ""My age is 1""
}
";
      var actual = sut.PrintObject(new B { Age = 1 });
      Assert.AreEqual(expected, actual);
    }




    [Test]
    public void Userstory_PrintUseToString_WhenAvailable()
    {
      var sut = GetPrinter();
      var expected = @"new A()
{
 X = 1
 b = new B()
 {
  ToString() = ""My age is 2""
 }
}
";
      var actual = sut.PrintObject(new A { X = 1 });
      Assert.AreEqual(expected, actual);
    }


    Stateprinter GetPrinter()
    {
      Configuration cfg = ConfigurationHelper.GetStandardConfiguration(" ");
      cfg.Add(new ToStringAwareHarvester());

      var sut = new Stateprinter(cfg);
      return sut;
    }
  }
}