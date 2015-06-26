# Making comprehensible tests on data containing  Guids

A common practice in sofware developments, is to guids as the identity of an object. This practice has a number of advantages, but at the same time, makes it tedius to test. 

One advantage is that the primary key can safely be generated in memory rather than the database. Thus an object graph may be stored in one request to the database. Another advantage is, that data from two or more instances of the software components can easily be joined/merged since there are no colliding keys. And despite its crititcs scorning the size of the guid is four times that of an int, and that id `4001` is easier to remember than say `855b152c-c7ee-4510-9449-b2c840b933a6`, it is a safe bet, that guid's are here to stay.



## Identifying the problem

Testing data containing guids can be problematic, in that we can rarely make assertions on the specific value of a guid field. Assume an online webshop with the data structure

```C#
class Order {
    Guid Id = Guid.NewGuid();
    List<OrderLine> Lines = new List<OrderLine>();
    
    public void Add(OrderLine line) {
        line.OrderId = Id;
        Lines.Add(line);
    }
}
```

and the order line

```C#
class OrderLine {
    public Guid Id = Guid.NewGuid();
    public Guid OrderId;

    public int Quantity;
    public Guid ItemId;
    public string Description;
}
```
