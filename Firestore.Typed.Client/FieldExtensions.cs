using System.Linq.Expressions;

using Firestore.Typed.Client.Visitor;

namespace Firestore.Typed.Client;

public static class FieldExtensions
{
    internal static string GetField(this Expression field)
    {
        var fieldNameVisitor = new FieldNameVisitor();
        fieldNameVisitor.Visit(field);
        return fieldNameVisitor.FieldName;
    }
}