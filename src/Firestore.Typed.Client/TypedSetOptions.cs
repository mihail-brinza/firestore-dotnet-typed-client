using System;
using System.Linq.Expressions;

using Firestore.Typed.Client.Extensions;

using Google.Cloud.Firestore;

namespace Firestore.Typed.Client
{
    public class TypedSetOptions<TDocument>
    {
        public static readonly TypedSetOptions<TDocument> Overwrite =
            new TypedSetOptions<TDocument>(SetOptions.Overwrite);

        public static readonly TypedSetOptions<TDocument>
            MergeAll = new TypedSetOptions<TDocument>(SetOptions.MergeAll);

        internal SetOptions SetOptions { get; }

        private TypedSetOptions(SetOptions fields)
        {
            this.SetOptions = fields;
        }

        public static TypedSetOptions<TDocument> MergeFields<TField>(
            params Expression<Func<TDocument, TField>>[] fields)
        {
            return new TypedSetOptions<TDocument>(SetOptions.MergeFields(fields.GetFieldNames()));
        }
    }
}