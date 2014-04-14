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
  public class JsonStyle : IOutputFormatter
  {
    /// <summary>
    /// Specifies how indentation is done. 
    /// </summary>
    readonly string IndentIncrement = "    ";

    public JsonStyle(string indentIncrement)
    {
      IndentIncrement = indentIncrement;
    }

    public string Print(List<Token> tokens)
    {
      var filter = new UnusedReferencesTokenFilter();
      var processed = filter.FilterUnusedReferences(tokens);

      return MakeString(processed);
    }

    string MakeString(List<Token> tokens)
    {
      var sb = new StringBuilder();
      string indent = "";

      for (int i = 0; i < tokens.Count; i++)
      {
        var token = tokens[i];

        string fieldnameColon = null;
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
            sb.AppendLine(indent + "[");
            indent += IndentIncrement;
            break;

          case TokenType.EndEnumeration:
            indent = indent.Substring(IndentIncrement.Length);
            sb.AppendLine(indent + "]");
            break;

          case TokenType.SimpleFieldValue:
          {
            // fieldname is empty if the ROOT-element-name has not been supplied
            fieldnameColon = token.Field == null || string.IsNullOrEmpty(token.Field.Name)
              ? ""
              : ("\"" + token.Field.Name + "\" : ");

            var optinalComma = OptinalComma(tokens, i);
            sb.AppendLine(string.Format("{0}{1}{2}{3}", indent, fieldnameColon,
              token.Value, optinalComma));
            break;
          }
          case TokenType.SeenBeforeWithReference:
          {
            // fieldname is empty if the ROOT-element-name has not been supplied
            fieldnameColon = token.Field == null || string.IsNullOrEmpty(token.Field.Name)
              ? ""
              : ("\"" + token.Field.Name + "\" : ");

            var seenBeforeReference = " -> " + token.ReferenceNo.Number;
            sb.AppendFormat("{0}{1}{2}", indent, fieldnameColon, seenBeforeReference);
            sb.AppendLine();
            break;
          }

          case TokenType.FieldnameWithTypeAndReference:
            // if we are part of an idex, do not print the field name as it has alreadty been printed
            if (token.Field.SimpleKeyInArrayOrDictionary == null)
            {
              // fieldname is empty if the ROOT-element-name has not been supplied
              fieldnameColon = token.Field == null || string.IsNullOrEmpty(token.Field.Name)
                ? ""
                : ("\"" + token.Field.Name + "\" :");
              sb.AppendLine(string.Format("{0}{1}", indent, fieldnameColon));
            }
            break;

          default:
            throw new ArgumentOutOfRangeException();
        }
      }

      return sb.ToString();
    }

    private static string OptinalComma(List<Token> tokens, int i)
    {
      bool isLastToken = i == tokens.Count - 1;
      if (isLastToken)
        return "";

      var nextToken = tokens[i + 1].Tokenkind;
      bool isNextEndScope = nextToken == TokenType.EndScope 
        || nextToken == TokenType.EndEnumeration;
      if(isNextEndScope)
        return "";
      
      return ",";
    }
  }
}