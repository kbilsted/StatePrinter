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

using System.Globalization;

using NUnit.Framework;

using StatePrinter.Configurations;

namespace StatePrinter.Tests
{
    static class TestHelper
    {

        public static Stateprinter CreateTestPrinter()
        {
            var cfg = ConfigurationHelper.GetStandardConfiguration()
                .SetAreEqualsMethod(NUnit.Framework.Assert.AreEqual)
                .SetCulture(CultureInfo.CreateSpecificCulture("da-DK"))
                .SetAutomaticTestRewrite((x) => false);

            return new Stateprinter(cfg);
        }

        public static Configuration CreateTestConfiguration()
        {
            var res = ConfigurationHelper
                .GetStandardConfiguration("")
                .SetNewlineDefinition(" ")
                .SetAreEqualsMethod(Assert.AreEqual);

            return res;
        }
    }
}
