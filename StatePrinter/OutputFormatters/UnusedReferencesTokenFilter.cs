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
using System.Linq;
using StatePrinter.Introspection;


public class UnusedReferencesTokenFilter
{
  /// <summary>
  /// In order to reduce clutter in the output, only show reference in the output if the object 
  /// is referred to from other objects using a back-reference.
  /// </summary>
  public List<Token> FilterUnusedReferences(List<Token> tokens)
  {
    var backreferences = GetBackreferences(tokens);

    var remappedReferences = RemappedReferences(backreferences);

    var result = tokens
      .Select(x => new Token(
        x.Tokenkind,
        x.Field,
        x.Value,
        CreateNewReference(remappedReferences, x.ReferenceNo),
        x.FieldType))
      .ToList();

    return result;
  }

  public Reference[] GetBackreferences(List<Token> tokens)
  {
    return tokens
      .Where(x => x.Tokenkind == TokenType.SeenBeforeWithReference)
      .Select(x => x.ReferenceNo)
      .Distinct()
      .ToArray();
  }

  Dictionary<Reference, Reference> RemappedReferences(Reference[] backreferences)
  {
    var remappedReferences = new Dictionary<Reference, Reference>();
    int newReference = 0;
    foreach (var backreference in backreferences)
      remappedReferences[backreference] = new Reference(newReference++);

    return remappedReferences;
  }

  Reference CreateNewReference(Dictionary<Reference, Reference> remappedReferences, Reference currentReference)
  {
    if (currentReference == null)
      return null;

    Reference newReference = null;
    remappedReferences.TryGetValue(currentReference, out newReference);
    return newReference;
  }
}