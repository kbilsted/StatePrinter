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
using System.Linq;
using NUnit.Framework;
using StatePrinting.Introspection;
using StatePrinting.OutputFormatters;

namespace StatePrinting.Tests.Introspection
{
    [TestFixture]
    class TokenTest
    {
        [Test]
        public void Equals()
        {
            var a = new Token(TokenType.StartScope);
            var b = new Token(TokenType.StartScope);
            Assert.AreEqual(a, a);
            Assert.AreEqual(a, b);
            Assert.IsFalse(a.Equals((Token)null));
            Assert.IsTrue(a.Equals(a));
            Assert.IsTrue(a.Equals((object)b));
        }

        [Test]
        public void NotEquals()
        {
            var a = new Token(TokenType.StartScope);
            var b = new Token(TokenType.EndScope);
            Assert.AreNotEqual(a, b);
            Assert.AreNotEqual(a, null);

            b = new Token(TokenType.StartScope, reference: new Reference(2));
            Assert.AreNotEqual(a, b);
        }
    }

    /// <summary>
    /// Outputformatter that can show what has been introspected. For unit testing only
    /// </summary>
    public class TokenOutputter : IOutputFormatter
    {
        public List<Token> IntrospectedTokens; 

        public string Print(List<Token> tokens)
        {
            IntrospectedTokens = tokens.ToList();
            return "The result of this outputter is found in the field 'IntrospectedTokens'";
        }
    }

}
