using System.Linq.Expressions;

using Google.Cloud.Firestore;

namespace Firestore.Typed.Client;

/// <summary>
/// A query against a collection.
/// </summary>
/// <remarks>
/// <see cref="TypedCollectionReference{TDocument}"/> derives from this class as a "return-all" query against the
/// collection it refers to.
/// </remarks>
/// <typeparam name="TDocument">The type of the elements in the collection to query</typeparam>
public class TypedQuery<TDocument> : IEquatable<TypedQuery<TDocument>>
{
    internal TypedQuery(Query query)
    {
        _query = query;
    }

    private readonly Query _query;

    /// <summary>
    /// The database this query will search over.
    /// </summary>
    public virtual FirestoreDb Database => _query.Database;

    /// <summary>
    /// Specifies the field paths to return in the results.
    /// </summary>
    /// <remarks>
    /// This call replaces any previously-specified projections in the query.
    /// </remarks>
    /// <param name="fields">Array of lambda expressions that type safely select the property.</param>
    /// <returns>A new query based on the current one, but with the specified projection applied.</returns>
    public TypedQuery<TDocument> Select(params Expression<Func<TDocument, object>>[] fields)
    {
        return new TypedQuery<TDocument>(_query.Select(fields.Select(field => field.GetField()).ToArray()));
    }

    public async Task<TypedQuerySnapshot<TDocument>> GetSnapshotAsync(CancellationToken cancellationToken = default)
    {
        QuerySnapshot? querySnapshot = await _query.GetSnapshotAsync(cancellationToken).ConfigureAwait(false);
        return new TypedQuerySnapshot<TDocument>(querySnapshot, this);
    }


    /// <summary>
    /// Returns a query with a filter specifying that the value in <paramref name="field"/> must be
    /// equal to <paramref name="value"/>.
    /// </summary>
    /// <remarks>
    /// This call adds additional filters to any previously-specified ones.
    /// </remarks>
    /// <param name="field">Lambda expression that allows to type safely select field</param>
    /// <param name="value">The value to compare in the filter.</param>
    /// <returns>A new query based on the current one, but with the additional specified filter applied.</returns>
    public TypedQuery<TDocument> WhereEqualTo<TField>(Expression<Func<TDocument, TField>> field, TField value)
    {
        return new TypedQuery<TDocument>(_query.WhereEqualTo(field.GetField(), value));
    }

    /// <summary>
    /// Returns a query with a filter specifying that the value in <paramref name="field"/> must be
    /// equal to <paramref name="value"/>.
    /// </summary>
    /// <remarks>
    /// This call adds additional filters to any previously-specified ones.
    /// </remarks>
    /// <param name="field">Lambda expression that allows to type safely select field</param>
    /// <param name="value">The value to compare in the filter.</param>
    /// <returns>A new query based on the current one, but with the additional specified filter applied.</returns>
    public TypedQuery<TDocument> WhereNotEqualTo<TField>(Expression<Func<TDocument, TField>> field, TField value)
    {
        return new TypedQuery<TDocument>(_query.WhereNotEqualTo(field.GetField(), value));
    }

    /// <summary>
    /// Returns a query with a filter specifying that the value in <paramref name="field"/> must be
    /// equal to <paramref name="value"/>.
    /// </summary>
    /// <remarks>
    /// This call adds additional filters to any previously-specified ones.
    /// </remarks>
    /// <param name="field">Lambda expression that allows to type safely select field</param>
    /// <param name="value">The value to compare in the filter.</param>
    /// <returns>A new query based on the current one, but with the additional specified filter applied.</returns>
    public TypedQuery<TDocument> WhereLessThan<TField>(Expression<Func<TDocument, TField>> field, TField value)
    {
        return new TypedQuery<TDocument>(_query.WhereLessThan(field.GetField(), value));
    }

