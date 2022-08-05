using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using Google.Cloud.Firestore;

namespace Firestore.Typed.Client.Visitor
{
    public class FieldNameVisitor : ExpressionVisitor
    {
        private const char FieldSeparator = '.';
        private static readonly Type FirestorePropertyAttribute = typeof(FirestorePropertyAttribute);
        private static readonly Type FirestorePropertyData = typeof(FirestorePropertyAttribute);
        public string FieldName { get; private set; } = string.Empty;

        protected override Expression VisitMember(MemberExpression node)
        {
            if (!string.IsNullOrEmpty(FieldName))
            {
                AddToFieldName(FieldSeparator);
            }

            Attribute? propAttribute = node.Member
                .GetCustomAttributes()
                .FirstOrDefault(attribute => attribute.GetType() == FirestorePropertyAttribute);

            if (propAttribute is FirestorePropertyAttribute firestoreAttribute
             && !string.IsNullOrEmpty(firestoreAttribute.Name))
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
}