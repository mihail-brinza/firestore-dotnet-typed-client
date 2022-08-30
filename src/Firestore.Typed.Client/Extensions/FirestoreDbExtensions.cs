using Google.Cloud.Firestore;

namespace Firestore.Typed.Client.Extensions
{
    /// <summary>
    ///     Extension on the official <see cref="FirestoreDb" />
    /// </summary>
    public static class FirestoreDbExtensions
    {
        /// <summary>
        ///     Method that allows the creation of TypedCollection
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


        /// <summary>
        /// Creates a typed write batch, which can be used to commit multiple mutations atomically.
        /// </summary>
        /// <returns>A write batch for this database.</returns>
        public static TypedWriteBatch<TDocument> StartTypedBatch<TDocument>(
            this FirestoreDb firestoreDb)
        {
            return new TypedWriteBatch<TDocument>(firestoreDb.StartBatch());
        }
    }
}