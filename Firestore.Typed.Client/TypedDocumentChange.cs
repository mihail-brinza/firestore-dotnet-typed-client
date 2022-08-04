using Google.Cloud.Firestore;

namespace Firestore.Typed.Client;

public class TypedDocumentChange<TDocument> : IEquatable<TypedDocumentChange<TDocument>>
{
    private readonly DocumentChange _documentChange;
    private TypedDocumentSnapshot<TDocument> Document { get; }


    public TypedDocumentChange(DocumentChange documentChange)
    {
        _documentChange = documentChange;
        Document        = new TypedDocumentSnapshot<TDocument>(documentChange.Document);
    }


    public bool Equals(TypedDocumentChange<TDocument>? other)
    {
        return _documentChange.Equals(other?._documentChange);
    }
}