// Copyright 2014-2015 Kasper B. Graversen
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
using System.Linq;
using System.Linq.Expressions;
using StatePrinting.Configurations;

namespace StatePrinting.FieldHarvesters
{
    /// <summary>
    /// Harvest only selected fields from types configured by lambdas. 
    /// 
    /// Predominantly for unit testing, when you need to easily filter away specific fields in order to 
    /// only test the content of relevant fields.
    /// </summary>  
    public class ProjectionHarvester : IFieldHarvester
    {
        readonly List<Implementation> excluders = new List<Implementation>();
        readonly List<Implementation> includers = new List<Implementation>();
        readonly List<Implementation> filters = new List<Implementation>();
        IEnumerable<Implementation> selected;
        Strategy selectedStrategy = Strategy.Includer;

        readonly IFieldHarvester harvestingStrategy;

        /// <summary>
        /// Create an instance and using the <see cref="AllFieldsAndPropertiesHarvester"/> to harvest the fields.
        /// </summary>
        public ProjectionHarvester()
            : this(null, null)
        {
        }

        /// <summary>
        /// Create instance and add to the configuration. using the <see cref="AllFieldsAndPropertiesHarvester"/> to harvest the fields
        /// </summary>
        public ProjectionHarvester(Configuration configuration)
            : this(configuration, null)
        {
        }

        /// <summary>
        /// Create instance and add to the configuration.
        /// </summary>
        public ProjectionHarvester(IFieldHarvester harvester)
            : this(null, harvester)
        {
        }

        /// <summary>
        /// Create instance and add to the configuration. 
        /// If <param name="harvester"/> is null, use the <see cref="AllFieldsAndPropertiesHarvester"/> to harvest the fields.
        /// </summary>
        public ProjectionHarvester(
            Configuration configuration = null,
            IFieldHarvester harvester = null)
        {
            if (configuration != null) 
                configuration.Add(this);
            harvestingStrategy = harvester ?? new AllFieldsAndPropertiesHarvester();
        }

        #region IFieldHarvester

        bool SelectStrategy(
            Type type,
            IEnumerable<Implementation> implementations,
            Strategy implementationKind)
        {
            selected =
                implementations.Where(x => x.Selector.IsAssignableFrom(type)).ToList();
            if (selected.Any())
            {
                selectedStrategy = implementationKind;
                return true;
            }
            return false;
        }

        public bool CanHandleType(Type type)
        {
            if (SelectStrategy(type, excluders, Strategy.Excluder)) return true;
            if (SelectStrategy(type, includers, Strategy.Includer)) return true;
            if (SelectStrategy(type, filters, Strategy.Filter)) return true;

            return false;
        }

        /// <summary>
        /// We ignore all properties as they, in the end, will only point to some computed state or other fields.
        /// Hence they do not provide information about the actual state of the object.
        /// </summary>
        List<SanitizedFieldInfo> IFieldHarvester.GetFields(Type type)
        {
            if (selectedStrategy == Strategy.Excluder
                || selectedStrategy == Strategy.Filter) return ExcludeOrFilterfields(type, harvestingStrategy.GetFields(type));
            else 
                return IncludeFields(type, harvestingStrategy.GetFields(type));
        }

        List<SanitizedFieldInfo> IncludeFields(Type type, List<SanitizedFieldInfo> fields)
        {
            var result = new List<SanitizedFieldInfo>();
            foreach (var implementation in selected) 
                result.AddRange(implementation.Filter(fields));

            return result;
        }

        List<SanitizedFieldInfo> ExcludeOrFilterfields(
            Type type,
            List<SanitizedFieldInfo> fields)
        {
            foreach (var implementation in selected) 
                fields = implementation.Filter(fields).ToList();

            return fields;
        }
        #endregion


