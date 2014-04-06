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
  public class XmlStyle : IOutputFormatter
  {
    /// <summary>
    /// Specifies how indentation is done. 
    /// </summary>
    readonly string IndentIncrement = "    ";

    public XmlStyle(string indentIncrement)
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

      Token last = null;
      var endTags = new Stack<string>();
      foreach (var token in tokens)
      {
        switch (token.Tokenkind)
        {
          case TokenType.StartScope:
            indent += IndentIncrement;
            endTags.Push(last.FieldName);
            break;

          case TokenType.EndScope:
            indent = indent.Substring(IndentIncrement.Length);
            sb.AppendLine(string.Format("{0}</{1}>", indent, endTags.Pop()));
            break;

          case TokenType.SimpleFieldValue:
            sb.AppendLine(string.Format("{0}<{1}>{2}</{1}>", indent, token.FieldName, token.Value));
            break;

          case TokenType.SeenBeforeWithReference:
            var seenBeforeReference = string.Format(" ref='{0}'", token.ReferenceNo.Number);
            sb.AppendLine(string.Format("{0}<{1}{2} />", indent, token.FieldName, seenBeforeReference));
            break;

          case TokenType.FieldnameWithTypeAndReference:
            var optionReferenceInfo = token.ReferenceNo != null
              ? string.Format(" ref='{0}'", token.ReferenceNo.Number)
              : "";
            var fieldType = OutputFormatterHelpers.MakeReadable(token.FieldType).Replace('<','(').Replace('>',')');
            sb.AppendFormat("{0}<{1} type='{2}'{3}>", indent, token.FieldName, fieldType, optionReferenceInfo);
            sb.AppendLine();
            break;

          default:
            throw new ArgumentOutOfRangeException();
        }
        last = token;
      }

      return sb.ToString();
    }

  }
}