using Google.Cloud.Firestore;

namespace Firestore.Typed.Client;

public class TypedCollectionReference<TDocument> : TypedQuery<TDocument>,
                                                   IEquatable<TypedCollectionReference<TDocument>>,
                                                   IComparable<TypedCollectionReference<TDocument>>
{
    internal TypedCollectionReference(CollectionReference collection) : base(collection)
    {
    }

    public string Id => Collection.Id;
    public string Path => Collection.Path;
    public FirestoreDb Database => Collection.Database;

    internal TypedDocumentReference<TParentCollection>? Parent<TParentCollection>()
    {
        if (Collection.Parent is null)
        {
            return null;
        }

        return new TypedDocumentReference<TParentCollection>(Collection.Parent);
    }

    public async Task<TypedDocumentReference<TDocument>> AddAsync(
        TDocument documentData,
        CancellationToken cancellationToken = default)
    {
        DocumentReference document = await Collection.AddAsync(documentData, cancellationToken).ConfigureAwait(false);
        return new TypedDocumentReference<TDocument>(document);
    }

    public bool Equals(TypedCollectionReference<TDocument>? other)
    {
        return other != null && Collection.Equals(other.Collection);
    }

    public int CompareTo(TypedCollectionReference<TDocument>? other)
    {
        return Collection.CompareTo(other?.Collection);
    }

    public override bool Equals(object? obj)
    {
        return Collection.Equals(obj);
    }

    public override int GetHashCode()
    {
        return Collection.GetHashCode();
    }
}