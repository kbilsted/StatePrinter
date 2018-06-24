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
using StatePrinting.OutputFormatters;

namespace StatePrinting.Tests.IntegrationTests
{
    [TestFixture]
    class ObjectGraphsTest
    {
        Stateprinter printer;

        Car car;
        Course course;

        [SetUp]
        public void Setup()
        {
            printer = TestHelper.CreateTestPrinter();

            car = new Car(new SteeringWheel(new FoamGrip("Plastic")))
            {
                Brand = "Toyota",
                Wheels = new [] { new Wheel(), new Wheel(), new Wheel(), new Wheel() }, // testing populated collection
                Passengers = new Passenger[0] // testing empty collection
            };

            course = new Course();
            course.Members.Add(new Student("Stan", course));
            course.Members.Add(new Student("Richy", course));

        }

        [Test]
        public void ThreeLinkedGraph_curly()
        {
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
    Wheels = new Wheel[]()
    {
        [0] = new Wheel()
        {
            Diameter = 0
        }
        [1] = new Wheel()
        {
            Diameter = 0
        }
        [2] = new Wheel()
        {
            Diameter = 0
        }
        [3] = new Wheel()
        {
            Diameter = 0
        }
    }
    Passengers = new Passenger[]()
    {
    }
}";
            printer.Assert.PrintEquals(expected, car);
        }

        [Test]
        public void ThreeLinkedGraph_strictCSharp()
        {
            printer.Configuration.OutputFormatter = new StrictCSharpStyle(printer.Configuration);
            
            var expected =
                @"new Car()
{
    StereoAmplifiers = null,
    steeringWheel = new SteeringWheel()
    {
        Size = 3,
        Grip = new FoamGrip()
        {
            Material = ""Plastic"",
        }
        Weight = 525,
    }
    Brand = ""Toyota"",
    Wheels = new Wheel[]
    {
        new Wheel()
        {
            Diameter = 0,
        },
        new Wheel()
        {
            Diameter = 0,
        },
        new Wheel()
        {
            Diameter = 0,
        },
        new Wheel()
        {
            Diameter = 0,
        }
    }
}";
            printer.Assert.PrintEquals(expected, car);
        }

        
        [Test]
        public void ThreeLinkedGraph_json()
        {
            printer.Configuration.OutputFormatter = new JsonStyle(printer.Configuration);

            var expected = @"{
    ""StereoAmplifiers"": null,
    ""steeringWheel"": {
        ""Size"": 3,
        ""Grip"": {
            ""Material"": ""Plastic""
        },
        ""Weight"": 525
    },
    ""Brand"": ""Toyota"",
    ""Wheels"": [
        {
            ""Diameter"": 0
        },
        {
            ""Diameter"": 0
        },
        {
            ""Diameter"": 0
        },
        {
            ""Diameter"": 0
        }
    ],
    ""Passengers"": []
}";

