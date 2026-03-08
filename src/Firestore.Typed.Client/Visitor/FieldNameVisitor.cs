using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using Google.Cloud.Firestore;

namespace Firestore.Typed.Client.Visitor
{
    public class FieldNameVisitor : ExpressionVisitor
    {
        private readonly List<string> _segments = new();

        public string FieldName => string.Join(".", Enumerable.Reverse(_segments));

        protected override Expression VisitMember(MemberExpression node)
        {
            FirestorePropertyAttribute? firestoreAttribute = node.Member.GetCustomAttribute<FirestorePropertyAttribute>();

            if (firestoreAttribute is not null && !string.IsNullOrEmpty(firestoreAttribute.Name))
            {
                _segments.Add(firestoreAttribute.Name);
            }
            else
            {
                _segments.Add(node.Member.Name);
            }

            return base.VisitMember(node);
        }
    }
}