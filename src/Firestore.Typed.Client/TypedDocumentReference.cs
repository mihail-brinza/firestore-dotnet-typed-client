using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

using Firestore.Typed.Client.Extensions;

using Google.Cloud.Firestore;

using Grpc.Core;

namespace Firestore.Typed.Client
{
    /// <summary>
    ///     A reference to a document in a Firestore database. The existence of
    ///     this object does not imply that the document currently exists in storage.
    ///     This object is typed with TDocument, meaning that all the conversions and actions are typed with TDocument by
    ///     design
    ///     <typeparam name="TDocument">The type of the document</typeparam>
    /// </summary>
    public sealed class TypedDocumentReference<TDocument> : IEquatable<TypedDocumentReference<TDocument>>,
                                                            IComparable<TypedDocumentReference<TDocument>>
    {
        public DocumentReference Untyped { get; }

        public TypedDocumentReference(DocumentReference documentUntyped)
        {
            Untyped = documentUntyped;
        }

        /// <summary>
        ///     The final part of the complete document path; this is the identity of
        ///     the document relative to its parent collection.
        /// </summary>
        public string Id => Untyped.Id;

        /// <summary>
        ///     The complete document path, including project and database ID.
        /// </summary>
        public string Path => Untyped.Path;

        /// <summary>
        ///     The database which contains the document.
        /// </summary>
        public FirestoreDb Database => Untyped.Database;

        // Note: this implementation wastefully compares the characters in "projects" and "databases" but means we don't need
        // to keep a database-relative path or perform more complex comparisons.
        /// <inheritdoc />
        public int CompareTo(TypedDocumentReference<TDocument>? other)
        {
            return Untyped.CompareTo(other?.Untyped);
        }

        /// <inheritdoc />
        public bool Equals(TypedDocumentReference<TDocument>? other)
        {
            return Untyped.Equals(other?.Untyped);
        }


        /// <summary>
        ///     The parent collection. Never null.
        ///     <typeparam name="TParent">The type of the parent collection's items.</typeparam>
        /// </summary>
        public TypedCollectionReference<TParent> Parent<TParent>()
        {
            return new TypedCollectionReference<TParent>(Untyped.Parent);
        }

        /// <summary>
        ///     Creates a <see cref="TypedCollectionReference{TDocument}" /> for a child collection of this document.
        /// </summary>
        /// <param name="path">
        ///     The path to the collection, relative to this document. Must not be null, and must contain
        ///     an odd number of slash-separated path elements.
        /// </param>
        /// <returns>A <see cref="TypedCollectionReference{TDocument}" /> for the specified collection.</returns>
        public TypedCollectionReference<TChildDocument> Collection<TChildDocument>(string path)
        {
            return new TypedCollectionReference<TChildDocument>(Untyped.Collection(path));
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Untyped.ToString();
        }

        /// <summary>
        ///     Asynchronously fetches a snapshot of the document.
        /// </summary>
        /// <returns>A snapshot of the document. The snapshot may represent a missing document.</returns>
        public async Task<TypedDocumentSnapshot<TDocument>> GetSnapshotAsync(
            CancellationToken cancellationToken = default)
        {
            DocumentSnapshot snapshot =
                await Untyped.GetSnapshotAsync(cancellationToken).ConfigureAwait(false);
            return new TypedDocumentSnapshot<TDocument>(snapshot, this);
        }

        public TypedCollectionReference<TField> Collection<TField>(Expression<Func<TDocument, TField>> field)
        {
            return new TypedCollectionReference<TField>(Untyped.Collection(field.GetFieldName()));
        }

        /// <summary>
        ///     Asynchronously creates a document on the server with the given data. The document must not exist beforehand.
        /// </summary>
        /// <param name="documentData">The data for the document. Must not be null.</param>
        /// <param name="cancellationToken">A cancellation token to monitor for the asynchronous operation.</param>
        /// <returns>The write result of the server operation.</returns>
        public Task<WriteResult> CreateAsync(
            TDocument documentData,
            CancellationToken cancellationToken = default)
        {
            return Untyped.CreateAsync(documentData, cancellationToken);
        }


        /// <summary>
        ///     Asynchronously deletes the document referred to by this path, with an optional precondition.
        /// </summary>
        /// <remarks>
        ///     If no precondition is specified and the document doesn't exist, this returned task will succeed. If a precondition
        ///     is specified and not met, the returned task will fail with an <see cref="RpcException" />.
        /// </remarks>
        /// <param name="precondition">
        ///     Optional precondition for deletion. May be null, in which case the deletion is
        ///     unconditional.
        /// </param>
        /// <param name="cancellationToken">A cancellation token to monitor for the asynchronous operation.</param>
        /// <returns>The write result of the server operation.</returns>
        public Task<WriteResult> DeleteAsync(
            Precondition? precondition = null,
            CancellationToken cancellationToken = default)
        {
            return Untyped.DeleteAsync(precondition, cancellationToken);
        }

        /// <summary>
        ///     Asynchronously performs a single field update on the document referred to by this path, with an optional
        ///     precondition.
        /// </summary>
        /// <param name="field">A lambda expression from which the field name will be calculated</param>
        /// <param name="value">The new value for the field. May be null.</param>
        /// <param name="precondition">
        ///     Optional precondition for updating the document. May be null, which is equivalent to
        ///     <see cref="Precondition.MustExist" />.
        /// </param>
        /// <param name="cancellationToken">A cancellation token to monitor for the asynchronous operation.</param>
        /// <returns>The write result of the server operation.</returns>
        public Task<WriteResult> UpdateAsync<TField>(
            Expression<Func<TDocument, TField>> field,
            TField value,
            Precondition? precondition = null,
            CancellationToken cancellationToken = default)
        {
            return Untyped.UpdateAsync(field.GetFieldName(), value, precondition, cancellationToken);
        }

        /// <summary>
        ///     Asynchronously performs a set of updates on the document referred to by this path, with an optional precondition.
        /// </summary>
        /// <param name="updateDefinition">
        ///     <see cref="UpdateDefinition{TDocument}" /> A builder that allows to select which field
        ///     to update in a type safe manner. Must not be null or empty.
        /// </param>
        /// <param name="precondition">
        ///     Optional precondition for updating the document. May be null, which is equivalent to
        ///     <see cref="Precondition.MustExist" />.
        /// </param>
        /// <param name="cancellationToken">A cancellation token to monitor for the asynchronous operation.</param>
        /// <returns>The write result of the server operation.</returns>
        public Task<WriteResult> UpdateAsync(
            UpdateDefinition<TDocument> updateDefinition,
            Precondition? precondition = null,
            CancellationToken cancellationToken = default)
        {
            return Untyped.UpdateAsync(updateDefinition.UpdateValues, precondition, cancellationToken);
        }


        /// <summary>
        ///     Asynchronously sets data in the document, either replacing it completely or merging fields.
        /// </summary>
        /// <param name="documentData">The data to store in the document. Must not be null.</param>
        /// <param name="options">
        ///     The options to use when updating the document. May be null, which is equivalent to
        ///     <see cref="SetOptions.Overwrite" />.
        /// </param>
        /// <param name="cancellationToken">A cancellation token to monitor for the asynchronous operation.</param>
        /// <returns>The write result of the server operation.</returns>
        public Task<WriteResult> SetAsync(
            object documentData,
            SetOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            return Untyped.SetAsync(documentData, options, cancellationToken);
        }

        /// <summary>
        ///     Asynchronously sets data in the document, either replacing it completely or merging fields.
        /// </summary>
        /// <param name="documentData">The data to store in the document. Must not be null.</param>
        /// <param name="options">
        ///     The options to use when updating the document. May be null, which is equivalent to
        ///     <see cref="SetOptions.Overwrite" />.
        /// </param>
        /// <param name="cancellationToken">A cancellation token to monitor for the asynchronous operation.</param>
        /// <returns>The write result of the server operation.</returns>
        public Task<WriteResult> SetAsync(
            TDocument documentData,
            TypedSetOptions<TDocument>? options = null,
            CancellationToken cancellationToken = default)
        {
            return Untyped.SetAsync(documentData, options?.SetOptions, cancellationToken);
        }

        /// <summary>
        ///     Watch this document for changes.
        /// </summary>
        /// <param name="callback">The callback to invoke each time the document changes. Must not be null.</param>
        /// <param name="cancellationToken">Optional cancellation token which may be used to cancel the listening operation.</param>
        /// <returns>
        ///     A <see cref="FirestoreChangeListener" /> which may be used to monitor the listening operation and stop it
        ///     gracefully.
        /// </returns>
        public FirestoreChangeListener Listen(
            Func<TypedDocumentSnapshot<TDocument>, CancellationToken, Task> callback,
            CancellationToken cancellationToken = default)
        {
            return Untyped.Listen(
                (snapshot, token) => callback(new TypedDocumentSnapshot<TDocument>(snapshot, this), token),
                cancellationToken);
        }


        /// <summary>
        ///     Watch this document for changes. This method is a convenience method over
        ///     <see cref="Listen(Func{TypedDocumentSnapshot{TDocument}, CancellationToken, Task}, CancellationToken)" />,
        ///     wrapping a synchronous callback to create an asynchronous one.
        /// </summary>
        /// <param name="callback">The callback to invoke each time the query results change. Must not be null.</param>
        /// <param name="cancellationToken">Optional cancellation token which may be used to cancel the listening operation.</param>
        /// <returns>
        ///     A <see cref="FirestoreChangeListener" /> which may be used to monitor the listening operation and stop it
        ///     gracefully.
        /// </returns>
        public FirestoreChangeListener Listen(
            Action<TypedDocumentSnapshot<TDocument>> callback,
            CancellationToken cancellationToken = default)
        {
            return Untyped.Listen(
                snapshot => callback(new TypedDocumentSnapshot<TDocument>(snapshot, this)),
                cancellationToken);
        }


        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return Equals(obj as TypedDocumentReference<TDocument>);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Untyped.GetHashCode();
        }


        /// <summary>
        /// Implicitly converts a typed object to an untyped object.
        /// </summary>
        public static implicit operator DocumentReference(TypedDocumentReference<TDocument> documentReference)
        {
            return documentReference.Untyped;
        }

        /// <summary>
        /// Implicitly converts an untyped object to a typed object.
        /// </summary>
        /// <returns></returns>
        public static implicit operator TypedDocumentReference<TDocument>(DocumentReference documentReference)
        {
            return new TypedDocumentReference<TDocument>(documentReference);
        }
    }
}