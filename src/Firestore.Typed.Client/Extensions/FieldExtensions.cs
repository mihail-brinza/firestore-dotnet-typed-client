using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using Firestore.Typed.Client.Visitor;

namespace Firestore.Typed.Client.Extensions
{
    /// <summary>
    ///     Extensions on <see cref="Expression" />
    /// </summary>
    internal static class FieldExtensions
    {
        /// <summary>
        ///     Processes the given <paramref name="field" /> and returns the FieldName
        /// </summary>
        /// <param name="field">A lambda that selects a field</param>
        /// <returns>A string containing the fieldName</returns>
        internal static string GetFieldName(this Expression field)
        {
            var fieldNameVisitor = new FieldNameVisitor();
            fieldNameVisitor.Visit(field);
            return fieldNameVisitor.FieldName;
        }

        /// <summary>
        ///     Processes the given <paramref name="fields" /> and returns an array with the field names
        /// </summary>
        /// <param name="fields">An array of lambda expressions that select the fields</param>
        /// <returns>An array containing the field names</returns>
        internal static string[] GetFieldNames(this IEnumerable<Expression> fields)
        {
            return fields.Select(field => field.GetFieldName()).ToArray();
        }
    }
}