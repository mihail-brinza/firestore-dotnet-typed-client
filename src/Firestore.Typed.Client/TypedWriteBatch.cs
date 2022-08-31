using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

using Firestore.Typed.Client.Extensions;

using Google.Api.Gax;
using Google.Cloud.Firestore;

namespace Firestore.Typed.Client
{
    public class TypedWriteBatch<TDocument>
    {
        private WriteBatch Batch { get; set; }

        public TypedWriteBatch(WriteBatch batch)
        {
            Batch = batch;
        }

        /// <summary>
        /// Adds a write operation which will create the specified document with a precondition
        /// that it doesn't exist already.
        /// </summary>
        /// <param name="documentReference">A document reference indicating the path of the document to create. Must not be null.</param>
        /// <param name="documentData">The data for the document. Must not be null.</param>
        /// <returns>This batch, for the purpose of method chaining</returns>
        public TypedWriteBatch<TDocument> Create(
            TypedDocumentReference<TDocument> documentReference,
            TDocument documentData)
        {
            Batch = Batch.Create(documentReference.UntypedReference, documentData);
            return this;
        }

        /// <summary>
        /// Adds a write operation that deletes the specified document, with an optional precondition.
        /// </summary>
        /// <param name="documentReference">A document reference indicating the path of the document to delete. Must not be null.</param>
        /// <param name="precondition">Optional precondition for deletion. May be null, in which case the deletion is unconditional.</param>
        /// <returns>This batch, for the purposes of method chaining.</returns>
        public TypedWriteBatch<TDocument> Delete(
            TypedDocumentReference<TDocument> documentReference,
            Precondition? precondition = null)
        {
            Batch = Batch.Delete(documentReference.UntypedReference, precondition);
            return this;
        }

        /// <summary>
        /// Adds an update operation that updates just the specified fields paths in the document, with the corresponding values.
        /// </summary>
        /// <param name="documentReference">A document reference indicating the path of the document to update. Must not be null.</param>
        /// <param name="updates">The updates to perform on the document. Must not be null or empty.</param>
        /// <param name="precondition">Optional precondition for updating the document. May be null, which is equivalent to <see cref="Precondition.MustExist"/>.</param>
        /// <returns>This batch, for the purposes of method chaining.</returns>
        public TypedWriteBatch<TDocument> Update(
            TypedDocumentReference<TDocument> documentReference,
            UpdateDefinition<TDocument> updates,
            Precondition? precondition = null)
        {
            Batch = Batch.Update(documentReference.UntypedReference, updates.UpdateValues, precondition);
            return this;
        }

        /// <summary>
        /// Adds an update operation that updates just the specified field in the document, with the corresponding values.
        /// </summary>
        /// <param name="documentReference">A document reference indicating the path of the document to update. Must not be null.</param>
        /// <param name="field">Lambda expression that allows to type safely select field</param>
        /// <param name="value">The new value for the field. May be null.</param>
        /// <param name="precondition">Optional precondition for updating the document. May be null, which is equivalent to <see cref="Precondition.MustExist"/>.</param>
        /// <returns>This batch, for the purposes of method chaining.</returns>
        public TypedWriteBatch<TDocument> Update<TField>(
            TypedDocumentReference<TDocument> documentReference,
            Expression<Func<TDocument, TField>> field,
            TField value,
            Precondition? precondition = null)
        {
            Batch = Batch.Update(documentReference.UntypedReference, field.GetFieldName(), value, precondition);
            return this;
        }

        /// <summary>
        /// Adds an operation that sets data in a document, either replacing it completely or merging fields.
        /// </summary>
        /// <param name="documentReference">A document reference indicating the path of the document to update. Must not be null.</param>
        /// <param name="documentData">The data to store in the document. Must not be null.</param>
        /// <param name="options">The options to use when setting data in the document. May be null, which is equivalent to <see cref="SetOptions.Overwrite"/>.</param>
        /// <returns>This batch, for the purposes of method chaining.</returns>
        public TypedWriteBatch<TDocument> Set(
            TypedDocumentReference<TDocument> documentReference,
            TDocument documentData,
            TypedSetOptions<TDocument>? options = null)
        {
            Batch = Batch.Set(documentReference.UntypedReference, documentData, options?.SetOptions);
            return this;
        }

        /// <summary>
        /// Commits the batch on the server.
        /// </summary>
        /// <returns>The write results from the commit.</returns>
        public Task<IList<WriteResult>> CommitAsync(CancellationToken cancellationToken = default)
        {
            return Batch.CommitAsync(cancellationToken);
        }
        
        /// <summary>
        /// Implicitly converts a typed object to an untyped object.
        /// </summary>
        public static implicit operator WriteBatch(TypedWriteBatch<TDocument> writeBatch)
        {
            return writeBatch.Batch;
        }
    }
}