        /// <summary>
        /// Add a filter that exludes one or more fields
        /// </summary>
        /// <typeparam name="TTarget">Target of filter</typeparam>
        /// <returns>Returns itself so you can chain the filter calls.</returns>
        public ProjectionHarvester AddFilter<TTarget>(
            Func<List<SanitizedFieldInfo>, IEnumerable<SanitizedFieldInfo>> filter)
        {
            PreConditionToAdd<TTarget>(Strategy.Filter);

            filters.Add(new Implementation(typeof(TTarget), filter));
            return this;
        }

        /// <summary>
        /// Includes one or more fields.
        /// </summary>
        /// <typeparam name="TTarget">The type to operate on</typeparam>
        /// <returns>Returns itself so you can chain the include calls.</returns>
        public ProjectionHarvester Include<TTarget>(
            params Expression<Func<TTarget, object>>[] fieldSpecifications)
        {
            PreConditionToAdd<TTarget>(Strategy.Includer);

            foreach (var fieldSpecification in fieldSpecifications)
                IncludeFieldImplementation(fieldSpecification);

            return this;
        }

        void IncludeFieldImplementation<TTarget, TAny>(
            Expression<Func<TTarget, TAny>> fieldSpecification)
        {
            string name = GetFieldNameFromExpression(fieldSpecification);
            includers.Add(
                new Implementation(
                    typeof(TTarget),
                    x => x.Where(y => y.SanitizedName == name)));
        }

        /// <summary>
        /// Includes one or more fields as specified by <typeparam name="TInclude"/>
        /// </summary>
        /// <typeparam name="TTarget">Target of filter</typeparam>
        /// <typeparam name="TInclude">Fields of this type are included</typeparam>
        /// <returns>Returns itself so you can chain the filter calls.</returns>
        public ProjectionHarvester IncludeByType<TTarget, TInclude>() 
            where TTarget : TInclude
        {
            IncludeByTypeImplementation<TTarget>(typeof(TInclude));
            return this;
        }

        /// <summary>
        /// Includes one or more fields as specified by <typeparam name="TInclude"/>
        /// </summary>
        /// <typeparam name="TTarget">Target of filter</typeparam>
        /// <typeparam name="TInclude">Fields of this type are included</typeparam>
        /// <typeparam name="TInclude2">Fields of this type are included</typeparam>
        /// <returns>Returns itself so you can chain the filter calls.</returns>
        public ProjectionHarvester IncludeByType<TTarget, TInclude, TInclude2>()
            where TTarget : TInclude, TInclude2
        {
            IncludeByTypeImplementation<TTarget>(typeof(TInclude), typeof(TInclude2));
            return this;
        }

        /// <summary>
        /// Includes one or more fields as specified by <typeparam name="TInclude"/>
        /// </summary>
        /// <typeparam name="TTarget">Target of filter</typeparam>
        /// <typeparam name="TInclude">Fields of this type are included</typeparam>
        /// <typeparam name="TInclude2">Fields of this type are included</typeparam>
        /// <typeparam name="TInclude3">Fields of this type are included</typeparam>
        /// <returns>Returns itself so you can chain the filter calls.</returns>
        public ProjectionHarvester IncludeByType<TTarget, TInclude, TInclude2, TInclude3>()
            where TTarget : TInclude, TInclude2, TInclude3
        {
            IncludeByTypeImplementation<TTarget>(typeof(TInclude), typeof(TInclude2), typeof(TInclude3));
            return this;
        }

        /// <summary>
        /// Includes one or more fields as specified by <typeparam name="TInclude"/>
        /// </summary>
        /// <typeparam name="TTarget">Target of filter</typeparam>
        /// <typeparam name="TInclude">Fields of this type are included</typeparam>
        /// <typeparam name="TInclude2">Fields of this type are included</typeparam>
        /// <typeparam name="TInclude3">Fields of this type are included</typeparam>
        /// <typeparam name="TInclude4">Fields of this type are included</typeparam>
        /// <returns>Returns itself so you can chain the filter calls.</returns>
        public ProjectionHarvester IncludeByType<TTarget, TInclude, TInclude2, TInclude3, TInclude4>()
            where TTarget : TInclude, TInclude2, TInclude3, TInclude4
        {
            IncludeByTypeImplementation<TTarget>(typeof(TInclude), typeof(TInclude2), typeof(TInclude3), typeof(TInclude4));
            return this;
        }

