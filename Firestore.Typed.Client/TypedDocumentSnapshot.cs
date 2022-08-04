using System.Linq.Expressions;

using Google.Cloud.Firestore;

namespace Firestore.Typed.Client;

public class TypedDocumentSnapshot<TDocument> : IEquatable<TypedDocumentSnapshot<TDocument>>
{
    private readonly Lazy<TDocument?> _objectInitializer;
    private readonly DocumentSnapshot _snapshot;

    public TypedDocumentSnapshot(DocumentSnapshot snapshot, TypedDocumentReference<TDocument> reference)
    {
        _snapshot          = snapshot;
        Reference          = reference;
        _objectInitializer = new Lazy<TDocument?>(() => _snapshot.ConvertTo<TDocument>());
    }

    public TypedDocumentSnapshot(DocumentSnapshot snapshot) :
        this(snapshot, new TypedDocumentReference<TDocument>(snapshot.Reference))
    {
    }

    public string Id => _snapshot.Id;
    public FirestoreDb Database => _snapshot.Database;
    public TDocument? Object => _objectInitializer.Value;

    public TDocument RequiredObject => _snapshot.Exists
        ? _objectInitializer.Value!
        : throw new Exception($"Could not find object with id {_snapshot.Id}");

    public TypedDocumentReference<TDocument> Reference { get; }

    /// <summary>
    ///     Whether or not the document exists.
    /// </summary>
    public bool Exists => _snapshot.Exists;

    /// <summary>
    ///     The creation time of the document if it exists, or null otherwise.
    /// </summary>
    public Timestamp? CreateTime => _snapshot.CreateTime;

    /// <summary>
    ///     The update time of the document if it exists, or null otherwise.
    /// </summary>
    public Timestamp? UpdateTime => _snapshot.UpdateTime;

    /// <summary>
    ///     The time at which this snapshot was read.
    /// </summary>
    public Timestamp ReadTime => _snapshot.ReadTime;

    public bool Equals(TypedDocumentSnapshot<TDocument>? other)
    {
        return _snapshot.Equals(other?._snapshot);
    }

    public TField GetValue<TField>(Expression<Func<TDocument, TField>> field)
    {
        // TODO
        return _snapshot.GetValue<TField>(field.ToString());
    }

    public bool TryGetValue<TField>(Expression<Func<TDocument, TField>> field, out TField value)
    {
        // TODO
        return _snapshot.TryGetValue(field.ToString(), out value);
    }

    public bool ContainsField<TField>(Expression<Func<TDocument, TField>> field)
    {
        // TODO
        return _snapshot.ContainsField(field.ToString());
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as TypedDocumentSnapshot<TDocument>);
    }

    public override int GetHashCode()
    {
        return _snapshot.GetHashCode();
    }
}