            printer.Assert.PrintEquals(expected, car);
        }

        [Test]
        public void ThreeLinkedGraph_xmlstyle()
        {
            printer.Configuration.OutputFormatter = new XmlStyle(printer.Configuration);

            var expected = @"<Root type='Car'>
    <StereoAmplifiers>null</StereoAmplifiers>
    <steeringWheel type='SteeringWheel'>
        <Size>3</Size>
        <Grip type='FoamGrip'>
            <Material>Plastic</Material>
        </Grip>
        <Weight>525</Weight>
    </steeringWheel>
    <Brand>Toyota</Brand>
    <Wheels type='Wheel[]'>
        <Element type='Wheel'>
            <Diameter>0</Diameter>
        </Element>
        <Element type='Wheel'>
            <Diameter>0</Diameter>
        </Element>
        <Element type='Wheel'>
            <Diameter>0</Diameter>
        </Element>
        <Element type='Wheel'>
            <Diameter>0</Diameter>
        </Element>
    </Wheels>
    <Passengers type='Passenger[]'>
    </Passengers>
</Root>";

            printer.Assert.PrintEquals(expected, car);
        }


        [Test]
        public void CyclicGraph_curly()
        {
            var expected = @"new Course(), ref: 0
{
    Members = new List<Student>()
    {
        [0] = new Student()
        {
            name = ""Stan""
            course = -> 0
        }
        [1] = new Student()
        {
            name = ""Richy""
            course = -> 0
        }
    }
}";
            printer.Assert.PrintEquals(expected, course);
        }

        [Test]
        public void CyclicGraph_strictCSharp()
        {
            printer.Configuration.OutputFormatter = new StrictCSharpStyle(printer.Configuration);
            Assert.Throws<NotSupportedException>(() => printer.PrintObject(course));
        }
        
        [Test]
        public void CyclicGraph_Json()
        {
            printer.Configuration.OutputFormatter = new JsonStyle(printer.Configuration);

            var expected = @"{
    ""Members"": [
        {
            ""name"": ""Stan"",
            ""course"": root
        },
        {
            ""name"": ""Richy"",
            ""course"": root
        }
    ]
}";
            printer.Assert.PrintEquals(expected, course);
        }


        [Test]
        public void MegaCyclicGraph_Curlybrace()
        {
            var mother = MakeFamily();

            var expected = @"new Human(), ref: 0
{
    Name = ""Mom""
    Mother = new Human()
    {
        Name = ""grandMom""
        Mother = null
        Children = new List<Human>()
        {
            [0] = -> 0
        }
        Father = null
    }
    Children = new List<Human>()
    {
        [0] = new Human(), ref: 1
        {
            Name = ""son""
            Mother = -> 0
            Children = new List<Human>()
            {
            }
            Father = new Human(), ref: 2
            {
                Name = ""grandDad""
                Mother = null
                Children = new List<Human>()
                {
                    [0] = -> 0
                    [1] = -> 1
                    [2] = new Human(), ref: 3
                    {
                        Name = ""daughter""
                        Mother = -> 0
                        Children = new List<Human>()
                        {
                        }
                        Father = -> 2
                    }
                }
                Father = null
            }
        }
        [1] = -> 3
    }
    Father = -> 2
}";
            //Console.WriteLine(printer.PrintObject(mother));
            printer.Assert.PrintEquals(expected, mother);
        }


        [Test]
        public void MegaCyclicGraph_Json()
        {
            printer.Configuration.OutputFormatter = new JsonStyle(printer.Configuration);

            var mother = MakeFamily();
            var expected = @"{
    ""Name"": ""Mom"",
    ""Mother"": {
        ""Name"": ""grandMom"",
        ""Mother"": null,
        ""Children"": [
            root
        ],
        ""Father"": null
    },
    ""Children"": [
        {
            ""Name"": ""son"",
            ""Mother"": root,
            ""Children"": [],
            ""Father"": {
                ""Name"": ""grandDad"",
                ""Mother"": null,
                ""Children"": [
                    root,
                    root.Children[0],
                    {
                        ""Name"": ""daughter"",
                        ""Mother"": root,
                        ""Children"": [],
                        ""Father"": root.Children[0].Father
                    }
                ],
                ""Father"": null
            }
        },
        root.Children[0].Father.Children[2]
    ],
    ""Father"": root.Children[0].Father
}";
            printer.Assert.PrintEquals(expected, mother);
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
            printer.Configuration.OutputFormatter = new XmlStyle(printer.Configuration);
            
            var course = new Course();
            course.Members.Add(new Student("Stan", course));
            course.Members.Add(new Student("Richy", course));

            var expected = @"<Root type='Course' ref='0'>
    <Members type='List(Student)'>
        <Element type='Student'>
            <name>Stan</name>
            <course ref='0'/>
        </Element>
        <Element type='Student'>
            <name>Richy</name>
            <course ref='0'/>
        </Element>
    </Members>
</Root>";
            printer.Assert.PrintEquals(expected, course);
        }
    }

    internal class Passenger
    {
    }

    #region car
    class Car
    {
        protected int? StereoAmplifiers = null;
        private SteeringWheel steeringWheel;
        public string Brand { get; set; }
        public Wheel[] Wheels { get; set; }
        public Passenger[] Passengers { get; set; }

        public Car(SteeringWheel steeringWheel)
        {
            this.steeringWheel = steeringWheel;
        }
    }

    internal class Wheel
    {
        public decimal Diameter { get; set; }
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
