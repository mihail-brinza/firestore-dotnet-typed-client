using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Google.Cloud.Firestore;

namespace Firestore.Typed.Client
{
    /// <summary>
    ///     An immutable snapshot of complete query results.
    ///     <typeparam name="TDocument">The type of the documents in the snapshot</typeparam>
    /// </summary>
    public sealed class TypedQuerySnapshot<TDocument> : IReadOnlyList<TypedDocumentSnapshot<TDocument>>,
                                                        IEquatable<TypedQuerySnapshot<TDocument>>
    {
        private readonly Lazy<IReadOnlyList<TypedDocumentChange<TDocument>>> _lazyTypedChangeList;
        private readonly Lazy<IReadOnlyList<TypedDocumentSnapshot<TDocument>>> _lazyTypedDocuments;
        private QuerySnapshot Snapshot { get; }

        public TypedQuerySnapshot(QuerySnapshot snapshot, TypedQuery<TDocument> query)
        {
            Query = query;
            Snapshot = snapshot;
            _lazyTypedDocuments = BuildLazyTypedDocuments(snapshot);
            _lazyTypedChangeList = BuildLazyTypedChangeList(snapshot);
        }

        /// <summary>
        ///     The query producing this snapshot.
        /// </summary>
        public TypedQuery<TDocument> Query { get; }

        /// <summary>
        ///     The documents in the snapshot.
        /// </summary>
        public IReadOnlyList<TypedDocumentSnapshot<TDocument>> Documents => _lazyTypedDocuments.Value;

        /// <summary>
        ///     The changes in the documents.
        /// </summary>
        public IReadOnlyList<TypedDocumentChange<TDocument>> Changes => _lazyTypedChangeList.Value;

        /// <summary>
        ///     The time at which the snapshot was read.
        /// </summary>
        public Timestamp ReadTime => Snapshot.ReadTime;

        /// <summary>
        ///     Returns the number of documents in this query snapshot.
        /// </summary>
        /// <value>The number of documents in this query snapshot.</value>
        public int Count => Documents.Count;

        /// <summary>
        ///     Returns the document snapshot with the specified index within this query snapshot.
        /// </summary>
        /// <param name="index">The index of the document to return.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     <paramref name="index" /> is less than 0, or greater than or equal to
        ///     <see cref="Count" />.
        /// </exception>
        /// <returns>The document snapshot with the specified index within this query snapshot.</returns>
        public TypedDocumentSnapshot<TDocument> this[int index] => Documents[index];

        private static Lazy<IReadOnlyList<TypedDocumentChange<TDocument>>> BuildLazyTypedChangeList(
            QuerySnapshot snapshot)
        {
            return new Lazy<IReadOnlyList<TypedDocumentChange<TDocument>>>(
                () => snapshot.Changes.Select(change => new TypedDocumentChange<TDocument>(change)).ToList(),
                LazyThreadSafetyMode.ExecutionAndPublication
            );
        }

        private static Lazy<IReadOnlyList<TypedDocumentSnapshot<TDocument>>> BuildLazyTypedDocuments(
            QuerySnapshot snapshot)
        {
            return new Lazy<IReadOnlyList<TypedDocumentSnapshot<TDocument>>>(
                () => snapshot.Documents
                    .Select(documentSnap => new TypedDocumentSnapshot<TDocument>(documentSnap))
                    .ToList(), LazyThreadSafetyMode.ExecutionAndPublication);
        }

        /// <inheritdoc />
        public bool Equals(TypedQuerySnapshot<TDocument>? other)
        {
            return Snapshot.Equals(other?.Snapshot);
        }

        /// <inheritdoc />
        public IEnumerator<TypedDocumentSnapshot<TDocument>> GetEnumerator()
        {
            return Documents.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return Equals(obj as TypedQuerySnapshot<TDocument>);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Snapshot.GetHashCode();
        }

        /// <summary>
        /// Implicitly converts a typed object to an untyped object.
        /// </summary>
        public static implicit operator QuerySnapshot(TypedQuerySnapshot<TDocument> querySnapshot)
        {
            return querySnapshot.Snapshot;
        }
    }
}