using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Google.Cloud.Firestore;

namespace Firestore.Typed.Client
{
    /// <summary>
    ///     A reference to a collection in a Firestore database. The existence of
    ///     this object does not imply that the collection currently exists in storage.
    ///     This object is also typed, so that all actions are based on the specified type.
    ///     <typeparam name="TDocument">The type of the elements in the collection</typeparam>
    /// </summary>
    public sealed class TypedCollectionReference<TDocument> : TypedQuery<TDocument>,
                                                              IEquatable<TypedCollectionReference<TDocument>>,
                                                              IComparable<TypedCollectionReference<TDocument>>
    {
        public CollectionReference Untyped { get; }

        internal TypedCollectionReference(CollectionReference untyped) : base(untyped)
        {
            Untyped = untyped;
        }

        /// <summary>
        ///     The final part of the complete collection path; this is the identity of
        ///     the collection relative to its parent document.
        /// </summary>
        public string Id => Untyped.Id;

        /// <summary>
        ///     The complete collection path, including project and database ID.
        /// </summary>
        public string Path => Untyped.Path;

        /// <summary>
        ///     The parent document, or null if this is a root collection.
        /// </summary>
        public override FirestoreDb Database => Untyped.Database;


        /// <summary>
        ///     The parent document, typed with TParentCollection, or null if this is a root collection.
        ///     <typeparam name="TParentCollection">The type of the parent collection's items.</typeparam>
        /// </summary>
        internal TypedDocumentReference<TParentCollection>? Parent<TParentCollection>()
        {
            if (Untyped.Parent is null)
            {
                return null;
            }

            return new TypedDocumentReference<TParentCollection>(Untyped.Parent);
        }

        /// <summary>
        ///     Creates a <see cref="TypedDocumentReference{TDocument}" /> for a direct child document of this collection with a
        ///     random ID.
        ///     This performs no server-side operations; it only generates the appropriate <c>DocumentReference</c>.
        /// </summary>
        /// <returns>A <see cref="TypedDocumentReference{TDocument}" /> to a child document of this collection with a random ID.</returns>
        public TypedDocumentReference<TDocument> Document()
        {
            return new TypedDocumentReference<TDocument>(Untyped.Document());
        }


        /// <summary>
        ///     Creates a <see cref="TypedDocumentReference{TDocument}" /> for a child document of this reference.
        /// </summary>
        /// <param name="path">
        ///     The path to the document, relative to this collection. Must not be null, and must contain
        ///     an odd number of slash-separated path elements.
        /// </param>
        /// <returns>A <see cref="TypedDocumentReference{TDocument}" /> for the specified document.</returns>
        public TypedDocumentReference<TDocument> Document(string path)
        {
            return new TypedDocumentReference<TDocument>(Untyped.Document(path));
        }


        /// <summary>
        ///     Asynchronously creates a document with the given data in this collection. The document has a randomly generated ID.
        /// </summary>
        /// <remarks>
        ///     If the <see cref="WriteResult" /> for the operation is required,
        ///     use <see cref="TypedDocumentReference{TDocument}.CreateAsync(TDocument, CancellationToken)" />
        ///     instead of this method.
        /// </remarks>
        /// <param name="documentData">The data for the document. Must not be null.</param>
        /// <param name="cancellationToken">A cancellation token to monitor for the asynchronous operation.</param>
        /// <returns>The reference for the newly-created document typed with the type of the collection.</returns>
        public async Task<TypedDocumentReference<TDocument>> AddAsync(
            TDocument documentData,
            CancellationToken cancellationToken = default)
        {
            DocumentReference document =
                await Untyped.AddAsync(documentData, cancellationToken).ConfigureAwait(false);
            return new TypedDocumentReference<TDocument>(document);
        }


        /// <summary>
        ///     Lists the documents in this collection. The results include documents which don't exist in their own right, but
        ///     which have nested documents which do exist.
        /// </summary>
        /// <returns>A lazily-iterated sequence of document references within this collection.</returns>
        public async IAsyncEnumerable<TypedDocumentReference<TDocument>> ListDocumentsAsync()
        {
            await foreach (DocumentReference? documentRef in Untyped.ListDocumentsAsync()
                .ConfigureAwait(false))
            {
                yield return new TypedDocumentReference<TDocument>(documentRef);
            }
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return Equals(obj as TypedCollectionReference<TDocument>);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Untyped.GetHashCode();
        }

        /// <inheritdoc />
        public int CompareTo(TypedCollectionReference<TDocument>? other)
        {
            return Untyped.CompareTo(other?.Untyped);
        }

        /// <inheritdoc />
        public bool Equals(TypedCollectionReference<TDocument>? other)
        {
            return Untyped.Equals(other?.Untyped);
        }

        /// <summary>
        /// Implicitly converts a typed object to an untyped object.
        /// </summary>
        /// <param name="collectionReference"></param>
        /// <returns></returns>
        public static implicit operator CollectionReference(TypedCollectionReference<TDocument> collectionReference)
        {
            return collectionReference.Untyped;
        }

        /// <summary>
        /// Implicitly converts an untyped object to a typed object.
        /// </summary>
        /// <param name="collectionReference"></param>
        /// <returns></returns>
        public static implicit operator TypedCollectionReference<TDocument>(CollectionReference collectionReference)
        {
            return new TypedCollectionReference<TDocument>(collectionReference);
        }
    }
}