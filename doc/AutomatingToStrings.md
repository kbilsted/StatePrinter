#  ![](https://raw.github.com/kbilsted/StatePrinter/master/StatePrinter/gfx/stateprinter.png) StatePrinter automating you ToString() methods


**Table of content**
* [1.1 Simple example usage](#11-simple-example-usage)
* [1.2 Object graphs and cycles](#12-object-graphs-and-cycles)
* [1.3 Configuration](#13-configuration)
* [1.4 Best practices](#14-best-practices)


If you are anything like me, there is nothing worse than having to edit all sorts of bizarre methods on a class whenever you add a field to a class. For that reason I always find myself reluctant to maintaining the `ToString()` method. With the stateprinter this situation has changed, since I can use the same standard implementation for all my classes. I can even add it as part of my code-template in my editor.


## 1.1 Simple example usage

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

we get (curlybrace-style)

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

or (json-style)

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

or (xml-style)
 
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
	
```
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


## 1.3 Configuration

The Stateprinter is *very* configurable and extendible. See [configuration](HowToConfigure.md) for the vast possibilities.


## 1.4 Best practices

When using StatePrinter as the ToString() implementation speed may be a consideration. StatePrinter is fairly quick, partly because the introspection of types is cached within the StatePrinter instance. Therefore, it is a bad idea from a performance perspective to instantiate a stateprinter for every use situation.

A better approach is to instantiate and hold a static reference in each type, or inject an instance using IoC into your types.


Have fun!

Kasper B. Graversen
