using Google.Cloud.Firestore;

namespace Firestore.Typed.Client.Tests.Model;

[FirestoreData]
public class User
{
    public const string SecondNameCustomField = "last_name";

    [FirestoreDocumentId]
    public string Id { get; set; } = null!;

    [FirestoreProperty]
    public string FirstName { get; set; } = null!;

    [FirestoreProperty]
    public string FullName { get; set; } = null!;

    [FirestoreProperty(SecondNameCustomField)]
    public string LastName { get; set; } = null!;

    [FirestoreProperty]
    public int Age { get; set; }

    [FirestoreProperty]
    public string[] PhoneNumbers { get; set; } = null!;

    [FirestoreProperty]
    public Location Location { get; set; } = null!;
}