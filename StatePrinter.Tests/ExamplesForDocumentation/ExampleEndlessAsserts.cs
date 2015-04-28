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

using NUnit.Framework;

namespace StatePrinter.Tests.ExamplesForDocumentation
{
    [TestFixture]
    class ExampleListAndArrays
    {
        
        [Test]
        public void EndlessAssertsAlternative()
        {
            var allocation = new AllocationData
            {
                Premium = 22,
                FixedCosts = 23,
                PremiumCosts = 140,
                Tax = 110
            };

            var sut = new Allocator();
            var allocateData = sut.CreateAllocation(allocation);
            
            var printer = TestHelper.CreateTestPrinter();
            var expected = @"new AllocationDataResult()
{
    Premium = 22
    OriginalDueDate = 01-01-2010 00:00:00
    Costs = new CostData()
    {
        MonthlyBillingFixedInternalCost = 38
        BillingInternalCost = 55
        MonthlyBillingFixedRunningRemuneration = 63
        MonthlyBillingFixedEstablishment = 53
        MonthlyBillingRegistration = 2
    }
    PremiumInternalCost = 1
    PremiumRemuneration = 2
    PremiumRegistration = 332
    PremiumEstablishment = 14
    PremiumInternalCostBeforeDiscount = 57
    PremiumInternalCostAfterDiscount = 37
    Tax = 110
}";
            printer.Assert.PrintAreAlike(expected, allocateData);
        }

    }

    class Allocator 
    {
        public AllocationDataResult CreateAllocation(AllocationData allocation)
        {
            var allocateData = new AllocationDataResult();
            allocateData.Premium = allocation.Premium;

            allocateData.OriginalDueDate = new DateTime(2010, 1, 1);

            allocateData.Costs = new CostData();
            allocateData.Costs.MonthlyBillingFixedInternalCost = 38;
            allocateData.Costs.BillingInternalCost = 55;
            allocateData.Costs.MonthlyBillingFixedRunningRemuneration = 63;
            allocateData.Costs.MonthlyBillingFixedEstablishment = 53;
            allocateData.Costs.MonthlyBillingRegistration = 2;

            allocateData.PremiumInternalCost = 1;
            allocateData.PremiumRemuneration = 2;
            allocateData.PremiumRegistration = 332;
            allocateData.PremiumEstablishment = 14;

            allocateData.PremiumInternalCostBeforeDiscount = 57;
            allocateData.PremiumInternalCostAfterDiscount = 37;

            allocateData.Tax = allocation.Tax;
            return allocateData;
        }
    }

    class AllocationDataResult  
    {
        public int Premium { get; set; }
        public DateTime OriginalDueDate { get; set; }
        public CostData  Costs { get; set; }
        public int PremiumInternalCost { get; set; }
        public int PremiumRemuneration { get; set; }
        public int PremiumRegistration { get; set; }
        public int PremiumEstablishment { get; set; }
        public int PremiumInternalCostBeforeDiscount { get; set; }
        public int PremiumInternalCostAfterDiscount { get; set; }
        public int Tax { get; set; }
    }

    class CostData  
    {
        public int MonthlyBillingFixedInternalCost { get; set; }
        public int BillingInternalCost { get; set; }
        public int MonthlyBillingFixedRunningRemuneration { get; set; }
        public int MonthlyBillingFixedEstablishment { get; set; }
        public int MonthlyBillingRegistration { get; set; }
    }

    class AllocationData    
    {
        public int Premium { get; set; }
        public int FixedCosts { get; set; }
        public int PremiumCosts { get; set; }
        public int Tax { get; set; }
    }
}
