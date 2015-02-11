#  ![](https://raw.github.com/kbilsted/StatePrinter/master/StatePrinter/gfx/stateprinter.png) StatePrinter 

[![Build status](https://ci.appveyor.com/api/projects/status/vx0nl4y4iins506u/branch/master?svg=true)](https://ci.appveyor.com/project/kbilsted/stateprinter/branch/master)
[![Nuget](https://img.shields.io/nuget/dt/stateprinter.svg)](http://nuget.org/packages/stateprinter)
[![Nuget](https://img.shields.io/nuget/v/stateprinter.svg)](http://nuget.org/packages/stateprinter)
[![Coverage Status](https://coveralls.io/repos/kbilsted/StatePrinter/badge.svg?branch=feature%2Fcodecoverage)](https://coveralls.io/r/kbilsted/StatePrinter?branch=feature%2Fcodecoverage)

A simple framework for automatic `Assert.AreEquals()`, `Assert.That()` and `ToString()` - Like a JSON serializer on drugs.

Requires C# 3.5 or newer


# What is Stateprinter

StatePrinter is a simple little library that turn any object-graph into a string representation. It is mainly intended automating writing `ToString` methods and help automating aspects of writing unit tests. 

This type of code is rather dreary and boring to write. Stateprinter is free, highly configurable and thread safe.  

Why you should take StatePrinter for a spin

* *No more manual .ToString()* - it is much easier to write robus and self-sufficient `ToString()` methods. 
* It becomes much easier to write unit tests. No more screens full of asserts. Especially testing against object-graphs is a bliss. 
* Very very configurable both in terms of what to harvest, and in terms of how to output.
* It is part of the back-end engine of the very nice ApprovalTests framework (http://approvaltests.sourceforge.net/).



# How do I get started

The short answer is 

```C#
var car = new Car(new SteeringWheel(new FoamGrip("Plastic")));
car.Brand = "Toyota";

Stateprinter printer = new Stateprinter();

Console.WriteLine(printer.PrintObject(car));
```

The documentation is split into
* [automating ToStrings](https://github.com/kbilsted/StatePrinter/blob/master/doc/AutomatingToStrings.md)
* [automating unit testing](https://github.com/kbilsted/StatePrinter/blob/master/doc/AutomatingUnitTesting.md)
* [configuration](https://github.com/kbilsted/StatePrinter/blob/master/doc/HowToConfigure.md) 


# Where can I get it?
Install Stateprinter from the package manager console:

```
PM> Install-Package StatePrinter
```

# How can I get help?
For quick questions, Stack Overflow is your best bet. For harder questions. For bugs, issues or feature requests, [create a GitHub Issue](https://github.com/kbilsted/StatePrinter/issues/new).


# History

Version History: http://github.com/kbilsted/StatePrinter/blob/master/CHANGELOG.md

This file describes the latest pushed changes. For documentation of earlier releases see:
[1.0.6](https://github.com/kbilsted/StatePrinter/blob/1.0.6/README.md) [1.0.5](https://github.com/kbilsted/StatePrinter/blob/1.0.5/README.md) [1.0.4](https://github.com/kbilsted/StatePrinter/blob/1.0.4/README.md)



# License

Stateprinter is under the Apache License 2.0, meaning that you can freely use this in other open source or commercial products. If you use it for commercial products please have the courtesy to leave me an email with a 'thank you'. 



Have fun!

Kasper B. Graversen
