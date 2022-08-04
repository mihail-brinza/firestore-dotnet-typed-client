using System.Linq.Expressions;

using Google.Cloud.Firestore;

namespace Firestore.Typed.Client;

public class TypedDocumentReference<TDocument> : IEquatable<TypedDocumentReference<TDocument>>,
                                                 IComparable<TypedDocumentReference<TDocument>>
{
    private readonly DocumentReference _documentReference;

    public string Id => _documentReference.Id;
    public string Path => _documentReference.Path;
    public FirestoreDb Database => _documentReference.Database;

    public TypedDocumentReference(DocumentReference documentReference)
    {
        _documentReference = documentReference;
    }

    public bool Equals(TypedDocumentReference<TDocument>? other)
    {
        return _documentReference.Equals(other?._documentReference);
    }

    public int CompareTo(TypedDocumentReference<TDocument>? other)
    {
        return _documentReference.CompareTo(other?._documentReference);
    }

    public override string ToString() => _documentReference.ToString();

    public async Task<TypedDocumentSnapshot<TDocument>> GetSnapshotAsync(CancellationToken cancellationToken = default)
    {
        DocumentSnapshot snapshot = await _documentReference.GetSnapshotAsync(cancellationToken).ConfigureAwait(false);
        return new TypedDocumentSnapshot<TDocument>(snapshot, this);
    }

    public TypedCollectionReference<TField> Collection<TField>(Expression<Func<TDocument, TField>> field)
    {
        CollectionReference collection = _documentReference.Collection(field.ToString());
        return new TypedCollectionReference<TField>(collection);
    }

    public Task<WriteResult> CreateAsync(
        TDocument documentData,
        CancellationToken cancellationToken = default)
    {
        return _documentReference.CreateAsync(documentData, cancellationToken);
    }

    public Task<WriteResult> DeleteAsync(
        Precondition? precondition = null,
        CancellationToken cancellationToken = default)
    {
        return _documentReference.DeleteAsync(precondition, cancellationToken);
    }

    public Task<WriteResult> UpdateAsync<TField>(
        Expression<Func<TDocument, TField>> field,
        TField value,
        Precondition? precondition = null,
        CancellationToken cancellationToken = default)
    {
        return _documentReference.UpdateAsync(field.ToString(), value, precondition, cancellationToken);
    }

    public Task<WriteResult> SetAsync(
        TDocument documentData,
        SetOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return _documentReference.SetAsync(documentData, options, cancellationToken);
    }


    public FirestoreChangeListener Listen(
        Func<TypedDocumentSnapshot<TDocument>, CancellationToken, Task> callback,
        CancellationToken cancellationToken = default)
    {
        return _documentReference.Listen(
            ((snapshot, token) => callback(new TypedDocumentSnapshot<TDocument>(snapshot, this), token)),
            cancellationToken);
    }

    public FirestoreChangeListener Listen(
        Action<TypedDocumentSnapshot<TDocument>> callback,
        CancellationToken cancellationToken = default)
    {
        return _documentReference.Listen(
            (snapshot => callback(new TypedDocumentSnapshot<TDocument>(snapshot, this))),
            cancellationToken: cancellationToken);
    }

    public override bool Equals(object? obj)
    {
        return _documentReference.Equals(obj);
    }

    public override int GetHashCode()
    {
        return _documentReference.GetHashCode();
    }
}