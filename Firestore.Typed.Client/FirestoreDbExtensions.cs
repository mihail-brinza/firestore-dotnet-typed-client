using Google.Cloud.Firestore;

namespace Firestore.Typed.Client;

public static class FirestoreDbExtensions
{
    public static TypedCollectionReference<TDocument> TypedCollection<TDocument>(
        this FirestoreDb firestoreDb,
        string path)
    {
        CollectionReference collection = firestoreDb.Collection(path);
        return new TypedCollectionReference<TDocument>(collection);
    }
}