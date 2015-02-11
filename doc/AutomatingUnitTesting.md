#  ![](https://raw.github.com/kbilsted/StatePrinter/master/StatePrinter/gfx/stateprinter.png) StatePrinter automating your unit tests



**Table of content**
* [3. Unit testing](#3-unit-testing)  
 * [3.1 Restricting fields harvested](#31-restricting-fields-harvested)

 
# 3. Unit testing

When unit testing, you often have to write a ton of asserts to check the state of a business object. The problem with such an approach are many fold:

* *It is laborious*. When I type and re-type over and over again `Assert.This`, `Assert.That` I always wonder why the computer cannot automate this stuff. All that needles typing takes time and drains my energy.
* *Code and test gets out of sync.*  When the code changes, say adding a field, you need to add asserts in some of your tests. Locating *where* is a manual process. On larger project where no one has the full overview, this is often neglected. Also when merging a bugfix or feature from a release branch to the development branch, people often forget to revisit and double the test suite to figure out if new tests on the development branch needs tweaking.
* *Poor readability*. You come a long way with good naming of test classes, test methods and standard naming of test elements such as the `SUT` abbreviation. Still all those asserts clutter the view of whats important. Especially when you are dealing with object-graphs or lists of partial data.
* *Poor convincibility*. When business objects grow large in number of fields, the opposite holds true for the convincibility of the tests. Are all fields covered? Are fields erroneously compared multiple times? You know the pain when you have to do 25 asserts on an object, and painstakingly ensure that correct fields are checked against correct fields. And then the reviewer has to go through the same exercise. Why isn't this automated?

**When using the StatePrinter these problems are mitigated as you are asserting against a easily read string representation**. You know all fields are covered, as all fields are printed. When the object changes in the future, so will its string representation, and thus your tests fail. **When tests fail, StatePrinter will generate code and suggest you copy-paste it to rectify the situation**.

## 3.1 Examples of hard to read unit tests

The introduction was a bit vague. You may not yet be convinced. Allow me to express concerns with typical issues I see in testing. Please feel contact me with more good examples.




## 3.1 Configuration - Restricting fields harvested

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




Have fun!

Kasper B. Graversen
