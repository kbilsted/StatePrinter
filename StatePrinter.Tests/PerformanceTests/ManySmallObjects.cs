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
using System.Diagnostics;
using NUnit.Framework;
using StatePrinter.Configurations;
using StatePrinter.OutputFormatters;

namespace StatePrinter.Tests.PerformanceTests
{
  [TestFixture]
  [Explicit]
  internal class ManySmallObjects
  {
    private const int N = 1000000;

    /// <summary>
    /// printing many times reveals the overhead of starting a print
    /// </summary>
    [Test]
    public void TestManyPrintings()
    {
      var toPrint = new Base();
      var printer = new Stateprinter();

      for (int i = 0; i < N; i++)
        printer.PrintObject(toPrint);
    }


    /// <summary>
    /// Many small objects reveals the overhead of introspecting types.
    /// </summary>
    [Test]
    public void DumpManySmallObjects()
    {
      for (int i = 1000; i <= N*2; i *= 2)
      {
        DumpNObjects(i);
      }
    }

    private void DumpNObjects(int max)
    {
      var x = new List<ToDump>();
      for (int i = 0; i < max; i++)
        x.Add(new ToDump());

      var cfg = ConfigurationHelper.GetStandardConfiguration();
      cfg.OutputFormatter = new JsonStyle(cfg.IndentIncrement);
      var mills = time(() =>
                       {
                         var printer = new Stateprinter(cfg);
                         printer.PrintObject(x);
                       });
      //Console.WriteLine(max + ":  " + mills);
    }


    internal class Base
    {
      internal string Boo;
    }


    private class ToDump : Base
    {
      internal string Poo = "dd";
    }



    private long time(Action a)
    {
      var watch = new Stopwatch();
      watch.Start();
      a();
      watch.Stop();
      return watch.ElapsedMilliseconds;
    }
  }

}
