#  ![](https://raw.github.com/kbilsted/StatePrinter/master/StatePrinter/gfx/stateprinter.png) StatePrinter configuration

# Table of Content
 * [1. Configuration of StatePrinter](#1-configuration-of-stateprinter)
   * [1.1 Stacked configuration principle](#11-stacked-configuration-principle)
   * [1.2 Simple changes](#12-simple-changes)
   * [1.3 Culture specific printing](#13-culture-specific-printing)
   * [1.4 Output as a single line](#14-output-as-a-single-line)
   * [1.5 Field harvesting](#15-field-harvesting)
   * [1.6 Simple value printing](#16-simple-value-printing)
   * [1.7 Output formatting](#17-output-formatting)
     * [Curly style](#curly-style)
     * [JSon style](#json-style)
     * [XML style](#xml-style)

 
 

# 1. Configuration of StatePrinter

Most of the inner workings of the StatePrinter is configurable. The configuration can be broken down to three parts each of which represents a sub-process of the state printer. Since the configuration is made through code, we'll just as well explain the interfaces.

* `IFieldHarvester` deals with how/which fields are harvested from types. E.g. only public fields are printed.
* `IValueConverter` handles Which types are converted into "simple values". E.g. the decimal type contains a lot of internal stuff, and usually we only want to get the numeric value printed. Or maybe you annotate enum values and prefer those values printed over the numeric value of the enum.
* `IOutputFormatter` deals with turning tokens (internal representation of the object state) into a string form. 
 
Finally, culture specific printing of dates and numbers are supported.


## 1.1 Stacked configuration principle

The Stateprinter has a configuration object that for the most cases be initialized with default behaviour. Don't worry about what they are, since you can easily re-configure the before use. This is due to the FILO principle. The StatePrinter retrieved configuration items in the reverse order they are added and stops when the first match has been found. The defaults are thus a cushion, a nice set of fall-back values.

```C#
var printer = new Stateprinter();
```

is equivalent to

```
var cfg = ConfigurationHelper.GetStandardConfiguration();
var printer = new Stateprinter(cfg);
```

Also notice that the configuration can be accessed directly from the Stateprinter. Eg.

```C#
var printer = new Stateprinter();
printer.Configuration.xxx
```

The `GetStandardConfiguration()` means the following set of configurations.

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

Once the StatePrinter has been initialized you should not change the configuration. A shallow clone of the configuration is made in the constructor to prevent you from shooting yourself in the foot.

Like wise, when implementing harvesters, outputformatters, etc. Do not worry about the stack of the configuration. Simply, through the interface you implement, return only the types you support. In case of an unsupported type, an automatic fall through mechanism will activate the next entity on the stack.


## 1.2 Simple changes

The `Configuration` class should be rather self-documenting. We can change the public fields and properties like setting the indentation characters.

```C#
var printer = new Stateprinter();
printer.Configuration
  .SetIndentIncrement(" ")
  .SetNewlineDefinition("");
```


## 1.3 Culture specific printing

The default culture is `CultureInfo.CurrentCulture`. You can change the `Culture` field in the configuration to suit your needs. 

```C#
const decimal decimalNumber = 12345.343M;
var dateTime = new DateTime(2010, 2, 28, 22, 10, 59);
```

First the us culture

```C#
var printer = new Stateprinter();
printer.Configuration
  .SetCulture(new CultureInfo("en-US")
  .SetNewlineDefinition(""));
  
Assert.AreEqual("12345.343", printer.PrintObject(decimalNumber));
Assert.AreEqual("2/28/2010 10:10:59 PM", printer.PrintObject(dateTime));
```

The same input with a different culture

```C#
var printer = new Stateprinter();
printer.Configuration
  .SetCulture(new CultureInfo("da-DK")
  .SetNewlineDefinition(""));

Assert.AreEqual("12345,343", printer.PrintObject(decimalNumber));
Assert.AreEqual("28-02-2010 22:10:59", printer.PrintObject(dateTime));
```



## 1.4 Output as a single line

When you print really small object you may prefer to use the  which will print the state on a single line.

When printing very small objects, it is some times preferable to print the state as a single line. Use the  `Configuration.SetNewlineDefinition("")` to achieve this. However, the method is more generally applicable. Any sequence of characters can be used as new lines, for example to enforce `LF`+`CR`.



## 1.5 Field harvesting

The StatePrinter comes with a [number of pre-defined harvesters](https://github.com/kbilsted/StatePrinter/tree/master/StatePrinter/FieldHarvesters): For example, the `AllFieldsHarvester` and `PublicFieldsHarvester`. By default we harvest all fields, but you can use whatever implementation you want.  
```C#
var printer = new Stateprinter();
printer.Configuration.Add(new PublicFieldsHarvester());
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

Notice that in `CanHandleType` we are free to set up any restriction. For example, it should apply only to classes in your department. Let's re-implement it.

```C#
public bool CanHandleType(Type type)
{
  return type.ToString().StartsWith("com.megacorp.");
}
```

The full selection of field harvesters are found at https://github.com/kbilsted/StatePrinter/tree/master/StatePrinter/FieldHarvesters




## 1.6 Simple value printing

After we have harvested the fields of the object graph, we may desire to turn a complex object into a simple value. That is one that doesn't hold any nested structure. You'd be surprised of the amount of "garbage" values we would print if we revealed the whole state of the string or decimal instances. If you have any interest in such fields, feel free to supply your own implementation.

For a moment lets ignore that we can configure how strings are formatted using the existing functionality via `printer.Configuration.Add(new StringConverter("'")`. So we'll implement our own value converter for strings making them be printed using the `'` delimiter. 

We simply create a class and implement `IValueConverter`.

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

Due to the stacking principle our value converter is consulted before the standard implementation. The full selection of value converters are found at https://github.com/kbilsted/StatePrinter/tree/master/StatePrinter/ValueConverters


## 1.7 Output formatting

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


 ```C#
var printer = new Stateprinter();
printer.Configuration.SetOutputFormatter(new CurlyBraceStyle(printer.Configuration)));

var course = new Course();
course.Members.Add(new Student("Stan", course));
course.Members.Add(new Student("Richy", course));

printer.PrintObject(course);
```

Yields the output

```
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
```


### JSon style

The JSon style follows the JSon format and describe cyclic references as paths from the root

```C#
printer.Configuration.SetOutputFormatter(new JsonStyle(printer.Configuration)));
```

Yields the output

```
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
```


### XML style

The Xml style is the most verbose

```C#
printer.Configuration.SetOutputFormatter(new XmlStyle(printer.Configuration)));

var car = new Car(new SteeringWheel(new FoamGrip("Plastic")));
printer.PrintObject(car);
```

Yields the output

```
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
```




Have fun!

Kasper B. Graversen
