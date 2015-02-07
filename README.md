![](https://raw.github.com/kbilsted/StatePrinter/master/StatePrinter/gfx/stateprinter.png)


# StatePrinter 

"Automatic `Asser.AreEqual()` and automatic `ToString()`."
Like a JSON serializer on drugs

* Latest version: 1.0.6 (Get it [here](https://www.nuget.org/packages/StatePrinter/))
* Requires C# 3.5 or newer
* Test coverage of 97%
* Build server status: [![Build status](https://ci.appveyor.com/api/projects/status/vx0nl4y4iins506u/branch/master?svg=true)](https://ci.appveyor.com/project/kbilsted/stateprinter/branch/master)
* Stats: <script type="text/javascript" src="http://www.openhub.net/p/715894/widgets/project_thin_badge.js"></script>


This file describes the latest pushed changes. For documentation of releases see: xxx

Table of content
* [1. Introduction](#1-introduction)
 * [1.1 Simple example usage](#11-simple-example-usage)
 * [1.2 Generic ToString() usage](#12-generic-tostring-usage)
* [2. Configuration](#2-configuration)
 * [2.1 Stacked configuration principle](#21-stacked-configuration-principle)
 * [2.2 Simple changes](#22-simple-changes)
 * [2.3 Culture specific printing](#23-culture-specific-printing)
 * [2.4 Output as a single line](#24-output-as-a-single-line)
 * [2.5 Field harvesting](#25-field-harvesting)
 * [2.6 Simple value printing](#26-simple-value-printing)
 * [2.7 Output formatting](#27-output-formatting)
* [3. Unit testing](#3-unit-testing)  
 * [3.1 Restricting fields harvested](#31-restricting-fields-harvested)
*  [4. License](#4-license)


# 1. Introduction

The StatePrinter is a free, highly configurable, thread safe utility that can turn any object-graph to a string representation. 

Why you should take StatePrinter for a spin

* *No more manual .ToString()* - it is much easier to write robus and self-sufficient `ToString()` methods. 
* It becomes much easier to write unit tests. No more screens full of asserts. Especially testing against object-graphs is a bliss. 
* It is part of the back-end engine of the very nice ApprovalTests framework (http://approvaltests.sourceforge.net/).
* Very very configurable both in terms of what to harvest, and in terms of how to output.

Version History: http://github.com/kbilsted/StatePrinter/blob/master/CHANGELOG.md


### 1.1 Simple example usage

To dump an object graph all you need to do is first to create an object graph

```C#
var car = new Car(new SteeringWheel(new FoamGrip("Plastic")));
car.Brand = "Toyota";
```

then print it

```C#
Stateprinter printer = new Stateprinter();
Console.WriteLine(printer.PrintObject(car));
```

and you get the following output
	
	new Car()
	{
	    StereoAmplifiers = null
	    steeringWheel = new SteeringWheel()
	    {
	        Size = 3
	        Grip = new FoamGrip()
	        {
	            Material = ""Plastic""
	        }
	        Weight = 525
	    }
	    Brand = ""Toyota""
	}


Naturally, circular references are supported

```C#
var course = new Course();
course.Members.Add(new Student("Stan", course));
course.Members.Add(new Student("Richy", course));

Console.WriteLine(printer.PrintObject(course));
```

yields	 
	     
	new Course(), ref: 0
	{
	    Members = new List<Student>()
	    Members[0] = new Student()
	    {
	        name = ""Stan""
	        course =  -> 0
	    }
	    Members[1] = new Student()
	    {
	        name = ""Richy""
	        course =  -> 0
	    }
	}

notice the `-> 0` this is a pointer back to an already printed object. Notice that references are only added to the output if needed. This amongst alot of other details are configurable.


### 1.2 Generic ToString() usage

If you are anything like me, there is nothing worse than having to edit all sorts of bizare methods on a class whenever you add a field to a class. For that reason I always find myself reluctant to maintaining the `ToString()` method. With the stateprinter this situation has changed, since I can use the same standard implementation for all my classes. I can even add it as part of my code-template in my editor.


```C#
class AClassWithToString
{
  string B = "hello";
  int[] C = {5,4,3,2,1};

  // Nice stuff ahead!
  static readonly Stateprinter printer = new Stateprinter();
  public override string ToString()
  {
    return printer.PrintObject(this);
  }
}
```

And with the code

```C#
Console.WriteLine( new AClassWithToString() );
```

we get

	new AClassWithToString()
	{
	    B = ""hello""
	    C = new Int32[]()
	    C[0] = 5
	    C[1] = 4
	    C[2] = 3
	    C[3] = 2
	    C[4] = 1
	}



# 2. Configuration

Most of the inner workings of the StatePrinter is configurable. The configuration can be broken down to three parts each of which represents a sub-process of the state printer. Since the configuration is made through code, we'll just as well explain the interfaces.

* `IFieldHarvester` deals with how/which fields are harvested from types. E.g. only public fields are printed.
* `IValueConverter` handles Which types are converted into "simple values". Eg. the decimal type contains a lot of internal stuff, and usually we only want to get the numeric value printed. Or maybe you annotate enum values preser those values printed.
* `IOutputFormatter` deals with turning tokens (internal representation of the object state) into a string form. 
 
Finally, culture specific printing of dates and numbers are supported.


## 2.1 Stacked configuration principle

The Stateprinter has a configuration object that for the most cases be initialized with default behaviour. Don't worry about what they are, since you can easily re-configure the before use. This is due to the FILO principle. The StatePrinter retrieved configuration items in the reverse order they are added and stops when the first match has been found. The defaults are thus a cushion, a nice set of fall-back values.

```C#
var printer = new Stateprinter();
```

is equivalent to

```
var cfg = ConfigurationHelper.GetStandardConfiguration();
var printer = new Stateprinter(cfg);
```

which really means

```C#
public static Configuration GetStandardConfiguration()
{
  var cfg = new Configuration();
  cfg.IndentIncrement = " ";

  // valueconverters
  cfg.Add(new StandardTypesConverter());
  cfg.Add(new StringConverter());
  cfg.Add(new DateTimeConverter());
  cfg.Add(new EnumConverter());
      
  // harvesters
  cfg.Add(new AllFieldsHarvester());

  // outputformatters
  cfg.OutputFormatter = new CurlyBraceStyle(cfg.IndentIncrement);
      
  return cfg;
}
```

Once the StatePrinter has been initialized you should not change the configuration. A shallow clone of the configuration is made in the constructor to prevent you from shooting youself in the foot.

Like wise, when implementing harvesters, outputformatters, etc. Do not worry about the stack of the configuration. Simply, through the interface you implement, return only the types you support. In case of an unsupported type, an automatic fall through mechanism will activate the next entity on the stack.


## 2.2 Simple changes

The `Configuration` class should be rather self-documenting. We can change the public fields and properties like setting the indentation characters.

```C#
var cfg = ConfigurationHelper.GetStandardConfiguration();
cfg.IndentIncrement = " ";
cfg.OutputAsSingleLine = true;

var printer = new Stateprinter(cfg);
```


## 2.3 Culture specific printing

The default culture is `CultureInfo.CurrentCulture`. You can change the `Culture` field in the configuration to suit your needs. 

      const decimal decimalNumber = 12345.343M;
      var dateTime = new DateTime(2010, 2, 28, 22, 10, 59);

First the us culture

      var cfg = ConfigurationHelper.GetStandardConfiguration();
      cfg.Culture = new CultureInfo("en-US");
      var printer = new Stateprinter(cfg);

      Assert.AreEqual("12345.343\r\n", printer.PrintObject(decimalNumber));
      Assert.AreEqual("2/28/2010 10:10:59 PM\r\n", printer.PrintObject(dateTime));

The same input with a different culture

      var cfg = ConfigurationHelper.GetStandardConfiguration();
      cfg.Culture = new CultureInfo("da-DK");
      var printer = new Stateprinter(cfg);
      Assert.AreEqual("12345,343\r\n", printer.PrintObject(decimalNumber));
      Assert.AreEqual("28-02-2010 22:10:59\r\n", printer.PrintObject(dateTime));



## 2.4 Output as a single line

When printing very small objects, it is some times preferable to print the state as a sinle line. Set the `Configuration.OutputAsSingleLine = true` to achieve this.



## 2.5 Field harvesting

The StatePrinter comes with two pre-defined harvesters: The `AllFieldsHarvester` and `PublicFieldsHarvester`. By default we harvest all fields, but you can use whatever implementation you want.

```C#
var cfg = ConfigurationHelper.GetStandardConfiguration();
cfg.Add(new PublicFieldsHarvester());

var printer = new Stateprinter(cfg);
```


Field harvesting is simpler than you'd expect. While you may never need to write one yourself, let's walk through the PublicFieldsHarvester for the fun of it. The harvester basically works by harvesting all fields and filtering away those it does not want. We want all public fields, and all private fields if they are the backing fields of public fields.

```C#
public class AllFieldsHarvester : IFieldHarvester
{
  public bool CanHandleType(Type type)
  {
    return true;
  }

  public IEnumerable<FieldInfo> GetFields(Type type)
  {
    var fields = new HarvestHelper().GetFields(type);
    return fields.Where(x => x.IsPublic || x.Name.EndsWith(HarvestHelper.BackingFieldSuffix));
  }
}
```

Notice that in `CanHandleType` we are free to setup any restriction. For example, it should apply only to classes in your department. Let's re-implement it.

```C#
public bool CanHandleType(Type type)
{
  return type.ToString().StartsWith("com.megacorp.");
}
```





## 2.6 Simple value printing

After we have harvested the fields of the object graph, we may desire to turn a complex object into a simple value. That is one that doesn't hold any nested structure. You'd be surprised of the amount of "garbage" values we would print if we revealed the whole state of the string or decimal instances. If you have any interest in such fields, feel free to supply your own implementation.

Let's re-write how we print strings. We want them printed using the `'` delimiter rather than the `"`

First we implement a `IValueConverter`

```C#
public class StringToPlingConverter : IValueConverter
{
  public bool CanHandleType(Type t)
  {
    return t == typeof (string);
  }

  public string Convert(object source)
  {
    return string.Format("'{0}'", source);
  }
}
```

then we add it to the configuration before usage

```C#
var cfg = ConfigurationHelper.GetStandardConfiguration();
cfg.Add(new StringToPlingConverter());

var printer = new Stateprinter(cfg);
```

Due to the FILO principle (First In Last Out) our valueconverter is consulted before the standard implementation.


## 2.7 Output formatting

the `IOutputFormatter` only contains a single method

```C#
string Print(List<Token> tokens);
```

It turns tokens into a "format". Much like traditional compiler design, a token represents a processed entity. In the StatePrinter they look like

```C#
public class Token : IEquatable<Token>
{
  public readonly TokenType Tokenkind;
  public readonly Type FieldType;
  public readonly string FieldName;
  public readonly string Value;
  public readonly Reference ReferenceNo;
}
```

So at this point in the process we need not worry about recursion, field traversal or the like. We focus on the formatting, turning the tokens into a XML-like, JSON-like, LISP-like S-Expressions or whatever you wish. In the current implementation we make two passes on the input to track which objects are referred to by later object. Those we wish to augment with a reference.

The following three outputters are implemented:




### Curly style

The curly style is reminiscent for C# code


      var cfg = ConfigurationHelper.GetStandardConfiguration();
      cfg.OutputFormatter = new CurlyBraceStyle(cfg.IndentIncrement);
      var printer = new Stateprinter(cfg);
      
      var car = new Car(new SteeringWheel(new FoamGrip("Plastic")));
      
      printer.PrintObject(car);

Yields the output

	new Course(), ref: 0
	{
	    Members = new List<Student>()
	    Members[0] = new Student()
	    {
	        name = "Stan"
	        course =  -> 0
	    }
	    Members[1] = new Student()
	    {
	        name = "Richy"
	        course =  -> 0
	    }
	}



### JSon style

The JSon style follows the JSon format and describe cyclic references as paths from the root

      var cfg = ConfigurationHelper.GetStandardConfiguration();
      cfg.OutputFormatter = new JsonStyle(cfg.IndentIncrement);
      var printer = new Stateprinter(cfg);
      
      var car = new Car(new SteeringWheel(new FoamGrip("Plastic")));
      
      printer.PrintObject(car);

Yields the output

	{
	    "Members" :
	    [
	        {
	            "name" : "Stan",
	            "course" :  root
	        }
	        {
	            "name" : "Richy",
	            "course" :  root
	        }
	    ]
	}



### XML style

The Xml style is the most verbose


      var cfg = ConfigurationHelper.GetStandardConfiguration();
      cfg.OutputFormatter = new XmlStyle(cfg.IndentIncrement);
      var printer = new Stateprinter(cfg);
      
      var car = new Car(new SteeringWheel(new FoamGrip("Plastic")));
      
      printer.PrintObject(car);

Yields the output


	<ROOT type='Car'>
	    <StereoAmplifiers>null</StereoAmplifiers>
	    <steeringWheel type='SteeringWheel'>
	        <Size>3</Size>
	        <Grip type='FoamGrip'>
	            <Material>"Plastic"</Material>
	        </Grip>
	        <Weight>525</Weight>
	    </steeringWheel>
	    <Brand>"Toyota"</Brand>
	</ROOT>




# 3. Unit testing

When unit testing, you often have to write a ton of asserts to check the state of a business object. The problem with such an approach are manyfold

* It is quite laborious
* Readability is average but lacks a terse feeling, especially when you are dealing with sub-objects
* Along the same line, it is difficult to see that all fields are in fact covered by the asserts
* When the business object is extended in the future, it is a manual task to identify and add asserts 

When using the StatePrinter these problems are mitigated as you are asserting against a easily read string representation. You know all fields are covered, as all fields are printed. When the object changes in the future, so will its string representation, and thus your tests fail.


## 3.1 Restricting fields harvested

Now, there are situations where there are fields in your business objects that are uninteresting for your tests. Thus those fields represent a challenge to your test. 

* They may hold uninteresting values polute the assert
* They may even change value from execution to execution

We can easilty remedy this situation using the FieldHarvester abstraction described above, however, we do not feel inclined to create an implementation of the harvesting interface pr. class to be tested. The `ProjectiveHarvester` has a wealth of possibilities to transform (project) a type into another. That is, only include certain fields, only exclude certain fields, or create a filter programatically. 

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



# 4. License

StatePrinter is under the Apache License 2.0, meaning that you can freely use this in other open source or commercial products. If you use it for commercial products please have the courtesy to leave me an email with a 'thank you'. 



Have fun!

Kasper B. Graversen
