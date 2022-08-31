using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using Firestore.Typed.Client.Extensions;

using Google.Cloud.Firestore;

namespace Firestore.Typed.Client
{
    /// <summary>
    ///     A query against a collection.
    /// </summary>
    /// <remarks>
    ///     <see cref="TypedCollectionReference{TDocument}" /> derives from this class as a "return-all" query against the
    ///     collection it refers to.
    /// </remarks>
    /// <typeparam name="TDocument">The type of the elements in the collection to query</typeparam>
    public class TypedQuery<TDocument> : IEquatable<TypedQuery<TDocument>>
    {
        internal Query Query { get; }

        internal TypedQuery(Query query)
        {
            Query = query;
        }

        /// <summary>
        ///     The database this query will search over.
        /// </summary>
        public virtual FirestoreDb Database => Query.Database;


        // Note: these methods should be equivalent to producing the proto representations and checking those for
        // equality, but that would be expensive.

        /// <summary>
        ///     Compares this query with another for equality. Every aspect of the query must be equal,
        ///     including the collection. A plain Query instance is not equal to a CollectionReference instance,
        ///     even if they are logically similar: <c>collection.Offset(0).Equals(collection)</c> will return
        ///     <c>false</c>, even though 0 is the default offset.
        /// </summary>
        /// <param name="other">The query to compare this one with</param>
        /// <returns><c>true</c> if this query is equal to <paramref name="other" />; <c>false</c> otherwise.</returns>
        public bool Equals(TypedQuery<TDocument>? other)
        {
            return Query.Equals(other?.Query);
        }

        /// <summary>
        ///     Specifies the field paths to return in the results.
        /// </summary>
        /// <remarks>
        ///     This call replaces any previously-specified projections in the query.
        /// </remarks>
        /// <param name="fields">Array of lambda expressions that type safely select the property.</param>
        /// <returns>A new query based on the current one, but with the specified projection applied.</returns>
        public TypedQuery<TDocument> Select(params Expression<Func<TDocument, object>>[] fields)
        {
            return new TypedQuery<TDocument>(Query.Select(fields.GetFieldNames()));
        }

        public async Task<TypedQuerySnapshot<TDocument>> GetSnapshotAsync(CancellationToken cancellationToken = default)
        {
            QuerySnapshot? querySnapshot = await Query.GetSnapshotAsync(cancellationToken).ConfigureAwait(false);
            return new TypedQuerySnapshot<TDocument>(querySnapshot, this);
        }


        /// <summary>
        ///     Returns a query with a filter specifying that the value in <paramref name="field" /> must be
        ///     equal to <paramref name="value" />.
        /// </summary>
        /// <remarks>
        ///     This call adds additional filters to any previously-specified ones.
        /// </remarks>
        /// <param name="field">Lambda expression that allows to type safely select field</param>
        /// <param name="value">The value to compare in the filter.</param>
        /// <returns>A new query based on the current one, but with the additional specified filter applied.</returns>
        public TypedQuery<TDocument> WhereEqualTo<TField>(Expression<Func<TDocument, TField>> field, TField value)
        {
            return new TypedQuery<TDocument>(Query.WhereEqualTo(field.GetFieldName(), value));
        }

        /// <summary>
        ///     Returns a query with a filter specifying that the value in <paramref name="field" /> must be
        ///     equal to <paramref name="value" />.
        /// </summary>
        /// <remarks>
        ///     This call adds additional filters to any previously-specified ones.
        /// </remarks>
        /// <param name="field">Lambda expression that allows to type safely select field</param>
        /// <param name="value">The value to compare in the filter.</param>
        /// <returns>A new query based on the current one, but with the additional specified filter applied.</returns>
        public TypedQuery<TDocument> WhereNotEqualTo<TField>(Expression<Func<TDocument, TField>> field, TField value)
        {
            return new TypedQuery<TDocument>(Query.WhereNotEqualTo(field.GetFieldName(), value));
        }

