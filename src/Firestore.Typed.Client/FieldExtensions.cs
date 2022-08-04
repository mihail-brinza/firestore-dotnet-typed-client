using System.Linq.Expressions;

using Firestore.Typed.Client.Visitor;

namespace Firestore.Typed.Client;

/// <summary>
/// Extensions on <see cref="Expression"/>
/// </summary>
internal static class FieldExtensions
{
    /// <summary>
    /// Processes the given <paramref name="field"/> and returns the FieldName
    /// </summary>
    /// <param name="field">A lambda that selects a field</param>
    /// <returns>A string containing the fieldName</returns>
    internal static string GetFieldName(this Expression field)
    {
        var fieldNameVisitor = new FieldNameVisitor();
        fieldNameVisitor.Visit(field);
        return fieldNameVisitor.FieldName;
    }
}