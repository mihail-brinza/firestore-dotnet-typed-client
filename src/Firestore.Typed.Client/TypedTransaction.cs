using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

using Firestore.Typed.Client.Extensions;

using Google.Cloud.Firestore;

namespace Firestore.Typed.Client
{
    public class TypedTransaction<TDocument>
    {
        private readonly Transaction _transaction;

        public TypedTransaction(Transaction transaction)
        {
            _transaction = transaction;
        }

        /// <summary>
        /// The cancellation token for this transaction
        /// </summary>
        public CancellationToken CancellationToken => _transaction.CancellationToken;

        /// <summary>
        /// The database for this transaction.
        /// </summary>
        public FirestoreDb Database => _transaction.Database;

        /// <summary>
        /// Fetch a snapshot of the document specified by <paramref name="documentReference"/>, with respect to this transaction.
        /// This method cannot be called after any write operations have been created.
        /// </summary>
        /// <param name="documentReference">The document reference to fetch. Must not be null.</param>
        /// <param name="cancellationToken">A cancellation token to monitor for the asynchronous operation.</param>
        /// <returns>A snapshot of the given document with respect to this transaction.</returns>
        public async Task<TypedDocumentSnapshot<TDocument>> GetSnapshotAsync(
            TypedDocumentReference<TDocument> documentReference,
            CancellationToken cancellationToken = default)
        {
            DocumentSnapshot? snapshot = await _transaction
                .GetSnapshotAsync(documentReference.Reference, cancellationToken)
                .ConfigureAwait(false);

            return new TypedDocumentSnapshot<TDocument>(snapshot);
        }


        /// <summary>
        /// Fetch snapshots of all the documents specified by <paramref name="documentReferences"/>, with respect to this transaction.
        /// This method cannot be called after any write operations have been created.
        /// </summary>
        /// <remarks>
        /// Any documents which are missing are represented in the returned list by a <see cref="DocumentSnapshot"/>
        /// with <see cref="DocumentSnapshot.Exists"/> value of <c>false</c>.
        /// </remarks>
        /// <param name="documentReferences">The document references to fetch. Must not be null, or contain null references.</param>
        /// <param name="cancellationToken">A cancellation token to monitor for the asynchronous operation.</param>
        /// <returns>The document snapshots, in the same order as <paramref name="documentReferences"/>.</returns>
        public Task<IList<TypedDocumentSnapshot<TDocument>>> GetAllSnapshotsAsync(
            IEnumerable<TypedDocumentReference<TDocument>> documentReferences,
            CancellationToken cancellationToken = default) =>
            GetAllSnapshotsAsync(documentReferences, fieldMask: null, cancellationToken);

        /// <summary>
        /// Fetch snapshots of all the documents specified by <paramref name="documentReferences"/>, with respect to this transaction,
        /// potentially limiting the fields returned.
        /// This method cannot be called after any write operations have been created.
        /// </summary>
        /// <remarks>
        /// Any documents which are missing are represented in the returned list by a <see cref="DocumentSnapshot"/>
        /// with <see cref="DocumentSnapshot.Exists"/> value of <c>false</c>.
        /// </remarks>
        /// <param name="documentReferences">The document references to fetch. Must not be null, or contain null references.</param>
        /// <param name="fieldMask">The field mask to use to restrict which fields are retrieved. May be null, in which
        /// case no field mask is applied, and the complete documents are retrieved.</param>
        /// <param name="cancellationToken">A cancellation token to monitor for the asynchronous operation.</param>
        /// <returns>The document snapshots, in the same order as <paramref name="documentReferences"/>.</returns>
        public async Task<IList<TypedDocumentSnapshot<TDocument>>> GetAllSnapshotsAsync(
            IEnumerable<TypedDocumentReference<TDocument>> documentReferences,
            FieldMask? fieldMask,
            CancellationToken cancellationToken = default)
        {
            IEnumerable<DocumentReference> documents = documentReferences
                .Select(doc => doc.Reference)
                .ToList();

            IList<DocumentSnapshot> snapshotList = await _transaction
                .GetAllSnapshotsAsync(documents, fieldMask, cancellationToken)
                .ConfigureAwait(false);

            return snapshotList.Select(snapshot => new TypedDocumentSnapshot<TDocument>(snapshot)).ToList();
        }

