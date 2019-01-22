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
using System.Data.SqlClient;
using StatePrinting.TestAssistance;

namespace StatePrinting.Configurations
{
    public class TestingBehaviour
    {
        readonly Configuration configuration;

        public TestingBehaviour(Configuration configuration)
        {
            this.configuration = configuration;
        }

        /// <summary>
        /// Configure how to call AreEquals in the unit testing framework of your choice. 
        /// Only set this field if you are using the <see cref="Stateprinter.Assert"/> functionality.
        /// </summary>
        public Configuration SetAreEqualsMethod(TestFrameworkAreEqualsMethod areEqualsMethod)
        {
            if (areEqualsMethod == null)
                throw new ArgumentNullException("areEqualsMethod");
            AreEqualsMethod = areEqualsMethod;

            return configuration;
        }

        private static readonly object[] NoArg = new object[0];

        /// <summary>
        /// Configure how to call AreEquals in the unit testing framework of your choice. 
        /// Only set this field if you are using the <see cref="Stateprinter.Assert"/> functionality.
        /// This overload is for easy integration with NUnit 3.x Asert.AreEqual
        /// </summary>
        public Configuration SetAreEqualsMethod(Action<string,string,string,object[]> nunitAreEqualsMethod)
        {
            if (nunitAreEqualsMethod == null)
                throw new ArgumentNullException("nunitAreEqualsMethod");
            AreEqualsMethod = new TestFrameworkAreEqualsMethod((actual, expected, message) => nunitAreEqualsMethod(actual,expected,message,NoArg));

            return configuration;
        }

        /// <summary>
        /// Configure how to call AreEquals in the unit testing framework of your choice. 
        /// Only set this field if you are using the <see cref="Stateprinter.Assert"/> functionality.
        /// </summary>
        public TestFrameworkAreEqualsMethod AreEqualsMethod { get; private set; }

        /// <summary>
        /// The signature for finding out if a test's expected value may be automatically re-written.
        /// </summary>
        /// <returns>True if the test may be rewritten with the new expected value to make the test pass again.</returns>
        public delegate bool TestRewriteIndicator(UnitTestLocationInfo location);

        /// <summary>
        /// Evaluate the function for each failing test. <para></para>
        /// Your function can rely on anything such as an environment variable or a file on the file system. <para></para> 
        /// If you only want to do this evaluation once pr. test suite execution you should wrap your function in a Lazy&lt;&gt;
        /// </summary>
        public TestRewriteIndicator AutomaticTestRewrite { get; private set; }

        /// <summary>
        /// Evaluate the function for each failing test. <para></para>
        /// Your function can rely on anything such as an environment variable or a file on the file system. <para></para> 
        /// If you only want to do this evaluation once pr. test suite execution you should wrap your function in a Lazy&lt;&gt;
        /// </summary>
        public Configuration SetAutomaticTestRewrite(TestRewriteIndicator indicator)
        {
            if (indicator == null)
                throw new ArgumentNullException("indicator");
            AutomaticTestRewrite = indicator;

            return configuration;
        }

        /// <summary>
        /// Defines how the error message shown when tests are failing
        /// </summary>
        public delegate string CreateAssertMessageCallback(
            string expected,
            string actual,
            string escapedActual,
            bool willPerformAutomaticRewrite,
            UnitTestLocationInfo location);

        public CreateAssertMessageCallback AssertMessageCreator { get; private set; }

        /// <summary>
        /// Set the method to be called when an assert message is to be created
        /// </summary>
        public Configuration SetAssertMessageCreator(CreateAssertMessageCallback assertMessageCreator)
        {
            if (assertMessageCreator == null)
                throw new ArgumentNullException("indicator");
            AssertMessageCreator = assertMessageCreator;

            return configuration;
        }
    }
}