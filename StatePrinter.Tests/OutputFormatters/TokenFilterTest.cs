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

using System.Collections.Generic;
using NUnit.Framework;
using StatePrinting.Introspection;
using StatePrinting.OutputFormatters;

namespace StatePrinter.Tests.OutputFormatters
{
    [TestFixture]
    class TokenFilterTest
    {

        [Test]
        public void GetHashcode()
        {
            var sut = new Token(TokenType.FieldnameWithTypeAndReference, null, null, null, null);
            Assert.AreEqual(1156279432, sut.GetHashCode());
        }

        [Test]
        public void Transform_noncycle()
        {
            var nonCycleTokens = new List<Token>()
                           {
                             new Token(TokenType.FieldnameWithTypeAndReference, new Field("fieldA"), "value1", new Reference(1), typeof(string) ),
                             new Token(TokenType.FieldnameWithTypeAndReference, new Field("fieldB"), "value2", new Reference(2), typeof(string)),
                           };

            var filter = new UnusedReferencesTokenFilter();
            var newlist = filter.FilterUnusedReferences(nonCycleTokens);

            // test
            var expected = new List<Token>()
                           {
                             new Token(TokenType.FieldnameWithTypeAndReference, new Field("fieldA"), "value1", null, typeof(string)),
                             new Token(TokenType.FieldnameWithTypeAndReference, new Field("fieldB"), "value2", null, typeof(string)),
                           };

            CollectionAssert.AreEqual(expected, newlist);
        }

        [Test]
        public void Transform_cycle()
        {
            var nonCycleTokens = new List<Token>()
                           {
                             new Token(TokenType.FieldnameWithTypeAndReference, new Field("fieldA"), "value1", new Reference(0), typeof(string)),
                             new Token(TokenType.FieldnameWithTypeAndReference, new Field("fieldB"), "value2", new Reference(1), typeof(int)),
                             Token.SeenBefore(new Field("FieldB"), new Reference(1)),
                           };

            var filter = new UnusedReferencesTokenFilter();
            var newlist = filter.FilterUnusedReferences(nonCycleTokens);

            // test
            var expected = new List<Token>()
                           {
                             new Token(TokenType.FieldnameWithTypeAndReference, new Field("fieldA"), "value1", null, typeof(string)),
                             new Token(TokenType.FieldnameWithTypeAndReference, new Field("fieldB"), "value2", new Reference(0), typeof(int)),
                             Token.SeenBefore(new Field("FieldB"), new Reference(0)),
                           };

            CollectionAssert.AreEqual(expected, newlist);
        }
    }
}
