# Semi/full automatic unit testing with StatePrinter

This document has the focus of how to use StatePrinter to improve the speed you flesh out your unit tests, make them more readable and even make them automatically re-write them selves. [The problems with traditional unit testing](TheProblemsWithTraditionalUnitTesting.md) explains in finer details why you should use StatePrinter.


This document explains a radically different approach to writing and maintaining asserts in unit tests. Read with an open mind!

> We honor the dead for giving us the world we inherited, however, we must recoqnize we are doomed if we allow the dead to govern us.

# Table of Content
 * [1. Using semi-automatic testing](#1-using-semi-automatic-testing)
   * [1.1 Create the test](#11-create-the-test)
   * [1.2 Run the test](#12-run-the-test)
   * [1.3 Copy-paste the generated asserts](#13-copy-paste-the-generated-asserts)
   * [1.4 Inspect and commit](#14-inspect-and-commit)
     * [Shortcut helpers](#shortcut-helpers)
     * [Conclusions](#conclusions)
 * [2. Using full automatic unit tests](#2-using-full-automatic-unit-tests)
   * [2.1. Instruct StatePrinter to allow auto-rewriting](#21-instruct-stateprinter-to-allow-auto-rewriting)
   * [2.2 Create the test](#22-create-the-test)
   * [2.3 Run the test](#23-run-the-test)
   * [2.4 Inspect and commit](#24-inspect-and-commit)
 * [3. Integrating with your unit test framework](#3-integrating-with-your-unit-test-framework)
 * [4. Configuration - Restricting fields harvested](#4-configuration---restricting-fields-harvested)
   * [4.1 Filtering by use of Types](#41-filtering-by-use-of-types)
 * [5. Stateprinter.Assert](#5-stateprinterassert)
 * [6. Best practices](#6-best-practices)
   * [StatePrinter configuration](#stateprinter-configuration)
   * [Asserting](#asserting)




 
# 1. Using semi-automatic testing

The work flow is as follows

1. Create a test, with an empty expected (or change existing code that has tests)
2. Run the tests
3. Copy-paste test expectations from the stateprinter error message to the test
4. Inspect the changes before pushing code to the repository

 
## 1.1 Create the test

To get started with the automatic asserting in unit testing, you first write your business code and an *empty test* which only exercise the code under test. 

Remember, no asserts are to be written or maintained manually any more. 

The below situation is comparable to havin gunit tests for business code that has changed, since last running the tests (i.e. they will fail upon execution).

For example:

```C#
[Test]
public void MakeOrderTest()
{ 
    var sut = new BusinessCode(a, b, ...);

    var printer = Helper.GetPrinter();
    var actual = printer.PrintObject(sut.ProcessOrder(c, d, ...));
    
    var expected = "";
    printer.Assert.AreEquals(expected, actual);    
}
```

As described in "best practices" further down the page, it is a good idea to have a single source from which all Stateprinter instances for unit testing are created. In the following example, we are creating a printing and bridging to Nunit's Assert method.

```C#
static class Helper
{
    public static Stateprinter GetPrinter()
    { 
        var printer = new Stateprinter();
        printer.Configuration.SetAreEqualsMethod(NUnit.Framework.Assert.AreEqual);
        
        return printer;
    }
}
```

## 1.2 Run the test
Running the test will *fail*. This is quite intentional! Notice the text at the top of the below error message, it says *Proposed output*. This is in fact **proposed C# code, that you can paste directly into your test to make it green**. 

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


## 1.3 Copy-paste the generated asserts

Copy-paste the `expected` definition into your test. I've taken the liberty to replace `AreEquals()` with `IsSame()`. For now, sufice to say that `IsSame()` is a more lenient. 

Your test now looks like:

```C#
[Test]
public void MakeOrderTest()
{ 
    var sut = new BusinessCode(a, b, ...);

    var printer = Helper.GetPrinter();
    var actual = printer.PrintObject(sut.ProcessOrder(c, d, ...));
    
    var expected = @"new Order()
    {
       OrderNo = 1
       OrderName = ""X-mas present""
    }";
    printer.Assert.IsSame(expected, actual);    
}
```

## 1.4 Inspect and commit

*"Now hold on a minute"*, I hear you say! You just generated the output. How does StatePrinter know that the assert is correct? The answer is: It doesn't. It merely outputs the state of the SUT (System Under Test). So StatePrinter does not prevent you from having to think, in fact you should not blindly trust the output. **StatePrinter simply takes away the typing in testing.**


### Shortcut helpers

Now that we understand the basics of the framework, it is time to introduce a shorthand method for printing and asserting in one go. This keeps typing at a minimu. We can re-write the above test simply as:


```C#
[Test]
public void MakeOrderTest()
{ 
    var sut = new BusinessCode(a, b, ...);
    
    var expected = "";
    Helper.GetPrinter().Assert.PrintAreAlike(expected, sut.ProcessOrder(c, d, ...));    
}
```

### Conclusions

* You get the assert creation for free
* When the order-object is extended in the future, your the unit test failure message will tell you how to go green again. 
 
If you don't like the output format of the `expected` variable, read  [configuration](https://github.com/kbilsted/StatePrinter/blob/master/doc/HowToConfigure.md) for heaps of ways to tweaking.





# 2. Using full automatic unit tests

The work flow is as follows

1. Instruct StatePrinter to allow auto-rewriting
2. Create a test, with an empty expected (or change existing code that has tests)
3. Run the test
4. Inspect the changes before pushing to the repository

This is a much simpler work flow than the semi-automatic approach since it does not involve any copy and pasting. You simply run your tests and they conform to the code. This is because StatePrinter will search/replace directly in your source code.

Naturally, this is not suitable for all development situations. But very often  I find myself in the situation, where a simple code changes require significant time to fix-up several existing tests. Often times, these tests are integration test, acceptance tests. 



## 2.1. Instruct StatePrinter to allow auto-rewriting

Simply extend the above `Helper.CreatePrinter()` with 

```C#
printer.Configuration.SetAutomaticTestRewrite( (fileName) => true );
```

meaning that, for any unit test file executed, allow automatic re-writing of expected values.

 
## 2.2 Create the test

Create a new test or think of this as one of your existing tests where the business code has changes since last running this test.

```C#
[Test]
public void MakeOrderTest()
{ 
    var sut = new BusinessCode(a, b, ...);

    var printer = Helper.GetPrinter();
    var actual = printer.PrintObject(sut.Foo(c, d, ...));
    
    var expected = "";
    printer.Assert.AreEquals(expected, actual);    
}
```

## 2.3 Run the test

From within visual studio or using an external gui.

The tests will go red, but the error message will notify you that the test has been re-written and thus upon re-execution will go green.


## 2.4 Inspect and commit

*"Now hold on a minute"*, I hear you say! You just generated the output. How does StatePrinter know that the assert is correct? The answer is, that it doesn't. It merely outputs the state of the SUT (System Under Test). So StatePrinter does not prevent you from having to think, in fact you should not blindly trust the output. StatePrinter simply takes away the typing in testing.





# 3. Integrating with your unit test framework

Stateprinter is not dependent on any unit testing framework, yetit will integrate with most if not all frameworks on the market. This is possible through explicit configuration where you tell how StatePrinter must call your testing frameworks' assert method. For Nunit the below suffices:

```C#
var printer = new Stateprinter();
printer.Configuration.SetAreEqualsMethod( Nunit.Framework.Assert.AreEquals );
```



# 4. Configuration - Restricting fields harvested

Now, there are situations where there are fields in your business objects that are uninteresting for your tests. Thus those fields represent a challenge to your test. 

* They may hold uninteresting values pollute the assert, e.g. a `Guid`
* They may change value from execution to execution, e.g. `DateTime.Now`

We can easily remedy this situation using the FieldHarvester abstraction described in the "configuration document", however, we do not feel inclined to create an implementation of the harvesting interface per class to be tested. The `ProjectiveHarvester` has a wealth of possibilities to transform (project) a type into another. That is, only include certain fields, only exclude certain fields, or create a filter programmatically. 

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
Asserter assert = TestHelper.CreateShortAsserter();
assert.Project.Exclude<A>(x => x.X, x => x.Y);

var state = new A { X = DateTime.Now, Name = "Charly" };
assert.PrintEquals("new A(){ Name = ""Charly""}", state);
```

alternatively, we can just mention the fields we'd like to see:

```C#
Asserter assert = TestHelper.CreateShortAsserter();
assert.Project.Include<A>(x => x.Name);

var state = new A { X = DateTime.Now, Name = "Charly" };
assert.PrintEquals("new A(){ Name = ""Charly""}", state);
```

or programmatically

```C#
Asserter assert = TestHelper.CreateShortAsserter();
assert.Project.AddFilter<A>(x => x.Where(y => y.SanitizedName != "X" && y.SanitizedName != "Y"));
```

You can now easily configure what to dump when testing. 

Notice though, that when you use the `Include` or `AddFilter` functionality, you are exluding yourself from failing tests when your business data is extended. So use it with care.



## 4.1 Filtering by use of Types

As of v2.1 you can specify filters by using other types. Say you have a class implementing multiple interfaces, you can specify to only include fields from specific interface(s).


```C#
[Test]
public void TestIncludeByType()
{
     var sut = new AtoD();
     Asserter assert;

     assert = TestHelper.CreateShortAsserter();
     assert.PrintEquals("new AtoD() { A = 1 B = 2 C = 3 D = 4 }", sut);

     assert = TestHelper.CreateShortAsserter();
     assert.Project.IncludeByType<AtoD, IA>();
     assert.PrintEquals("new AtoD() { A = 1 }", sut);

     assert = TestHelper.CreateShortAsserter();
     assert.Project.IncludeByType<AtoD, IA, IB>();
     assert.PrintEquals("new AtoD() { A = 1 B = 2 }", sut);

     assert = TestHelper.CreateShortAsserter();
     assert.Project.IncludeByType<AtoD, IA, IB, IC>();
     assert.PrintEquals("new AtoD() { A = 1 B = 2 C = 3 }", sut);

     assert = TestHelper.CreateShortAsserter();
     assert.Project.IncludeByType<AtoD, IA, IB, IC, ID>();
     assert.PrintEquals("new AtoD() { A = 1 B = 2 C = 3 D = 4 }", sut);
 }

 [Test]
 public void TestExcludeByType()
 {
     var sut = new AtoD();
     Asserter assert;

     assert = TestHelper.CreateShortAsserter();
     assert.PrintEquals("new AtoD() { A = 1 B = 2 C = 3 D = 4 }", sut);

     assert = TestHelper.CreateShortAsserter();
     assert.Project.ExcludeByType<AtoD, IA>();
     assert.PrintEquals("new AtoD() { B = 2 C = 3 D = 4 }", sut);

     assert = TestHelper.CreateShortAsserter();
     assert.Project.ExcludeByType<AtoD, IA, IB>();
     assert.PrintEquals("new AtoD() { C = 3 D = 4 }", sut);

     assert = TestHelper.CreateShortAsserter();
     assert.Project.ExcludeByType<AtoD, IA, IB, IC>();
     assert.PrintEquals("new AtoD() { D = 4 }", sut);

     assert = TestHelper.CreateShortAsserter();
     assert.Project.ExcludeByType<AtoD, IA, IB, IC, ID>();
     assert.PrintEquals("new AtoD() { }", sut);
 }
```


# 5. Stateprinter.Assert

As of v2.0, StatePrinter ships with assert methods accessible from `printer.Assert`. These assert methods are preferable to the ordinary assert methods of your unit testing framework:

* They wrap the current unit testing framework of your choice 
* They code generate your expected values. It is almost fully automatic to write your asserts and update them when the code changes.
* Some of them are lenient to newline issues by unifying the line ending representation before asserting. This is particularly nice when you are coding and testing on multiple operating systems (such as deploying to the cloud) or when you plugins such as Resharper is incapable of proper line ending handling when copy/pasting.

Need more explanation here. For now look at: https://github.com/kbilsted/StatePrinter/blob/master/StatePrinter/TestAssistance/Asserter.cs




# 6. Best practices

## StatePrinter configuration

The bast practices when using StatePrinter for unit testing is different from using it to do ToString-implementations. The caching of field harvesting of types is not compatible with using the `Include<>`, `Exclude<>` and filter properties of the `ProjectionHarvester`. Thus to ensure correctness of our tests and to ensure that our tests are independent of each other, the best strategy is to use a fresh instance of StatePrinter in each test.

There are many ways of achieving this, but I found the best way to use a `TestHelper` class that provides general configuration shared among all tests, and then for specific tests, tune this configuration. Eg. with filters.

Things to consider in the general case

* The use of a specific culture to ensure that your tests work across platforms. For example, AppVeyor has a completely different setup than my local machine.
* Setup type converters as you want them. E.g. do you want strings in the output to be enclosd in `""` or maybe you have attributes on your enum-values that you want to see in the output.
* Hook up your unit testing framwork 

You come a long way with the following code

```C#
static class Create
{
    public static Asserter Asserter()
    {
        var cfg = ConfigurationHelper.GetStandardConfiguration()
            .SetAreEqualsMethod(NUnit.Framework.Assert.AreEqual)
            .SetCulture(CultureInfo.CreateSpecificCulture("da-DK"))
            .SetAutomaticTestRewrite((fileName) => true);

            return new Stateprinter(cfg).Assert;
    }
}
```

Then you can use, and further fine-tune in your test like

```C#
[Test]
public void Foo()
{
    var assert = Create.Asserter();
    assert.Configuration  ... // fine tuning

    assert.PrintAreEquals(...);
```



## Asserting

When not using automatic rewrite, I prefer the `AreAlike()` over the `AreEquals()`. I've come to really appreciate the `AreAlike()` method since it ignores differences in line ending. Line endings differ from operating system to operating system, and some tools such as Resharper seems to have problems when copying from its output window into tests. Here the line endings are truncated to `\n`. 

With automatic rewrite, there are no issues with copy-pasting, and thus it is more right to use the AreEqual variant which does not modify the input before comparison.



Have fun!

Kasper B. Graversen
