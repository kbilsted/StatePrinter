![](https://raw.github.com/kbilsted/StatePrinter/master/StatePrinter/gfx/stateprinter.png)

StatePrinter
=============


The StatePrinter is a free, highly configurable, thread safe utility that can turn any object-graph to a string representation. This makes it much easier to write robus and self-sufficient ToString() methods and makes it easier to write unit tests against object-graphs. For example, using the very nice ApprovalTests framework.

To dump an object graph all you need to do is to


    var car = new Car(new Wheels(new Hubcaps(), ExtraThickRubber(4));
    
    var printer = new StatePrinter();
    string result = printer.PrintObject(car);
    
    Console.WriteLine(result);

	
Kasper B. Graversen <kbilsted@users.noreply.github.com>
