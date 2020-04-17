// Copyright 2014 Kasper B. Graversen
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using StatePrinting.Configurations;
using StatePrinting.Introspection;

namespace StatePrinting.OutputFormatters
{
    /// <summary>
    /// Formatting the tokens to a compilable C# representation.
    /// 
    /// Circular refereces are not supported, as this would lead to infinte loops.
    /// </summary>
    public class StrictCSharpStyle : IOutputFormatter
    {
        Configuration configuration;

        public StrictCSharpStyle(Configuration configuration)
        {
            this.configuration = configuration;
        }

        public string Print(List<Token> tokens)
        {
            var filter = new UnusedReferencesTokenFilter();
            var processed = filter.FilterUnusedReferences(tokens);

            return MakeString(processed);
        }

        string MakeString(IList<Token> tokens)
        {
            var sb = new IndentingStringBuilder(configuration);

            for (var i = 0; i < tokens.Count; i++)
            {
                var token = tokens[i];
                MakeTokenString(token, sb);
            }

            sb.TrimLast();
            var result = sb.ToString();
            
            // remove final superfluous trailing comma - very hacky, could probably be done better
            return result.Remove(result.Length - 1);
        }

        void MakeTokenString(Token token, IndentingStringBuilder sb)
        {
            switch (token.Tokenkind)
            {
                case TokenType.StartScope:
                case TokenType.StartList:
                case TokenType.StartDict:
                    sb.AppendFormatLine("{{");
                    sb.Indent();
                    break;

                case TokenType.EndScope:
                case TokenType.EndList:
                case TokenType.EndDict:
                    sb.DeIndent();
                    sb.AppendFormatLine("}},");
                    break;
                
                case TokenType.SimpleFieldValue:
                    sb.AppendFormatLine("{0}", MakeFieldValue(token, token.Value));
                    break;

                case TokenType.SeenBeforeWithReference:
                    throw CreateCyclicalObjectGraphException();

                case TokenType.FieldnameWithTypeAndReference:
                    if (token.ReferenceNo != null)
                    {
                        throw CreateCyclicalObjectGraphException();
                    }

                    var fieldType = OutputFormatterHelpers.MakeReadable(token.FieldType);

                    string value = string.Format("new {0}{1}", fieldType, token.FieldType.IsArray ? "" : "()");
                    sb.AppendFormatLine("{0}", MakeFieldValue(token, value));
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        string MakeFieldValue(Token token,  string value, Token nextToken = null)
        {
            if (token.Field == null)
                return string.Format("{0}", value);

            if (token.Field.Index.HasValue)
                return string.Format("{0}", value);

            if (token.Field.Key != null)
                return string.Format("[{0}] = {1}", token.Field.Key, value);

            if (!string.IsNullOrEmpty(token.Field.Name))
            {
                var isEnumerable = token.FieldType != null && token.FieldType.IsAssignableFrom(typeof(IEnumerable));
                return string.Format("{0} = {1}{2}", token.Field.Name, value, isEnumerable ? "" : ",");
            }

            return string.Format("{0}", value);
        }

        NotSupportedException CreateCyclicalObjectGraphException()
        {
            return new NotSupportedException("Cyclical object graphs are not supported in StrictCSharp style.");
        }
    }
}
