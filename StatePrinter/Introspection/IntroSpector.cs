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

    /// <summary>
    /// Each entry is assigned a reference number used for back-referencing
    /// </summary>
    int referenceCounter = 0;

    readonly Dictionary<object, Reference> seenBefore = new Dictionary<object, Reference>();
    readonly List<Token> tokens = new List<Token>();
    
    public IntroSpector(Configuration configuration)
    {
      Configuration = configuration;
    }

    public List<Token> PrintObject(object objectToPrint, string rootname)
    {
      Introspect(objectToPrint, new Field(rootname));
      return tokens;
    }


    void Introspect(object source, Field field)
    {
      if (IntrospectNullValue(source, field))
        return;

      var sourceType = source.GetType();

      if (IntrospectSimpleValue(source, field, sourceType))
        return;

      if (SeenObjectBefore(source, field))
        return;

      // handle a subset of dictionaries separately for terser printing
      if (IntrospectDictionaryWithSimpleKey(source as IDictionary, field, sourceType))
        return;

      if (IntrospectIEnumerable(source, field))
        return;

      IntrospectComplexType(source, field, sourceType);
    }

    private bool SeenObjectBefore(object source, Field field)
    {
      Reference reference;
      if(seenBefore.TryGetValue(source, out reference))
      {
        tokens.Add(Token.SeenBefore(field, reference));
        return true;
      }
      else
      {
        seenBefore[source] = new Reference(referenceCounter++);
        return false;
      }
    }

    void IntrospectComplexType(object source, Field field, Type sourceType)
    {
      IFieldHarvester harvester;
      if (!Configuration.TryGetFieldHarvester(sourceType, out harvester))
        throw new Exception(string.Format("No fieldharvester is configured for handling type '{0}'", sourceType));

      Reference optionReferenceInfo = null;
      seenBefore.TryGetValue(source, out  optionReferenceInfo);

      tokens.Add(new Token(TokenType.FieldnameWithTypeAndReference, field, null, optionReferenceInfo, sourceType));
      tokens.Add(new Token(TokenType.StartScope));

      var fields = harvester.GetFields(sourceType);
      var helper = new HarvestHelper();
      foreach (FieldInfo ffield in fields)
      {
        var name = helper.SanitizeFieldName(ffield.Name);

        Introspect(ffield.GetValue(source), new Field(name));
      }
      tokens.Add(new Token(TokenType.EndScope));
    }


    bool IntrospectSimpleValue(object source, Field field, Type sourceType)
    {
      IValueConverter handler = null;
      if (!Configuration.TryGetValueConverter(sourceType, out handler))
        return false;

      tokens.Add(new Token(TokenType.SimpleFieldValue, field, handler.Convert(source)));
      return true;
    }


    bool IntrospectNullValue(object source, Field field)
    {
      if (source != null)
        return false;

      tokens.Add(new Token(TokenType.SimpleFieldValue, field, "null"));
      return true;
    }


    bool IntrospectDictionaryWithSimpleKey(IDictionary source, Field field, Type sourceType)
    {
      if (source == null)
        return false;

      if(sourceType.GetGenericArguments().Length != 2)
        return false;

      IValueConverter handler;
      var keyType = sourceType.GetGenericArguments().First();
      var isKeyTypeSimple = Configuration.TryGetValueConverter(keyType, out handler);

      if (!isKeyTypeSimple)
        return false; // print as enumerable which is more verbose

      tokens.Add(new Token(TokenType.StartEnumeration));

      var keys = source.Keys;
      foreach (var key in keys)
      {
        var valueValue = source[key];
        var keyValue = handler.Convert(key);
        var outputfieldName = new Field(field.Name, keyValue);
        Introspect(valueValue, outputfieldName);
      }
      tokens.Add(new Token(TokenType.EndEnumeration));

      return true;
    }

    private bool IntrospectIEnumerable(object source, Field field)
    {
      var enumerable = source as IEnumerable;
      if (enumerable == null)
        return false;

      Reference optionReferenceInfo = null;
      seenBefore.TryGetValue(source, out  optionReferenceInfo);

      tokens.Add(new Token(TokenType.FieldnameWithTypeAndReference, field, null, optionReferenceInfo, source.GetType()));
      tokens.Add(new Token(TokenType.StartEnumeration));

      int i = 0;
      foreach (var x in enumerable)
      {
        var outputFieldName = new Field(field.Name, ""+ i++);
        Introspect(x, outputFieldName);
      }
      tokens.Add(new Token(TokenType.EndEnumeration));

      return true;
    }
  }


  public class Field
  {
    public readonly string Name;
    public readonly string SimpleKeyInArrayOrDictionary;

    public Field(string name, string simpleKeyInArrayOrDictionary = null)
    {
      Name = name;
      SimpleKeyInArrayOrDictionary = simpleKeyInArrayOrDictionary;
    }
  }
}