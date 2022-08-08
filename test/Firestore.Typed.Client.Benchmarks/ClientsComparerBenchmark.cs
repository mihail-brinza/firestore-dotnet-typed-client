using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

using Firestore.Typed.Client.Tests.Model;
using Firestore.Typed.Client.Tests.Utils;

using Google.Api.Gax;
using Google.Cloud.Firestore;

namespace Firestore.Typed.Client.Benchmarks;

[RankColumn]
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class ClientsComparerBenchmark
{
    private readonly IDictionary<int, List<User>> _typedUsers;
    private readonly IDictionary<int, List<User>> _untypedUsers;

    private readonly FirestoreDb _firestoreDb = new FirestoreDbBuilder
    {
        ProjectId         = Environment.GetEnvironmentVariable("FIRESTORE_PROJECT_ID"),
        EmulatorDetection = EmulatorDetection.EmulatorOnly
    }.Build();


    public ClientsComparerBenchmark()
    {
        _typedUsers = new Dictionary<int, List<User>>
        {
            [1]   = new UserFaker().Generate(1),
            [5]   = new UserFaker().Generate(5),
            [10]  = new UserFaker().Generate(10),
            [50]  = new UserFaker().Generate(50),
            [100] = new UserFaker().Generate(100),
            [200] = new UserFaker().Generate(200),
            [400] = new UserFaker().Generate(400),
        };
        _untypedUsers = _typedUsers.ToDictionary(pair => pair.Key, pair => pair.Value.ToList());
    }

    [Benchmark]
    [Arguments(1)]
    [Arguments(5)]
    [Arguments(10)]
    [Arguments(50)]
    [Arguments(100)]
    [Arguments(200)]
    [Arguments(400)]
    public async Task TypedClient(int numberOfUsers)
    {
        List<User> users = _typedUsers[numberOfUsers];
        // Get a random collection
        TypedCollectionReference<User> collection = _firestoreDb.TypedCollection<User>(Guid.NewGuid().ToString());

        // Add all users in batch
        TypedWriteBatch<User> batch = _firestoreDb.StartTypedBatch<User>();
        foreach (User user in users)
        {
            TypedDocumentReference<User> documentRef = collection.Document();
            user.Id = documentRef.Id;
            batch.Create(documentRef, user);
        }

        // Commit results
        IList<WriteResult> writeResults = await batch.CommitAsync().ConfigureAwait(false);

        // Query users
        TypedQuerySnapshot<User> adultsFromPortugalQuery = await collection
            .WhereGreaterThanOrEqualTo(user => user.Age, users[0].Age)
            .WhereEqualTo(user => user.Location.Country, users[0].Location.Country)
            .OrderByDescending(user => user.Age)
            .GetSnapshotAsync()
            .ConfigureAwait(false);

        // Get the snapshots
        TypedQuerySnapshot<User> snapshot = await collection.GetSnapshotAsync().ConfigureAwait(false);

        // Delete all users
        IReadOnlyList<TypedDocumentSnapshot<User>> documents = snapshot.Documents;
        while (documents.Count > 0)
        {
            foreach (TypedDocumentSnapshot<User> document in documents)
            {
                await document.Reference.DeleteAsync().ConfigureAwait(false);
            }

            snapshot  = await collection.GetSnapshotAsync().ConfigureAwait(false);
            documents = snapshot.Documents;
        }
    }

    [Benchmark]
    [Arguments(1)]
    [Arguments(5)]
    [Arguments(10)]
    [Arguments(50)]
    [Arguments(100)]
    [Arguments(200)]
    [Arguments(400)]
    public async Task OfficialClient(int numberOfUsers)
    {
        List<User> users = _untypedUsers[numberOfUsers];
        // Get a random collection
        CollectionReference collection = _firestoreDb.Collection(Guid.NewGuid().ToString());
        // Add all users in batch
        WriteBatch batch = _firestoreDb.StartBatch();
        foreach (User user in users)
        {
            DocumentReference documentRef = collection.Document();
            user.Id = documentRef.Id;
            batch.Create(documentRef, user);
        }

        // Commit results
        IList<WriteResult> writeResults = await batch.CommitAsync().ConfigureAwait(false);

        // Query users
        QuerySnapshot adultsFromPortugalQuery = await collection
            .WhereGreaterThanOrEqualTo("Age", users[0].Age)
            .WhereEqualTo("Location.home_country", users[0].Location.Country)
            .OrderByDescending("Age")
            .GetSnapshotAsync()
            .ConfigureAwait(false);

        // Get the snapshots
        QuerySnapshot snapshot = await collection.GetSnapshotAsync().ConfigureAwait(false);

        // Delete all users
        IReadOnlyList<DocumentSnapshot> documents = snapshot.Documents;
        while (documents.Count > 0)
        {
            foreach (DocumentSnapshot document in documents)
            {
                await document.Reference.DeleteAsync().ConfigureAwait(false);
            }

            snapshot  = await collection.GetSnapshotAsync().ConfigureAwait(false);
            documents = snapshot.Documents;
        }
    }
}