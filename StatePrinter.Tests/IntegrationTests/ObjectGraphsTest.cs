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

using System.Collections.Generic;
using NUnit.Framework;
using StatePrinter.Configurations;
using StatePrinter.OutputFormatters;

namespace StatePrinter.Tests.IntegrationTests
{
  [TestFixture]
  class ObjectGraphsTest
  {
    readonly StatePrinter printer = new StatePrinter();


    [Test]
    public void ThreeLinkedGraph()
    {
      var car = new Car(new SteeringWheel(new FoamGrip("Plastic")));
      car.Brand = "Toyota";

      var expected =
@"ROOT = <Car>
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
    </steeringWheel>
    <Brand>""Toyota""</Brand>
</ROOT>
";
      Assert.AreEqual(expected, printer.PrintObject(car));
    }


    [Test]
    public void CyclicGraph()
    {
      var course = new Course();
      course.Members.Add(new Student("Stan", course));
      course.Members.Add(new Student("Richy", course));

      var expected =
@"ROOT = <Course>, ref: 0
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
";
      Assert.AreEqual(expected, printer.PrintObject(course));
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
    <Members[0] type='Student'>
        <name>""Stan""</name>
        <course ref='0' />
    </Members[0]>
    <Members[1] type='Student'>
        <name>""Richy""</name>
        <course ref='0' />
    </Members[1]>
</ROOT>
";
      Assert.AreEqual(expected, printer.PrintObject(course));
    }
  }

  #region car
  class Car
  {
    protected int? StereoAmplifiers;
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

}
