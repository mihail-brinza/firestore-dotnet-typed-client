using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Firestore.Typed.Client
{
    /// <summary>
    ///     Representation of an update chain, this object allows to chain multiple calls to construct an update command
    /// </summary>
    /// <typeparam name="TDocument">Type of the document to be updated</typeparam>
    public sealed class UpdateDefinition<TDocument>
    {
        /// <summary>
        ///     Dictionary that holds the untyped values to be updated
        /// </summary>
        internal IDictionary<string, object?> UpdateValues { get; } = new Dictionary<string, object?>();

        /// <summary>
        ///     Defines a new field to be set using a typed lambda expression
        /// </summary>
        /// <param name="field">Lambda expression that allows to type safely select field</param>
        /// <param name="value">New value to assign to the selected property</param>
        /// <typeparam name="TField">Type of the field</typeparam>
        /// <returns></returns>
        public UpdateDefinition<TDocument> Set<TField>(Expression<Func<TDocument, TField>> field, TField value)
        {
            string fieldName = field.GetFieldName();
            if (UpdateValues.TryAdd(fieldName, value))
            {
                return this;
            }

            UpdateValues[fieldName] = value;
            return this;
        }
    }
}