        /// <summary>
        ///     Returns a query with a filter specifying that the value in <paramref name="field" /> must be
        ///     equal to <paramref name="value" />.
        /// </summary>
        /// <remarks>
        ///     This call adds additional filters to any previously-specified ones.
        /// </remarks>
        /// <param name="field">Lambda expression that allows to type safely select field</param>
        /// <param name="value">The value to compare in the filter.</param>
        /// <returns>A new query based on the current one, but with the additional specified filter applied.</returns>
        public TypedQuery<TDocument> WhereLessThan<TField>(Expression<Func<TDocument, TField>> field, TField value)
        {
            return new TypedQuery<TDocument>(Query.WhereLessThan(field.GetFieldName(), value));
        }

        /// <summary>
        ///     Returns a query with a filter specifying that the value in <paramref name="field" /> must be
        ///     equal to <paramref name="value" />.
        /// </summary>
        /// <remarks>
        ///     This call adds additional filters to any previously-specified ones.
        /// </remarks>
        /// <param name="field">Lambda expression that allows to type safely select field</param>
        /// <param name="value">The value to compare in the filter.</param>
        /// <returns>A new query based on the current one, but with the additional specified filter applied.</returns>
        public TypedQuery<TDocument> WhereLessThanOrEqualTo<TField>(
            Expression<Func<TDocument, TField>> field,
            TField value)
        {
            return new TypedQuery<TDocument>(Query.WhereLessThanOrEqualTo(field.GetFieldName(), value));
        }

        /// <summary>
        ///     Returns a query with a filter specifying that the value in <paramref name="field" /> must be
        ///     equal to <paramref name="value" />.
        /// </summary>
        /// <remarks>
        ///     This call adds additional filters to any previously-specified ones.
        /// </remarks>
        /// <param name="field">Lambda expression that allows to type safely select field</param>
        /// <param name="value">The value to compare in the filter.</param>
        /// <returns>A new query based on the current one, but with the additional specified filter applied.</returns>
        public TypedQuery<TDocument> WhereGreaterThan<TField>(Expression<Func<TDocument, TField>> field, TField value)
        {
            return new TypedQuery<TDocument>(Query.WhereGreaterThan(field.GetFieldName(), value));
        }

        /// <summary>
        ///     Returns a query with a filter specifying that the value in <paramref name="field" /> must be
        ///     equal to <paramref name="value" />.
        /// </summary>
        /// <remarks>
        ///     This call adds additional filters to any previously-specified ones.
        /// </remarks>
        /// <param name="field">Lambda expression that allows to type safely select field</param>
        /// <param name="value">The value to compare in the filter.</param>
        /// <returns>A new query based on the current one, but with the additional specified filter applied.</returns>
        public TypedQuery<TDocument> WhereGreaterThanOrEqualTo<TField>(
            Expression<Func<TDocument, TField>> field,
            TField value)
        {
            return new TypedQuery<TDocument>(Query.WhereGreaterThanOrEqualTo(field.GetFieldName(), value));
        }

        /// <summary>
        ///     Returns a query with a filter specifying that the value in <paramref name="field" /> must be
        ///     equal to <paramref name="value" />.
        /// </summary>
        /// <remarks>
        ///     This call adds additional filters to any previously-specified ones.
        /// </remarks>
        /// <param name="field">Lambda expression that allows to type safely select field</param>
        /// <param name="value">The value to compare in the filter.</param>
        /// <returns>A new query based on the current one, but with the additional specified filter applied.</returns>
        public TypedQuery<TDocument> WhereArrayContains<TField>(
            Expression<Func<TDocument, IEnumerable<TField>>> field,
            TField value)
        {
            return new TypedQuery<TDocument>(Query.WhereArrayContains(field.GetFieldName(), value));
        }


        /// <summary>
        ///     Returns a query with a filter specifying that <paramref name="field" /> must be
        ///     a field present in the document, with a value which is an array containing at least one value in
        ///     <paramref name="values" />.
        /// </summary>
        /// <remarks>
        ///     This call adds additional filters to any previously-specified ones.
        /// </remarks>
        /// <param name="field">Lambda expression that allows to type safely select field</param>
        /// <param name="values">The values to compare in the filter. Must not be null.</param>
        /// <returns>A new query based on the current one, but with the additional specified filter applied.</returns>
        public TypedQuery<TDocument> WhereArrayContainsAny<TField>(
            Expression<Func<TDocument, IEnumerable<TField>>> field,
            IEnumerable<TField> values)
        {
            return new TypedQuery<TDocument>(Query.WhereArrayContainsAny(field.GetFieldName(), values));
        }

