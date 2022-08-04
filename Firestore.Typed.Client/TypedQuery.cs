using System.Collections;

using Google.Cloud.Firestore;

namespace Firestore.Typed.Client;

public class TypedQuery<TDocument>
{
    private protected CollectionReference Collection { get; }

    internal TypedQuery(CollectionReference collection)
    {
        Collection = collection;
        Where      = new WhereQueryBuilder<TDocument>(collection);
    }

    public WhereQueryBuilder<TDocument> Where { get; }

    public async Task<TypedQuerySnapshot<TDocument>> GetSnapshotAsync(CancellationToken cancellationToken = default)
    {
        QuerySnapshot? querySnapshot = await Collection.GetSnapshotAsync(cancellationToken).ConfigureAwait(false);
        return new TypedQuerySnapshot<TDocument>(querySnapshot, this);
    }
}

public class TypedQuerySnapshot<TDocument> : IReadOnlyList<TypedDocumentSnapshot<TDocument>>,
                                             IEquatable<TypedQuerySnapshot<TDocument>>
{
    private readonly QuerySnapshot _snapshot;
    private readonly Lazy<IReadOnlyList<TypedDocumentSnapshot<TDocument>>> _lazyTypedDocuments;

    public TypedQuerySnapshot(QuerySnapshot snapshot, TypedQuery<TDocument> query)
    {
        Query     = query;
        _snapshot = snapshot;
        _lazyTypedDocuments = new Lazy<IReadOnlyList<TypedDocumentSnapshot<TDocument>>>(
            () => snapshot.Documents
                .Select(documentSnap => new TypedDocumentSnapshot<TDocument>(documentSnap))
                .ToList(), LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public TypedQuery<TDocument> Query { get; }

    public IReadOnlyList<TypedDocumentSnapshot<TDocument>> Documents => _lazyTypedDocuments.Value;

    public IEnumerator<TypedDocumentSnapshot<TDocument>> GetEnumerator()
    {
        return Documents.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public int Count => Documents.Count;
    public Timestamp ReadTime => _snapshot.ReadTime;

    public TypedDocumentSnapshot<TDocument> this[int index] => Documents[index];

    public bool Equals(TypedQuerySnapshot<TDocument>? other)
    {
        return other != null && _snapshot.Equals(other._snapshot);
    }
}