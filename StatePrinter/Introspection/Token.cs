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
using StatePrinter.OutputFormatters;

namespace StatePrinter.Introspection
{
    /// <summary>
    /// The result of introspection. 
    /// A <see cref="IOutputFormatter"/> transforms tokens into the final output format.
    /// </summary>
    public class Token : IEquatable<Token>
    {
        public readonly TokenType Tokenkind;
        public readonly Type FieldType;
        public readonly Field Field;
        public readonly string Value;

        /// <summary>
        /// Each entry is assigned an internal referenceno.
        /// </summary>
        public readonly Reference ReferenceNo;

        /// <summary>
        /// constructor method
        /// </summary>
        public static Token SeenBefore(Field field, Reference reference)
        {
            return new Token(TokenType.SeenBeforeWithReference, field, null, reference, null);
        }

        public Token(TokenType type, Field field = null, string value = null, Reference reference = null, Type fieldFieldType = null)
        {
            Tokenkind = type;
            Field = field;

            ReferenceNo = reference;
            Value = value;
            FieldType = fieldFieldType;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Token);
        }

        public bool Equals(Token other)
        {
            if (other == null) return false;
            if (ReferenceEquals(other, this)) return true;
            if (Tokenkind != other.Tokenkind) return false;
            if (!Equals(ReferenceNo, other.ReferenceNo)) return false;
            if (Value != other.Value) return false;
            if (FieldType != other.FieldType) return false;

            return true;
        }


        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int)Tokenkind;
                hashCode = (hashCode * 397) ^ (FieldType != null ? FieldType.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Field != null ? Field.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Value != null ? Value.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ReferenceNo != null ? ReferenceNo.GetHashCode() : 0);

                return hashCode;
            }
        }
    }
}