        /// <summary>
        ///     Returns a query with a filter specifying that <paramref name="field" /> must be
        ///     a field present in the document, with a value which is an array containing at least one value in
        ///     <paramref name="values" />.
        /// </summary>
        /// <remarks>
        ///     This call adds additional filters to any previously-specified ones.
        /// </remarks>
        /// <param name="field">Lambda expression that allows to type safely select field</param>
        /// <param name="values">The values to compare in the filter. Must not be null.</param>
        /// <returns>A new query based on the current one, but with the additional specified filter applied.</returns>
        public TypedQuery<TDocument> WhereIn<TField>(
            Expression<Func<TDocument, TField>> field,
            IEnumerable<TField> values)
        {
            return new TypedQuery<TDocument>(Query.WhereIn(field.GetFieldName(), values));
        }

        /// <summary>
        /// Returns a query with a filter specifying that <paramref name="field"/> must be
        /// a field present in the document, with a value which is not one of the values in <paramref name="values"/>.
        /// </summary>
        /// <remarks>
        ///     This call adds additional filters to any previously-specified ones.
        /// </remarks>
        /// <param name="field">Lambda expression that allows to type safely select field</param>
        /// <param name="values">The values to compare in the filter. Must not be null.</param>
        /// <returns>A new query based on the current one, but with the additional specified filter applied.</returns>
        public TypedQuery<TDocument> WhereNotIn<TField>(
            Expression<Func<TDocument, TField>> field,
            IEnumerable<TField> values)
        {
            return new TypedQuery<TDocument>(Query.WhereNotIn(field.GetFieldName(), values));
        }

        /// <summary>
        ///     Adds an additional ascending ordering by the specified path.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         Unlike LINQ's OrderBy method, this call adds additional subordinate orderings to any
        ///         additionally specified. So <c>query.OrderBy("foo").OrderBy("bar")</c> is similar
        ///         to a LINQ <c>query.OrderBy(x => x.Foo).ThenBy(x => x.Bar)</c>.
        ///     </para>
        ///     <para>
        ///         This method cannot be called after a start/end cursor has been specified with
        ///         <see cref="StartAt(object[])" />, <see cref="StartAfter(object[])" />, <see cref="EndAt(object[])" /> or
        ///         <see cref="EndBefore(object[])" />.
        ///     </para>
        /// </remarks>
        /// <param name="field">Lambda expression that allows to type safely select field</param>
        /// <returns>A new query based on the current one, but with the additional specified ordering applied.</returns>
        public TypedQuery<TDocument> OrderBy<TField>(Expression<Func<TDocument, TField>> field)
        {
            return new TypedQuery<TDocument>(Query.OrderBy(field.GetFieldName()));
        }

        /// <summary>
        ///     Adds an additional descending ordering by the specified path.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         Unlike LINQ's OrderBy method, this call adds additional subordinate orderings to any
        ///         additionally specified. So <c>query.OrderBy("foo").OrderByDescending("bar")</c> is similar
        ///         to a LINQ <c>query.OrderBy(x => x.Foo).ThenByDescending(x => x.Bar)</c>.
        ///     </para>
        ///     <para>
        ///         This method cannot be called after a start/end cursor has been specified with
        ///         <see cref="StartAt(object[])" />, <see cref="StartAfter(object[])" />, <see cref="EndAt(object[])" /> or
        ///         <see cref="EndBefore(object[])" />.
        ///     </para>
        /// </remarks>
        /// <param name="field">Lambda expression that allows to type safely select field</param>
        /// <returns>A new query based on the current one, but with the additional specified ordering applied.</returns>
        public TypedQuery<TDocument> OrderByDescending<TField>(Expression<Func<TDocument, TField>> field)
        {
            return new TypedQuery<TDocument>(Query.OrderByDescending(field.GetFieldName()));
        }