        void IncludeByTypeImplementation<TTarget>(params Type[] types)
        {
            PreConditionToAdd<TTarget>(Strategy.Includer);
            var helper = new HarvestHelper();
            
            // we evaluate outside of the usage in the lambda to prevent multiple calls to GetFieldsAndProperties()
            var cachedList = new List<SanitizedFieldInfo>();
            foreach (var type in types)
                cachedList.AddRange(helper.GetFieldsAndProperties(type));
            includers.Add(new Implementation(typeof(TTarget), x => cachedList));
        }


        /// <summary>
        /// Excludes one or more fields.
        /// </summary>
        /// <typeparam name="TTarget">The type to operate on</typeparam>
        /// <returns>Returns itself so you can chain the exclude calls.</returns>
        public ProjectionHarvester Exclude<TTarget>(
            params Expression<Func<TTarget, object>>[] fieldSpecifications)
        {
            PreConditionToAdd<TTarget>(Strategy.Excluder);

            foreach (var fieldSpecification in fieldSpecifications) 
                ExcludeField(fieldSpecification);

            return this;
        }

        /// <summary>
        /// Excludes one or more fields.
        /// </summary>
        /// <typeparam name="TTarget">The type to operate on</typeparam>
        /// <typeparam name="TExclude">The type's fields will be excluded on the target</typeparam>
        /// <returns>Returns itself so you can chain the exclude calls.</returns>
        public ProjectionHarvester ExcludeByType<TTarget, TExclude>()
            where TTarget : TExclude
        {
            ExcludeByTypeImplementation<TTarget>(typeof(TExclude));
            return this;
        }

        /// <summary>
        /// Excludes one or more fields.
        /// </summary>
        /// <typeparam name="TTarget">The type to operate on</typeparam>
        /// <typeparam name="TExclude">The type's fields will be excluded on the target</typeparam>
        /// <typeparam name="TExclude2">The type's fields will be excluded on the target</typeparam>
        /// <returns>Returns itself so you can chain the exclude calls.</returns>
        public ProjectionHarvester ExcludeByType<TTarget, TExclude, TExclude2>()
            where TTarget : TExclude, TExclude2
        {
            ExcludeByTypeImplementation<TTarget>(typeof(TExclude), typeof(TExclude2));
            return this;
        }


        /// <summary>
        /// Excludes one or more fields.
        /// </summary>
        /// <typeparam name="TTarget">The type to operate on</typeparam>
        /// <typeparam name="TExclude">The type's fields will be excluded on the target</typeparam>
        /// <typeparam name="TExclude2">The type's fields will be excluded on the target</typeparam>
        /// <typeparam name="TExclude3">The type's fields will be excluded on the target</typeparam>
        /// <returns>Returns itself so you can chain the exclude calls.</returns>
        public ProjectionHarvester ExcludeByType<TTarget, TExclude, TExclude2, TExclude3>()
            where TTarget : TExclude, TExclude2, TExclude3
        {
            ExcludeByTypeImplementation<TTarget>(typeof(TExclude), typeof(TExclude2), typeof(TExclude3));
            return this;
        }


        /// <summary>
        /// Excludes one or more fields.
        /// </summary>
        /// <typeparam name="TTarget">The type to operate on</typeparam>
        /// <typeparam name="TExclude">The type's fields will be excluded on the target</typeparam>
        /// <typeparam name="TExclude2">The type's fields will be excluded on the target</typeparam>
        /// <typeparam name="TExclude3">The type's fields will be excluded on the target</typeparam>
        /// <typeparam name="TExclude4">The type's fields will be excluded on the target</typeparam>
        /// <returns>Returns itself so you can chain the exclude calls.</returns>
        public ProjectionHarvester ExcludeByType<TTarget, TExclude, TExclude2, TExclude3, TExclude4>()
            where TTarget : TExclude, TExclude2, TExclude3, TExclude4
        {
            ExcludeByTypeImplementation<TTarget>(typeof(TExclude), typeof(TExclude2), typeof(TExclude3), typeof(TExclude4));
            return this;
        }

