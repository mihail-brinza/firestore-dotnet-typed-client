using Google.Cloud.Firestore;

namespace Firestore.Typed.Client;

public static class FirestoreDbExtensions
{
    public static TypedCollectionReference<TDocument> TypedCollection<TDocument>(
        this FirestoreDb firestoreDb,
        string path)
    {
        return new TypedCollectionReference<TDocument>(firestoreDb.Collection(path));
    }
}