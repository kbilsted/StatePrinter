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
using System.Linq;
using System.Text.RegularExpressions;

namespace StatePrinter.TestAssistance
{
    public class Parser
    {
        static readonly RegexOptions options = RegexOptions.Singleline | RegexOptions.RightToLeft;

        public string ReplaceExpected(string content, int lineNo, string originalExpected, string newExpected)
        {
            int index = FindLastIndexOfLine(content, lineNo);

            var reString = EscapeForString(originalExpected);
            var reVerbString = EscapeForVerbatimString(originalExpected);
            Regex re= new Regex( "("
                + reString
                + "|"
                + reVerbString
                + ")", options);
            
            var match = re.Match(content, index);
            if (!match.Success)
                throw new ArgumentException("Did not find '" + originalExpected + "'");

            int start = match.Index;
            int end = match.Index + match.Length;
            var res = content.Substring(0, start)
                + newExpected
                + content.Substring(end);

            return res;
        }

        string EscapeForString(string s)
        {
            return "@?\"" + EscapeForRegEx(s)
                .Replace("\n", "\\n")
                .Replace("\r", "\\r")
                .Replace("\t", "\\t")
                .Replace("\"", "\\\"")
                .Replace(".", "\\.")
                + "\"";
        }

        string EscapeForVerbatimString(string s)
        {
            return "@\"" 
                + EscapeForRegEx(s).Replace("\"", "\"\"")
                + "\"";
        }

        string EscapeForRegEx(string s)
        {
            return s.Replace("(", "\\(")
                .Replace(")", "\\)")
                .Replace("|", "\\|")
                .Replace("+", "\\+");
        }

        /// <summary>
        /// This method does not support files using only \r as newlines
        /// </summary>
        int FindLastIndexOfLine(string content, int lineNo)
        {
            int line = 1;
            bool found = false;
            int i = 0;
            for (; i < content.Count(); i++)
            {
                if (line == lineNo)
                    found = true;
                if (content[i] == '\n')
                {
                    if (found)
                        return i;
                    line++;
                }
            }

            if(found)
                return i;

            throw new ArgumentOutOfRangeException("content", "File does not have " + lineNo + " lines. Only " + line + " lines.");
        }
    }
}

