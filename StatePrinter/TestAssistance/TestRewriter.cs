// Copyright 2014-2020 Kasper B. Graversen
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
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace StatePrinting.TestAssistance
{
    
    public class UnitTestLocationInfo
    {
        public string Filepath;
        public int LineNumber;
    }

    class TestRewriter
    {
        static readonly Encoding[] encodings = new[] 
        { 
            new UTF8Encoding(true), 
            Encoding.BigEndianUnicode, 
            new UTF32Encoding(false, true),
            new UTF32Encoding(true, true),

            // fallback
            new UTF8Encoding(false),
        };

        Func<FileRepository> fileRepositoryFactory;

        public TestRewriter(Func<FileRepository> fileRepositoryFactory)
        {
            this.fileRepositoryFactory = fileRepositoryFactory;
        }

        public void RewriteTest(UnitTestLocationInfo info, string originalExpected, string newExpected)
        {
            Encoding enc = null;
            string content = null;
            var fileRepository = fileRepositoryFactory();
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

}

