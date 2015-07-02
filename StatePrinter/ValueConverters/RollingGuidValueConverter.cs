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

namespace StatePrinter.ValueConverters
{
    /// <summary>
    /// Handles the printing of guids, where each guid is transformed into a very readable form by 
    /// transforming a small integer into the guid-format.
    /// 
    /// When the same guid is to be printed multiple times, it retains its new simpler-to-read value.
    /// 
    /// This is nice for unit testing, since guids now become stable across multiple executions
    /// </summary>
    public class RollingGuidValueConverter : IValueConverter
    {
        readonly Dictionary<Guid, string> producedGuids = new Dictionary<Guid, string>();
        int count = 0;

        public bool CanHandleType(Type t)
        {
            return t == typeof(Guid);
        }

        public string Convert(object source)
        {
            var guid = (Guid)source;
            string result;
            if (!producedGuids.TryGetValue(guid, out result))
            {
                result = CreateRollingGuid();
                producedGuids.Add(guid, result);
            }
            return result;
        }

        string CreateRollingGuid()
        {
            if(count == int.MaxValue)
                throw new Exception("Such hign number is Not yet supported");
            count++;

            return string.Format("00000000-0000-0000-0000-00{0:D10}", count);
        }
    }
}