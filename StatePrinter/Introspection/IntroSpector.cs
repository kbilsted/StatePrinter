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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using StatePrinter.Configurations;
using StatePrinter.FieldHarvesters;
using StatePrinter.ValueConverters;

namespace StatePrinter.Introspection
{
  /// <summary>
  /// Is responsible for traversing an objectgraph and returning a stream of tokens 
  /// </summary>
  class IntroSpector 
  {
    readonly Configuration Configuration;

    int referenceCounter = 0;
    readonly Dictionary<object, Reference> seenBefore = new Dictionary<object, Reference>();
    readonly List<Token> tokens = new List<Token>();
    
    public IntroSpector(Configuration configuration)
    {
      Configuration = configuration;
    }

    public List<Token> PrintObject(object objectToPrint, string rootname)
    {
      Introspect(objectToPrint, rootname);
      return tokens;
    }


    void Introspect(object source, string fieldName)
    {
      if (IntrospectNullValue(source, fieldName))
        return;

      var sourceType = source.GetType();

      if (IntrospectSimpleValue(source, fieldName, sourceType))
        return;

      if (SeenObjectBefore(source, fieldName))
        return;

      // handle a subset of dictionaries separately for terser printing
      if (IntrospectDictionaryWithSimpleKey(source as IDictionary, fieldName, sourceType))
        return;

      if (IntrospectIEnumerable(source, fieldName))
        return;

      IntrospectComplexType(source, fieldName, sourceType);
    }

    private bool SeenObjectBefore(object source, string fieldName)
    {
      Reference reference;
      if(seenBefore.TryGetValue(source, out reference))
      {
        tokens.Add(Token.SeenBefore(fieldName, reference));
        return true;
      }
      else
      {
        seenBefore[source] = new Reference(referenceCounter++);
        return false;
      }
    }

    void IntrospectComplexType(object source, string fieldName, Type sourceType)
    {
      IFieldHarvester harvester;
      if (!Configuration.TryGetFieldHarvester(sourceType, out harvester))
        throw new Exception(string.Format("No fieldharvester is configured for handling type '{0}'", sourceType));

      Reference optionReferenceInfo = null;
      seenBefore.TryGetValue(source, out  optionReferenceInfo);

      tokens.Add(new Token(TokenType.FieldnameWithTypeAndReference, fieldName, null, optionReferenceInfo, sourceType));
      tokens.Add(new Token(TokenType.StartScope));

      var fields = harvester.GetFields(sourceType);
      var helper = new HarvestHelper();
      foreach (FieldInfo field in fields)
      {
        var name = helper.SanitizeFieldName(field.Name);

        Introspect(field.GetValue(source), name);
      }
      tokens.Add(new Token(TokenType.EndScope));
    }


    bool IntrospectSimpleValue(object source, string fieldName, Type sourceType)
    {
      IValueConverter handler = null;
      if (!Configuration.TryGetValueConverter(sourceType, out handler))
        return false;

      tokens.Add(new Token(TokenType.SimpleFieldValue, fieldName, handler.Convert(source)));
      return true;
    }


    bool IntrospectNullValue(object source, string fieldName)
    {
      if (source != null)
        return false;

      tokens.Add(new Token(TokenType.SimpleFieldValue, fieldName, "null"));
      return true;
    }


    bool IntrospectDictionaryWithSimpleKey(IDictionary source, string fieldName, Type sourceType)
    {
      if (source == null)
        return false;

      if(sourceType.GetGenericArguments().Length != 2)
        return false;

      IValueConverter printer;
      var keyType = sourceType.GetGenericArguments().First();
      var isKeyTypeSimple = Configuration.TryGetValueConverter(keyType, out printer);

      if (!isKeyTypeSimple)
        return false; // print as enumerable which is more verbose

      var keys = source.Keys;
      foreach (var key in keys)
      {
        var valueValue = source[key];
        var keyValue = printer.Convert(key);
        var outputfieldName = string.Format("{0}[{1}]", fieldName, keyValue);
        Introspect(valueValue, outputfieldName);
      }
      return true;
    }

    private bool IntrospectIEnumerable(object source, string fieldName)
    {
      var enumerable = source as IEnumerable;
      if (enumerable == null)
        return false;

      Reference optionReferenceInfo = null;
      seenBefore.TryGetValue(source, out  optionReferenceInfo);

      tokens.Add(new Token(TokenType.FieldnameWithTypeAndReference, fieldName, null, optionReferenceInfo, source.GetType()));

      int i = 0;
      foreach (var x in enumerable)
      {
        var outputFieldName = string.Format("{0}[{1}]", fieldName, i++);
        Introspect(x, outputFieldName);
      }
      return true;
    }
  }
}