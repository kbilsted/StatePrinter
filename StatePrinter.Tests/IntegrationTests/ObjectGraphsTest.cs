// Copyright 2014 Kasper B. Graversen
// 
// Licensed to the Apache Software Foundation (ASF) under one
// or more contributor license agreements.  See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership.  The ASF licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License.  You may obtain a copy of the License at
// 
//   http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied.  See the License for the
// specific language governing permissions and limitations
// under the License.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using StatePrinter.Configurations;
using StatePrinter.OutputFormatters;

namespace StatePrinter.Tests.IntegrationTests
{
  [TestFixture]
  class ObjectGraphsTest
  {
    StatePrinter printer;

    [SetUp]
    public void Setup()
    {
      var cfg = ConfigurationHelper.GetStandardConfiguration();
      cfg.OutputFormatter = new CurlyBraceStyle(cfg.IndentIncrement);
      printer = new StatePrinter(cfg);
    }

    [Test]
    public void ThreeLinkedGraph()
    {
      var car = new Car(new SteeringWheel(new FoamGrip("Plastic")));
      car.Brand = "Toyota";

      var expected =
@"new Car()
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
";
      Assert.AreEqual(expected, printer.PrintObject(car));
    }


    [Test]
    public void ThreeLinkedGraph_json()
    {
      var cfg = ConfigurationHelper.GetStandardConfiguration();
      cfg.OutputFormatter = new JsonStyle(cfg.IndentIncrement);
      var printer = new StatePrinter(cfg);

      var car = new Car(new SteeringWheel(new FoamGrip("Plastic")));
      car.Brand = "Toyota";

      var expected =
@"
{
    ""StereoAmplifiers"" : null,
    ""steeringWheel"" :
    {
        ""Size"" : 3,
        ""Grip"" :
        {
            ""Material"" : ""Plastic""
        }
        ""Weight"" : 525
    }
    ""Brand"" : ""Toyota""
}
";

      Assert.AreEqual(expected, printer.PrintObject(car));
    }

    [Test]
    public void ThreeLinkedGraph_xmlstyle()
    {
      var cfg = ConfigurationHelper.GetStandardConfiguration();
      cfg.OutputFormatter = new XmlStyle(cfg.IndentIncrement);
      var printer = new StatePrinter(cfg);
      var car = new Car(new SteeringWheel(new FoamGrip("Plastic")));
      car.Brand = "Toyota";

      var expected =
@"<ROOT type='Car'>
    <StereoAmplifiers>null</StereoAmplifiers>
    <steeringWheel type='SteeringWheel'>
        <Size>3</Size>
        <Grip type='FoamGrip'>
            <Material>""Plastic""</Material>
        </Grip>
        <Weight>525</Weight>
    </steeringWheel>
    <Brand>""Toyota""</Brand>
</ROOT>
";
      Assert.AreEqual(expected, printer.PrintObject(car));
    }


    [Test]
    public void CyclicGraph_curly()
    {
      var course = new Course();
      course.Members.Add(new Student("Stan", course));
      course.Members.Add(new Student("Richy", course));

      var expected =
@"new Course(), ref: 0
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
";
      Assert.AreEqual(expected, printer.PrintObject(course));
    }


    [Test]
    public void CyclicGraph_Json()
    {
      var cfg = ConfigurationHelper.GetStandardConfiguration();
      cfg.OutputFormatter = new JsonStyle(cfg.IndentIncrement);
      var printer = new StatePrinter(cfg);

      var course = new Course();
      course.Members.Add(new Student("Stan", course));
      course.Members.Add(new Student("Richy", course));

      var expected =
@"
{
    ""Members"" :
    [
        {
            ""name"" : ""Stan"",
            ""course"" :  root
        }
        {
            ""name"" : ""Richy"",
            ""course"" :  root
        }
    ]
}
";
      Assert.AreEqual(expected, printer.PrintObject(course));
    }


    [Test]
    public void MegaCyclicGraph_Curlybrace()
    {
      var mother = MakeFamily();

      var expected =
        @"new Human(), ref: 0
{
    Name = ""Mom""
    Mother = new Human()
    {
        Name = ""grandMom""
        Mother = null
        Children = new List<Human>()
        Children[0] =  -> 0
        Father = null
    }
    Children = new List<Human>()
    Children[0] = new Human(), ref: 1
    {
        Name = ""son""
        Mother =  -> 0
        Children = new List<Human>()
        Father = new Human(), ref: 2
        {
            Name = ""grandDad""
            Mother = null
            Children = new List<Human>()
            Children[0] =  -> 0
            Children[1] =  -> 1
            Children[2] = new Human(), ref: 3
            {
                Name = ""daughter""
                Mother =  -> 0
                Children = new List<Human>()
                Father =  -> 2
            }
            Father = null
        }
    }
    Children[1] =  -> 3
    Father =  -> 2
}
";
      Console.WriteLine(printer.PrintObject(mother));
      Assert.AreEqual(expected, printer.PrintObject(mother));
    }


