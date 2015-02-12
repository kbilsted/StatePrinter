#  ![](https://raw.github.com/kbilsted/StatePrinter/master/StatePrinter/gfx/stateprinter.png) StatePrinter automating your unit tests



**Table of content**
* [3. Unit testing](#3-unit-testing)  
 * [3.1 Examples of hard to read unit tests](#31-examples-of-hard-to-read-unit-tests)
 * [3.2 Restricting fields harvested](#32-restricting-fields-harvested)
 * [3.3 Stateprinter.Assert](#33-Stateprinter-asserts)
 
# 3. Unit testing

When unit testing, you often have to write a ton of asserts to check the state of a business object. The problem with such an approach are many fold:

* *It is laborious*. When I type and re-type over and over again `Assert.This`, `Assert.That` I always wonder why the computer cannot automate this stuff. All that needles typing takes time and drains my energy.
* *Code and test gets out of sync.*  When the code changes, say adding a field, you need to add asserts in some of your tests. Locating *where* is a manual process. On larger project where no one has the full overview, this is often neglected. Also when merging a bugfix or feature from a release branch to the development branch, people often forget to revisit and double the test suite to figure out if new tests on the development branch needs tweaking.
* *Poor readability*. You come a long way with good naming of test classes, test methods and standard naming of test elements such as the `SUT` abbreviation. Still all those asserts clutter the view of whats important. Especially when you are dealing with object-graphs or lists of partial data.
* *Poor convincibility*. When business objects grow large in number of fields, the opposite holds true for the convincibility of the tests. Are all fields covered? Are fields erroneously compared multiple times? You know the pain when you have to do 25 asserts on an object, and painstakingly ensure that correct fields are checked against correct fields. And then the reviewer has to go through the same exercise. Why isn't this automated?

**When using the StatePrinter these problems are mitigated as you are asserting against a easily read string representation**. You know all fields are covered, as all fields are printed. When the object changes in the future, so will its string representation, and thus your tests fail. **When tests fail, StatePrinter will generate code and suggest you copy-paste it to rectify the situation**.

## 3.1 Examples of hard to read unit tests

The introduction was a bit vague. You may not yet be convinced. Allow me to express concerns with typical issues I see in testing. Please feel contact me with more good examples.


### 3.1.1 Example 1 - Testing against Xml

```
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

Gosh! I'm getting sick to my stomack. All that typing. But worse. Where is the overview!?

How about just compare a string from StatePrinter

```
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
</Customer>"
```


### 3.1.2 Example 2 - Endless amounts of asserts

```
  var allocation = new allocationData
  {
      Premium = 22,
      FixedCosts = 23,
      PremiumCosts = 140,
      Tax = 110
   };

    var sut = Allocator();
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
 
### 3.1.3 Example 3 - Asserting on lists and arrays

```
var vendorManager = new TaxvendorManager(products, vendors, year);

vendorManager.AddVendor(JobType.JobType1, added1);
vendorManager.AddVendor(JobType.JobType2, added2);
vendorManager.AddVendor(JobType.JobType3, added3);

Assert.That(vendorManager.VendorJobSplit[0], Is.EqualTo(consumption1 + added1));
Assert.That(vendorManager.VendorJobSplit[0].Price, Is.EqualTo(fee + added1));
Assert.That(vendorManager.VendorJobSplit[0].Share, Is.EqualTo(20);
Assert.That(vendorManager.VendorJobSplit[1], Is.EqualTo(consumption2));
Assert.That(vendorManager.VendorJobSplit[1].Price, Is.EqualTo(fee2 + consumption2));
Assert.That(vendorManager.VendorJobSplit[1].Share, Is.EqualTo(30);
Assert.That(vendorManager.VendorJobSplit[2], Is.EqualTo(added3));
Assert.That(vendorManager.VendorJobSplit[2].Price, Is.EqualTo(added3.price));
Assert.That(vendorManager.VendorJobSplit[3].Share, Is.EqualTo(50);
Assert.That(vendorManager.VendorJobSplit[3], Is.EqualTo(consumption2));
Assert.That(vendorManager.VendorJobSplit[3].Price, Is.EqualTo(fconsumption3));
Assert.That(vendorManager.VendorJobSplit[3].Share, Is.EqualTo(50);
```

Now there are a little more pain with arrays and lists when asserting. Did you notice the following problems with the test?

1. We are not sure that there are only 4 elements! And when there are less we get a nasty exception.
2. Did you spot the mistaken `VendorJobSplit[2].Share` was never asserted?

True, you can use `CollectionAssert` and the like. But it requires you to implement `Equals()` on all types. And when implementing that, best practice is to also implement `GetHashCode()`. Now you spend more time building needles infra structure, that testing and getting the job done!





## 3.2 Configuration - Restricting fields harvested

Now, there are situations where there are fields in your business objects that are uninteresting for your tests. Thus those fields represent a challenge to your test. 

* They may hold uninteresting values pollute the assert
* They may even change value from execution to execution

We can easily remedy this situation using the FieldHarvester abstraction described above, however, we do not feel inclined to create an implementation of the harvesting interface per class to be tested. The `ProjectiveHarvester` has a wealth of possibilities to transform (project) a type into another. That is, only include certain fields, only exclude certain fields, or create a filter programmatically. 

given

```C#
    class A
    {
      public DateTime X;
      public DateTime Y { get; set; }
      public string Name;
    }
```

You can *in a type safe manner, and using auto-completion of visual studio* include or exclude fields. Notice that the type is provided in the call (`A`) therefore the editor can help suggest which properties or fields to include or exclude. Unlike the normal field-harvester, the `ProjectiveHarvester` uses the FieldsAndProperties fieldharvester so it will by default include more than what you might be used to from using the normal field processor.

```C#
      var cfg = ConfigurationHelper.GetStandardConfiguration(" ");
      cfg.Projectionharvester().Exclude<A>(x => x.X, x => x.Y);
      var printer = new Stateprinter(cfg);

      var state = printer.PrintObject(new A { X = DateTime.Now, Name = "Charly" });
      Assert.AreEqual(@"new A(){ Name = ""Charly""}", state.Replace("\r\n", ""));
```

and

```C#
      var cfg = ConfigurationHelper.GetStandardConfiguration(" ");
      cfg.Projectionharvester().Include<A>(x => x.Name);
      var printer = new Stateprinter(cfg);

      var state = printer.PrintObject(new A { X = DateTime.Now, Name = "Charly" });
      Assert.AreEqual(@"new A(){ Name = ""Charly""}", state.Replace("\r\n", ""));
```

or programmatically

```C#
 var cfg = ConfigurationHelper.GetStandardConfiguration(" ");
      cfg.Projectionharvester()
        .AddFilter<A>(x => x.Where(y => y.SanitizedName != "X" && y.SanitizedName != "Y"));
```

You can now easily configure what to dump when testing. 



## 3.3 Stateprinter.Assert

Advantages of using stateprinter's assert methods

* Hooks into any unit testing framework of your choice
* Code generates your expected values. It is almost fully automatic to write your asserts and update them when the code is changed.
* Handles newline issues by unifying the line ending representation before asserting. This is particularly nice when you are coding and testing on multiple operating systems (such as deploying to the cloud) or when you plugins such as Resharper is incapable of proper line ending handling.

Need more explanation here. For now look at: https://github.com/kbilsted/StatePrinter/blob/master/StatePrinter/TestAssistance/Asserter.cs




Have fun!

Kasper B. Graversen
