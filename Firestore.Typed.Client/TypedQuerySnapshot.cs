using System.Collections;

using Google.Cloud.Firestore;

namespace Firestore.Typed.Client;

public sealed class TypedQuerySnapshot<TDocument> : IReadOnlyList<TypedDocumentSnapshot<TDocument>>,
                                             IEquatable<TypedQuerySnapshot<TDocument>>
{
    private readonly Lazy<IReadOnlyList<TypedDocumentSnapshot<TDocument>>> _lazyTypedDocuments;
    private readonly Lazy<IReadOnlyList<TypedDocumentChange<TDocument>>> _lazyTypedChangeList;
    private readonly QuerySnapshot _snapshot;

    public TypedQuerySnapshot(QuerySnapshot snapshot, TypedQuery<TDocument> query)
    {
        Query                = query;
        _snapshot            = snapshot;
        _lazyTypedDocuments  = BuildLazyTypedDocuments(snapshot);
        _lazyTypedChangeList = BuildLazyTypedChangeList(snapshot);
    }

    private static Lazy<IReadOnlyList<TypedDocumentChange<TDocument>>> BuildLazyTypedChangeList(QuerySnapshot snapshot)
    {
        return new Lazy<IReadOnlyList<TypedDocumentChange<TDocument>>>(
            () => snapshot.Changes.Select(change => new TypedDocumentChange<TDocument>(change)).ToList(),
            LazyThreadSafetyMode.ExecutionAndPublication
        );
    }

    private static Lazy<IReadOnlyList<TypedDocumentSnapshot<TDocument>>> BuildLazyTypedDocuments(QuerySnapshot snapshot)
    {
        return new Lazy<IReadOnlyList<TypedDocumentSnapshot<TDocument>>>(
            () => snapshot.Documents
                .Select(documentSnap => new TypedDocumentSnapshot<TDocument>(documentSnap))
                .ToList(), LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public TypedQuery<TDocument> Query { get; }

    public IReadOnlyList<TypedDocumentSnapshot<TDocument>> Documents => _lazyTypedDocuments.Value;
    public IReadOnlyList<TypedDocumentChange<TDocument>> Changes => _lazyTypedChangeList.Value;
    public Timestamp ReadTime => _snapshot.ReadTime;

    public bool Equals(TypedQuerySnapshot<TDocument>? other)
    {
        return _snapshot.Equals(other?._snapshot);
    }

    public IEnumerator<TypedDocumentSnapshot<TDocument>> GetEnumerator()
    {
        return Documents.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public int Count => Documents.Count;

    public TypedDocumentSnapshot<TDocument> this[int index] => Documents[index];

    public override bool Equals(object obj)
    {
        return Equals(obj as TypedQuerySnapshot<TDocument>);
    }
}