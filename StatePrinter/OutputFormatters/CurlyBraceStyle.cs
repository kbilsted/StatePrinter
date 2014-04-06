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
using System.Runtime.InteropServices;
using System.Text;
using StatePrinter.Introspection;

namespace StatePrinter.OutputFormatters
{
  /// <summary>
  /// Formatting the tokens to a curly-brace style representation.
  /// 
  /// In order to reduce clutter in the output, only reference that are referred to by later
  /// outputted objects will have a referencenumber attached to them.
  /// </summary>
  public class CurlyBraceStyle : IOutputFormatter
  {
    /// <summary>
    /// Specifies how indentation is done. 
    /// </summary>
    readonly string IndentIncrement = "    ";

    public CurlyBraceStyle(string indentIncrement)
    {
      IndentIncrement = indentIncrement;
    }

    public string Print(List<Token> tokens)
    {
      var filter = new UnusedReferencesTokenFilter();
      var processed = filter.FilterUnusedReferences(tokens);

      return MakeString(processed);
    }

    string MakeString(IEnumerable<Token> tokens)
    {
      var sb = new StringBuilder();
      string indent = "";

      foreach (var token in tokens)
      {
        switch (token.Tokenkind)
        {
          case TokenType.StartScope:
            sb.AppendLine(indent + "{");
            indent += IndentIncrement;
            break;

          case TokenType.EndScope:
            indent = indent.Substring(IndentIncrement.Length);
            sb.AppendLine(indent + "}");
            break;

          case TokenType.SimpleFieldValue:
            sb.AppendFormat("{0}{1} = {2}", indent, token.FieldName, token.Value);
            sb.AppendLine();
            break;

          case TokenType.SeenBeforeWithReference:
            var seenBeforeReference = " -> " + token.ReferenceNo.Number;
            sb.AppendFormat("{0}{1} = {2}", indent, token.FieldName, seenBeforeReference);
            sb.AppendLine();
            break;

          case TokenType.FieldnameWithTypeAndReference:
            var optionReferenceInfo = token.ReferenceNo != null
              ? string.Format(", ref: {0}", token.ReferenceNo.Number)
              : "";
            var fieldType = OutputFormatterHelpers.MakeReadable(token.FieldType);
            sb.AppendFormat("{0}{1} = <{2}>{3}", indent, token.FieldName, fieldType, optionReferenceInfo);
            sb.AppendLine();
            break;

          default:
            throw new ArgumentOutOfRangeException();
        }
      }

      return sb.ToString();
    }

  }
}