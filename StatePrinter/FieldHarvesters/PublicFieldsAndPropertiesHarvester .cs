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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace StatePrinting.FieldHarvesters
{
    /// <summary>
    /// Harvest only public fields and properties from types.
    /// 
    /// We ignore the types from the following namespaces
    /// <see cref="System.Reflection"/> 
    /// <see cref="System.Runtime"/>
    /// <see cref="System.Func"/>
    /// </summary>
    public class PublicFieldsAndPropertiesHarvester : IFieldHarvester
    {
        public bool CanHandleType(Type type)
        {
            return true;
        }

        /// <summary>
        /// Harvest only public fields and properties.
        /// </summary>
        public List<SanitizedFieldInfo> GetFields(Type type)
        {
            var fields = new HarvestHelper().GetFieldsAndProperties(type);

            return fields.Where(IsPublic).ToList();
        }

        bool IsPublic(SanitizedFieldInfo field)
        {
            switch (field.FieldInfo.MemberType)
            {
                case MemberTypes.Field:
                    if ((field.FieldInfo as FieldInfo).IsPublic) return true;
                    break;
                case MemberTypes.Property:
                    var propertyInfo = (PropertyInfo)field.FieldInfo;
                    if (propertyInfo.GetGetMethod(false) != null) return true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return false;
        }
    }
}