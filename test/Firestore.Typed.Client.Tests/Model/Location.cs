using Google.Cloud.Firestore;

namespace Firestore.Typed.Client.Tests.Model
{
    [FirestoreData]
    public class Location
    {
        public const string CountryCustomName = "home_country";

        [FirestoreProperty]
        public string City { get; set; } = null!;

        [FirestoreProperty(CountryCustomName)]
        public string Country { get; set; } = null!;
    }
}