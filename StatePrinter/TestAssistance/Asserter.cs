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
using StatePrinting.Configurations;
using StatePrinting.FieldHarvesters;

namespace StatePrinting.TestAssistance
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
        readonly StringUtils stringUtils = new StringUtils();

        /// <summary>
        /// The StatePrinter configuration
        /// </summary>
        public Configuration Configuration { get { return printer.Configuration; } }
        
        /// <summary>
        /// Access an instance of <see cref="ProjectionHarvester"/> which can be configured to include or exclude a number of fields from a specified type.
        /// </summary>
        public ProjectionHarvester Project { get { return printer.Configuration.Project; } }

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
            
            var reflector = new CallStackReflector();
            var info = reflector.TryGetLocation();
            
            CallUnderlyingAssert(expected, actual, info);
        }

        /// <summary>
        /// Shortcut method for printing <param name="objectToPrint"></param> using the stateprinter and call <see cref="AreEqual"/> on the result.
        /// </summary>
        public void PrintEquals(string expected, object objectToPrint)
        {
            AreEqual(expected, printer.PrintObject(objectToPrint));
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
        public void AreAlike(string expected, string actual)
        {
            AreEqual(stringUtils.UnifyNewLines(expected), stringUtils.UnifyNewLines(actual));
        }


        /// <summary>
        /// Shortcut method for printing <param name="objectToPrint"></param> using the stateprinter and call <see cref="AreAlike"/> on the result.
        /// </summary>
        public void PrintAreAlike(string expected, object objectToPrint)
        {
            AreAlike(expected, printer.PrintObject(objectToPrint));
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
        [Obsolete("Instead use AreAlike(). The IsSame() has a special meaning unit testing frameworks that we do not follow. E.g. NUnit: http://www.nunit.org/index.php?p=identityAsserts&r=2.6.3 and XUnit: https://github.com/xunit/xunit/blob/master/src/xunit.assert/Asserts/IdentityAsserts.cs  . Hence its name is confusing.")]
        public void IsSame(string expected, string actual)
        {
            AreAlike(expected, actual);
        }

        /// <summary>
        /// Shortcut method for printing <param name="objectToPrint"></param> using the stateprinter and call <see cref="IsSame"/> on the result.
        /// </summary>
        [Obsolete("Instead use PrintAreAlike(). The IsSame() has a special meaning unit testing frameworks that we do not follow. E.g. NUnit: http://www.nunit.org/index.php?p=identityAsserts&r=2.6.3 and XUnit: https://github.com/xunit/xunit/blob/master/src/xunit.assert/Asserts/IdentityAsserts.cs  . Hence its name is confusing.")]
        public void PrintIsSame(string expected, object objectToPrint)
        {
            IsSame(expected, printer.PrintObject(objectToPrint));
        }

        /// <summary>
        /// Emulate Nunits Assert.That
        /// </summary>
        public void That(string actual, Expected expected)
        {
            switch (expected.Kind)
            {
                case Expected.ComparisonKind.AreEquals:
                   AreEqual(expected.ExpectedValue, actual);
                    break;
                case Expected.ComparisonKind.AreAlike:
                   AreAlike(expected.ExpectedValue, actual);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        void CallUnderlyingAssert(
          string expected,
          string actual,
          UnitTestLocationInfo info)
        {
            var escapedActual = stringUtils.Escape(actual);
            bool rewriteTest = info != null 
                && printer.Configuration.Test.AutomaticTestRewrite(info);

            var message = printer.Configuration.Test.AssertMessageCreator(
                expected,
                actual,
                escapedActual,
                rewriteTest,
                info);

            if (rewriteTest)
            {
                var rewriter = new TestRewriter(Configuration.FactoryFileRepository);
                rewriter.RewriteTest(
                    info,
                    expected,
                    escapedActual);
            }

            Configuration.Test.AreEqualsMethod(expected, actual, message);
        }
    }

    /// <summary>
    /// Emulate Nunits Is.EqualTo() 
    /// </summary>
    public class Expected
    {
        public ComparisonKind Kind = ComparisonKind.AreEquals;
        public string ExpectedValue;

        public enum ComparisonKind { AreEquals, AreAlike }
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

        /// <summary>
        /// Different syntax for calling Assert.AreAlike
        /// </summary>
        public static Expected AlikeTo(string exptected)
        {
            return new Expected() { ExpectedValue = exptected, Kind = Expected.ComparisonKind.AreAlike };
        }
    }
}

