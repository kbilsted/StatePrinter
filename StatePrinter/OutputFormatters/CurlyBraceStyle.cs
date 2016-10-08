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
using System.Collections.Generic;
using StatePrinting.Configurations;
using StatePrinting.Introspection;

namespace StatePrinting.OutputFormatters
{
    /// <summary>
    /// Formatting the tokens to a curly-brace style representation.
    /// 
    /// In order to reduce clutter in the output, only reference that are referred to by later
    /// outputted objects will have a referencenumber attached to them.
    /// </summary>
    public class CurlyBraceStyle : IOutputFormatter
    {
        Configuration configuration;

        public CurlyBraceStyle(Configuration configuration)
        {
            this.configuration = configuration;
        }

        public string Print(List<Token> tokens)
        {
            var filter = new UnusedReferencesTokenFilter();
            var processed = filter.FilterUnusedReferences(tokens);

            return MakeString(processed);
        }

        string MakeString(IEnumerable<Token> tokens)
        {
            var sb = new IndentingStringBuilder(configuration);

            foreach (var token in tokens)
            {
                MakeTokenString(token, sb);
            }

            sb.TrimLast();

            return sb.ToString();
        }

        void MakeTokenString(Token token, IndentingStringBuilder sb)
        {
            switch (token.Tokenkind)
            {
                case TokenType.StartScope:
                case TokenType.StartList:
                case TokenType.StartDict:
                    sb.AppendLine("{");
                    sb.Indent();
                    break;

                case TokenType.EndScope:
                case TokenType.EndList:
                case TokenType.EndDict:
                    sb.DeIndent();
                    sb.AppendLine("}");
                    break;

                case TokenType.SimpleFieldValue:
                    sb.AppendLine(MakeFieldValue(token, token.Value));
                    break;

                case TokenType.SeenBeforeWithReference:
                    sb.AppendLine(MakeFieldValue(token, "-> " + token.ReferenceNo.Number));
                    break;

                case TokenType.FieldnameWithTypeAndReference:
                    var optionReferenceInfo = token.ReferenceNo == null
                      ? ""
                      : string.Concat(", ref: ", token.ReferenceNo.Number);

                    var fieldType = OutputFormatterHelpers.MakeReadable(token.FieldType);

                    string value = string.Concat("new ", fieldType, "()", optionReferenceInfo);
                    sb.AppendLine(MakeFieldValue(token, value));
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        string MakeFieldValue(Token token, string value)
        {
            if (token.Field == null)
                return value;

            if (token.Field.Index.HasValue)
                return string.Concat("[", token.Field.Index, "] = ", value);

            if (token.Field.Key != null)
                return string.Concat("[", token.Field.Key, "] = ", value);

            // Field.Name is empty if the ROOT-element-name has not been supplied.
            if (string.IsNullOrEmpty(token.Field.Name))
                return value;

            return string.Concat(token.Field.Name, " = ", value);
        }
    }
}
