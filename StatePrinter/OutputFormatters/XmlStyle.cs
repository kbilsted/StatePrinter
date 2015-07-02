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

            Token previous = null;
            var endTags = new Stack<string>();
            int pos = 0;
            for (int i = 0; i < tokens.Count; i++)
            {
                var token = tokens[i];
                int skip = MakeTokenString(tokens, pos++, sb, endTags, previous);
                previous = token;
                i += skip;
            }

            if (endTags.Any())
                throw new Exception("Internal logic error");

            sb.TrimLast();
            return sb.ToString();
        }

        int  MakeTokenString(List<Token> tokens, int pos, IndentingStringBuilder sb, Stack<string> endTags, Token previous)
        {
            int skip = 0;
            Token token = tokens[pos];
            string tagName = GetTagName(token);

            switch (token.Tokenkind)
            {
                case TokenType.StartScope:
                    sb.Indent();
                    endTags.Push(GetTagName(previous));
                    break;

                case TokenType.EndScope:
                    sb.DeIndent();
                    sb.AppendFormatLine("</{0}>", endTags.Pop());
                    break;

                case TokenType.StartEnumeration:
                    if (pos + 1 < tokens.Count)
                    {
                        var nextToken = tokens[pos + 1];

                        if (nextToken.Tokenkind == TokenType.EndEnumeration)
                        {
                            sb.AppendFormatLine("<Enumeration></Enumeration>");
                            skip++;
                            break;
                        }

                        if (nextToken.Tokenkind == TokenType.SimpleFieldValue
                            && nextToken.Field.SimpleKeyInArrayOrDictionary != null)
                        {
                            tagName = GetTagName(nextToken);
                            endTags.Push(tagName);
                            sb.AppendFormatLine("<{0}>", tagName);
                            sb.Indent();
                            sb.AppendFormatLine("<Enumeration>");
                            break;
                        }
                    }
                    
                    endTags.Push(previous == null ? null : GetTagName(previous));
                    sb.Indent();
                    sb.AppendFormatLine("<Enumeration>");
                    break;

                case TokenType.EndEnumeration:
                    sb.AppendFormatLine("</Enumeration>");
                    sb.DeIndent();
                    var endtag = endTags.Pop();
                    if(endtag != null)
                       sb.AppendFormatLine("</{0}>", endtag);
                    break;

                case TokenType.SimpleFieldValue:
                    if (token.Field.SimpleKeyInArrayOrDictionary != null)
                    {
                        sb.AppendFormatLine(
                            "<key>{0}</key><value>{1}</value>",
                            token.Field.SimpleKeyInArrayOrDictionary,
                            token.Value);
                    }
                    else
                    {
                        sb.AppendFormatLine("<{0}>{1}</{0}>", tagName, token.Value);
                    }
                    break;

                case TokenType.SeenBeforeWithReference:
                    var seenBeforeReference = string.Format(" ref='{0}'", token.ReferenceNo.Number);
                    sb.AppendFormatLine("<{0}{1} />", tagName, seenBeforeReference);
                    break;

                case TokenType.FieldnameWithTypeAndReference:
                    var optionReferenceInfo = token.ReferenceNo != null
                      ? string.Format(" ref='{0}'", token.ReferenceNo.Number)
                      : "";
                    var fieldType = OutputFormatterHelpers.MakeReadable(token.FieldType)
                        .Replace('<', '(')
                        .Replace('>', ')');
                    sb.AppendFormatLine("<{0} type='{1}'{2}>", tagName, fieldType, optionReferenceInfo);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            return skip;
        }

        private string GetTagName(Token token)
        {
            // fieldname is empty if the ROOT-element-name has not been supplied
            if (token.Field == null || string.IsNullOrEmpty(token.Field.Name))
                return "ROOT"; // Cannot be empyt like the other styles since all tags must have a name
            return token.Field.Name;
        }
    }
}