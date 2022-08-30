using Google.Cloud.Firestore;

namespace Firestore.Typed.Client.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="DocumentReference"/>.
    /// </summary>
    public static class DocumentReferenceExtensions
    {
        /// <summary>
        /// Gets a typed sub-collection reference.
        /// </summary>
        /// <param name="documentReference"></param>
        /// <param name="path"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static TypedCollectionReference<T> TypedCollection<T>(
            this DocumentReference documentReference,
            string path)
        {
            return new TypedCollectionReference<T>(documentReference.Collection(path));
        }
    }
}