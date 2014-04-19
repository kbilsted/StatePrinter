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

namespace StatePrinter.Tests.PerformanceTests
{
  [TestFixture]
  [Explicit]
  class ManySmallObjects
  {
    private const int N = 1000000;

    /// <summary>
    /// printing many times reveals the overhead of starting a print
    /// </summary>
    [Test]
    public void TestManyPrintings()
    {
      var toPrint = new Base();
      var printer = new StatePrinter();

      for (int i = 0; i < N; i++)
        printer.PrintObject(toPrint);
    }


    /// <summary>
    /// Many small objects reveals the overhead of introspecting types.
    /// </summary>
    [Test]
    public void DumpManySmallObjects()
    {
      var x = new List<ToDump>();
      for (int i = 0; i < N; i++)
      {
        x.Add(new ToDump());    
      }

      var printer = new StatePrinter();
      printer.PrintObject(x);
    }

    internal class Base
    {
      internal string Boo;
    }
    
    class ToDump : Base
    {
      internal string Poo = "dd";
    }

  }


}
