#  ![](https://raw.github.com/kbilsted/StatePrinter/master/StatePrinter/gfx/stateprinter.png) StatePrinter automating your unit tests



**Table of content**
* [1. Unit testing](#3-unit-tests)
 * [1.1 Getting started](#31-getting-started)
 * [1.2 Integrating with your unit test framework](#34-integrating-with-your-unit-test-framework)
 * [1.3 Configuration - Restricting fields harvested](#35-configuration---restricting-fields-harvested)
 * [1.4 Stateprinter.Assert](#36-stateprinterassert)

 
 
# 1. Unit tests

When writing unit tests for business code, I find myself often having to write a ton of asserts that check the state of numerous fields. This results in a number of maintenance and readability problems. The kind of problems one run into every day with traditional unit testing is elaborated in https://github.com/kbilsted/StatePrinter/blob/master/doc/TheProblemsWithTraditionalUnitTesting.md 

This document has the focus of how to use StatePrinter to improve the speed you flesh out your unit tests, make them more readable and even make them automatically re-write them selves.

 
## 1.1 Getting started

To get started with the automatic asserting when unit testing, you first write your business code and an *empty test* similar to

```C#
[Test]
public void GetDocumentWhenAllDataIsAvailable()
{ 
    var sut = new BusinessCode(a, b, ...);

    var printer = Helper.GetPrinter();
    var actual = printer.PrintObject(sut.Foo(c, d, ...));
    
    var expected = "";
    printer.Assert.IsSame(expected, actual);    
}
```

A standard printer for unit testing can be retrieved with a general helper method

```
static class Helper
{
    public static StatePrinter CreatePrinter()
    { 
        var printer = new Stateprinter();
        printer.Configuration.AreEqualsMethod = Assert.AreEquals;
        
        return printer;
    }
}
```

Running the test will *FAIL*. This is quite intentional! The error message will contain C# code you can paste directly into the code:

```C#
Proposed output for unit test:
var expected = @"new Order()
{
    OrderNo = 1
    OrderName = ""X-mas present""
}
";

  Expected string length 0 but was 127. Strings differ at index 0.
  Expected: <string.Empty>
  But was:  "new Order()\r\n{\r\n    OrderNo = 1\r\n    ..."
  -----------^
```

Copy-paste the `expected` definition and your test becomes:

```C#
[Test]
public void GetDocumentWhenAllDataIsAvailable()
{ 
    var sut = new BusinessCode(a, b, ...);

    var printer = Helper.GetPrinter();
    var actual = printer.PrintObject(sut.Foo(c, d, ...));
    
    var expected = @"new Order()
    {
       OrderNo = 1
       OrderName = ""X-mas present""
    }
    printer.Assert.IsSame(expected, actual);    
}
```


Now that we understand the basics of the framework, it is time to introduce a shorthand method for printing and asserting. Thus we re-write the above simply as:


```C#
[Test]
public void GetDocumentWhenAllDataIsAvailable()
{ 
    var sut = new BusinessCode(a, b, ...);
    
    var expected = @"new Order()
    {
       OrderNo = 1
       OrderName = ""X-mas present""
    }
    Helper.GetPrinter().Assert.PrintIsSame(expected, sut.Foo(c, d, ...));    
}
```


Not only did you get the assert creation for free, when the order-object gets extended in the future you will get those updates for free as well. If you dont like the format of the `expected` variable, check (configuration)[https://github.com/kbilsted/StatePrinter/blob/master/doc/HowToConfigure.md] for heaps of ways to tweak the output.





## 1.2 Integrating with your unit test framework

Stateprinter is not dependent on any unit testing framework, but it will integrate with most if not all. Since unit testing frameworks do not share a common interface that StatePrinter can use, you have to configure StatePrinter to call your testing frameworks' assert method. For Nunit the below suffices:

```C#
var printer = new StatePrinter();
printer.Configuration.AreEqualsMethod = Assert.AreEquals;
```

or 

```C#
var cfg = new Configuration().SetAreEqualsMethod(Assert.AreEquals);
var printer = new StatePrinter(cfg);
```


## 1.3 Configuration - Restricting fields harvested

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

Notice though, that when you use the `Include` or `AddFilter` functionality, you are exlcuding yourself from failing tests when your business data is extended. So use it with care.

## 1.4 Stateprinter.Assert

From v2.0, StatePrinter ships with assert methods accessible from `printer.Assert`. These assert methods are preferable to the ordinary assert methods of your unit testing framework:

* They wrap the current unit testing framework of your choice 
* They code generate your expected values. It is almost fully automatic to write your asserts and update them when the code changes.
* Some of them are lenient to newline issues by unifying the line ending representation before asserting. This is particularly nice when you are coding and testing on multiple operating systems (such as deploying to the cloud) or when you plugins such as Resharper is incapable of proper line ending handling when copy/pasting.

Need more explanation here. For now look at: https://github.com/kbilsted/StatePrinter/blob/master/StatePrinter/TestAssistance/Asserter.cs




Have fun!

Kasper B. Graversen
