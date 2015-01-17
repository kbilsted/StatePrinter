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
    readonly string IndentIncrement;

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
      var sb = new IndentingStringBuilder(IndentIncrement);

      Token previous = null;
      var endTags = new Stack<string>();
      foreach (var token in tokens)
      {
        MakeTokenString(token, sb, endTags, previous);
        previous = token;
      }

      if(endTags.Any())
        throw new Exception("Internal logic error");

      return sb.ToString();
    }

    void MakeTokenString(Token token, IndentingStringBuilder sb, Stack<string> endTags, Token previous)
    {
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
          sb.Indent();
          sb.AppendFormatLine("<Enumeration>");
          endTags.Push(GetTagName(previous));
          break;

        case TokenType.EndEnumeration:
          sb.AppendFormatLine("</Enumeration>");
          sb.DeIndent();
          sb.AppendFormatLine("</{0}>", endTags.Pop());
          break;

        case TokenType.SimpleFieldValue:
          sb.AppendFormatLine("<{0}>{1}</{0}>", tagName, token.Value);
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