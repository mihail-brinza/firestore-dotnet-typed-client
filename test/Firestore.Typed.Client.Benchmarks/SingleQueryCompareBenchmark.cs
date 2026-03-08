using System.Diagnostics;

using Firestore.Typed.Client.Extensions;
using Firestore.Typed.Client.Tests.Model;
using Firestore.Typed.Client.Tests.Utils;

using Google.Cloud.Firestore;

namespace Firestore.Typed.Client.Benchmarks;

/// <summary>
/// Compares Typed vs Official Firestore client for single document CRUD + query.
/// Uses simple timing instead of BenchmarkDotNet to avoid emulator connection issues.
/// </summary>
public static class SingleQueryCompareBenchmark
{
    private const int Iterations = 50;

    private static readonly User TestUser = new()
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
    };

    public static async Task Run()
    {
        FirestoreDb db = TestUtils.CreateEmulatorDb();

        // Warm up gRPC channel
        var warmup = db.Collection("warmup").Document();
        await warmup.CreateAsync(new { init = true });
        await warmup.DeleteAsync();

        // Warmup iterations
        await RunTypedClient(db);
        await RunOfficialClient(db);

        // Interleave iterations for fair comparison
        var typedSw = new Stopwatch();
        var officialSw = new Stopwatch();

        for (int i = 0; i < Iterations; i++)
        {
            typedSw.Start();
            await RunTypedClient(db);
            typedSw.Stop();

            officialSw.Start();
            await RunOfficialClient(db);
            officialSw.Stop();
        }

        double typedMs = typedSw.Elapsed.TotalMilliseconds / Iterations;
        double officialMs = officialSw.Elapsed.TotalMilliseconds / Iterations;
        double diff = ((typedMs - officialMs) / officialMs) * 100;

        Console.WriteLine($"{"Method",-18} {"Mean",-14}");
        Console.WriteLine(new string('-', 32));
        Console.WriteLine($"{"TypedClient",-18} {typedMs,8:F2} ms");
        Console.WriteLine($"{"OfficialClient",-18} {officialMs,8:F2} ms");
        Console.WriteLine($"{"Difference",-18} {diff,+7:F1}%");
    }

    private static async Task RunTypedClient(FirestoreDb db)
    {
        TypedCollectionReference<User> collection = db.TypedCollection<User>(Guid.NewGuid().ToString());
        TypedDocumentReference<User> documentReference = collection.Document();
        await documentReference.CreateAsync(TestUser);

        TypedDocumentSnapshot<User> snapshot = await documentReference.GetSnapshotAsync();
        User user = snapshot.RequiredObject;

        if (user is not { Age: 20 })
        {
            throw new InvalidOperationException("Bad State");
        }

        TypedQuerySnapshot<User> japanUsersSnapshot = await collection
            .WhereEqualTo(u => u.Location.Country, "Japan")
            .GetSnapshotAsync();

        if (!japanUsersSnapshot.Any() || japanUsersSnapshot[0].GetValue(u => u.Location.Country) != "Japan")
        {
            throw new InvalidOperationException("Bad State");
        }
    }

    private static async Task RunOfficialClient(FirestoreDb db)
    {
        CollectionReference collection = db.Collection(Guid.NewGuid().ToString());
        DocumentReference documentReference = collection.Document();
        await documentReference.CreateAsync(TestUser);

        DocumentSnapshot snapshot = await documentReference.GetSnapshotAsync();
        User user = snapshot.ConvertTo<User>();

        if (user is not { Age: 20 })
        {
            throw new InvalidOperationException("Bad State");
        }

        QuerySnapshot japanUsersSnapshot = await collection
            .WhereEqualTo("Location.home_country", "Japan")
            .GetSnapshotAsync();

        if (!japanUsersSnapshot.Any() || japanUsersSnapshot[0].GetValue<string>("Location.home_country") != "Japan")
        {
            throw new InvalidOperationException("Bad State");
        }
    }
}
