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

namespace StatePrinter.FieldHarvesters
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
        private Dictionary<Type, bool> cache = new Dictionary<Type, bool>();
        private Dictionary<Type, MethodInfo> methodInfos = new Dictionary<Type, MethodInfo>();
        private IFieldHarvester harvester;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="backupHarvester">The harvestor that will be used if the type has no explicit ToString implementation</param>
        public ToStringAwareHarvester(IFieldHarvester backupHarvester)
        {
            harvester = backupHarvester;
        }

        public bool CanHandleType(Type type)
        {
            return true;
        }

        /// <summary>
        ///   We ignore all properties as they, in the end, will only point to some computed state or other fields.
        ///   Hence they do not provide information about the actual state of the object.
        /// </summary>
        public List<SanitiedFieldInfo> GetFields(Type type)
        {
            bool hasToString;
            if (!cache.TryGetValue(type, out hasToString))
            {
                //This more thorough way to avoid the "Ambiguous match found" exception while retrieving the ToString method
                //Explained here http://stackoverflow.com/questions/11443707/getproperty-reflection-results-in-ambiguous-match-found-on-new-property
                var methodInfo = type.GetMethod(
                    "ToString",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly,
                    null,
                    new Type[] {}, // Method ToString() without parameters
                    null);
                hasToString = methodInfo != null;
                cache[type] = hasToString;
                if (hasToString) methodInfos[type] = methodInfo;
            }
            
            if (!hasToString)
                return harvester.GetFields(type);

            Func<object, object> valueProvider = o => methodInfos[type].Invoke(o, new object[0]);
            var syntesizedField = new SanitiedFieldInfo(null, "ToString()", valueProvider);

            return new List<SanitiedFieldInfo>() {syntesizedField};
            }
    }
}