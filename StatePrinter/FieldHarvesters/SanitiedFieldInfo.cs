// Copyright 2014-2015 Kasper B. Graversen
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
using System.Reflection;

namespace StatePrinter.FieldHarvesters
{
    /// <summary>
    /// For each type we print, we hold the reflected and the readable version of the fields
    /// </summary>
    public class SanitiedFieldInfo
    {
        public readonly MemberInfo FieldInfo;

        /// <summary>
        /// The sanitized name is the name the user would expect.
        /// E.g. the field 'X' has the value 'X' and the property 'Y' has the value 'Y' rather than the value '&lt;Y&gt;k__BackingField'.
        /// </summary>
        public readonly string SanitizedName;

        /// <summary>
        /// Functionality to fetch the value of the field.
        /// </summary>
        public readonly Func<object, object> ValueProvider;

        public SanitiedFieldInfo(
            MemberInfo fieldInfo,
            string sanitizedName,
            Func<object, object> valueProvider)
        {
            FieldInfo = fieldInfo;
            SanitizedName = sanitizedName;
            ValueProvider = valueProvider;
        }
    }
}