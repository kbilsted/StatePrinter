![](https://raw.github.com/kbilsted/StatePrinter/master/StatePrinter/gfx/stateprinter.png)

StatePrinter
=============


The StatePrinter is a free, highly configurable, thread safe utility that can turn any object-graph to a string representation. 

* This makes it much easier to write robus and self-sufficient ToString() methods 
* It becomes much easier to write unit tests against object-graphs. 
* It is part of the back-end engine of the very nice ApprovalTests framework.



### Simple usage


To dump an object graph all you need to do is first to create an object graph

      var car = new Car(new SteeringWheel(new FoamGrip("Plastic")));
      car.Brand = "Toyota";

then print it

    StatePrinter printer = new StatePrinter();
    Console.WriteLine(printer.PrintObject(car));

and you get the following output
	
	ROOT = <Car>
	{
	    StereoAmplifiers = null
	    steeringWheel = <SteeringWheel>
	    {
	        Size = 3
	        Grip = <FoamGrip>
	        {
	            Material = ""Plastic""
	        }
	    }
	    Brand = ""Toyota""
	}


Naturally, circular references are supported

      var course = new Course();
      course.Members.Add(new Student("Stan", course));
      course.Members.Add(new Student("Richy", course));

      Console.WriteLine(printer.PrintObject(course, "Start"));

yields	 
	     
	Start = <Course>, ref: 0
	{
	    Members = <List<Student>>
	    Members[0] = <Student>
	    {
	        name = ""Stan""
	        course =  -> 0
	    }
	    Members[1] = <Student>
	    {
	        name = ""Richy""
	        course =  -> 0
	    }
	}

notice the `-> 0` this is a pointer back to an already printed object. Notice that references are only added to the output if needed. This amongst alot of other details are configurable.


### Generic ToString() usage

If you are anything like me, there is nothing worse than having to edit all sorts of bizare methods on a class whenever you add a field to a class. For that reason I always find myself not wanting to maintain the `ToString()` method. With the stateprinter this situation has changed, since I can use the same standard implementation for all my classes. I can even add it as part of my code-template in my editor.



	  class AClassWithToString
	  {
	    string B = "hello";
	    int[] C = {5,4,3,2,1};
	    static readonly StatePrinter printer = new StatePrinter();
	
	    // Nice stuff ahead!
	    public override string ToString()
	    {
	      return printer.PrintObject(this, "");
	    }
	  }

And with the code

      Console.WriteLine( new AClassWithToString() );

we get

	 = <AClassWithToString>
	{
	    B = ""hello""
	    C = <Int32[]>
	    C[0] = 5
	    C[1] = 4
	    C[2] = 3
	    C[3] = 2
	    C[4] = 1
	}



## Configuration

Now, this is the fun part. Most of the inner workings of the StatePrinter is configurable. The configuration can be broken down to three parts

* `IFieldHarvester` deals with how/which fields are harvested from types. E.g. only public fields are printed.
* `IValueConverter` handles Which types are converted into "simple values". Eg. the decimal type contains a lot of internal stuff, and usually we only want to get the numeric value printed. Or maybe you annotate enum values preser those values printed.
* `IOutputFormatter` deals with turning tokens (internal representation of the object state) into a string form. 
 

### FILO configuration - First In, Last Out

The stateprinter has a configuration object that for the most cases be initialized with default behaviour. Don't worry about what they are, since you can easily re-configure the before use. This is due to the FILO principle. The StatePrinter retrieved configuration items in the reverse order they are added and stops when the first match has been found. The defaults are thus a cusion, a nice set of fall-back values.

    var printer = new StatePrinter();
    
is equivalent to

    var cfg = ConfigurationHelper.GetStandardConfiguration();
    var printer = new StatePrinter(cfg);


which really means

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

Once the StatePrinter has been initialized you should not change the configuration. A shallow clone of the configuration is made in the constructor to prevent you from shooting youself in the foot.


### Simple changes

The `Configuration` class should be rather self-documenting. We can change the public fields and properties like setting the indentation characters.

    var cfg = ConfigurationHelper.GetStandardConfiguration();
    cfg.IndentIncrement = " ";
    
    var printer = new StatePrinter(cfg);



### FieldHarvesting

The StatePrinter comes with two pre-defined harvesters: The `AllFieldsHarvester` and `PublicFieldsHarvester`. By default we harvest all fields, but you can use whatever implementation you want.

    var cfg = ConfigurationHelper.GetStandardConfiguration();
    cfg.Add(new PublicFieldsHarvester());

    var printer = new StatePrinter(cfg);


Field harvesting is simpler than what you expect. While you may never need to write one yourself, let's walk through the PublicFieldsHarvester for the fun of it. The harvester basically works by harvesting all fields and filtering away those it does not want. We want all public fields, and all private fields if they are the backing fields of public fields.

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

Notice that in `CanHandleType` we are free to setup any restriction. For example, it should apply only to classes in your department. Let's re-implement it.


	    public bool CanHandleType(Type type)
	    {
	      return type.ToString().StartsWith("com.megacorp.");
	    }


### Simple value printing

After we have harvested the fields of the object graph, we may desire to turn a complex object into a simple value. That is one that doesn't hold any nested structure. You'd be surprised of the amount of "garbage" values we would print if we revealed the whole state of the string or decimal instances. If you have any interest in such fields, feel free to supply your own implementation.

Let's re-write how we print strings. We want them printed using the `'` delimiter rather than the `"`

First we implement a `IValueConverter`

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

then we add it to the configuration before usage

    var cfg = ConfigurationHelper.GetStandardConfiguration();
    cfg.Add(new StringToPlingConverter());

    var printer = new StatePrinter(cfg);

Due to the FILO principle (First In Last Out) our valueconverter is consulted before the standard implementation.


### Output formatting

the `IOutputFormatter` only contains a single method

    string Print(List<Token> tokens);

It turns tokens into a "format". Much like traditional compiler design, a token represents a processed entity. In the StatePrinter they look like

    public class Token : IEquatable<Token>
    {
      public readonly TokenType Tokenkind;
      public readonly Type FieldType;
      public readonly string FieldName;
      public readonly string Value;
      public readonly Reference ReferenceNo;
    }

So at this point in the process we need not worry about recursion, field traversal or the like. We focus on the formatting, turning the tokens into a XML-like, JSON-like, LISP-like S-Expressions or whatever you wish. In the current implementation we make two passes on the input to track which objects are referred to by later object. Those we wish to augment with a reference.


Have fun!

Kasper B. Graversen


