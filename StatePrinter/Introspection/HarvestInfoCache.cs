﻿// Copyright 2014 Kasper B. Graversen
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
using System.Threading;
using StatePrinter.FieldHarvesters;

namespace StatePrinter.Introspection
{
    /// <summary>
    /// Assume that types don't change at run-time. Cache harvested information.
    /// </summary>
    class HarvestInfoCache : IDisposable
    {
        /// <summary>
        /// Due to supporting C# 3.5 we cannot use ConcurrentDictionary
        /// </summary>
        readonly Dictionary<Type, List<SanitizedFieldInfo>> harvestCache = new Dictionary<Type, List<SanitizedFieldInfo>>();
        readonly ReaderWriterLockSlim cacheLock = new ReaderWriterLockSlim();

        public List<SanitizedFieldInfo> TryGet(Type type)
        {
            cacheLock.EnterReadLock();
            try
            {
                List<SanitizedFieldInfo> res;
                harvestCache.TryGetValue(type, out res);
                return res;
            }
            finally
            {
                cacheLock.ExitReadLock();
            }
        }

        public void TryAdd(Type type, List<SanitizedFieldInfo> fields)
        {
            cacheLock.EnterWriteLock();
            try
            {
                harvestCache.Add(type, fields);
            }
            finally
            {
                cacheLock.ExitWriteLock();
            }
        }

        public void Dispose()
        {
            cacheLock.Dispose();
        }
    }
}
    