        /// <summary>
        /// Performs a query and returned a snapshot of the the results, with respect to this transaction.
        /// This method cannot be called after any write operations have been created.
        /// </summary>
        /// <param name="query">The query to execute. Must not be null.</param>
        /// <param name="cancellationToken">A cancellation token to monitor for the asynchronous operation.</param>
        /// <returns>A snapshot of results of the given query with respect to this transaction.</returns>
        public async Task<TypedQuerySnapshot<TDocument>> GetSnapshotAsync(
            TypedQuery<TDocument> query,
            CancellationToken cancellationToken = default)
        {
            QuerySnapshot? snapshot = await _transaction
                .GetSnapshotAsync(query.Query, cancellationToken)
                .ConfigureAwait(false);

            return new TypedQuerySnapshot<TDocument>(snapshot, query);
        }


        /// <summary>
        /// Adds an operation to create a document in this transaction.
        /// </summary>
        /// <param name="documentReference">The document reference to create. Must not be null.</param>
        /// <param name="documentData">The data for the document. Must not be null.</param>
        public void Create(TypedDocumentReference<TDocument> documentReference, TDocument documentData)
        {
            _transaction.Create(documentReference.Reference, documentData);
        }

        /// <summary>
        /// Adds an operation to set a document's data in this transaction.
        /// </summary>
        /// <param name="documentReference">The document in which to set the data. Must not be null.</param>
        /// <param name="documentData">The data for the document. Must not be null.</param>
        /// <param name="options">The options to use when updating the document. May be null, which is equivalent to <see cref="SetOptions.Overwrite"/>.</param>
        public void Set(
            TypedDocumentReference<TDocument> documentReference,
            TDocument documentData,
            SetOptions? options = null)
        {
            _transaction.Set(documentReference.Reference, documentData, options);
        }

        /// <summary>
        /// Adds an operation to update a document's data in this transaction.
        /// </summary>
        /// <param name="documentReference">A document reference indicating the path of the document to update. Must not be null.</param>
        /// <param name="updates">The updates to perform on the document, keyed by the dot-separated field path to update. Fields not present in this dictionary are not updated. Must not be null or empty.</param>
        /// <param name="precondition">Optional precondition for updating the document. May be null, which is equivalent to <see cref="Precondition.MustExist"/>.</param>
        public void Update(
            TypedDocumentReference<TDocument> documentReference,
            IDictionary<string, object> updates,
            Precondition? precondition = null)
        {
            _transaction.Update(documentReference.Reference, updates, precondition);
        }

        /// <summary>
        /// Adds an operation to update a document's data in this transaction.
        /// </summary>
        /// <param name="documentReference">A document reference indicating the path of the document to update. Must not be null.</param>
        /// <param name="field">The dot-separated name of the field to update. Must not be null.</param>
        /// <param name="value">The new value for the field. May be null.</param>
        /// <param name="precondition">Optional precondition for updating the document. May be null, which is equivalent to <see cref="Precondition.MustExist"/>.</param>
        public void Update<TField>(
            TypedDocumentReference<TDocument> documentReference,
            Expression<Func<TDocument, TField>> field,
            TDocument value,
            Precondition? precondition = null)
        {
            _transaction.Update(documentReference.Reference, field.GetFieldName(), value, precondition);
        }

        /// <summary>
        /// Adds an operation to update a document's data in this transaction.
        /// </summary>
        /// <param name="documentReference">The document to update. Must not be null.</param>
        /// <param name="updates">The updates to perform on the document, keyed by the field path to update. Fields not present in this dictionary are not updated. Must not be null or empty.</param>
        /// <param name="precondition">Optional precondition for updating the document. May be null, which is equivalent to <see cref="Precondition.MustExist"/>.</param>
        public void Update(
            TypedDocumentReference<TDocument> documentReference,
            UpdateDefinition<TDocument> updates,
            Precondition? precondition = null)
        {
            _transaction.Update(documentReference.Reference, updates.UpdateValues, precondition);
        }

        /// <summary>
        /// Adds an operation to delete a document's data in this transaction.
        /// </summary>
        /// <param name="documentReference">The document to delete. Must not be null.</param>
        /// <param name="precondition">Optional precondition for deletion. May be null, in which case the deletion is unconditional.</param>
        public void Delete(TypedDocumentReference<TDocument> documentReference, Precondition? precondition = null)
        {
            _transaction.Delete(documentReference.Reference, precondition);
        }
    }
}