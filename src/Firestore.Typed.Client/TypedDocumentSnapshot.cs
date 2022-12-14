using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

using Firestore.Typed.Client.Exceptions;
using Firestore.Typed.Client.Extensions;

using Google.Cloud.Firestore;

namespace Firestore.Typed.Client
{
    /// <summary>
    ///     An immutable snapshot of the data for a document.
    ///     <typeparam name="TDocument">The type of the elements in the collection</typeparam>
    /// </summary>
    public sealed class TypedDocumentSnapshot<TDocument> : IEquatable<TypedDocumentSnapshot<TDocument>>
    {
        public TypedDocumentSnapshot(DocumentSnapshot untyped, TypedDocumentReference<TDocument> reference)
        {
            Untyped = untyped;
            Reference = reference;
        }

        public TypedDocumentSnapshot(DocumentSnapshot untyped) :
            this(untyped, new TypedDocumentReference<TDocument>(untyped.Reference))
        {
        }

        public DocumentSnapshot Untyped { get; }

        /// <summary>
        ///     The ID of the document.
        /// </summary>
        public string Id => Untyped.Id;

        /// <summary>
        ///     The database that owns the document.
        /// </summary>
        public FirestoreDb Database => Untyped.Database;

        /// <summary>
        ///     The data converted to the specified type. Returns null if object does not exist.
        ///     For each invocation returns a new instance, just like <see cref="DocumentSnapshot.ConvertTo{T}"/>
        /// </summary>
        public TDocument Object => Untyped.ConvertTo<TDocument>();

        /// <summary>
        ///     The data converted to the specified type.
        ///     <exception cref="DocumentNotFoundException">The document does not exist.</exception>
        /// </summary>
        public TDocument RequiredObject => Untyped.Exists
            ? Object
            : throw new DocumentNotFoundException(Untyped.Id);

        /// <summary>
        ///     The full reference to the document.
        /// </summary>
        public TypedDocumentReference<TDocument> Reference { get; }

        /// <summary>
        ///     Whether or not the document exists.
        /// </summary>
        public bool Exists => Untyped.Exists;

        /// <summary>
        ///     The creation time of the document if it exists, or null otherwise.
        /// </summary>
        public Timestamp? CreateTime => Untyped.CreateTime;

        /// <summary>
        ///     The update time of the document if it exists, or null otherwise.
        /// </summary>
        public Timestamp? UpdateTime => Untyped.UpdateTime;

        /// <summary>
        ///     The time at which this snapshot was read.
        /// </summary>
        public Timestamp ReadTime => Untyped.ReadTime;

        /// <summary>
        ///     Compares this snapshot with another for equality. Only the document data and document reference
        ///     are considered; the timestamps are ignored.
        /// </summary>
        /// <param name="other">The snapshot to compare this one with</param>
        /// <returns><c>true</c> if this snapshot is equal to <paramref name="other" />; <c>false</c> otherwise.</returns>
        public bool Equals(TypedDocumentSnapshot<TDocument>? other)
        {
            return Untyped.Equals(other?.Untyped);
        }

        public Dictionary<string, object> ToDictionary()
        {
            return Untyped.ToDictionary();
        }


        /// <summary>
        ///     Fetches a field value from the document, throwing an exception if the field does not exist.
        /// </summary>
        /// <param name="field">Lambda expression that allows to type safely select field</param>
        /// <exception cref="InvalidOperationException">The field does not exist in the document data.</exception>
        /// <returns>The deserialized value.</returns>
        public TField GetValue<TField>(Expression<Func<TDocument, TField>> field)
        {
            return Untyped.GetValue<TField>(field.GetFieldName());
        }


        /// <summary>
        ///     Attempts to fetch the given field path from the document, returning whether or not it was found, and deserializing
        ///     it if it was found.
        /// </summary>
        /// <remarks>
        ///     This method does not throw an exception if the field is not found, but does throw an exception if the field was
        ///     found
        ///     but cannot be deserialized.
        /// </remarks>
        /// <typeparam name="TField">The type to deserialize the value to, if it is found.</typeparam>
        /// <param name="field">Lambda expression that allows to type safely select field</param>
        /// <param name="value">
        ///     When this method returns, contains the deserialized value if the field was found, or the default value
        ///     of <typeparamref name="TField" /> otherwise.
        /// </param>
        /// <returns>true if the field was found; false otherwise.</returns>
        public bool TryGetValue<TField>(Expression<Func<TDocument, TField>> field, [NotNullWhen(true)] out TField value)
        {
            return Untyped.TryGetValue(field.GetFieldName(), out value);
        }

        /// <summary>
        ///     Determines whether or not the given field path is present in the document. If this snapshot represents
        ///     a missing document, this method will always return false.
        /// </summary>
        /// <param name="field">Lambda expression that allows to type safely select field</param>
        /// <returns>true if the specified path represents a field in the document; false otherwise</returns>
        public bool ContainsField<TField>(Expression<Func<TDocument, TField>> field)
        {
            return Untyped.ContainsField(field.GetFieldName());
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return Equals(obj as TypedDocumentSnapshot<TDocument>);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Untyped.GetHashCode();
        }

        /// <summary>
        /// Implicitly converts a typed object to an untyped object.
        /// </summary>
        public static implicit operator DocumentSnapshot(TypedDocumentSnapshot<TDocument> documentSnapshot)
        {
            return documentSnapshot.Untyped;
        }

        /// <summary>
        /// Implicitly converts an untyped object to a typed object.
        /// </summary>
        /// <returns></returns>
        public static implicit operator TypedDocumentSnapshot<TDocument>(DocumentSnapshot documentSnapshot)
        {
            return new TypedDocumentSnapshot<TDocument>(documentSnapshot);
        }
    }
}