        /// <summary>
        ///     Specifies the maximum number of results to return.
        /// </summary>
        /// <remarks>
        ///     This call replaces any previously-specified limit in the query.
        /// </remarks>
        /// <param name="limit">The maximum number of results to return. Must be greater than or equal to 0.</param>
        /// <returns>A new query based on the current one, but with the specified limit applied.</returns>
        public TypedQuery<TDocument> Limit(int limit)
        {
            return new TypedQuery<TDocument>(Query.Limit(limit));
        }

        /// <summary>
        ///     Creates and returns a new query that only returns the last <paramref name="limit" /> matching documents.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         You must specify at least one <see cref="OrderBy{TField}(Expression{Func{TDocument,TField}})" /> clause for
        ///         limit-to-last queries. Otherwise,
        ///         an <see cref="InvalidOperationException" /> is thrown during execution.
        ///     </para>
        ///     <para>
        ///         Results for limit-to-last queries are only available once all documents are received, which means
        ///         that these queries cannot be streamed using the <see cref="StreamAsync(CancellationToken)" /> method.
        ///     </para>
        /// </remarks>
        /// <param name="limit">The maximum number of results to return. Must be greater than or equal to 0.</param>
        /// <returns>A new query based on the current one, but with the specified limit applied.</returns>
        public TypedQuery<TDocument> LimitToLast(int limit)
        {
            return new TypedQuery<TDocument>(Query.LimitToLast(limit));
        }

        /// <summary>
        ///     Specifies a number of results to skip.
        /// </summary>
        /// <remarks>
        ///     This call replaces any previously-specified offset in the query.
        /// </remarks>
        /// <param name="offset">The number of results to skip. Must be greater than or equal to 0.</param>
        /// <returns>A new query based on the current one, but with the specified offset applied.</returns>
        public TypedQuery<TDocument> Offset(int offset)
        {
            return new TypedQuery<TDocument>(Query.Offset(offset));
        }

        /// <summary>
        ///     Creates and returns a new query that starts at the provided fields relative to the order of the
        ///     query. The order of the field values must match the order of the order-by clauses of the query.
        /// </summary>
        /// <remarks>
        ///     This call replaces any previously specified start position in the query.
        /// </remarks>
        /// <param name="fieldValues">The field values. Must not be null or empty, or have more values than query has orderings.</param>
        /// <returns>A new query based on the current one, but with the specified start position.</returns>
        public TypedQuery<TDocument> StartAt(params object[] fieldValues)
        {
            return new TypedQuery<TDocument>(Query.StartAt(fieldValues));
        }

        /// <summary>
        ///     Creates and returns a new query that starts after the provided fields relative to the order of the
        ///     query. The order of the field values must match the order of the order-by clauses of the query.
        /// </summary>
        /// <remarks>
        ///     This call replaces any previously specified start position in the query.
        /// </remarks>
        /// <param name="fieldValues">The field values. Must not be null or empty, or have more values than query has orderings.</param>
        /// <returns>A new query based on the current one, but with the specified start position.</returns>
        public TypedQuery<TDocument> StartAfter(params object[] fieldValues)
        {
            return new TypedQuery<TDocument>(Query.StartAfter(fieldValues));
        }

        /// <summary>
        ///     Creates and returns a new query that ends before the provided fields relative to the order of the
        ///     query. The order of the field values must match the order of the order-by clauses of the query.
        /// </summary>
        /// <remarks>
        ///     This call replaces any previously specified end position in the query.
        /// </remarks>
        /// <param name="fieldValues">The field values. Must not be null or empty, or have more values than query has orderings.</param>
        /// <returns>A new query based on the current one, but with the specified end position.</returns>
        public TypedQuery<TDocument> EndBefore(params object[] fieldValues)
        {
            return new TypedQuery<TDocument>(Query.EndBefore(fieldValues));
        }

        /// <summary>
        ///     Creates and returns a new query that ends at the provided fields relative to the order of the
        ///     query. The order of the field values must match the order of the order-by clauses of the query.
        /// </summary>
        /// <remarks>
        ///     This call replaces any previously specified end position in the query.
        /// </remarks>
        /// <param name="fieldValues">The field values. Must not be null or empty, or have more values than query has orderings.</param>
        /// <returns>A new query based on the current one, but with the specified end position.</returns>
        public TypedQuery<TDocument> EndAt(params object[] fieldValues)
        {
            return new TypedQuery<TDocument>(Query.EndAt(fieldValues));
        }

