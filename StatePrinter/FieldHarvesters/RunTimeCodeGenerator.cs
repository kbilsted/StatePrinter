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
using System.Linq.Expressions;
using System.Reflection;

namespace StatePrinter.FieldHarvesters
{
    /// <summary>
    /// Run-time code generation is much faster than using ordinary reflection
    /// </summary>
    public class RunTimeCodeGenerator
    {
        /// <summary>
        /// A fast alternative to the reflection methods <see cref="FieldInfo.GetValue"/> and <see cref="PropertyInfo.GetValue(object,object[])"/>
        /// </summary>
        public Func<object, object> CreateGetter(MemberInfo memberInfo)
        {
            if (!(memberInfo is FieldInfo) && !(memberInfo is PropertyInfo))
            {
                throw new ArgumentException("Parameter memberInfo must be of type FieldInfo or PropertyInfo.");
            }

            if (memberInfo.DeclaringType == null)
            {
                throw new ArgumentException("MemberInfo cannot be a global member.");
            }

            var p = Expression.Parameter(typeof(object), "p");
            var objExpr = Expression.Convert(p, memberInfo.DeclaringType);
            var field = Expression.PropertyOrField(objExpr, memberInfo.Name);
            var castRes = Expression.Convert(field, typeof(object));
            return Expression.Lambda<Func<object, object>>(castRes, p).Compile();
        }

        /// <summary>
        /// A fast alternative to the reflection methods <see cref="FieldInfo.GetValue"/> and <see cref="PropertyInfo.GetValue(object,object[])"/>
        /// </summary>
        public Func<object, object> CreateGetter(MemberInfo memberInfo, Expression mapexpression, Type basetype)
        {
            if (!(memberInfo is FieldInfo) && !(memberInfo is PropertyInfo))
            {
                throw new ArgumentException("Parameter memberInfo must be of type FieldInfo or PropertyInfo.");
            }

            if (memberInfo.DeclaringType == null)
            {
                throw new ArgumentException("MemberInfo cannot be a global member.");
            }

            if (mapexpression == null)
            {
                throw new ArgumentException("Mapexpression cannot be a null.");
            }

            if (basetype == null)
            {
                throw new ArgumentException("Basetype cannot be a null.");
            }

            var p = Expression.Parameter(typeof(object), "p");
            var objExpr = Expression.Convert(p, basetype);
            var invokeMap = Expression.Invoke(mapexpression, objExpr);
            var typedMapExpr = Expression.Convert(invokeMap, memberInfo.DeclaringType);
            var field = Expression.PropertyOrField(typedMapExpr, memberInfo.Name);
            var castRes = Expression.Convert(field, typeof(object));
            return Expression.Lambda<Func<object, object>>(castRes, p).Compile();
        }
    }
}