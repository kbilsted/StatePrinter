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
using System.Globalization;
using System.Text;
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
    public class FastestPossibleStyle : IOutputFormatter
    {
        readonly Configuration configuration;

        public FastestPossibleStyle(Configuration configuration)
        {
            this.configuration = configuration;
            configuration.Culture= CultureInfo.InvariantCulture;
        }

        public string Print(List<Token> tokens)
        {
            var sb = new StringBuilder(tokens.Count*7);
            var newline = configuration.NewLineDefinition;
            var count = tokens.Count;
            for (int i = 0; i < count; i++)
            {
                var token = tokens[i];

                switch (token.Tokenkind)
                {
                    case TokenType.StartScope:
                        sb.Append(newline);
                        break;
                    case TokenType.StartList:
                    case TokenType.StartDict:
                        sb.Append("["+newline+"]");
                        break;

                    case TokenType.EndScope:
                        sb.Append(configuration.NewLineDefinition);
                        break;
                    case TokenType.EndList:
                    case TokenType.EndDict:
                        sb.Append("]"+configuration.NewLineDefinition);
                        break;

                    case TokenType.SimpleFieldValue:
                    case TokenType.SeenBeforeWithReference:
                        sb.Append(token.Value);
                        break;

                    case TokenType.FieldnameWithTypeAndReference:
                        var fieldType = OutputFormatterHelpers.MakeReadable(token.FieldType);
                        sb.Append(MakeFieldValue(token, newline+"new ("+fieldType+")") );
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            return sb.ToString();
        }

        string MakeFieldValue(Token token, string value)
        {
            if (token.Field == null)
                return value;

            if (token.Field.Index.HasValue)
                return string.Format("[{0}] = {1}", token.Field.Index, value);

            if (token.Field.Key != null)
                return string.Format("[{0}] = {1}", token.Field.Key, value);

            // Field.Name is empty if the ROOT-element-name has not been supplied.
            if (string.IsNullOrEmpty(token.Field.Name))
                return value;

            return string.Format("{0} = {1}", token.Field.Name, value);
        }
    }
}
