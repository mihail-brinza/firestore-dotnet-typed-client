using System.Linq.Expressions;

using Google.Cloud.Firestore;

namespace Firestore.Typed.Client;

public class WhereQueryBuilder<TDocument> : TypedQuery<TDocument>
{
    private readonly CollectionReference _collection;

    public WhereQueryBuilder(CollectionReference collection) : base(collection)
    {
        _collection = collection;
    }

    public WhereQueryBuilder<TDocument> EqualTo<TField>(Expression<Func<TDocument, TField>> field, TField value)
    {
        // TODO
        _collection.WhereEqualTo(field.ToString(), value);
        return this;
    }

    public WhereQueryBuilder<TDocument> NotEqualTo<TField>(Expression<Func<TDocument, TField>> field, TField value)
    {
        // TODO
        _collection.WhereNotEqualTo(field.ToString(), value);
        return this;
    }
}