        /// <summary>
        ///     Creates and returns a new query that starts at the document snapshot provided fields relative to the order of the
        ///     query.
        /// </summary>
        /// <remarks>
        ///     This call replaces any previously specified start position in the query.
        /// </remarks>
        /// <param name="snapshot">The snapshot of the document to start at. Must not be null.</param>
        /// <returns>A new query based on the current one, but with the specified start position.</returns>
        public TypedQuery<TDocument> StartAt(TypedDocumentSnapshot<TDocument> snapshot)
        {
            return new TypedQuery<TDocument>(Query.StartAt(snapshot.UntypedSnapshot));
        }

        /// <summary>
        ///     Creates and returns a new query that starts after the document snapshot provided fields relative to the order of
        ///     the
        ///     query.
        /// </summary>
        /// <remarks>
        ///     This call replaces any previously specified start position in the query.
        /// </remarks>
        /// <param name="snapshot">The snapshot of the document to start after. Must not be null.</param>
        /// <returns>A new query based on the current one, but with the specified start position.</returns>
        public TypedQuery<TDocument> StartAfter(TypedDocumentSnapshot<TDocument> snapshot)
        {
            return new TypedQuery<TDocument>(Query.StartAfter(snapshot.UntypedSnapshot));
        }

        /// <summary>
        ///     Creates and returns a new query that ends before the document snapshot provided fields relative to the order of the
        ///     query.
        /// </summary>
        /// <remarks>
        ///     This call replaces any previously specified end position in the query.
        /// </remarks>
        /// <param name="snapshot">The snapshot of the document to end before. Must not be null.</param>
        /// <returns>A new query based on the current one, but with the specified end position.</returns>
        public TypedQuery<TDocument> EndBefore(TypedDocumentSnapshot<TDocument> snapshot)
        {
            return new TypedQuery<TDocument>(Query.EndBefore(snapshot.UntypedSnapshot));
        }

        /// <summary>
        ///     Creates and returns a new query that ends at the document snapshot provided fields relative to the order of the
        ///     query.
        /// </summary>
        /// <remarks>
        ///     This call replaces any previously specified end position in the query.
        /// </remarks>
        /// <param name="snapshot">The snapshot of the document to end at.</param>
        /// <returns>A new query based on the current one, but with the specified end position.</returns>
        public TypedQuery<TDocument> EndAt(TypedDocumentSnapshot<TDocument> snapshot)
        {
            return new TypedQuery<TDocument>(Query.EndAt(snapshot.UntypedSnapshot));
        }

        /// <summary>
        ///     Returns an asynchronous sequence of snapshots matching the query.
        /// </summary>
        /// <remarks>
        ///     Each time you iterate over the sequence, a new query will be performed.
        /// </remarks>
        /// <param name="cancellationToken">
        ///     The cancellation token to apply to the streaming operation. Note that even if this is
        ///     <see cref="CancellationToken.None" />, a cancellation token can still be applied when iterating over
        ///     the stream, by passing it into <see cref="IAsyncEnumerable{T}.GetAsyncEnumerator(CancellationToken)" />.
        ///     If a cancellation token is passed both to this method and GetAsyncEnumerator,
        ///     then cancelling either of the tokens will result in the operation being cancelled.
        /// </param>
        /// <returns>An asynchronous sequence of document snapshots matching the query.</returns>
        public async IAsyncEnumerable<TypedDocumentSnapshot<TDocument>> StreamAsync(
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await foreach (DocumentSnapshot snapshot in Query.StreamAsync(cancellationToken).ConfigureAwait(false))
            {
                yield return new TypedDocumentSnapshot<TDocument>(snapshot);
            }
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return Equals(obj as TypedQuery<TDocument>);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Query.GetHashCode();
        }

        /// <summary>
        /// Implicitly converts a typed object to an untyped object.
        /// </summary>
        public static implicit operator Query(TypedQuery<TDocument> typedQuery)
        {
            return typedQuery.Query;
        }
    }
}