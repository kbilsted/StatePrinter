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
using StatePrinter.Configurations;


namespace StatePrinter.TestAssistance
{
    /// <summary>
    /// How to call the testing framework the user is using
    /// </summary>
    public delegate void TestFrameworkAreEqualsMethod(string actual, string expected, string message);

    /// <summary>
    /// Helper class for writing asserts that help output the actual output in a c#-escaped format.
    /// This makes it easy in case of failed test to see locally what went wrong, and see/copy the actual output.
    /// </summary>
    public class Asserter
    {
        readonly Stateprinter printer;
        
        public Configuration Configuration { get { return printer.Configuration; } }
        
        internal Asserter(Stateprinter printer)
        {
            this.printer = printer;
        }

        /// <summary>
        /// Assert that two strings are the same using the configures asserter method. Upon a failure, a suggested string for correcting the test
        /// is printed. 
        /// </summary>
        public void AreEqual(string expected, string actual)
        {
            if (expected == actual)
                return;
            
            var escapedActual = Escape(actual);
            var newExpected = string.Format("var expected = {0};", escapedActual);
            var message = string.Format("{0}{0}Proposed output for unit test:{0}{0}{1}{0}", Environment.NewLine, newExpected);

            var reflector = new CallStackReflector();
            var info = reflector.TryGetLocation();
            
            CallUnderlyingAssert(expected, actual, info, message, escapedActual);
        }

        void CallUnderlyingAssert(
            string expected,
            string actual,
            UnitTestLocationInfo info,
            string message,
            string escapedActual)
        {
            if (info == null)
            {
                Configuration.AreEqualsMethod(expected, actual, message);
                return;
            }

            if (printer.Configuration.AutomaticTestRewrite(info.Filepath))
            {
                new TestRewriter(Configuration.FactoryFileRepository)
                    .RewriteTest(info, expected, escapedActual);
                
                message =
                    "Rewritting test expectations in '" 
                    + info.Filepath 
                    + "'."+ @"
Compile and re-run to see green lights.
New expectations:
"
                    + escapedActual;
            }

            Configuration.AreEqualsMethod(expected, actual, message);
        }

        /// <summary>
        /// Assert that two strings are the "same" ignoring differences in line ending characters \r, \n. 
        /// For all practical purposes, this method rectifies some of the many problems with source files stored in 
        /// different methods on diffrent operating systems.
        /// <para>
        /// This method calls <see cref="AreEqual"/> after first unifiying the line endings. "\r" and "\r\n" are changed into "\n"
        /// </para>
        /// <para>
        /// Upon a failure, a suggested string for correcting the test is printed.
        /// </para>
        /// </summary>
        public void IsSame(string expected, string actual)
        {
            AreEqual(UnifyNewLines(expected), UnifyNewLines(actual));
        }

        /// <summary>
        /// Shortcut method for printing <param name="objectToPrint"></param> using the stateprinter and call <see cref="IsSame"/> on the result.
        /// </summary>
        public void PrintIsSame(string expected, object objectToPrint)
        {
            IsSame(expected, printer.PrintObject(objectToPrint));
        }
        
        /// <summary>
        /// Shortcut method for printing <param name="objectToPrint"></param> using the stateprinter and call <see cref="IsSame"/> on the result.
        /// </summary>
        public void PrintEquals(string expected, object objectToPrint)
        {
            AreEqual(expected, printer.PrintObject(objectToPrint));
        }

        string UnifyNewLines(string text)
        {
            return text
                .Replace("\r\n", "\n")
                .Replace("\r", "\n");
        }

        string Escape(string actual)
        {
            var needEscaping = actual.Contains("\"") || actual.Contains("\n");
            if(needEscaping)
               return string.Format("@\"{0}\"", actual.Replace("\"", "\"\""));
            return string.Format("\"{0}\"", actual);
        }

        /// <summary>
        /// Emulate Nunits Assert.That
        /// </summary>
        public void That(string actual, Expected expected)
        {
            AreEqual(expected.ExpectedValue, actual);
        }
    }

    /// <summary>
    /// Emulate Nunits Is.EqualTo() 
    /// </summary>
    public class Expected
    {
        public string ExpectedValue;
    }

    /// <summary>
    /// Emulate Nunits Is.EqualTo() 
    /// </summary>
    public static class Is
    {
        /// <summary>
        /// Different syntax for calling Assert.AreEquals
        /// </summary>
        public static Expected EqualTo(string exptected)
        {
            return new Expected() { ExpectedValue = exptected };
        }
    }
}

