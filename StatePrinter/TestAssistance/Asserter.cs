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
        readonly TestFrameworkAreEqualsMethod assert;

        public Asserter(TestFrameworkAreEqualsMethod assert)
        {
            this.assert = assert;
        }

        /// <summary>
        /// Emulate Nunits Assert.AreEqual
        /// </summary>
        public void AreEqual(string expected, string actual)
        {
            var message = string.Format("{0}{0}Proposed output for unit test:{0}{1}{0}", Environment.NewLine, Escape(actual));
            assert(expected, actual, message);
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
}

namespace StatePrinter
{
    using StatePrinter.TestAssistance;

    /// <summary>
    /// Emulate Nunits Is.EqualTo() 
    /// </summary>
    public static class Is
    {
        public static Expected EqualTo(string exptected)
        {
            return new Expected() { ExpectedValue = exptected };
        }
    }

}