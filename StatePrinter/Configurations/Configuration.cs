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
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using StatePrinter.FieldHarvesters;
using StatePrinter.OutputFormatters;
using StatePrinter.ValueConverters;

namespace StatePrinter.Configurations
{
  /// <summary>
  /// The configuration for the object printer. We implement <see cref="ICloneable"/> as 
  /// we want to clone at the start of printing state to ensure the configuration is unchaned whilst printing.
  /// </summary>
  public class Configuration : ICloneable
  {
    /// <summary>
    /// Specifies how indentation is done. 
    /// </summary>
    public string IndentIncrement = "    ";

    public CultureInfo Culture = CultureInfo.CurrentCulture;

    public Configuration(IEnumerable<IFieldHarvester> fieldHarvesters, 
      IEnumerable<IValueConverter> valueConverters, 
      string indentIncrement, 
      IOutputFormatter outputFormatter,
      CultureInfo culture)
    {
      this.valueConverters = valueConverters.ToList();
      this.fieldHarvesters = fieldHarvesters.ToList();
      IndentIncrement = indentIncrement;
      OutputFormatter = outputFormatter;
      Culture = culture;
    }

    public Configuration()
    {
      OutputFormatter = new CurlyBraceStyle(IndentIncrement);
    }

    
    public IOutputFormatter OutputFormatter;

    private readonly List<IValueConverter> valueConverters = new List<IValueConverter>();

    public ReadOnlyCollection<IValueConverter> SimplePrinters
    {
      get { return new ReadOnlyCollection<IValueConverter>(valueConverters); }
    }

    private readonly List<IFieldHarvester> fieldHarvesters = new List<IFieldHarvester>();

    public ReadOnlyCollection<IFieldHarvester> FieldHarvesters 
    {
      get { return new ReadOnlyCollection<IFieldHarvester>(fieldHarvesters); }
    }

    /// <summary>
    /// Add a configuration. Adding will override the existing behaviour only when the 
    /// added handler handles a type that is already handlable by the current configuration.
    /// </summary>
    public void Add(IValueConverter handler)
    {
      valueConverters.Insert(0, handler);
    }

    /// <summary>
    /// Add a configuration. Adding will override the existing behaviour only when the 
    /// added handler handles a type that is already handlable by the current configuration.
    /// </summary>
    public void Add(IFieldHarvester handler)
    {
      fieldHarvesters.Insert(0, handler);
    }

    Dictionary<Type, IValueConverter> converterLookup = new Dictionary<Type, IValueConverter>(); 
    
    /// <summary>
    /// Find a handler for the type. Handlers are examined in the reverse order of adding and the first match is returned.
    /// </summary>
    public bool TryGetValueConverter(Type source, out IValueConverter result)
    {
      if (!converterLookup.TryGetValue(source, out result))
      {
        result = valueConverters.FirstOrDefault(x => x.CanHandleType(source));
        converterLookup.Add(source, result);        
      }
      return result != null;
    }

    /// <summary>
    /// Find a handler for the type. Handlers are examined in the reverse order of adding and the first match is returned.
    /// </summary>
    public bool TryGetFieldHarvester(Type source, out IFieldHarvester result)
    {
      result = fieldHarvesters.FirstOrDefault(x => x.CanHandleType(source));
      return result != null;
    }

    public object Clone()
    {
      var res = new Configuration(fieldHarvesters, valueConverters, IndentIncrement, OutputFormatter, Culture);
      return res;
    }
  }
}
