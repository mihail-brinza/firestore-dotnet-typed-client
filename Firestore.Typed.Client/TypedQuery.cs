using System.Linq.Expressions;

using Google.Cloud.Firestore;

namespace Firestore.Typed.Client;

public class TypedQuery<TDocument> : IEquatable<TypedQuery<TDocument>>
{
    internal TypedQuery(Query query)
    {
        _query = query;
    }

    private readonly Query _query;

    public async Task<TypedQuerySnapshot<TDocument>> GetSnapshotAsync(CancellationToken cancellationToken = default)
    {
        QuerySnapshot? querySnapshot = await _query.GetSnapshotAsync(cancellationToken).ConfigureAwait(false);
        return new TypedQuerySnapshot<TDocument>(querySnapshot, this);
    }

    public TypedQuery<TDocument> WhereEqualTo<TField>(Expression<Func<TDocument, TField>> field, TField value)
    {
        return new TypedQuery<TDocument>(_query.WhereEqualTo(field.GetField(), value));
    }

    public TypedQuery<TDocument> WhereNotEqualTo<TField>(Expression<Func<TDocument, TField>> field, TField value)
    {
        return new TypedQuery<TDocument>(_query.WhereNotEqualTo(field.GetField(), value));
    }

    public TypedQuery<TDocument> WhereLessThan<TField>(Expression<Func<TDocument, TField>> field, TField value)
    {
        return new TypedQuery<TDocument>(_query.WhereLessThan(field.GetField(), value));
    }

    public TypedQuery<TDocument> WhereLessThanOrEqualTo<TField>(Expression<Func<TDocument, TField>> field, TField value)
    {
        return new TypedQuery<TDocument>(_query.WhereLessThan(field.GetField(), value));
    }

    public TypedQuery<TDocument> WhereGreaterThan<TField>(Expression<Func<TDocument, TField>> field, TField value)
    {
        return new TypedQuery<TDocument>(_query.WhereGreaterThan(field.GetField(), value));
    }

    public TypedQuery<TDocument> WhereGreaterThanOrEqualTo<TField>(
        Expression<Func<TDocument, TField>> field,
        TField value)
    {
        return new TypedQuery<TDocument>(_query.WhereGreaterThanOrEqualTo(field.GetField(), value));
    }

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