﻿// Copyright 2014-2015 Kasper B. Graversen
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
using StatePrinter.TestAssistance;
using StatePrinter.ValueConverters;

namespace StatePrinter.Configurations
{
    /// <summary>
    /// The configuration for the object printer.
    /// </summary>
    public class Configuration
    {
        public const string DefaultIndention = "    ";

        public LegacyBehaviour LegacyBehaviour { get; set; }

        /// <summary>
        /// Specifies the indentation size.
        /// </summary>
        public readonly string IndentIncrement;

        /// <summary>
        /// The culture to use when generating string output
        /// </summary>
        public CultureInfo Culture = CultureInfo.CurrentCulture;

        /// <summary>
        /// Set the definition of the newline. The default configuration is a <see cref="Environment.NewLine"/>
        /// </summary>
        public Configuration SetCulture(CultureInfo culture)
        {
            if (culture == null)
                throw new ArgumentNullException("culture");
            Culture = culture;

            return this;
        }

        /// <summary>
        /// For small objects, the assert may be better presented on a single line rather than multiple lines.
        /// </summary>
        public string NewLineDefinition { get; private set; }

        /// <summary>
        /// Set the definition of the newline. The default configuration is a <see cref="Environment.NewLine"/>
        /// </summary>
        public Configuration SetNewlineDefinition(string newlineCharacters)
        {
            if(newlineCharacters == null)
                throw new ArgumentNullException("newlineCharacters");
            NewLineDefinition = newlineCharacters;
            
            return this;
        }


        /// <summary>
        /// Instantiate using the <see cref="DefaultIndention"/> and the <see cref="CurlyBraceStyle"/>
        /// </summary>
        public Configuration(
            string indentIncrement = DefaultIndention, 
            TestFrameworkAreEqualsMethod areEqualsMethod = null)
        {
            IndentIncrement = indentIncrement;
            OutputFormatter = new CurlyBraceStyle(this);
            AreEqualsMethod = areEqualsMethod;
            NewLineDefinition = Environment.NewLine;
            LegacyBehaviour = new LegacyBehaviour();
            AutomaticTestRewrite = (x) => false;
        }

        /// <summary>
        /// Defines how the output is formatted.
        /// </summary>
        public IOutputFormatter OutputFormatter;

        readonly List<IValueConverter> valueConverters = new List<IValueConverter>();

        /// <summary>
        /// Gets a view of the value converters
        /// </summary>
        public ReadOnlyCollection<IValueConverter> ValueConverters
        {
            get { return new ReadOnlyCollection<IValueConverter>(valueConverters); }
        }

        readonly List<IFieldHarvester> fieldHarvesters = new List<IFieldHarvester>();

        /// <summary>
        /// Gets a view of the harvesters
        /// </summary>
        public ReadOnlyCollection<IFieldHarvester> FieldHarvesters
        {
            get { return new ReadOnlyCollection<IFieldHarvester>(fieldHarvesters); }
        }

        /// <summary>
        /// Add a configuration. Adding will override the existing behaviour only when the
        /// added handler handles a type that is already handleable by the current configuration.
        /// </summary>
        public Configuration Add(IValueConverter handler)
        {
            valueConverters.Insert(0, handler);
            return this;
        }

        /// <summary>
        /// Add a configuration. Adding will override the existing behaviour only when the
        /// added handler handles a type that is already handleable by the current configuration.
        /// </summary>
        public Configuration Add(IFieldHarvester handler)
        {
            fieldHarvesters.Insert(0, handler);
            return this;
        }

        readonly Dictionary<Type, IValueConverter> converterLookup = new Dictionary<Type, IValueConverter>();

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

        #region unit testing support
        ProjectionHarvester projection;

        /// <summary>
        /// Adds to the configuration a <see cref="ProjectionHarvester"/> and returns it.
        /// </summary>
        public ProjectionHarvester Projectionharvester()
        {
            return projection ?? (projection = new ProjectionHarvester(this));
        }

        /// <summary>
        /// Configure how to call AreEquals in the unit testing framework of your choice. 
        /// Only set this field if you are using the <see cref="Stateprinter.Assert"/> functionality.
        /// </summary>
        public TestFrameworkAreEqualsMethod AreEqualsMethod { get; private set; }

        /// <summary>
        /// Configure how to call AreEquals in the unit testing framework of your choice. 
        /// Only set this field if you are using the <see cref="Stateprinter.Assert"/> functionality.
        /// </summary>
        public Configuration SetAreEqualsMethod(TestFrameworkAreEqualsMethod areEqualsMethod)
        {
            if (areEqualsMethod == null)
                throw new ArgumentNullException("areEqualsMethod");
            AreEqualsMethod = areEqualsMethod;
            
            return this;
        }

        /// <summary>
        /// The signature for finding out if a test's expected value may be automatically re-written.
        /// </summary>
        /// <param name="pathToUnitTest">Path to the failing test.</param>
        /// <returns>True if the test may be rewritten with the new expected value to make the test pass again.</returns>
        public delegate bool TestRewriteIndicator(string pathToUnitTest);

        /// <summary>
        /// Evaluate the function for each failing test. <para></para>
        /// Your function can rely on anything such as an environment variable or a file on the file system. <para></para> 
        /// If you only want to do this evaluation once pr. test suite execution you should wrap your function in a <see cref="Lazy"/>
        /// </summary>
        public Configuration SetAutomaticTestRewrite(TestRewriteIndicator indicator)
        {
            if (indicator == null)
                throw new ArgumentNullException("indicator");
            AutomaticTestRewrite = indicator;
            
            return this;
        }

        /// <summary>
        /// Evaluate the function for each failing test. <para></para>
        /// Your function can rely on anything such as an environment variable or a file on the file system. <para></para> 
        /// If you only want to do this evaluation once pr. test suite execution you should wrap your function in a <see cref="Lazy"/>
        /// </summary>
        public TestRewriteIndicator AutomaticTestRewrite { get; private set; } 
        
        #endregion

        public Func<FileRepository> FactoryFileRepository = () => new FileRepository();
    }

    public class LegacyBehaviour
    {
        /// <summary>
        /// To mimic the behaviour of v1.0.6 and below, set this to false.
        /// </summary>
        public bool TrimTrailingNewlines = true;
    }
}
