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
        string fieldName = null;
        if (token.Field != null)
        {
          fieldName = token.Field.Name
                      + (token.Field.SimpleKeyInArrayOrDictionary != null
                        ? ("[" + token.Field.SimpleKeyInArrayOrDictionary + "]")
                        : "");
        }

        // fieldname is empty if the ROOT-element-name has not been supplied
        string fieldnameAssign = string.IsNullOrEmpty(fieldName)
          ? ""
          : (fieldName + " = ");

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

          case TokenType.StartEnumeration:
          case TokenType.EndEnumeration:
            break;

          case TokenType.SimpleFieldValue:
            sb.AppendLine(string.Format("{0}{1}{2}", indent, fieldnameAssign, token.Value));
            break;

          case TokenType.SeenBeforeWithReference:
            var seenBeforeReference = " -> " + token.ReferenceNo.Number;
            sb.AppendLine(string.Format("{0}{1}{2}", indent, fieldnameAssign, seenBeforeReference));
            break;

          case TokenType.FieldnameWithTypeAndReference:
            var optionReferenceInfo = token.ReferenceNo == null
              ? ""
              : string.Format(", ref: {0}", token.ReferenceNo.Number);

            var fieldType = OutputFormatterHelpers.MakeReadable(token.FieldType);

            sb.AppendLine(string.Format("{0}{1}new {2}(){3}", indent, fieldnameAssign, fieldType, optionReferenceInfo));
            break;

          default:
            throw new ArgumentOutOfRangeException();
        }
      }

      return sb.ToString();
    }

  }
}