    /// <summary>
    /// Returns a query with a filter specifying that the value in <paramref name="field"/> must be
    /// equal to <paramref name="value"/>.
    /// </summary>
    /// <remarks>
    /// This call adds additional filters to any previously-specified ones.
    /// </remarks>
    /// <param name="field">Lambda expression that allows to type safely select field</param>
    /// <param name="value">The value to compare in the filter.</param>
    /// <returns>A new query based on the current one, but with the additional specified filter applied.</returns>
    public TypedQuery<TDocument> WhereLessThanOrEqualTo<TField>(Expression<Func<TDocument, TField>> field, TField value)
    {
        return new TypedQuery<TDocument>(_query.WhereLessThan(field.GetField(), value));
    }

    /// <summary>
    /// Returns a query with a filter specifying that the value in <paramref name="field"/> must be
    /// equal to <paramref name="value"/>.
    /// </summary>
    /// <remarks>
    /// This call adds additional filters to any previously-specified ones.
    /// </remarks>
    /// <param name="field">Lambda expression that allows to type safely select field</param>
    /// <param name="value">The value to compare in the filter.</param>
    /// <returns>A new query based on the current one, but with the additional specified filter applied.</returns>
    public TypedQuery<TDocument> WhereGreaterThan<TField>(Expression<Func<TDocument, TField>> field, TField value)
    {
        return new TypedQuery<TDocument>(_query.WhereGreaterThan(field.GetField(), value));
    }

    /// <summary>
    /// Returns a query with a filter specifying that the value in <paramref name="field"/> must be
    /// equal to <paramref name="value"/>.
    /// </summary>
    /// <remarks>
    /// This call adds additional filters to any previously-specified ones.
    /// </remarks>
    /// <param name="field">Lambda expression that allows to type safely select field</param>
    /// <param name="value">The value to compare in the filter.</param>
    /// <returns>A new query based on the current one, but with the additional specified filter applied.</returns>
    public TypedQuery<TDocument> WhereGreaterThanOrEqualTo<TField>(
        Expression<Func<TDocument, TField>> field,
        TField value)
    {
        return new TypedQuery<TDocument>(_query.WhereGreaterThanOrEqualTo(field.GetField(), value));
    }

    /// <summary>
    /// Returns a query with a filter specifying that the value in <paramref name="field"/> must be
    /// equal to <paramref name="value"/>.
    /// </summary>
    /// <remarks>
    /// This call adds additional filters to any previously-specified ones.
    /// </remarks>
    /// <param name="field">Lambda expression that allows to type safely select field</param>
    /// <param name="value">The value to compare in the filter.</param>
    /// <returns>A new query based on the current one, but with the additional specified filter applied.</returns>
    public TypedQuery<TDocument> WhereArrayContains<TField>(
        Expression<Func<TDocument, IEnumerable<TField>>> field,
        TField value)
    {
        return new TypedQuery<TDocument>(_query.WhereArrayContains(field.GetField(), value));
    }

    public TypedQuery<TDocument> WhereArrayContainsAny<TField>(
        Expression<Func<TDocument, IEnumerable<TField>>> field,
        IEnumerable<TField> values)
    {
        return new TypedQuery<TDocument>(_query.WhereArrayContainsAny(field.GetField(), values));
    }

    public TypedQuery<TDocument> WhereIn<TField>(
        Expression<Func<TDocument, TField>> field,
        IEnumerable<TField> values)
    {
        return new TypedQuery<TDocument>(_query.WhereIn(field.GetField(), values));
    }

    public TypedQuery<TDocument> WhereNotIn<TField>(
        Expression<Func<TDocument, TField>> field,
        IEnumerable<TField> values)
    {
        return new TypedQuery<TDocument>(_query.WhereNotIn(field.GetField(), values));
    }

    public TypedQuery<TDocument> Limit(int limit)
    {
        return new TypedQuery<TDocument>(_query.Limit(limit));
    }

    public TypedQuery<TDocument> LimitToLast(int limit)
    {
        return new TypedQuery<TDocument>(_query.LimitToLast(limit));
    }

    public TypedQuery<TDocument> Offset(int limit)
    {
        return new TypedQuery<TDocument>(_query.Offset(limit));
    }


    public bool Equals(TypedQuery<TDocument>? other)
    {
        return _query.Equals(other?._query);
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as TypedQuery<TDocument>);
    }

    public override int GetHashCode()
    {
        return _query.GetHashCode();
    }
}