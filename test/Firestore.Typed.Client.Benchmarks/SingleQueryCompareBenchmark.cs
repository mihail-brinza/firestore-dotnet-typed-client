using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

using Firestore.Typed.Client.Extensions;
using Firestore.Typed.Client.Tests.Model;

using Google.Api.Gax;
using Google.Cloud.Firestore;

namespace Firestore.Typed.Client.Benchmarks;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class SingleQueryCompareBenchmark
{
    private readonly FirestoreDb _firestoreDb = new FirestoreDbBuilder
    {
        ProjectId         = Environment.GetEnvironmentVariable("FIRESTORE_PROJECT_ID"),
        EmulatorDetection = EmulatorDetection.EmulatorOnly
    }.Build();

    [Benchmark]
    public async Task OfficialClient()
    {
        CollectionReference collection = _firestoreDb.Collection(Guid.NewGuid().ToString());
        DocumentReference documentReference = collection.Document();
        WriteResult? writeResult = await documentReference.CreateAsync(new User()
        {
            FirstName = "John",
            LastName  = "Doe",
            FullName  = "John Doe",
            Age       = 20,
            Location = new Location()
            {
                City    = "Tokyo",
                Country = "Japan"
            }
        }).ConfigureAwait(false);

        DocumentSnapshot? snapshot = await documentReference.GetSnapshotAsync().ConfigureAwait(false);
        User? user = snapshot.ConvertTo<User>();

        if (user is not { Age: 20 })
        {
            throw new Exception("Bad State");
        }

        QuerySnapshot japanUsersSnapshot = await collection
            .WhereEqualTo("Location.home_country", "Japan")
            .GetSnapshotAsync()
            .ConfigureAwait(false);

        if (!japanUsersSnapshot.Any() || japanUsersSnapshot[0].GetValue<string>("Location.home_country") != "Japan")
        {
            throw new Exception("Bad State");
        }
    }

    [Benchmark]
    public async Task TypedClient()
    {
        TypedCollectionReference<User> collection = _firestoreDb.TypedCollection<User>(Guid.NewGuid().ToString());
        TypedDocumentReference<User> documentReference = collection.Document();
        WriteResult writeResult = await documentReference.CreateAsync(new User()
        {
            FirstName = "John",
            LastName  = "Doe",
            FullName  = "John Doe",
            Age       = 20,
            Location = new Location
            {
                City    = "Tokyo",
                Country = "Japan"
            }
        }).ConfigureAwait(false);

        TypedDocumentSnapshot<User> snapshot = await documentReference.GetSnapshotAsync().ConfigureAwait(false);
        User user = snapshot.RequiredObject;

        if (user is not { Age: 20 })
        {
            throw new Exception("Bad State");
        }

        TypedQuerySnapshot<User> japanUsersSnapshot = await collection
            .WhereEqualTo(u => u.Location.Country, "Japan")
            .GetSnapshotAsync()
            .ConfigureAwait(false);

        if (!japanUsersSnapshot.Any() || japanUsersSnapshot[0].GetValue(u => u.Location.Country) != "Japan")
        {
            throw new Exception("Bad State");
        }
    }
}