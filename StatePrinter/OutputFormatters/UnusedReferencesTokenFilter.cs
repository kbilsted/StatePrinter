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