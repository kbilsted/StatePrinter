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
    class DefaultAssertMessage
    {
        readonly StringUtils stringUtils = new StringUtils();

        const string AreAlikeNotice = @"Info: Expected value and Actual value are not equal, but they are alike. Use 'Asserter.AreAlike()' if you expected the values to be alike.
";
        public string Create(
            string expected,
            string actual,
            string escapedActual,
            bool willPerformAutomaticRewrite,
            UnitTestLocationInfo location)
        {
            // It is important to use verbatim-strings in order to get the correct Environment.NewLine at line endings
            string message = "";

            bool areAlike = stringUtils.UnifyNewLines(expected) == stringUtils.UnifyNewLines(actual);
            if (areAlike)
                message = AreAlikeNotice;

            if (willPerformAutomaticRewrite)
            {
                message += string.Format(@"Rewritting test expectations in '{0}:{1}'.
Compile and re-run to see green lights.
New expectations:
{2}", location.Filepath, location.LineNumber, escapedActual);
            }
            else
            {
                var newExpected = string.Format("var expected = {0};", escapedActual);
                message += string.Format("{0}{0}Proposed output for unit test:{0}{0}{1}{0}", Environment.NewLine, newExpected);
            }

            return message;
        }

    }
}
