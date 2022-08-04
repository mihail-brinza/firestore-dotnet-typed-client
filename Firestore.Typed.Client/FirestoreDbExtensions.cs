using Google.Cloud.Firestore;

namespace Firestore.Typed.Client;

/// <summary>
/// Extension on the official <see cref="FirestoreDb"/>
/// </summary>
public static class FirestoreDbExtensions
{
    /// <summary>
    /// Method that allows the creation of TypedCollection
    /// </summary>
    /// <param name="firestoreDb">The firestore database</param>
    /// <param name="path">Path to the collection</param>
    /// <typeparam name="TDocument">Type of the documents in the collection</typeparam>
    /// <returns></returns>
    public static TypedCollectionReference<TDocument> TypedCollection<TDocument>(
        this FirestoreDb firestoreDb,
        string path)
    {
        return new TypedCollectionReference<TDocument>(firestoreDb.Collection(path));
    }
}