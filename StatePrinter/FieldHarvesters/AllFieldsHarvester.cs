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
  /// Harvest all fields, public and private. 
  /// 
  /// We ignore the types from the following namespaces
  /// <see cref="System.Reflection"/> 
  /// <see cref="System.Runtime"/>
  /// <see cref="System.Func"/>
  /// </summary>
  public class AllFieldsHarvester : IFieldHarvester
  {
    public bool CanHandleType(Type type)
    {
      return true;
    }

    /// <summary>
    ///   We ignore all properties as they, in the end, will only point to some computated state or other fields.
    ///   Hence they do not provide information about the actual state of the object.
    /// </summary>
    public List<FieldInfo> GetFields(Type type)
    {
      return new HarvestHelper().GetFields(type);
    }
  }
}