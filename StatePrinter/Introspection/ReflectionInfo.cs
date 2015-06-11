﻿// Copyright 2014 Kasper B. Graversen
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
using StatePrinter.FieldHarvesters;

namespace StatePrinter.Introspection
{
    /// <summary>
    /// For a given type we hold the relevant fields with regards to printing, and functors for retrieving the content of the fields. 
    /// The functors enables synthetic fields. E.g. a field that represents the call to the object's ToString().
    /// </summary>
    class ReflectionInfo
    {
        public readonly List<Field> Fields;
        public readonly List<Func<object, object>> ValueProviders;

        public ReflectionInfo(List<SanitizedFieldInfo> rawReflectedFields)
        {
            Fields = rawReflectedFields.Select(x => new Field(x.SanitizedName)).ToList();
            ValueProviders = rawReflectedFields.Select(x => x.ValueProvider).ToList();
        }
    }
}