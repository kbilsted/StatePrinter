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

using System.Collections.Generic;
using NUnit.Framework;

namespace StatePrinter.Tests.IntegrationTests
{
  [TestFixture]
  class IEnumeratbleTest
  {
    readonly StatePrinter printer = new StatePrinter();

    [Test]
    public void StringArray()
    {
      Assert.AreEqual(
@"ROOT = <Int32[]>
ROOT[0] = 1
ROOT[1] = 2
ROOT[2] = 3
", printer.PrintObject(new int[]{1,2,3}));
    }


    [Test]
    public void Dictionary_person_address()
    {
      var d = new Dictionary<Person, Address>
              {
                {
                  new Person {Age = 37, FirstName = "Klaus", LastName = "Meyer"},
                  new Address() {Street = "Fairway Dr.", StreetNumber = 50267, Country = Country.USA, Zip = "CA 91601"}
                },
              };

      var expected =
@"ROOT = <Dictionary<Person, Address>>
ROOT[0] = <KeyValuePair<Person, Address>>
{
    key = <Person>
    {
        Age = 37
        FirstName = ""Klaus""
        LastName = ""Meyer""
    }
    value = <Address>
    {
        Street = ""Fairway Dr.""
        StreetNumber = 50267
        Zip = ""CA 91601""
        Country = USA
    }
}
";
      Assert.AreEqual(expected, printer.PrintObject(d));
    }


    class Person
    {
      public int Age;
      public string FirstName, LastName;
    }


    class Address
    {
      public string Street;
      public int StreetNumber;
      public string Zip;
      public Country Country;
    }


    enum Country
    {
      Denmark, USA,
    }
  }
}
