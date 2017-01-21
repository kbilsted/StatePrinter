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

namespace StatePrinting.FieldHarvesters
{
    /// <summary>
    /// Harvest from a type as specified by the provided functions.
    /// </summary>
    public class AnonymousHarvester : IFieldHarvester
    {
        private readonly Func<Type, bool> canHandleTypeFunc;
        private readonly Func<Type, List<SanitizedFieldInfo>> getFieldsFunc;

        public AnonymousHarvester(Func<Type, bool> canHandleTypeFunc, Func<Type, List<SanitizedFieldInfo>> getFieldsFunc)
        {
            this.canHandleTypeFunc = canHandleTypeFunc;
            this.getFieldsFunc = getFieldsFunc;
        }

        public bool CanHandleType(Type type)
        {
            return canHandleTypeFunc(type);
        }

        public List<SanitizedFieldInfo> GetFields(Type type)
        {
            return getFieldsFunc(type);
        }
    }
}