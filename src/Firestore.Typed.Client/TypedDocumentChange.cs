using System;

using Google.Cloud.Firestore;

namespace Firestore.Typed.Client
{
    /// <summary>
    ///     A DocumentChange represents a change to the documents matching a query. It contains the document
    ///     affected and a the type of change that occurred (added, modified, or removed).
    ///     <typeparam name="TDocument">The type of the changed document</typeparam>
    /// </summary>
    public sealed class TypedDocumentChange<TDocument> : IEquatable<TypedDocumentChange<TDocument>>
    {
        public DocumentChange Untyped { get; }

        public TypedDocumentChange(DocumentChange documentUntyped)
        {
            Untyped = documentUntyped;
            Document = new TypedDocumentSnapshot<TDocument>(documentUntyped.Document);
        }

        /// <summary>
        ///     The newly added or modified document, or the document that was deleted.
        /// </summary>
        public TypedDocumentSnapshot<TDocument> Document { get; }

        /// <summary>
        ///     The type of change that was observed.
        /// </summary>
        public DocumentChange.Type ChangeType => Untyped.ChangeType;

        /// <summary>
        ///     The index of the changed document in the result set immediately prior to this DocumentChange
        ///     (i.e. supposing that all prior DocumentChange objects have been applied), or null
        ///     if the change type is <see cref="DocumentChange.Type.Added" />. The index will never be negative.
        /// </summary>
        public int? OldIndex => Untyped.OldIndex;

        /// <summary>
        ///     The index of the changed document in the result set immediately after this DocumentChange
        ///     (i.e. supposing that all prior DocumentChange objects and this one have been applied),
        ///     null if the change type is <see cref="DocumentChange.Type.Removed" />. The index will never be negative.
        /// </summary>
        public int? NewIndex => Untyped.NewIndex;


        /// <summary>
        ///     Compares this snapshot with another for equality. Only the document data and document reference
        ///     are considered; the timestamps are ignored.
        /// </summary>
        /// <param name="other">The snapshot to compare this one with</param>
        /// <returns><c>true</c> if this snapshot is equal to <paramref name="other" />; <c>false</c> otherwise.</returns>
        public bool Equals(TypedDocumentChange<TDocument>? other)
        {
            return Untyped.Equals(other?.Untyped);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return Equals(obj as TypedDocumentChange<TDocument>);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Untyped.GetHashCode();
        }

        /// <summary>
        /// Implicitly converts a typed object to an untyped object.
        /// </summary>
        public static implicit operator DocumentChange(TypedDocumentChange<TDocument> documentChange)
        {
            return documentChange.Untyped;
        }
    }
}