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
using StatePrinting.Configurations;
using StatePrinting.FieldHarvesters;
using StatePrinting.ValueConverters;

namespace StatePrinting.Introspection
{
    /// <summary>
    /// Is responsible for traversing an object graph and returning a stream of tokens
    /// </summary>
    class IntroSpector
    {
        static readonly Token Startscope = new Token(TokenType.StartScope);
        static readonly Token Endscope = new Token(TokenType.EndScope);
        static readonly Token StartList = new Token(TokenType.StartList);
        static readonly Token EndList = new Token(TokenType.EndList);
        static readonly Token StartDict = new Token(TokenType.StartDict);
        static readonly Token EndDict = new Token(TokenType.EndDict);

        readonly Configuration configuration;
        readonly HarvestInfoCache harvestCache;

        /// <summary>
        /// Each entry is assigned a reference number used for back-referencing
        /// </summary>
        int referenceCounter = 0;

        readonly Dictionary<object, Reference> seenBefore = new Dictionary<object, Reference>();
        readonly List<Token> tokens = new List<Token>();

        public IntroSpector(Configuration configuration, HarvestInfoCache harvestCache)
        {
            this.configuration = configuration;
            this.harvestCache = harvestCache;
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

            if (IntrospectIEnumerable(source, field, sourceType))
                return;

            IntrospectComplexType(source, field, sourceType);
        }

        private bool SeenObjectBefore(object source, Field field)
        {
            Reference reference;
            if (seenBefore.TryGetValue(source, out reference))
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
            Reference optionReferenceInfo;
            seenBefore.TryGetValue(source, out optionReferenceInfo);

            tokens.Add(new Token(TokenType.FieldnameWithTypeAndReference, field, null, optionReferenceInfo, sourceType));
            tokens.Add(Startscope);

            ReflectionInfo reflection = ReflectFields(sourceType);
            for (int i = 0; i < reflection.Fields.Length; i++)
            {
                Introspect(reflection.ValueProviders[i](source), reflection.Fields[i]);
            }

            tokens.Add(Endscope);
        }

        ReflectionInfo ReflectFields(Type sourceType)
        {
            ReflectionInfo reflection;
            if ((reflection = harvestCache.TryGet(sourceType)) == null)
            {
                IFieldHarvester harvester;
                if (!configuration.TryGetFieldHarvester(sourceType, out harvester))
                    throw new Exception(
                            string.Format(
                                    "No fieldharvester is configured for handling type '{0}'. Try using 'ConfigurationHelper.GetStandardConfiguration()' to get started.",
                                    sourceType));

                List<SanitizedFieldInfo> fields = harvester.GetFields(sourceType);
                reflection = new ReflectionInfo(fields);
                harvestCache.TryAdd(sourceType, reflection);
            }

            return reflection;
        }

        bool IntrospectSimpleValue(object source, Field field, Type sourceType)
        {
            IValueConverter handler;
            if (!configuration.TryGetValueConverter(sourceType, out handler))
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

            if (sourceType.GetGenericArguments().Length != 2)
                return false;

            IValueConverter handler;
            var keyType = sourceType.GetGenericArguments().First();
            var isKeyTypeSimple = configuration.TryGetValueConverter(keyType, out handler);

            if (!isKeyTypeSimple)
                return false; // print as enumerable which is more verbose

            Reference optionReferenceInfo;
            seenBefore.TryGetValue(source, out optionReferenceInfo);

            tokens.Add(new Token(TokenType.FieldnameWithTypeAndReference, field, null, optionReferenceInfo, source.GetType()));
            tokens.Add(StartDict);

            IEnumerable keys = source.Keys;
            if (configuration.TryGetEnumerableOrderer(keys.GetType(), out var orderer))
            {
                keys = orderer.Order(keys);
            }

            foreach (var key in keys)
            {
                var valueValue = source[key];
                var keyValue = handler.Convert(key);
                var outputfieldName = new Field(field.Name, keyValue);
                Introspect(valueValue, outputfieldName);
            }
            tokens.Add(EndDict);

            return true;
        }

        private bool IntrospectIEnumerable(object source, Field field, Type sourceType)
        {
            var enumerable = source as IEnumerable;
            if (enumerable == null)
                return false;

            Reference optionReferenceInfo;
            seenBefore.TryGetValue(source, out optionReferenceInfo);

            tokens.Add(new Token(TokenType.FieldnameWithTypeAndReference, field, null, optionReferenceInfo, source.GetType()));
            tokens.Add(StartList);

            if (configuration.TryGetEnumerableOrderer(sourceType, out var orderer))
            {
                enumerable = orderer.Order(enumerable);
            }

            int i = 0;
            foreach (var x in enumerable)
            {
                Introspect(x, new Field(field.Name, null, i++));
            }
            tokens.Add(EndList);

            return true;
        }
    }


    public class Field : IEquatable<Field>
    {
        public readonly string Name;
        public readonly string Key;
        public readonly int? Index;

        public Field(string name, string key = null, int? index = null)
        {
            Name = name;
            Key = key;
            Index = index;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Field);
        }

        public bool Equals(Field other)
        {
            return other != null
                && Name == other.Name
                && Key == other.Key
                && Index == other.Index;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 131;
                hashCode = (hashCode * 397) ^ (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Key != null ? Key.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Index != null ? Index.Value.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}