using Google.Cloud.Firestore;

namespace Firestore.Typed.Client;

public class TypedDocumentChange<TDocument> : IEquatable<TypedDocumentChange<TDocument>>
{
    private readonly DocumentChange _documentChange;


    public TypedDocumentChange(DocumentChange documentChange)
    {
        _documentChange = documentChange;
        Document        = new TypedDocumentSnapshot<TDocument>(documentChange.Document);
    }

    private TypedDocumentSnapshot<TDocument> Document { get; }

    public DocumentChange.Type ChangeType => _documentChange.ChangeType;

    /// <summary>
    ///     The index of the changed document in the result set immediately prior to this DocumentChange
    ///     (i.e. supposing that all prior DocumentChange objects have been applied), or null
    ///     if the change type is <see cref="Type.Added" />. The index will never be negative.
    /// </summary>
    public int? OldIndex => _documentChange.OldIndex;

    /// <summary>
    ///     The index of the changed document in the result set immediately after this DocumentChange
    ///     (i.e. supposing that all prior DocumentChange objects and this one have been applied),
    ///     null if the change type is <see cref="Type.Removed" />. The index will never be negative.
    /// </summary>
    public int? NewIndex => _documentChange.NewIndex;

    public bool Equals(TypedDocumentChange<TDocument>? other)
    {
        return _documentChange.Equals(other?._documentChange);
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as TypedDocumentChange<TDocument>);
    }

    public override int GetHashCode()
    {
        return _documentChange.GetHashCode();
    }
}