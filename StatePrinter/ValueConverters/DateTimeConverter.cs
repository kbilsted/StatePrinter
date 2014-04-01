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

namespace StatePrinter.ValueConverters
{
  /// <summary>
  /// Handles the printing of <see cref="DateTime"/> and <see cref="DateTimeOffset"/>
  /// </summary>
  public class DateTimeConverter : IValueConverter
  {
    public bool CanHandleType(Type t)
    {
      return t == typeof(DateTime) || t == typeof(DateTimeOffset);
    }

    public string Convert(object source)
    {
      if (source is DateTime)
        return ((DateTime)source).ToString("dd.MM.yyyy HH:m:ss");
      
      if (source is DateTimeOffset)
        return ((DateTimeOffset)source).ToString("dd.MM.yyyy HH:m:ss zzz");

      throw new Exception("We should never reach here");
    }
  }
}