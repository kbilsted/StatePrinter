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

namespace StatePrinting.ValueConverters
{
    /// <summary>
    /// A handler that is able to convert a value of specific types to on a single line string.
    /// </summary>
    public interface IValueConverter
    {
        /// <summary>
        /// Is the type covered by this handler.
        /// </summary>
        bool CanHandleType(Type type);

        /// <summary>
        /// Convert objects of handled types into a simple one-line representation.
        /// </summary>
        string Convert(object source);
    }
}