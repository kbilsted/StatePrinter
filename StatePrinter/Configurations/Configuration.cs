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

using StatePrinting.FieldHarvesters;
using StatePrinting.OutputFormatters;
using StatePrinting.TestAssistance;
using StatePrinting.ValueConverters;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;

namespace StatePrinting.Configurations
{
    /// <summary>
    /// The configuration for the object printer.
    /// </summary>
    public class Configuration
    {
        public const string DefaultIndention = "    ";

        /// <summary>
        /// Set behaviour so StatePrinter works as in previous versions.
        /// </summary>
        public LegacyBehaviour LegacyBehaviour { get; set; }

        /// <summary>
        /// Configure the behaviour concerning automated testing.
        /// </summary>
        public TestingBehaviour Test { get; set; }

        /// <summary>
        /// Specifies the indentation size.
        /// </summary>
        public string IndentIncrement { get; private set; }

        /// <summary>
        /// Set the definition of the newline. The default configuration is a <see cref="Environment.NewLine"/>
        /// </summary>
        public Configuration SetIndentIncrement(string indention)
        {
            if (indention == null)
                throw new ArgumentNullException("indention");
            IndentIncrement = indention;

            return this;
        }

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
            NewLineDefinition = Environment.NewLine;
            LegacyBehaviour = new LegacyBehaviour();

            Test = new TestingBehaviour(this);
            Test.SetAutomaticTestRewrite(x => false);
            Test.SetAssertMessageCreator(new DefaultAssertMessage().Create);
    
            if(areEqualsMethod != null)
                Test.SetAreEqualsMethod(areEqualsMethod);
        }

        /// <summary>
        /// Defines how the output is formatted.
        /// </summary>
        public IOutputFormatter OutputFormatter;
        
        public Configuration SetOutputFormatter(IOutputFormatter formatter)
        {
            if (formatter == null)
                throw new ArgumentNullException("formatter");
            OutputFormatter = formatter;
            return this;
        }
        
        readonly Stack<IValueConverter> valueConverters = new Stack<IValueConverter>();

        /// <summary>
        /// Gets a view of the value converters
        /// </summary>
        public ReadOnlyCollection<IValueConverter> ValueConverters
        {
            get { return new ReadOnlyCollection<IValueConverter>(valueConverters.ToArray()); }
        }

        readonly Stack<IFieldHarvester> fieldHarvesters = new Stack<IFieldHarvester>();

        /// <summary>
        /// Gets a view of the harvesters
        /// </summary>
        public ReadOnlyCollection<IFieldHarvester> FieldHarvesters
        {
            get { return new ReadOnlyCollection<IFieldHarvester>(fieldHarvesters.ToArray()); }
        }

        /// <summary>
        /// Add a configuration. Adding will override the existing behaviour only when the
        /// added handler handles a type that is already handleable by the current configuration.
        /// </summary>
        public Configuration Add(IValueConverter handler)
        {
            if (handler == null)
                throw new ArgumentNullException("handler");

            valueConverters.Push(handler);
            converterLookup.Clear();
            return this;
        }

        /// <summary>
        /// Add a handler. Adding will override the existing behaviour only when the
        /// added handler handles a type that is already handleable by the current configuration.
        /// </summary>
        public Configuration Add(IFieldHarvester handler)
        {
            if (handler == null)
                throw new ArgumentNullException("handler");

            fieldHarvesters.Push(handler);
            fieldHarvesterLookup.Clear();
            return this;
        }

        /// <summary>
        /// Add an anonymous handler implementation. Adding will override the existing behaviour only when the
        /// added handler handles a type that is already handleable by the current configuration.
        /// </summary>
        public Configuration AddHandler(Func<Type, bool> canHandleTypeFunc, Func<Type, List<SanitizedFieldInfo>> getFieldsFunc)
        {
            if (canHandleTypeFunc== null)
                throw new ArgumentNullException("canHandleTypeFunc");

            if (getFieldsFunc == null)
                throw new ArgumentNullException("getFieldsFunc");

            fieldHarvesters.Push(new AnonymousHarvester(canHandleTypeFunc, getFieldsFunc));
            fieldHarvesterLookup.Clear();
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

        readonly Dictionary<Type, IFieldHarvester> fieldHarvesterLookup = new Dictionary<Type, IFieldHarvester>();

        /// <summary>
        /// Find a handler for the type. Handlers are examined in the reverse order of adding and the first match is returned.
        /// </summary>
        public bool TryGetFieldHarvester(Type source, out IFieldHarvester result)
        {
            if (!fieldHarvesterLookup.TryGetValue(source, out result))
            {
                result = fieldHarvesters.FirstOrDefault(x => x.CanHandleType(source));
                fieldHarvesterLookup.Add(source, result);
            }

            return result != null;
        }

        ProjectionHarvester projection;

        /// <summary>
        /// Adds to the configuration a <see cref="ProjectionHarvester"/> and returns it.
        /// </summary>
        public ProjectionHarvester Project
        {
            get { return projection ?? (projection = new ProjectionHarvester(this)); }
        }

        /// <summary>
        /// Instead use <see cref="Project"/>
        /// </summary>
        [Obsolete("Use the Project property instead")]
        public ProjectionHarvester Projectionharvester()
        {
            return Project;
        }

        /// <summary>
        /// Instead use <see cref="TestingBehaviour.SetAreEqualsMethod"/> ie. "printer.Configuration.Test.SetAreEqualsMethod()".
        /// </summary>
        [Obsolete("Use the Configuration.Test.SetAreEqualsMethod instead")]
        public Configuration SetAreEqualsMethod(TestFrameworkAreEqualsMethod areEqualsMethod)
        {
            return Test.SetAreEqualsMethod(areEqualsMethod);
        }

        /// <summary>
        /// Evaluate the function for each failing test. <para></para>
        /// Your function can rely on anything such as an environment variable or a file on the file system. <para></para> 
        /// If you only want to do this evaluation once pr. test suite execution you should wrap your function in a Lazy&lt;&gt;
        /// </summary>
        [Obsolete("Use the Configuration.Test.SetAutomaticTestRewrite instead")]
        public Configuration SetAutomaticTestRewrite(TestingBehaviour.TestRewriteIndicator indicator)
        {
            return Test.SetAutomaticTestRewrite(indicator);
        }

        public Func<FileRepository> FactoryFileRepository = () => new FileRepository();
    }
}
