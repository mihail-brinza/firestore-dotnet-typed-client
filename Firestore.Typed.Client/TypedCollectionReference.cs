using Google.Cloud.Firestore;

namespace Firestore.Typed.Client;

public class TypedCollectionReference<TDocument> : TypedQuery<TDocument>,
                                                   IEquatable<TypedCollectionReference<TDocument>>,
                                                   IComparable<TypedCollectionReference<TDocument>>
{
    internal TypedCollectionReference(CollectionReference collection) : base(collection)
    {
        _collection = collection;
    }

    private readonly CollectionReference _collection;

    public string Id => _collection.Id;
    public string Path => _collection.Path;
    public FirestoreDb Database => _collection.Database;

    public int CompareTo(TypedCollectionReference<TDocument>? other)
    {
        return _collection.CompareTo(other?._collection);
    }

    public bool Equals(TypedCollectionReference<TDocument>? other)
    {
        return _collection.Equals(other?._collection);
    }

    internal TypedDocumentReference<TParentCollection>? Parent<TParentCollection>()
    {
        if (_collection.Parent is null)
        {
            return null;
        }

        return new TypedDocumentReference<TParentCollection>(_collection.Parent);
    }

    public async Task<TypedDocumentReference<TDocument>> AddAsync(
        TDocument documentData,
        CancellationToken cancellationToken = default)
    {
        DocumentReference document = await _collection.AddAsync(documentData, cancellationToken).ConfigureAwait(false);
        return new TypedDocumentReference<TDocument>(document);
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as TypedCollectionReference<TDocument>);
    }

    public override int GetHashCode()
    {
        return _collection.GetHashCode();
    }
}