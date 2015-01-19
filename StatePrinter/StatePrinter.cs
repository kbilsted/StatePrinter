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

using StatePrinter.Configurations;
using StatePrinter.FieldHarvesters;
using StatePrinter.Introspection;

namespace StatePrinter
{
  /// <summary>
  /// A class able to dump an object graph to a string.
  /// class inheritance hierarchies as well as cyclic graphs are supported.
  /// 
  /// Simply call the <see cref="PrintObject"/> with an object or object-graph to get a string representation.
  /// 
  /// The printing process is thread safe.
  /// 
  /// The state printer is highly customizable both in terms of which types and fields are printed as well as how printing of values is done.
  /// </summary>
  public class Stateprinter
  {
    readonly Configuration configuration;

    /// <summary>
    /// The cache cannot be static since we have many different harvesters, and potentially many different usages of <see cref="ProjectionHarvester"/>
    /// </summary>
    readonly HarvestInfoCache harvestCache = new HarvestInfoCache(); 

    /// <summary>
    /// Create an state printer using the supplied configuration.
    /// </summary>
    public Stateprinter(Configuration configuration)
    {
      if (configuration == null)
        throw new ArgumentNullException("configuration");

      this.configuration = (Configuration)configuration.Clone();
    }

    /// <summary>
    /// Create an state printer using the <see cref="ConfigurationHelper.GetStandardConfiguration"/>
    /// </summary>
    public Stateprinter()
    {
      configuration = ConfigurationHelper.GetStandardConfiguration();
    }

    /// <summary>
    /// Print an object graph to a string.
    /// </summary>
    /// <param name="objectToPrint">What to print.</param>
    /// <param name="rootname">The name of the root as it is printed.</param>
    public string PrintObject(object objectToPrint, string rootname = null)
    {
      var introSpector = new IntroSpector(configuration, harvestCache);
      var tokens = introSpector.PrintObject(objectToPrint, rootname);

      var formatter = configuration.OutputFormatter;

      return formatter.Print(tokens);
    }
  }

  /// <summary>
  /// WARNING! This is a legacy stub and will be removed in future releases. Instead use the  <see cref="Stateprinter"/>
  /// </summary>
  [Obsolete]
  public class StatePrinter : Stateprinter
  {
    public StatePrinter() : base()
    {
    }

    public StatePrinter(Configuration configuration) : base(configuration)
    {
    }
  }
}