    [Test]
    public void MegaCyclicGraph_Json()
    {
      var cfg = ConfigurationHelper.GetStandardConfiguration();
      cfg.OutputFormatter = new JsonStyle(cfg.IndentIncrement);
      var printer = new StatePrinter(cfg);


      var mother = MakeFamily();

      var expected =
@"
{
    ""Name"" : ""Mom"",
    ""Mother"" :
    {
        ""Name"" : ""grandMom"",
        ""Mother"" : null,
        ""Children"" :
        [
            ""Children"" :  root
        ],
        ""Father"" : null
    }
    ""Children"" :
    [
        {
            ""Name"" : ""son"",
            ""Mother"" :  root,
            ""Children"" : [],
            ""Father"" :
            {
                ""Name"" : ""grandDad"",
                ""Mother"" : null,
                ""Children"" :
                [
                    ""Children"" :  root,
                    ""Children"" :  root.Children[0],
                    {
                        ""Name"" : ""daughter"",
                        ""Mother"" :  root,
                        ""Children"" : [],
                        ""Father"" :  root.Children[0].Father
                    }
                ],
                ""Father"" : null
            }
        }
        ""Children"" :  root.Children[0].Father.Children[2]
    ],
    ""Father"" :  root.Children[0].Father
}
";
      var actual = printer.PrintObject(mother);
      Assert.AreEqual(expected, actual);
    }


    /// <summary>
    ///   grandMom <---+ +---> grandDad
    ///                | |     ^      ^
    ///                | |     |      | 
    ///                | |     |      |
    ///                Mom<--- son    |
    ///                  ^            |    
    ///                  |            |    
    ///                  +--------- daughter
    /// </summary>
    Human MakeFamily()
    {
      var grandDad = new Human("grandDad");
      var grandMom = new Human("grandMom");

      var mother = new Human("Mom", grandDad, grandMom);
      grandDad.Children.Add(mother);
      grandMom.Children.Add(mother);

      var son = new Human("son", grandDad, mother);
      grandDad.Children.Add(son);
      mother.Children.Add(son);

      var daughter = new Human("daughter", grandDad, mother);
      grandDad.Children.Add(daughter);
      mother.Children.Add(daughter);
      return mother;
    }

    [Test]
    public void CyclicGraph_xmlstyle()
    {
      var cfg = ConfigurationHelper.GetStandardConfiguration();
      cfg.OutputFormatter = new XmlStyle(cfg.IndentIncrement);
      var printer = new StatePrinter(cfg);
      var course = new Course();
      course.Members.Add(new Student("Stan", course));
      course.Members.Add(new Student("Richy", course));

      var expected =
@"<ROOT type='Course' ref='0'>
    <Members type='List(Student)'>
        <Enumeration>
        <Members type='Student'>
            <name>""Stan""</name>
            <course ref='0' />
        </Members>
        <Members type='Student'>
            <name>""Richy""</name>
            <course ref='0' />
        </Members>
        </Enumeration>
    </Members>
</ROOT>
";
      Assert.AreEqual(expected, printer.PrintObject(course));
    }
  }

  #region car
  class Car
  {
    protected int? StereoAmplifiers = null;
    private SteeringWheel steeringWheel;
    public string Brand { get; set; }
    public Car(SteeringWheel steeringWheel)
    {
      this.steeringWheel = steeringWheel;
    }
  }

  internal class SteeringWheel
  {
    internal int Size = 3;
    protected FoamGrip Grip;
  
    public SteeringWheel(FoamGrip grip)
    {
      Grip = grip;
    }
    internal int Weight = 525;

  }

  class FoamGrip
  {
    private string Material;
    public FoamGrip(string material)
    {
      Material = material;
    }
  }
  #endregion


  #region cyclic graph
  class Course
  {
    public List<Student> Members = new List<Student>();
  }


  internal class Student
  {
    public Student(string name, Course course)
    {
      this.name = name;
      this.course = course;
    }

    internal string name;
    private Course course;
  }
#endregion


#region even more cyclic job

  class Human
  {
    readonly public string Name;
    readonly public Human Mother;
    readonly public List<Human> Children = new List<Human>();
    readonly public Human Father;

    public Human(string name, Human father = null, Human mother = null)
    {
      Name = name;
      Father = father;
      Mother = mother;
    }
  }

#endregion

}
