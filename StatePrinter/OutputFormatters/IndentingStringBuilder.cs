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

using System.Text;
using StatePrinting.Configurations;

namespace StatePrinting.OutputFormatters
{
    class IndentingStringBuilder
    {
        readonly StringBuilder sb = new StringBuilder();
        readonly string IndentIncrement;
        readonly string NewLineDefinition;
        readonly Configuration configuration;

        string indent = "";
        bool mustIndent = true;

        public IndentingStringBuilder(Configuration configuration)
        {
            IndentIncrement = configuration.IndentIncrement;
            NewLineDefinition = configuration.NewLineDefinition;
            this.configuration = configuration;
        }

        public void Indent()
        {
            indent += IndentIncrement;
        }

        public void DeIndent()
        {
            indent = indent.Substring(IndentIncrement.Length);
        }

        public void AppendFormat(string format, params object[] args)
        {
            if (mustIndent)
            {
                sb.Append(indent);
                mustIndent = false;
            }
            sb.AppendFormat(format, args);
        }

        public void AppendFormatLine(string format, params object[] args)
        {
            AppendFormat(format, args);
            sb.Append(NewLineDefinition);
            mustIndent = true;
        }

        public void Append(string text)
        {
            if (mustIndent)
            {
                sb.Append(indent);
                mustIndent = false;
            }
            sb.Append(text);
        }

        public void AppendLine(string text)
        {
            Append(text);
            sb.Append(NewLineDefinition);
            mustIndent = true;
        }

        public override string ToString()
        {
            return sb.ToString();
        }

        public void TrimLast()
        {
            int trim = new StringBuilderTrimmer(configuration.LegacyBehaviour.TrimTrailingNewlines).TrimLast(sb);
            if(trim > 0)
                sb.Remove(sb.Length - trim, trim);
        }
    }
}
