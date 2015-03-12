#  ![](https://raw.github.com/kbilsted/StatePrinter/master/StatePrinter/gfx/stateprinter.png) StatePrinter automating your unit tests



**Table of content**
* [1. The problems with how we assert in unit tests](#1-the-problems-with-how-we-assert-in-unit-tests)
* [2. Examples of hard to read and maintain unit tests](#2-examples-of-hard-to-read-and-maintain-unit-tests)

 
This page highlights the pains and problems with how we write unit tests today. It is a philosophical discussion that applies to asserting in unit tests. To see how to use StatePrinter to remedy these problems please refer to https://github.com/kbilsted/StatePrinter/blob/master/doc/AutomatingToStrings.md

# 1. The problems with how we assert in unit tests

We've quickly looked at how you can automate your unit testing process. But so far, we have not looked at what the problems are with the traditional way of doing unit testing. Here are my current thoughts.


#### It is laborious. 

When I type and re-type over and over again: `Assert.This`, `Assert.That`, ... can't help but wonder why the computer cannot automate this stuff for me. All that needless typing takes time and drains my energy.

*When using Stateprinter, the asserts are generated for you whenever there is a mismatch between expected and actual values.*

#### Code and test gets out of sync

When the code changes, say by adding a field to a class, you need to add asserts in some of your tests. Locating  where, though, is an entirely manual process. On larger project where no one has the full overview of all classes, the needed changes are not performed in all the places it should. 

A similar situation arises when merging code from one branch to another. Say you merge a bug fix or feature from a release branch to the development branch, what I observe over and over again is that the code gets merged, all the tests are run and then the merge is committed. People forget to revisit and double check the entire test suite to figure out there are tests existing on the development branch and not on the branch from where the merge occured, an adjust these accordingly.

*When using Stateprinter, object graphs are compared rather than single fields. Thus, when a new field is created, all relevant tests fail. You can adjust the printing to specific fields, but you loose the ability to automatically detect changes in the graph.*


#### Detrimental to change

Ironically, while tests initially makes you code faster and with more confidence, tests, or rather the way we do asserts, can easily be detrimental to code changes later on. A fact of life is that business requirements change. When they do, you have to change the implementation and all the code. Most of the time, a hand full of tests are unit testing the heart of the requirements, while the other tests, say module-, integration- and acceptance-tests serve to put into perspective the requirement executed in relation to other data and other requirements. Most of the time when correcting the asserts of such tests is time consuming, annoying. You no longer feel free, you feel shackled and dread the next requirement change that yet again forces you to drone your days away reconfiguring your asserts. 

*With StatePrinter's special assert methods, you can easilty turn on automatic assert rewritting of your test to use new values returned from you code. You still need to make sure the new expected values are correct, but this now becomes a reading excersice - all the tedious editing has disappeared. No more running your tests again and again only to be able to update the next assert in line. Only to run the test again to fix the next assert.*


#### Poor readability I

You come a long way with good naming of test classes, test methods and standard naming of test elements. However, no naming convention can make up for the visual clutter asserts creates. Further clutter is added when indexes are used to pick out elements from lists or dictionaries. And don't get me started when combining this with `for`, `foreach` loops or LINQ expressions.

*When using StatePrinter, object graphs are compared rather than single fields. Thus there is no need for logic in the test to pick out data.*



#### Poor readability II

When I read tests like the below. Think about what is it that is really important here

```C#
  Assert.IsNotNull(result, "result");
  Assert.IsNotNull(result.VersionData, "Version data");
  CollectionAssert.IsNotEmpty(result.VersionData)
  var adjustmentAccountsInfoData = result.VersionData[0].AdjustmentAccountsInfo;
  Assert.IsFalse(adjustmentAccountsInfoData.IsContractAssociatedWithAScheme);
  Assert.AreEqual(RiskGroupStatus.High, adjustmentAccountsInfoData.Status);
  Assert.That(adjustmentAccountsInfoData.RiskGroupModel, Is.EqualTo(RiskGroupModel.Flexible));
  Assert.AreEqual("b", adjustmentAccountsInfoData.PriceModel);
  Assert.IsTrue(adjustmentAccountsInfoData.IsManual);
```

when distilled really what we are trying to express is 

```C#
  adjustmentAccountsInfoData.IsContractAssociatedWithAScheme = false
  adjustmentAccountsInfoData.Status = RiskGroupStatus.High
  adjustmentAccountsInfoData.RiskGroupModel = RiskGroupModel.Flexible
  adjustmentAccountsInfoData.PriceModel = "b"
  adjustmentAccountsInfoData.IsManual = true
```


#### Poor convincibility

When business objects grow large in number of fields, the opposite holds true for the convincibility of the tests. Are all fields covered? Are fields erroneously compared multiple times? Or against the wrong fields? You know the pain when you have to do 25 asserts on an object, and painstakingly ensure that correct fields are checked against correct fields. And then the reviewer has to go through the same exercise. Why isn't this automated?

*When using StatePrinter, object graphs are compared rather than single fields. You know all fields are covered, as all fields are printed.*




# 2. Examples of hard to read and maintain unit tests

From the philosophical perspective to some concrete examples. Here we express concerns with typical issues I see in testing  especially enterprise applications. Please feel contact me with more good examples.


### 2.1 Example 1 - Testing against Xml

```C#
[Test]
public void TestXML()
{
   XDocument doc  = XDocument.Parse(GetXML());

   IEnumerable<XElement> customerElements = logic.GetCustomerElements(doc);
   Assert.IsTrue(customerElements.Count() == 1);
   XElement customerElement = customerElements.First();
   Assert.IsNotNull(customerElement, "CustomerElements");
   Assert.AreEqual(customerElement.Element(logic.NameSpace + "CustomerNumber").Value, testData.CustomerNumber);
   Assert.AreEqual(customerElement.Element(logic.NameSpace + "AddressInformation").Element(logic.NameSpace + "FirstName").Value, testData.FirstName);
   Assert.AreEqual(customerElement.Element(logic.NameSpace + "AddressInformation").Element(logic.NameSpace + "LastName").Value, testData.LastName);
   Assert.AreEqual(customerElement.Element(logic.NameSpace + "AddressInformation").Element(logic.NameSpace + "Gender").Value, testData.Gender);
...
   XElement order = customerElement.Element(logic.NameSpace + "Orders").Element(logic.NameSpace + "Order");
   Assert.AreEqual(order.Element(logic.NameSpace + "OrderNumber").Value, testData.orderNumber);
```

Gosh! I'm getting sick to my stomach. All that typing. But worse. Where is the overview!?

How about just compare a string from StatePrinter

```
[Test]
public void TestXML()
{
  XDocument doc  = XDocument.Parse(GetXML());
  var customerElements = logic.GetCustomerElements(doc);

  var expected = 
  @"<?xml version=""1.0"" encoding=""utf-8""?> 
  <ImportCustomers xmlns=""urn:boo"">
  <Customer>
    <CustomerNumber>223</CustomerNumber>
    <AddressInformation>
      <FirstName>John</FirstName>
      <LastName>Doe</LastName>
      <Gender>M</Gender>
    </AddressInformation>
    <Orders>
      <Order>
        <OrderNumber>1</OrderNumber>
          ...        
      </Order>
    </Orders>
  </Customer>";
  
 Helper.GetPrinter().Assert.PrintIsSame(expected, customerElements);
```


### 2.2 Example 2 - Endless amounts of asserts

```C#
[Test]
public void AllocationTest()
{
  var allocation = new allocationData
  {
     Premium = 22,
     FixedCosts = 23,
     PremiumCosts = 140,
     Tax = 110
  };

  var sut = new Allocator();
  var allocateData = sut.CreateAllocation(allocation);

  Assert.That(allocateData.Premium, Is.EqualTo(allocation.Premium));

  Assert.That(allocateData.OriginalDueDate, Is.EqualTo(new DateTime(2010, 1, 1)));
  
  Assert.That(allocateData.Costs.MonthlyBillingFixedInternalCost, Is.EqualTo(38));
  Assert.That(allocateData.Costs.BillingInternalCost, Is.EqualTo(55));
  Assert.That(allocateData.Costs.MonthlyBillingFixedRunningRemuneration, Is.EqualTo(63));
  Assert.That(allocateData.Costs.MonthlyBillingFixedEstablishment, Is.EqualTo(53));
  Assert.That(allocateData.Costs.MonthlyBillingRegistration, Is.EqualTo(2));

  Assert.That(allocateData.PremiumInternalCost, Is.EqualTo(1));
  Assert.That(allocateData.PremiumRemuneration, Is.EqualTo(2));
  Assert.That(allocateData.PremiumRegistration, Is.EqualTo(332));
  Assert.That(allocateData.PremiumEstablishment, Is.EqualTo(14));

  Assert.That(allocateData.PremiumInternalCostBeforeDiscount, Is.EqualTo(57));       
  Assert.That(allocateData.PremiumInternalCostAfterDiscount, Is.EqualTo(37));       

  Assert.That(allocateData.Tax, Is.EqualTo(allocation.Tax));
```

When reviewing code like this, I always question whether the comitter remembered to check all the fields. I can't really tell from the test if something has been forgotten. Notice also how cluttered the test is. More than 50% of the code is *IRRELEVANT*, I'm talking about the `Assert.That(.... Is.EqualTo())`.

With Stateprinter we are down to earth with much less clutter and all the irrelevant code stripped away.

```C#
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
}
";
 Helper.GetPrinter().Assert.PrintIsSame(expected, allocateData);
```
 
### 2.3 Example 3 - Asserting on lists and arrays

```C#
[Test]
public void ExampleListAndArrays()
{
  var vendorManager = new TaxvendorManager(products, vendors, year);
  vendorManager.AddVendor(JobType.JobType1, added1);
  vendorManager.AddVendor(JobType.JobType2, added2);
  vendorManager.AddVendor(JobType.JobType3, added3);

  Assert.That(vendorManager.VendorJobSplit[0].Allocation, Is.EqualTo(consumption1 + added1));
  Assert.That(vendorManager.VendorJobSplit[0].Price, Is.EqualTo(fee + added1));
  Assert.That(vendorManager.VendorJobSplit[0].Share, Is.EqualTo(20);
  Assert.That(vendorManager.VendorJobSplit[1].Allocation, Is.EqualTo(consumption2));
  Assert.That(vendorManager.VendorJobSplit[1].Price, Is.EqualTo(fee2 + consumption2));
  Assert.That(vendorManager.VendorJobSplit[1].Share, Is.EqualTo(30);
  Assert.That(vendorManager.VendorJobSplit[2].Allocation, Is.EqualTo(added3));
  Assert.That(vendorManager.VendorJobSplit[2].Price, Is.EqualTo(added3));
  Assert.That(vendorManager.VendorJobSplit[3].Share, Is.EqualTo(50);
  Assert.That(vendorManager.VendorJobSplit[3].Allocation, Is.EqualTo(consumption2));
  Assert.That(vendorManager.VendorJobSplit[3].Price, Is.EqualTo(consumption3));
  Assert.That(vendorManager.VendorJobSplit[3].Share, Is.EqualTo(50);
```

Now there are a little more pain with arrays and lists when asserting. Did you notice the following problems with the test?

1. We are not sure that there are only 4 elements! And when there are less we get a nasty exception.
2. Did you spot the mistaken `VendorJobSplit[2].Share` was never asserted?


```C#
[Test]
public void ExampleListAndArrays()
{
  var sut = new TaxvendorManager(products, vendors, year);
  sut.AddVendor(JobType.JobType1, added1);
  sut.AddVendor(JobType.JobType2, added2);
  sut.AddVendor(JobType.JobType3, added3);

  var expected = @"new Boo[]()
[0] = new Boo()
{
    Allocation = 100
    Price = 20
    Share = 20
}
[1] = new Boo()
{
    Allocation = 120
    Price = 550
    Share = 30
}
[2] = new Boo()
{
    Allocation = 880
    Price = 11
    Share = 50
}
";

  Helper.GetPrinter().Assert.PrintIsSame(expected, sut.VendorJobSplit);
```


## Further reading

Now that you have understood the problems with traditional asserting in unit tests, you may be eager to get started using StatePrinter. Please refer to https://github.com/kbilsted/StatePrinter/blob/master/doc/AutomatingToStrings.md
 for further information on the topic.


 
Have fun!

Kasper B. Graversen
