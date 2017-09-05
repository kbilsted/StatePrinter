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
    public class ManySmallCollections: PerformanceTestsBase
    {
        const int N = 1000000;

        /// <summary>
        /// printing many times reveals the overhead of starting a print
        /// Version 3.0.0 - HPPavilion 7
        /// 3687
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
        //1000:  Time:    159 length     123003
        //2000:  Time:     18 length     246003
        //4000:  Time:     35 length     492003
        //8000:  Time:    164 length     984003
        //16000:  Time:    218 length    1968003
        //32000:  Time:    323 length    3936003
        //64000:  Time:    762 length    7872003
        //128000:  Time:   1523 length   15744003
        //256000:  Time:   2953 length   31488003
        //512000:  Time:   5260 length   62976003
        //1024000:  Time:  11159 length  125952003        
        /// </summary>
        [Test]
        public void DumpManySmallCollections()
        {
            for (int i = 1000; i <= N * 2; i *= 2)
            {
                DumpNObjects(i);
            }
        }

        /// <summary>
        /// Printing 1.000.000 objects.
        //curly: 17906 length:  195888912
        //json:  11433 length:  123000003
        //xml:   17106 length:  232000033
        /// </summary>
        [Test]
        public void TiminAllOutputFormattersAtNElements()
        {
            //var warmup 
            new Stateprinter().PrintObject(new ToDumpList());
            new Stateprinter().PrintObject(new ToDumpList());
            new Stateprinter().PrintObject(new ToDumpList());

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
            List<Base> x = CreateObjectsToDump(max);

            var cfg = ConfigurationHelper.GetStandardConfiguration();
            cfg.OutputFormatter = new JsonStyle(cfg);
            int length = 0;
            var mills = Time(() =>
                             {
                                 var printer = new Stateprinter(cfg);
                                 var res = printer.PrintObject(x);
                                 length = res.Length;
                             });
            Console.WriteLine("{0,8}:  Time: {1,6} length {2,10}", max, mills, length);
        }

        static List<Base> CreateObjectsToDump(int max)
        {
            var x = new List<Base>(max);
            for (int i = 0; i < max / 2; i++)
            {
                x.Add(new ToDumpList());
            }
            for (int i = 0; i < max / 2; i++)
            {
                x.Add(new ToDumpList());
            }
            return x;
        }

        internal class Base
        {
            internal string Boo = null;
        }

        private class ToDumpList : Base
        {
            internal List<string> Poos = new List<string>() { "dd", "bb", "cc" };
        }
        private class ToDumpDic : Base
        {
            internal Dictionary<string, bool> Poos = new Dictionary<string, bool>() {{"aa", false}, {"bb", true}};
        }
    }
}