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
    /// Formatting the tokens to a JSON style representation.
    /// 
    /// In order to reduce clutter in the output, circular references are referenced by a
    /// path starting from the root object.
    /// </summary>
    public class JsonStyle : IOutputFormatter
    {
        Configuration configuration;

        public JsonStyle(Configuration configuration)
        {
            this.configuration = configuration;
        }

        public string Print(List<Token> tokens)
        {
            var filter = new UnusedReferencesTokenFilter();
            var backreferences = filter.GetBackreferences(tokens);
            Dictionary<Reference, string> referencePaths = CreatePathsFromReferences(tokens, backreferences);

            return MakeString(tokens, referencePaths);
        }

        Dictionary<Reference, string> CreatePathsFromReferences(List<Token> tokens, Reference[] backreferences)
        {
            var paths = new Dictionary<Reference, string>();
            var path = new List<string> { "root" };

            string last = null;

            foreach (var token in tokens)
            {
                switch (token.Tokenkind)
                {
                    case TokenType.StartScope:
                        if (last != null)
                            path.Add(last);
                        break;

                    case TokenType.EndScope:
                        if (path.Any())
                            path.RemoveAt(path.Count - 1);
                        break;

                    case TokenType.FieldnameWithTypeAndReference:
                        if (token.Field.Name != null)
                        {
                            var keyname = token.Field.SimpleKeyInArrayOrDictionary == null
                              ? ""
                              : "[" + token.Field.SimpleKeyInArrayOrDictionary + "]";
                            last = token.Field.Name + keyname;
                        }

                        if (token.ReferenceNo != null && !paths.ContainsKey(token.ReferenceNo))
                        {
                            var parts = (last == null ? path : path.Concat(new[] { last })).ToArray();
                            paths[token.ReferenceNo] = string.Join(".", parts);
                        }
                        break;

                    case TokenType.StartEnumeration:
                    case TokenType.EndEnumeration:
                    case TokenType.SeenBeforeWithReference:
                    case TokenType.SimpleFieldValue:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return paths;
        }

        string MakeString(List<Token> tokens, Dictionary<Reference, string> referencePaths)
        {
            var sb = new IndentingStringBuilder(configuration.IndentIncrement, configuration.NewLineDefinition);

            for (int i = 0; i < tokens.Count; i++)
            {
                int skip = MakeStringFromToken(tokens, i, referencePaths, sb);
                i += skip;
            }

            sb.TrimLast();
            return sb.ToString();
        }

        int MakeStringFromToken(List<Token> tokens, int pos, Dictionary<Reference, string> referencePaths, IndentingStringBuilder sb)
        {
            var token = tokens[pos];

            int skip = 0;

            string fieldnameColon = null;
            switch (token.Tokenkind)
            {
                case TokenType.StartScope:
                    sb.AppendFormatLine("{{");
                    sb.Indent();
                    break;

                case TokenType.EndScope:
                    sb.DeIndent();
                    sb.AppendFormatLine("}}");
                    break;

                case TokenType.StartEnumeration:
                    sb.AppendFormatLine("[");
                    sb.Indent();
                    break;

                case TokenType.EndEnumeration:
                    sb.DeIndent();
                    sb.AppendFormatLine("]{0}", OptionalComma(tokens, pos));
                    break;

                case TokenType.SimpleFieldValue:
                    {
                        // fieldname is empty if the ROOT-element-name has not been supplied
                        fieldnameColon = GetEmptyOrFieldname(token, "\"{0}\" : ");

                        var optinalComma = OptionalComma(tokens, pos);
                        sb.AppendFormatLine("{0}{1}{2}", fieldnameColon, token.Value, optinalComma);
                        break;
                    }

                case TokenType.SeenBeforeWithReference:
                    {
                        // fieldname is empty if the ROOT-element-name has not been supplied
                        fieldnameColon = GetEmptyOrFieldname(token, "\"{0}\" : ");

                        var seenBeforeReference = " " + referencePaths[token.ReferenceNo];
                        sb.AppendFormatLine("{0}{1}{2}", fieldnameColon, seenBeforeReference, OptionalComma(tokens, pos));
                        break;
                    }

                case TokenType.FieldnameWithTypeAndReference:
                    // if we are part of an idex, do not print the field name as it has alreadty been printed
                    if (token.Field.SimpleKeyInArrayOrDictionary == null)
                    {
                        // fieldname is empty if the ROOT-element-name has not been supplied
                        fieldnameColon = GetEmptyOrFieldname(token, "\"{0}\" :");

                        // inline-print empty collections
                        string optionalValue = "";
                        bool isNextEmptyEnumeration = pos + 2 < tokens.Count // TODO optimize by introducing a variable holding "count-2"
                                && tokens[pos + 1].Tokenkind == TokenType.StartEnumeration
                                && tokens[pos + 2].Tokenkind == TokenType.EndEnumeration;
                        if (isNextEmptyEnumeration)
                        {
                            skip += 2;
                            optionalValue = (fieldnameColon == "" ? "" : " ") + "[]";
                        }

                        sb.AppendFormatLine("{0}{1}{2}", fieldnameColon, optionalValue, OptionalComma(tokens, pos + skip));
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            return skip;
        }

        string GetEmptyOrFieldname(Token token, string formatting)
        {
            return token.Field == null || string.IsNullOrEmpty(token.Field.Name)
              ? ""
              : string.Format(formatting, token.Field.Name);
        }

        /// <summary>
        /// produces an extra "," if needed
        /// </summary>
        string OptionalComma(List<Token> tokens, int position)
        {
            bool isLastToken = position == tokens.Count - 1;
            if (isLastToken)
                return "";

            var nextToken = tokens[position + 1].Tokenkind;
            bool isNextScope = nextToken == TokenType.EndScope
                               || nextToken == TokenType.StartEnumeration
                               || nextToken == TokenType.StartScope
                               || nextToken == TokenType.EndEnumeration;
            if (isNextScope)
                return "";

            return ",";
        }
    }
}