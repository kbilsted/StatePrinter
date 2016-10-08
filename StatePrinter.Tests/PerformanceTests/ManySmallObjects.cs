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
using System.Collections.Generic;
using NUnit.Framework;
using StatePrinting.Configurations;
using StatePrinting.OutputFormatters;

namespace StatePrinting.Tests.PerformanceTests
{
    [TestFixture]
    [Explicit]
    class ManySmallObjects : PerformanceTestsBase
    {
        const int N = 1000000;

        /// <summary>
        /// printing many times reveals the overhead of starting a print
        /// Version 2.20 - HPPavilion 7
        /// 3687
        /// 
        /// version 3.0.1
        /// 57
        /// </summary>
        [Test]
        public void TestTheOverheadOfStartingUp()
        {
            var toPrint = new Base();

            //var warmup = 
            new Stateprinter().PrintObject(toPrint);
            new Stateprinter().PrintObject(toPrint);
            new Stateprinter().PrintObject(toPrint);

            var mills = Time(
              () =>
              {
                  for (int i = 0; i < N/400; i++)
                  {
                      var printer = new Stateprinter();
                      printer.PrintObject(toPrint);
                  }
              });
            Console.WriteLine("  " + mills);
		}


        /// <summary>
        /// Many small objects reveals the overhead of introspecting types.
        /// 
        /// Version 1.04 - HPPavilion 7
        /// 1000:  130
        /// 2000:  29
        /// 4000:  56
        /// 8000:  122
        /// 16000:  259
        /// 32000:  497
        /// 64000:  1021
        /// 128000:  2062
        /// 256000:  4332
        /// 512000:  8567
        /// 1024000:  16878
        /// 
        /// 
        /// Version 2.00 - HPPavilion 7
        /// 1000:  132
        /// 2000:  26
        /// 4000:  58
        /// 8000:  122
        /// 16000:  257
        /// 32000:  510
        /// 64000:  1022
        /// 128000:  2064
        /// 256000:  4220
        /// 512000:  8426
        /// 1024000:  16904
        /// 
        /// 
        /// Version 2.2 - HPPavilion 7
        /// 1000:  158
        /// 2000:  8
        /// 4000:  19
        /// 8000:  50
        /// 16000:  86
        /// 32000:  175
        /// 64000:  371
        /// 128000:  781
        /// 256000:  1590
        /// 512000:  3217
        /// 1024000:  6265
        /// 
        /// 
        /// version 3.0.1
        ///     1000:  Time:    149 length      58003
        ///    2000:  Time:      5 length     116003
        ///    4000:  Time:      9 length     232003
        ///    8000:  Time:     20 length     464003
        ///   16000:  Time:     40 length     928003
        ///   32000:  Time:     93 length    1856003
        ///   64000:  Time:    208 length    3712003
        ///  128000:  Time:    408 length    7424003
        ///  256000:  Time:    896 length   14848003
        ///  512000:  Time:   1788 length   29696003
        /// 1024000:  Time:   4009 length   59392003
        /// 
        /// </summary>
        [Test]
        public void DumpManySmallObjects()
        {
            for (int i = 1000; i <= N * 2; i *= 2)
            {
                DumpNObjects(i);
            }
        }

		/// <summary>
		/// Printing 1.000.000 objects.
		/// curly: 6959 length:   62888908
		/// json:  6578 length:   59000006
		/// xml:   8036 length:   89000074
		/// 
		/// version 3.0.1
		/// Printing 1.000.000 objects.
		/// curly: 4762 length:   84888916
		/// json:  3581 length:   59000003
		/// xml:   4767 length:   93000035
		/// </summary>
		[Test]
        public void TiminAllOutputFormattersAtNElements()
        {
            //var warmup 
            new Stateprinter().PrintObject(new ToDump());
            new Stateprinter().PrintObject(new ToDump());
            new Stateprinter().PrintObject(new ToDump());

            var x = CreateObjectsToDump(N);
            int length = 0;
            Console.WriteLine("Printing {0:0,0} objects.", N);
            
            var curly = new Stateprinter();
            curly.Configuration.SetOutputFormatter(new CurlyBraceStyle(curly.Configuration));
            long time = Time(() => length = curly.PrintObject(x).Length);
            Console.WriteLine("curly: {0} length: {1,10}", time, length);

            var json = new Stateprinter();
            json.Configuration.SetOutputFormatter(new JsonStyle(json.Configuration));
            time = Time(() => length = json.PrintObject(x).Length);
            Console.WriteLine("json:  {0} length: {1,10}", time, length);

            var xml = new Stateprinter();
            xml.Configuration.SetOutputFormatter(new XmlStyle(xml.Configuration));
            time = Time(() => length = xml.PrintObject(x).Length);
            Console.WriteLine("xml:   {0} length: {1,10}", time, length);
        }

        private void DumpNObjects(int max)
        {
            List<ToDump> x = CreateObjectsToDump(max);

            var cfg = ConfigurationHelper.GetStandardConfiguration();
            cfg.OutputFormatter = new JsonStyle(cfg);
            int length = 0;
            var mills = Time(() =>
                             {
                                 var printer = new Stateprinter(cfg);
                                 length = printer.PrintObject(x).Length;
                             });
            Console.WriteLine("{0,8}:  Time: {1,6} length {2,10}", max, mills, length);
        }

        static List<ToDump> CreateObjectsToDump(int max)
        {
            var x = new List<ToDump>(max);
            for (int i = 0; i < max; i++)
            {
                x.Add(new ToDump());
            }
            return x;
        }

        internal class Base
        {
            internal string Boo = null;
        }


        private class ToDump : Base
        {
            internal string Poo = "dd";
        }
    }
}