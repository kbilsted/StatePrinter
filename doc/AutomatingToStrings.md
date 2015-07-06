#  ![](https://raw.github.com/kbilsted/StatePrinter/master/StatePrinter/gfx/stateprinter.png) StatePrinter automating your ToString() methods


**Table of content**
* [1.1 Simple example usage](#11-simple-example-usage)
* [1.2 Object graphs and cycles](#12-object-graphs-and-cycles)
* [1.3 Configuration](#13-configuration)
* [1.4 Best practices](#14-best-practices)


If you are anything like me, there is nothing worse than having to edit all sorts of bizarre methods on a class whenever you add a field to a class. For that reason I always find myself reluctant to maintaining the `ToString()` method. 

With  Stateprinter the situation changes as only a standard implementation is needed, and which does not require changing when fields are added or removed. For an extra productivity boost, I can even add it as part of my code-template in my editor (see  [vs code templates](https://msdn.microsoft.com/en-us/library/ms247121.aspx) and [ReSharper code templates](https://www.jetbrains.com/resharper/features/code_templates.html)).

Stateprinter can seamlesly integrate with projects already having a number of ToString-implementations. When printing an object, or field of an object, that implements the ToString method, that method is used in preference of the standard stateprinter printing.


## 1.1 Simple example usage

To understand just how easy it is to use consider this implementation of a class with two fields. The class has a `ToString` method simply invocing the stateprinter which then using run-time code generation generates code that recursively visits all objects of the graph and prints them.

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

The static `printer` reference is not a requirement, but ensures optimal performance since aspects of the introspection of types is cached in each printer instance.


We can in an object as always by:

```C#
Console.WriteLine( new AClassWithToString() );
```

We print the object. Stateprinter support a number of pre-defined styles of output, and you can easily create your own style if you want (since the output styles have been created by *dog fooding* we know this to be true :-). 

The output styles are easily configured using the `printer.Configuration.SetOutputStyle()`

we get (**curlybrace-style**)

```C#
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
```

or (**json-style**)

```JSON
{
    ""B"" : ""hello"",
    ""C"" :
    [
        ""C"" : 5,
        ""C"" : 4,
        ""C"" : 3,
        ""C"" : 2,
        ""C"" : 1
    ]
}
```

or (**xml-style**)
 
```XML
<ROOT type='AClassWithToString'>
    <B>hello</B>
    <C type='Int32[]'>
        <Enumeration>
        <C>5</C>
        <C>4</C>
        <C>3</C>
        <C>2</C>
        <C>1</C>
        </Enumeration>
    </C>
</ROOT>
``` 
 


## 1.2 Object graphs and cycles

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
	
```C#
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
``` 

Naturally, circular references are supported

```C#
var course = new Course();
course.Members.Add(new Student("Stan", course));
course.Members.Add(new Student("Richy", course));

Console.WriteLine(printer.PrintObject(course));
```

yields	 

```	     
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
```

notice the `-> 0` this is a pointer back to an already printed object. Notice that references are only added to the output if needed. This amongst alot of other details are configurable.


## 1.3 Configuration

The Stateprinter is *very* configurable and extendible. See [configuration](HowToConfigure.md) for the vast possibilities.


## 1.4 Best practices

When using StatePrinter as the ToString() implementation speed may be a consideration. StatePrinter is fairly quick, partly because the introspection of types is cached within the StatePrinter instance. Therefore, it is a bad idea from a performance perspective to instantiate a stateprinter for every use situation.

A better approach is to instantiate and hold a static reference in each type, or inject an instance using IoC into your types.


## 1.5 Performance

```C#
int N = 10000;
var objects = new List<AClass>(N);
for(int i = 0; i < N; i++)
  objects.Add(new AClassWithToString());
  
string lastStrign;
var t1 = Time(() => {foreach(var x in objects) lastString = x.Print1()});
Console.WriteLine(lastString + " = " + t1.ElapsedMillis);

var t2 = Time(() => {foreach(var x in objects) lastString = x.Print2()});
Console.WriteLine(lastString + " = " + t2.ElapsedMillis);

var t3 = Time(() => {foreach(var x in objects) lastString = x.Print3()});
Console.WriteLine(lastString + " = " + t3.ElapsedMillis);
```

and the three implementations

```C#
class AClass
{
    string B = "hello";
    int[] C = {5,4,3,2,1};

    static readonly Stateprinter printer = new Stateprinter();
    public string ToString1()
    {
        return new Stateprinter().PrintObject(this);
    }
  
    public string ToString2()
    {
        return printer.PrintObject(this);
    }
  
    public string ToString3()
    {
      string result = "B = " + B;
      result += " C = "
      foreach(var i in C)
          result += i.ToString();
      return result;
    }
```



Have fun!

Kasper B. Graversen
