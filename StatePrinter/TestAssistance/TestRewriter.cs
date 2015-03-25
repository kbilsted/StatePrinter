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
using System.Reflection;

namespace StatePrinter.TestAssistance
{
    
    public class UnitTestLocationInfo
    {
        public string Filepath;
        public int LineNumber;
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
            new UTF8Encoding(false),
        };

        public void RewriteTest(UnitTestLocationInfo info, string originalExpected, string newExpected)
        {
            Encoding enc = null;
            string content = null;
            var fileRepository = new FileRepository();
            var bytes = fileRepository.Read(info.Filepath);

            enc = encodings.First(x => TryConvertFromEncoding(x, bytes, out content));

            var newTestContent = new Parser().ReplaceExpected(content, info.LineNumber, originalExpected, newExpected);
            fileRepository.Write(info.Filepath, enc.GetBytes(newTestContent));
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

    public class FileRepository
    {
        public static byte[] UnitTestFakeReadContent = null;
        
        public byte[] Read(string path)
        {
            if (UnitTestFakeReadContent != null)
                return UnitTestFakeReadContent;

            var bytes = File.ReadAllBytes(path);

            if (bytes.Length < 50)
                throw new FileLoadException("Input file '" + path+ "' contains less than 50 bytes.");

            return bytes;
        }

        public void Write(string path, byte[] content)
        {
            if (UnitTestFakeReadContent == null)
                File.WriteAllBytes(path, content);
        }

    }


    public class CallStackReflector
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
                };
        }

        /// <summary>
        /// This code only works in debug mode, when in release mode, null is returned
        /// </summary>
        StackFrame TryFindFirstCallOutsideStatePrinterAssembly()
        {
            var trace = new StackTrace(true);

            var stateprinterAssembly = trace.GetFrame(0).GetMethod().Module.Assembly;
            for (int i = 1; i < trace.FrameCount; i++)
            {
                var frame = trace.GetFrame(i);
                var mth = frame.GetMethod();
                if (!stateprinterAssembly.Equals(mth.Module.Assembly) && frame.GetFileName() != null)
                    return frame;
            }

            return null;
        }
    }

    public class Parser
    {
        static RegexOptions options = RegexOptions.Singleline | RegexOptions.RightToLeft;

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

