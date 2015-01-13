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
using System.Linq;
using System.Reflection;

namespace StatePrinter.FieldHarvesters
{
  /// <summary>
  /// Reusable helper methods when implementing <see cref="IFieldHarvester"/>
  /// </summary>
  public class HarvestHelper
  {
    internal const string BackingFieldSuffix = ">k__BackingField";

    /// <summary>
    /// We ignore all properties as they, in the end, will only point to some computated state or other fields.
    /// Hence they do not provide information about the actual state of the object.
    /// </summary>
    public List<SanitiedFieldInfo> GetFields(Type type)
    {
      const BindingFlags flags = BindingFlags.Public
                            | BindingFlags.NonPublic
                            | BindingFlags.Instance
                            | BindingFlags.DeclaredOnly;

      return GetFields(type, flags)
        .Select(x => new SanitiedFieldInfo(x, SanitizeFieldName(x.Name), (object o) => x.GetValue(o)))
        .ToList();
    }

    /// <summary>
    /// We ignore all properties as they, in the end, will only point to some computated state or other fields.
    /// Hence they do not provide information about the actual state of the object.
    /// </summary>
    public IEnumerable<FieldInfo> GetFields(Type type, BindingFlags flags)
    {
      if (type == null)
        return Enumerable.Empty<FieldInfo>();

      if (!IsHarvestable(type))
        return Enumerable.Empty<FieldInfo>();

      return GetFields(type.BaseType, flags).Concat(type.GetFields(flags));
    }

    /// <summary>
    /// Tell if the type makes any sense to dump
    /// </summary>
    public bool IsHarvestable(Type type)
    {
      var typename = type.ToString();
      if (typename.StartsWith("System.Reflection")
          || typename.StartsWith("System.Runtime")
          || typename.StartsWith("System.SignatureStruct")
          || typename.StartsWith("System.Func"))
        return false;
      return true;
    }

    /// <summary>
    /// Replaces the name of properties to remove the k__BackingField nonsense from the name.
    /// </summary>
    public string SanitizeFieldName(string fieldName)
    {
      return fieldName.StartsWith("<")
        ? fieldName.Substring(1).Replace(BackingFieldSuffix, "")
        : fieldName;
    }
  }

  /// <summary>
  /// For each type we print, we hold the reflected and the readable version of the fields
  /// </summary>
  public class SanitiedFieldInfo
  {
    public readonly FieldInfo FieldInfo;

    /// <summary>
    /// The sanitized name is the name the user would expect.
    /// E.g. the field 'X' has the value 'X' and the property 'Y' has the value 'Y' rather than the value '&lt;Y&gt;k__BackingField'.
    /// </summary>
    public readonly string SanitizedName;

    /// <summary>
    /// Functionality to fetch the value of the field.
    /// </summary>
    public readonly Func<object, object> ValueProvider;

    public SanitiedFieldInfo(FieldInfo fieldInfo, string sanitizedName, Func<object, object> valueProvider)
    {
      FieldInfo = fieldInfo;
      SanitizedName = sanitizedName;
      ValueProvider = valueProvider;
    }
  }
}