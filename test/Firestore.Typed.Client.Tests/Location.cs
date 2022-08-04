using Google.Cloud.Firestore;

namespace Firestore.Typed.Client.Tests;

[FirestoreData]
public class Location
{
    public const string CountryCustomName = "home_countryyy";

    [FirestoreProperty]
    public string City { get; set; }

    [FirestoreProperty(CountryCustomName)]
    public string Country { get; set; }
}