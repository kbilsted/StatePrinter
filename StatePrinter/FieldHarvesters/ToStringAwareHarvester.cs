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
using System.Reflection;

namespace StatePrinting.FieldHarvesters
{
    /// <summary>
    /// Harvest all fields, public and private. Or use the ToString() if such is implemented on the class.
    /// 
    /// We ignore the types from the following namespaces
    /// <see cref="System.Reflection"/> 
    /// <see cref="System.Runtime"/>
    /// <see cref="System.Func"/>
    /// </summary>
    public class ToStringAwareHarvester : IFieldHarvester
    {
        readonly Dictionary<Type, bool> cache = new Dictionary<Type, bool>();

        readonly Dictionary<Type, MethodInfo> methodInfos = new Dictionary<Type, MethodInfo>();

        public ToStringAwareHarvester()
        {
        }

        public bool CanHandleType(Type type)
        {
            bool hasToString;
            if (!cache.TryGetValue(type, out hasToString))
            {
                var methodInfo = GetMethodInfo(type);
                hasToString = methodInfo != null;
                cache[type] = hasToString;
                if (hasToString)
                    methodInfos[type] = methodInfo;
            }

            return hasToString;
        }

        /// <summary>
        ///   We ignore all properties as they, in the end, will only point to some computed state or other fields.
        ///   Hence they do not provide information about the actual state of the object.
        /// </summary>
        public List<SanitizedFieldInfo> GetFields(Type type)
        {
            Func<object, object> valueProvider = (o) => methodInfos[type].Invoke(o, new object[0]);
            var syntesizedField = new SanitizedFieldInfo(null, "ToString()", valueProvider);

            return new List<SanitizedFieldInfo>() { syntesizedField };
        }

        /// <summary>
        /// This more thorough way to avoid the "Ambiguous match found" exception while retrieving the ToString method
        /// Explained here http://stackoverflow.com/questions/11443707/getproperty-reflection-results-in-ambiguous-match-found-on-new-property
        /// </summary>
        MethodInfo GetMethodInfo(Type type)
        {
            var methodInfo = type.GetMethod(
              "ToString",
              BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly,
              null,
              new Type[] { }, // Method ToString() without parameters
              null);
            return methodInfo;
        }
    }
}