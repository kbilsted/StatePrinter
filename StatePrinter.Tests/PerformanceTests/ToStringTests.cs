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
using System.Linq;

using NUnit.Framework;

using StatePrinter.ValueConverters;

namespace StatePrinter.Tests.PerformanceTests
{

    [TestFixture]
    [Explicit]
    class ToStringTests : PerformanceTestsBase
    {
        readonly int N = 200000;

        /* test scores
         * v2.1.220
            newStateprinter:     8550
            cachedPrinter:       3110
            warmCachedPrinter    3260
            tunedPrinter:        2240
            nativeWithLoop:        97
            nativeWithLinq:       161
         * v2.2.274 (extrene slowdown is due to the run-time code generation)
            newStateprinter:   274710
            cachedPrinter:       2200
            warmCachedPrinter    1770
            cachedPrinterArr:     970
            nativeWithLoop:       150
            nativeWithLinq:       220
         * v2.2.x (caching of generated getters)
            newStateprinter:     5275
            cachedPrinter:       1353
            tunedPrinter:         747
            nativeWithLoop:        95
            nativeWithLinq:       167
         
        */
        [Test]
        public void Testtiming()
        {
            var objects = new List<AClass>(N);
            for (int i = 0; i < N; i++)
                objects.Add(new AClass());

            // we assign and print the strings in order to ensure that we are actually executing the tostring code
            string lastString = null;
            var newStateprinter = Time(() => { foreach (var x in objects) lastString = x.UseNewStateprinter(); });
            Console.WriteLine(lastString + " = " + newStateprinter);

            var cachedPrinter = Time(() => { foreach (var x in objects) lastString = x.ReuseStateprinter(); });
            Console.WriteLine(lastString + " = " + cachedPrinter);

            var tunedPrinter = Time(() => { foreach (var x in objects) lastString = x.ReuseTunedPrinter(); });
            Console.WriteLine(lastString + " = " + tunedPrinter);

            var nativeWithLoop = Time(() => { foreach (var x in objects) lastString = x.NativeWithLoop(); });
            Console.WriteLine(lastString + " = " + nativeWithLoop);

            var nativeWithLinq = Time(() => { foreach (var x in objects) lastString = x.NativeWithLinq(); });
            Console.WriteLine(lastString + " = " + nativeWithLinq);

            Console.WriteLine("****************");
            Console.WriteLine("****************");
            Console.WriteLine("newStateprinter:  {0,7}", newStateprinter);
            Console.WriteLine("cachedPrinter:    {0,7}", cachedPrinter);
            Console.WriteLine("tunedPrinter:     {0,7}", tunedPrinter);
            Console.WriteLine("nativeWithLoop:   {0,7}", nativeWithLoop);
            Console.WriteLine("nativeWithLinq:   {0,7}", nativeWithLinq);
        }

        class AClass
        {
            string B = "hello";

            int[] C = { 5, 4, 3, 2, 1 };

            static readonly Stateprinter printer = new Stateprinter();
            static readonly Stateprinter tunedPrinter;

            static AClass()
            {
                tunedPrinter = new Stateprinter();

                var genericConverter = new GenericValueConverter<int[]>(y => string.Join(", ", y.Select(x => x.ToString()).ToArray()));
                tunedPrinter.Configuration.Add(genericConverter);
                tunedPrinter.PrintObject(new AClass()); // warmup
            }

            public string UseNewStateprinter()
            {
                return new Stateprinter().PrintObject(this);
            }

            public string ReuseStateprinter()
            {
                return printer.PrintObject(this);
            }

            public string ReuseTunedPrinter()
            {
                return tunedPrinter.PrintObject(this);
            }

            public string NativeWithLoop()
            {
                string result = "B = " + B;
                
                result += " C = ";
                if (C != null)
                {
                    if (C.Length > 0)
                    {
                        int i = 0;
                        for (; i < C.Length - 1; i++) 
                            result += C[i] + ", ";
                        result += C[i];
                    }
                }
                else
                    result += "null";
                return result;
            }

            public string NativeWithLinq()
            {
                return string.Format("B = {0} C = {1}", 
                    B, 
                    C == null 
                        ? "null" 
                        : string.Join(", ", C.Select(x => x.ToString()).ToArray()));
            }
        }
    }
}