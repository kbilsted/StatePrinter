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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace StatePrinter.TestAssistance
{
    
    public class UnitTestLocationInfo
    {
        public string Filepath;
        public int LineNumber;

        /// <summary>
        /// we cannot check if a local variable is named "expected", so all we can do is to check that there exists a local string variable.
        /// </summary>
        public bool TestMethodHasAStringVariable;
    }

    class TestRewriter
    {
        Encoding[] encodings = new[] 
        { 
            new UTF8Encoding(true), 
            Encoding.BigEndianUnicode, 
            new UTF32Encoding(false, true),
            new UTF32Encoding(true, true),

            // fallback
            new UTF8Encoding(false) 
        };

        public void RewriteTest(UnitTestLocationInfo info, string newExpected)
        {
            var bytes = File.ReadAllBytes(info.Filepath);

            Encoding enc = null;
            string content = null;
            //foreach (var encoding in encodings)
            //{
            //    enc = encoding;
            //    if (TryConvertFromEncoding(encoding, bytes, out content))
            //        break;
            //}

            enc = encodings.First(x => TryConvertFromEncoding(x, bytes, out content));

            var newTestContent = new Parser().ReplaceExpected(content, info.LineNumber, newExpected);

            File.WriteAllBytes(info.Filepath, enc.GetBytes(newTestContent));
        }

        bool TryConvertFromEncoding(Encoding enc, byte[] bytes, out string result)
        {
            var preamble = enc.GetPreamble();
            if (preamble.Where((p, i) => p != bytes[i]).Any())
            {
                result = null;
                return false;
            }
        
            result = enc.GetString(bytes.Skip(preamble.Length).ToArray());
            return true;
        }
    }

    public class Reflector
    {
        public UnitTestLocationInfo TryGetLocation()
        {
            var info = TryFindFirstCallOutsideStatePrinterAssembly();
            if (info == null)
                return null;

            return new UnitTestLocationInfo()
                {
                    Filepath = info.GetFileName(),
                    LineNumber = info.GetFileLineNumber(),
                    TestMethodHasAStringVariable = info.GetMethod().GetMethodBody().LocalVariables.Any(x=>x.LocalType == typeof(string)),
                };
        }

        /// <summary>
        /// This code only works in debug mode, when in release mode, null is returned
        /// </summary>
        StackFrame TryFindFirstCallOutsideStatePrinterAssembly()
        {
            var trace = new StackTrace(true);

            var stateprinterAssembly = trace.GetFrame(0).GetMethod().Module.Assembly;
//            var stateprinterAssembly = new StackFrame(0).GetMethod().Module.Assembly;
            for (int i = 1; i < trace.FrameCount; i++)
            {
                var frame = trace.GetFrame(i);

                if (!stateprinterAssembly.Equals(frame.GetMethod().Module.Assembly))
                    return frame;
            }

            return null;
        }
    }

    public class Parser
    {
        static string verbatimString = "\"([^\"]|(\"\"))*?\"";
        static string ensureLineStartsBlank = "(?<prespace>(\n|\r)[ \t]*?)";
        static RegexOptions options = RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.RightToLeft;
        static Regex re = new Regex(ensureLineStartsBlank + @"var expected\s*=\s*@" + verbatimString + @"\s*;", options);

        public string ReplaceExpected(string content, int lineNo, string newExpected)
        {
            int index = FindIndexOfLine(content, lineNo);
            var match = re.Match(content, index);
            if (!match.Success)
                throw new ArgumentException("Did not find 'var expected = @\"..\";'");

            int start = match.Index + match.Groups["prespace"].Length;
            int end = match.Index + match.Length;
            var res = content.Substring(0, start) 
                + newExpected 
                + content.Substring(end);

            return res;
        }
        
        /// <summary>
        /// This method does not support files using only \r as newlines
        /// </summary>
        int FindIndexOfLine(string content, int lineNo)
        {
            int line = 1;

            for (int i = 0; i < content.Count(); i++)
            {
                if (line == lineNo)
                    return i;
                if (content[i] == '\n')
                    line++;
            }

            throw new ArgumentOutOfRangeException("File does not have " + lineNo + " lines. Only " + line + " lines.");
        }
    }

}

