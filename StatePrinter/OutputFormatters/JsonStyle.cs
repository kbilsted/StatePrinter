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
using StatePrinting.Configurations;
using StatePrinting.Introspection;

namespace StatePrinting.OutputFormatters
{
    /// <summary>
    /// Formatting the tokens to a JSON style representation.
    /// 
    /// In order to reduce clutter in the output, circular references are referenced by a
    /// path starting from the root object.
    /// </summary>
    public class JsonStyle : IOutputFormatter
    {
        readonly Configuration configuration;

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

        // TODO backreference-arg anvendes ikke..
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
                            object key = token.Field.Key != null ? token.Field.Key :
                                         token.Field.Index.HasValue ? token.Field.Index.Value.ToString() :
                                         null;
                            string subscript = key == null ? "" : string.Concat("[", key, "]");
                            last = string.Concat(token.Field.Name, subscript);
                        }

                        if (token.ReferenceNo != null && !paths.ContainsKey(token.ReferenceNo))
                        {
                            var parts = (last == null ? path : path.Concat(new[] { last })).ToArray();
                            paths[token.ReferenceNo] = string.Join(".", parts);
                        }
                        break;

                    case TokenType.StartList:
                    case TokenType.EndList:
                    case TokenType.StartDict:
                    case TokenType.EndDict:
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
            var sb = new IndentingStringBuilder(configuration);

            for (int i = 0; i < tokens.Count; i++)
            {
                int skip = MakeStringFromToken(tokens, i, referencePaths, sb);
                i += skip;
            }

            sb.TrimLast();

            return sb.ToString();
        }

        static readonly TokenType? None = new TokenType?();

        int MakeStringFromToken(List<Token> tokens, int pos, Dictionary<Reference, string> referencePaths, IndentingStringBuilder sb)
        {
            var token = tokens[pos];
            TokenType? next = pos + 1 < tokens.Count ? tokens[pos + 1].Tokenkind : None;
            int skip = 0;

            switch (token.Tokenkind)
            {
                case TokenType.StartScope:
                case TokenType.StartDict:
                    if (next == TokenType.EndScope || next == TokenType.EndDict)
                    {
                        sb.Append("{}");
                        sb.AppendLine(OptionalComma(tokens, pos + 1));
                        ++skip;
                    }
                    else
                    {
                        sb.AppendLine("{");
                        sb.Indent();
                    }
                    break;

                case TokenType.EndScope:
                case TokenType.EndDict:
                    sb.DeIndent();
                    sb.Append("}"); sb.AppendLine(OptionalComma(tokens, pos));
                    break;

                case TokenType.StartList:
                    if (next == TokenType.EndList)
                    {
                        sb.Append("[]"); sb.AppendLine(OptionalComma(tokens, pos + 1));
                        ++skip;
                    }
                    else
                    {
                        sb.AppendLine("[");
                        sb.Indent();
                    }
                    break;

                case TokenType.EndList:
                    sb.DeIndent();
                    sb.Append("]"); sb.AppendLine(OptionalComma(tokens, pos));
                    break;

                case TokenType.SimpleFieldValue:
                    sb.Append(MakeFieldValue(token, token.Value)); sb.AppendLine(OptionalComma(tokens, pos));
                    break;

                case TokenType.SeenBeforeWithReference:
                    sb.Append(MakeFieldValue(token, referencePaths[token.ReferenceNo])); sb.AppendLine(OptionalComma(tokens, pos));
                    break;

                case TokenType.FieldnameWithTypeAndReference:
                    sb.Append(MakeFieldValue(token, ""));
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            return skip;
        }


        string MakeFieldValue(Token token, string value)
        {
            if (token.Field == null)
                return value;

            if (token.Field.Index.HasValue)
                return value;

            if (token.Field.Key != null)
            {
                return IsQuoted(token.Field.Key)
                    ? string.Concat(token.Field.Key, ": ", value)
                    : string.Concat("\"", token.Field.Key, "\": ", value);
            }

            // Field.Name is empty if the ROOT-element-name has not been supplied.
            if (string.IsNullOrEmpty(token.Field.Name))
                return value;

            return string.Concat("\"", token.Field.Name, "\": ", value);
        }

        /// <summary>
        /// produces an extra "," if needed
        /// </summary>
        string OptionalComma(List<Token> tokens, int position)
        {
            var nextPos = position + 1;
            bool isLastToken = nextPos == tokens.Count;
            if (isLastToken)
                return "";

            switch (tokens[nextPos].Tokenkind)
            {
                case TokenType.StartScope:
                case TokenType.EndScope:
                case TokenType.StartList:
                case TokenType.EndList:
                case TokenType.StartDict:
                case TokenType.EndDict:
                    return "";
            }

            return ",";
        }

        bool IsQuoted(string s)
        {
            return s.Length >= 2 && s.StartsWith("\"") && s.EndsWith("\"");
        }
    }
}