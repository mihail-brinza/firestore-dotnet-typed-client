using Google.Cloud.Firestore;

namespace Firestore.Typed.Client.Tests.Model
{
    [FirestoreData]
    public class User
    {
        public const string SecondNameCustomField = "second_name";

        [FirestoreDocumentId]
        public string Id { get; set; } = null!;

        [FirestoreProperty]
        public string FirstName { get; set; } = null!;

        [FirestoreProperty(SecondNameCustomField)]
        public string SecondName { get; set; } = null!;

        [FirestoreProperty]
        public int Age { get; set; }

        [FirestoreProperty]
        public Location Location { get; set; } = null!;
    }
}