        void ExcludeByTypeImplementation<TTarget>(params Type[] types)
        {
            PreConditionToAdd<TTarget>(Strategy.Excluder);
            var helper = new HarvestHelper();

            // we evaluate outside of the usage in the lambda to prevent multiple calls to GetFieldsAndProperties()
            var cachedList = new List<string>();
            foreach (var type in types)
                cachedList.AddRange(helper.GetFieldsAndProperties(type).Select(x=>x.SanitizedName));
            excluders.Add(new Implementation(typeof(TTarget), x => x.Where(y => !cachedList.Contains(y.SanitizedName))));
        }

        void PreConditionToAdd<TTarget>(Strategy addingStrategy)
        {
            if (addingStrategy != Strategy.Excluder)
                Forbid<TTarget>(excluders, "an excluder");

            if (addingStrategy != Strategy.Includer)
                Forbid<TTarget>(includers, "an includer");

            if (addingStrategy != Strategy.Filter)
                Forbid<TTarget>(filters, "a filter");
        }

        void Forbid<TTarget>(List<Implementation> filter, string filterKind)
        {
            var type = typeof(TTarget);
            if (filter.Any(x => x.Selector == type))
                throw new ArgumentException(
                    string.Format("Type {0} has already been configured as {1}.", type.Name, filterKind));
        }

        void ExcludeField<TTarget, TAny>(Expression<Func<TTarget, TAny>> fieldSpecification)
        {
            var name = GetFieldNameFromExpression(fieldSpecification);
            excluders.Add(
                new Implementation(
                    typeof(TTarget),
                    x => x.Where(y => y.SanitizedName != name)));
        }

        string GetFieldNameFromExpression<TTarget, TAny>(
            Expression<Func<TTarget, TAny>> fieldSpecification)
        {
            const string error = "Field specification must refer to a field";
            // Console.WriteLine("!"+fieldSpecification.Body.GetType().ToString());
            if (fieldSpecification.Body is UnaryExpression)
            {
                var body = Cast<UnaryExpression>(fieldSpecification.Body, error);
                return GetNameFromMemberExpression<TTarget>(body.Operand, error);
            }

            if (fieldSpecification.Body is MemberExpression)
                return GetNameFromMemberExpression<TTarget>(
                    fieldSpecification.Body,
                    error);
            throw new Exception("This can never happen");
        }

        string GetNameFromMemberExpression<TTarget>(
            Expression fieldSpecification,
            string error)
        {
            var field = Cast<MemberExpression>(fieldSpecification, error);
            var target = typeof(TTarget);

            if (field.Member.DeclaringType.IsAssignableFrom(target)) return field.Member.Name;

            throw new ArgumentException(
                string.Format(
                    "Field '{0}' is declared on type '{1}' not on argument: '{2}'",
                    field.Member.Name,
                    field.Member.DeclaringType.Name,
                    target.Name));
        }

        T Cast<T>(object objectToCast, string errorMessage) where T : class
        {
            T x = objectToCast as T;
            if (x == null) 
                throw new ArgumentException(errorMessage);
            return x;
        }

        class Implementation
        {
            public readonly Type Selector;

            public readonly Func<List<SanitizedFieldInfo>, IEnumerable<SanitizedFieldInfo>> Filter;

            public Implementation(
                Type selector,
                Func<List<SanitizedFieldInfo>, IEnumerable<SanitizedFieldInfo>> filter)
            {
                Selector = selector;
                Filter = filter;
            }
        }

        enum Strategy
        {
            Includer,
            Excluder,
            Filter,
        }
    }
}