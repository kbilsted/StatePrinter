![](https://raw.github.com/kbilsted/StatePrinter/master/StatePrinter/gfx/stateprinter.png)


StatePrinter - "The automatic ".ToString()" utility."

Version 1.0.4 - get it here (https://www.nuget.org/packages/StatePrinter/)

Requires C# 3.5 or newer


# 1. Introduction

The StatePrinter is a free, highly configurable, thread safe utility that can turn any object-graph to a string representation. 

* *No more manual .ToString()* - it is much easier to write robus and self-sufficient `ToString()` methods. 
* It becomes much easier to write unit tests. No more screenfuls of asserts. Especially testing against object-graphs is a bliss. 
* It is part of the back-end engine of the very nice ApprovalTests framework (http://approvaltests.sourceforge.net/).


Build server status: <img src="https://ci.appveyor.com/api/projects/status/github/StatePrinter?svg=true&branch=master" />

### 1.1 Simple usage


To dump an object graph all you need to do is first to create an object graph

```C#
var car = new Car(new SteeringWheel(new FoamGrip("Plastic")));
car.Brand = "Toyota";
```

then print it

```C#
StatePrinter printer = new StatePrinter();
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
  static readonly StatePrinter printer = new StatePrinter();
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


## 2.1 FILO configuration - First In, Last Out

The stateprinter has a configuration object that for the most cases be initialized with default behaviour. Don't worry about what they are, since you can easily re-configure the before use. This is due to the FILO principle. The StatePrinter retrieved configuration items in the reverse order they are added and stops when the first match has been found. The defaults are thus a cusion, a nice set of fall-back values.

```C#
var printer = new StatePrinter();
```

is equivalent to

```
var cfg = ConfigurationHelper.GetStandardConfiguration();
var printer = new StatePrinter(cfg);
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


## 2.2 Simple changes

The `Configuration` class should be rather self-documenting. We can change the public fields and properties like setting the indentation characters.

```C#
var cfg = ConfigurationHelper.GetStandardConfiguration();
cfg.IndentIncrement = " ";
   
var printer = new StatePrinter(cfg);
```


## 2.3 Culture specific printing

The default culture is `CultureInfo.CurrentCulture`. You can change the `Culture` field in the configuration to suit your needs. 

      const decimal decimalNumber = 12345.343M;
      var dateTime = new DateTime(2010, 2, 28, 22, 10, 59);

First the us culture

      var cfg = ConfigurationHelper.GetStandardConfiguration();
      cfg.Culture = new CultureInfo("en-US");
      var printer = new StatePrinter(cfg);

      Assert.AreEqual("12345.343\r\n", printer.PrintObject(decimalNumber));
      Assert.AreEqual("2/28/2010 10:10:59 PM\r\n", printer.PrintObject(dateTime));

The same input with a different culture

      var cfg = ConfigurationHelper.GetStandardConfiguration();
      cfg.Culture = new CultureInfo("da-DK");
      var printer = new StatePrinter(cfg);
      Assert.AreEqual("12345,343\r\n", printer.PrintObject(decimalNumber));
      Assert.AreEqual("28-02-2010 22:10:59\r\n", printer.PrintObject(dateTime));






## 2.4 Field harvesting

The StatePrinter comes with two pre-defined harvesters: The `AllFieldsHarvester` and `PublicFieldsHarvester`. By default we harvest all fields, but you can use whatever implementation you want.

```C#
var cfg = ConfigurationHelper.GetStandardConfiguration();
cfg.Add(new PublicFieldsHarvester());

var printer = new StatePrinter(cfg);
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





## 2.5 Simple value printing

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

var printer = new StatePrinter(cfg);
```

Due to the FILO principle (First In Last Out) our valueconverter is consulted before the standard implementation.


## 2.6 Output formatting

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
      var printer = new StatePrinter(cfg);
      
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
      var printer = new StatePrinter(cfg);
      
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
      var printer = new StatePrinter(cfg);
      
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


## 3.1 Restricting fields when dumping

Now, there are situations where there are fields in your business objects that are uninteresting for your tests. Thus those fields represent a challenge to your test. 

* They may hold uninteresting values polute the assert
* They may even change value from execution to execution

We can easilty remedy this situation using the FieldHarvester abstraction described above, however, we do not feel inclined to create an implementation of the harvesting interface pr. class to be tested. Instead, a generic implementation can be made that operates on `Func`'s and thus is more light-weigt. 


```C#
 public class SelectiveHarvester : IFieldHarvester
 {
    Implementation selected = null;
    readonly List<Implementation> implementations = new List<Implementation>(); 

    public bool CanHandleType(Type type)
    {
      selected = implementations.FirstOrDefault(x => x.Selector.IsAssignableFrom(type));
      return selected != null;
    }

    public List<FieldInfo> GetFields(Type type)
    {
      var fields = new HarvestHelper().GetFields(type);
      return selected.Filter(fields).ToList();
    }

    public void Add(Type selector, Func<List<FieldInfo>, IEnumerable<FieldInfo>> filter)
    {
      implementations.Add(new Implementation(selector, filter));
    }


    class Implementation
    {
      public readonly Type Selector;
      public readonly Func<List<FieldInfo>, IEnumerable<FieldInfo>> Filter;

      public Implementation(Type selector, Func<List<FieldInfo>, IEnumerable<FieldInfo>> filter)
      {
        Selector = selector;
        Filter = filter;
      }
    }
  }
}
```

in fact this implementation will be included in the next release.

You can now easily configure what to dump when testing

### 3.1.1 Example

First we define 3 business classes. Where `X` in the classes `A` and `B` is a unwanted property with respect to testing. For `C` instances, however, we do want to print the state of `X` since it holds a different meaning despite the naming coincidence.

```C#
class A
{
  public DateTime X;
  public string Name;
}

class B : A
{
  public int Age;
}

class C
{
  public DateTime X;
}
```


A normal test would look like


```C#
[Test]
public void UserStory()
{
  var cfg = ConfigurationHelper.GetStandardConfiguration();
  var printer = new StatePrinter(cfg);

  var state = printer.PrintObject(new A { X = DateTime.Now, Name = "Charly"});
  ...
}
```

but this will dump `X`. Thus we need to configure	


```C#
var harvester = new SelectiveHarvester();
harvester.Add(typeof(A), x => x.Where(y => y.Name != "X"));

cfg.Add(harvester);
```

And the end result looks like


```C#
[Test]
public void UserStory()
{
  var cfg = ConfigurationHelper.GetStandardConfiguration();

  var harvester = new SelectiveHarvester();
  harvester.Add(typeof(A), x => x.Where(y => y.Name != "X"));
  cfg.Add(harvester);
  
  var printer = new StatePrinter(cfg);

  var state = printer.PrintObject(new A { X = DateTime.Now, Name = "Charly"});
  Assert.AreEqual(@"new A(){    Name = ""Charly""}", state.Replace("\r\n", ""));
}
```



# 4. License

StatePrinter is under the Apache License 2.0, meaning that you can freely use this in other open source or commercial products. If you use it for commercial products please have the courtesy to leave me an email with a 'thank you'. 



Have fun!

Kasper B. Graversen
