using System.Linq.Expressions;
using System.Transactions;

using Firestore.Typed.Client.Visitor;

using Google.Cloud.Firestore;

namespace Firestore.Typed.Client;

public class TypedQuery<TDocument> : IEquatable<TypedQuery<TDocument>>
{
    internal TypedQuery(Query query)
    {
        _query = query;
    }

    private Query _query;

    private static string GetField(Expression field)
    {
        var fieldNameVisitor = new FieldNameVisitor();
        fieldNameVisitor.Visit(field);
        return fieldNameVisitor.FieldName;
    }

    public async Task<TypedQuerySnapshot<TDocument>> GetSnapshotAsync(CancellationToken cancellationToken = default)
    {
        QuerySnapshot? querySnapshot = await _query.GetSnapshotAsync(cancellationToken).ConfigureAwait(false);
        return new TypedQuerySnapshot<TDocument>(querySnapshot, this);
    }

    public TypedQuery<TDocument> WhereEqualTo<TField>(Expression<Func<TDocument, TField>> field, TField value)
    {
        _query = _query.WhereEqualTo(GetField(field), value);
        return this;
    }

    public TypedQuery<TDocument> WhereNotEqualTo<TField>(Expression<Func<TDocument, TField>> field, TField value)
    {
        // TODO
        _query = _query.WhereNotEqualTo(GetField(field), value);
        return this;
    }

    public TypedQuery<TDocument> WhereLessThan<TField>(Expression<Func<TDocument, TField>> field, TField value)
    {
        // TODO
        _query = _query.WhereLessThan(GetField(field), value);
        return this;
    }

    public TypedQuery<TDocument> WhereLessThanOrEqualTo<TField>(Expression<Func<TDocument, TField>> field, TField value)
    {
        // TODO
        _query = _query.WhereLessThan(GetField(field), value);
        return this;
    }

    public TypedQuery<TDocument> WhereGreaterThan<TField>(Expression<Func<TDocument, TField>> field, TField value)
    {
        // TODO
        _query = _query.WhereGreaterThan(GetField(field), value);
        return this;
    }

    public TypedQuery<TDocument> WhereGreaterThanOrEqualTo<TField>(
        Expression<Func<TDocument, TField>> field,
        TField value)
    {
        // TODO
        _query = _query.WhereGreaterThanOrEqualTo(GetField(field), value);
        return this;
    }

    public TypedQuery<TDocument> WhereArrayContains<TField>(
        Expression<Func<TDocument, IEnumerable<TField>>> field,
        TField value)
    {
        // TODO
        _query = _query.WhereArrayContains(GetField(field), value);
        return this;
    }

    public TypedQuery<TDocument> WhereArrayContainsAny<TField>(
        Expression<Func<TDocument, IEnumerable<TField>>> field,
        IEnumerable<TField> values)
    {
        // TODO
        _query = _query.WhereArrayContainsAny(GetField(field), values);
        return this;
    }

    public TypedQuery<TDocument> WhereIn<TField>(
        Expression<Func<TDocument, TField>> field,
        IEnumerable<TField> values)
    {
        // TODO
        _query = _query.WhereIn(GetField(field), values);
        return this;
    }

    public TypedQuery<TDocument> WhereNotIn<TField>(
        Expression<Func<TDocument, TField>> field,
        IEnumerable<TField> values)
    {
        // TODO
        _query = _query.WhereNotIn(GetField(field), values);
        return this;
    }

    public TypedQuery<TDocument> Limit(int limit)
    {
        _query = _query.Limit(limit);
        return this;
    }

    public TypedQuery<TDocument> LimitToLast(int limit)
    {
        _query = _query.LimitToLast(limit);
        return this;
    }

    public TypedQuery<TDocument> Offset(int limit)
    {
        _query = _query.Offset(limit);
        return this;
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