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
using System.Linq;

using StatePrinter.Configurations;
using StatePrinter.Introspection;
using System.Security;

namespace StatePrinter.OutputFormatters
{
    /// <summary>
    /// Formatting the tokens to an XML style representation.
    /// 
    /// In order to reduce clutter in the output, only reference that are referred to by later
    /// outputted objects will have a referencenumber attached to them.
    /// </summary>
    public class XmlStyle : IOutputFormatter
    {
        readonly Configuration configuration;

        public XmlStyle(Configuration configuration)
        {
            this.configuration = configuration;
        }

        public string Print(List<Token> tokens)
        {
            var filter = new UnusedReferencesTokenFilter();
            var processed = filter.FilterUnusedReferences(tokens);
            
            return MakeString(processed);
        }

        string MakeString(List<Token> tokens)
        {
            var sb = new IndentingStringBuilder(configuration);

            var endTags = new Stack<string>();
            int pos = 0;
            foreach (Token token in tokens)
            {
                MakeTokenString(token, sb, endTags);
            }

            if (endTags.Any())
                throw new Exception("Internal logic error");

            sb.TrimLast();
            return sb.ToString();
        }

        void MakeTokenString(Token token, IndentingStringBuilder sb, Stack<string> endTags)
        {
            string tagName;
            string keyAttr;
            switch (token.Tokenkind)
            {
                case TokenType.StartScope:
                case TokenType.StartList:
                case TokenType.StartDict:
                    sb.Indent();
                    break;

                case TokenType.EndScope:
                case TokenType.EndList:
                case TokenType.EndDict:
                    sb.DeIndent();
                    sb.AppendFormatLine("</{0}>", endTags.Pop());
                    break;

                case TokenType.SimpleFieldValue:
                    tagName = TagName(token, out keyAttr);
                    sb.AppendFormatLine("<{0}{1}>{2}</{0}>", tagName, keyAttr, token.Value);
                    break;

                case TokenType.SeenBeforeWithReference:
                    tagName = TagName(token, out keyAttr);
                    sb.AppendFormatLine("<{0}{1} ref='{2}'/>", tagName, keyAttr, token.ReferenceNo.Number);
                    break;

                case TokenType.FieldnameWithTypeAndReference:
                    var optionReferenceInfo = token.ReferenceNo != null
                      ? string.Format(" ref='{0}'", token.ReferenceNo.Number)
                      : "";
                    var fieldType = OutputFormatterHelpers.MakeReadable(token.FieldType)
                        .Replace('<', '(')
                        .Replace('>', ')');
                    tagName = TagName(token, out keyAttr);
                    endTags.Push(tagName);
                    sb.AppendFormatLine("<{0}{1} type='{2}'{3}>", tagName, keyAttr, fieldType, optionReferenceInfo);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private string TagName(Token token, out string keyAttr)
        {
            string tag = null;
            string key = null;
            if (token.Field != null && token.Field.Key != null)
            {
                tag = "Element";
                key = token.Field.Key;
            }
            else if (token.Field != null && token.Field.Index.HasValue)
            {
                tag = "Element";
            }
            else if (token.Field != null && !string.IsNullOrEmpty(token.Field.Name))
            {
                tag = token.Field.Name;
            }
            else if (string.IsNullOrEmpty(tag))
            {
                // Cannot be empty like with the other styles since all tags must have a name.
                tag = "Root";
            }

            keyAttr = key == null ? "" : string.Format(" key='{0}'", SecurityElement.Escape(key));
            return tag;
        }
    }
}