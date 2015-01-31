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

using System.Linq;
using NUnit.Framework;
using StatePrinter.FieldHarvesters;
using StatePrinter.Tests.IntegrationTests;

namespace StatePrinter.Tests.FieldHarvesters
{
    [TestFixture]
    class AllHarvesterTest
    {
        HarvestHelper helper = new HarvestHelper();

        [Test]
        public void AllFieldsHarvestTest()
        {
            var harvester = new AllFieldsHarvester();

            var fields = harvester.GetFields(typeof(Car)).Select(x => x.SanitizedName).ToArray();
            CollectionAssert.AreEquivalent(new[] { "StereoAmplifiers", "steeringWheel", "Brand" }, fields);

            fields = harvester.GetFields(typeof(SteeringWheel)).Select(x => x.SanitizedName).ToArray();
            CollectionAssert.AreEquivalent(new[] { "Size", "Grip", "Weight" }, fields);
        }

        [Test]
        public void PublicFieldsHarvesterTest()
        {
            var harvester = new PublicFieldsHarvester();

            var fields = harvester.GetFields(typeof(Car)).Select(x => x.SanitizedName).ToArray();
            CollectionAssert.AreEquivalent(new[] { "Brand" }, fields);

            fields = harvester.GetFields(typeof(SteeringWheel)).Select(x => x.SanitizedName).ToArray();
            CollectionAssert.AreEquivalent(new string[0] { }, fields);
        }
    }
}