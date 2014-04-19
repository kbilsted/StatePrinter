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
using System.Reflection;
using StatePrinter.FieldHarvesters;

namespace StatePrinter.Introspection
{
  /// <summary>
  /// For each type we print, we hold the reflected and the readable version of the fields
  /// </summary>
  class ReflectionInfo
  {
    public readonly List<FieldInfo> RawReflectedFields;
    public readonly List<Field> Fields;  

    public ReflectionInfo(List<FieldInfo> rawReflectedFields)
    {
      Fields = new List<Field>(rawReflectedFields.Count);
      RawReflectedFields = rawReflectedFields;

      var helper = new HarvestHelper();

      foreach (var field in rawReflectedFields)
      {
        var name = helper.SanitizeFieldName(field.Name);
        Fields.Add(new Field(name));
      }
    }
  }
}