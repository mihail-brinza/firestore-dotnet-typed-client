using System.Linq.Expressions;
using System.Reflection;

using Google.Cloud.Firestore;

namespace Firestore.Typed.Client.Visitor;

public class FieldNameVisitor : ExpressionVisitor
{
    public string FieldName { get; private set; } = string.Empty;

    private const char FieldSeparator = '.';
    private static readonly Type FirestorePropertyAttribute = typeof(FirestorePropertyAttribute);
    private static readonly Type FirestorePropertyData = typeof(FirestorePropertyAttribute);

    protected override Expression VisitMember(MemberExpression node)
    {
        if (FieldName is not { Length: 0 })
        {
            AddToFieldName(FieldSeparator);
        }

        Attribute? propAttribute = node.Member
            .GetCustomAttributes()
            .FirstOrDefault(attribute => attribute.GetType() == FirestorePropertyAttribute);

        if (propAttribute is FirestorePropertyAttribute { Name.Length: > 0 } firestoreAttribute)
        {
            AddToFieldName(firestoreAttribute.Name);
            return base.VisitMember(node);
        }

        AddToFieldName(node.Member.Name);
        return base.VisitMember(node);
    }

    private void AddToFieldName(string field)
    {
        FieldName = field + FieldName;
    }

    private void AddToFieldName(char character)
    {
        FieldName = character + FieldName;
    }
}