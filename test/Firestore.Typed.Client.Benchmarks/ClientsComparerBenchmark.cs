using System.Diagnostics;

using Firestore.Typed.Client.Extensions;
using Firestore.Typed.Client.Tests.Model;
using Firestore.Typed.Client.Tests.Utils;

using Google.Cloud.Firestore;

namespace Firestore.Typed.Client.Benchmarks;

/// <summary>
/// Compares Typed vs Official Firestore client performance using a simple timing approach.
/// BenchmarkDotNet is not suitable here because it spawns separate processes per benchmark,
/// overwhelming the Firestore emulator with rapid gRPC connection cycling.
/// </summary>
public static class ClientsComparerBenchmark
{
    private static readonly int[] UserCounts = [1, 5, 10, 50, 100];
    private const int Iterations = 10;

    public static async Task Run()
    {
        FirestoreDb db = TestUtils.CreateEmulatorDb();

        // Warm up gRPC channel
        var warmup = db.Collection("warmup").Document();
        await warmup.CreateAsync(new { init = true });
        await warmup.DeleteAsync();

        Console.WriteLine($"{"Users",-8} {"TypedClient",-18} {"OfficialClient",-18} {"Diff",-10}");
        Console.WriteLine(new string('-', 56));

        foreach (int count in UserCounts)
        {
            List<User> users = new UserFaker().Generate(count);

            (double typedMs, double officialMs) = await RunInterleaved(db, users);
            double diff = ((typedMs - officialMs) / officialMs) * 100;

            Console.WriteLine($"{count,-8} {typedMs,12:F2} ms    {officialMs,12:F2} ms    {diff,+6:F1}%");
        }
    }

    private static async Task<(double typedMs, double officialMs)> RunInterleaved(
        FirestoreDb db, List<User> users)
    {
        // Warmup
        await RunTypedClient(db, users);
        await RunOfficialClient(db, users);

        var typedSw = new Stopwatch();
        var officialSw = new Stopwatch();

        for (int i = 0; i < Iterations; i++)
        {
            typedSw.Start();
            await RunTypedClient(db, users);
            typedSw.Stop();

            officialSw.Start();
            await RunOfficialClient(db, users);
            officialSw.Stop();
        }

        return (typedSw.Elapsed.TotalMilliseconds / Iterations,
                officialSw.Elapsed.TotalMilliseconds / Iterations);
    }

    private static async Task RunTypedClient(FirestoreDb db, List<User> users)
    {
        TypedCollectionReference<User> collection = db.TypedCollection<User>(Guid.NewGuid().ToString());

        TypedWriteBatch<User> batch = db.StartTypedBatch<User>();
        foreach (User user in users)
        {
            batch.Create(collection.Document(), user);
        }

        await batch.CommitAsync();

        await collection
            .WhereGreaterThanOrEqualTo(user => user.Age, users[0].Age)
            .WhereEqualTo(user => user.Location.Country, users[0].Location.Country)
            .OrderByDescending(user => user.Age)
            .GetSnapshotAsync();

        TypedQuerySnapshot<User> snapshot = await collection.GetSnapshotAsync();

        WriteBatch deleteBatch = db.StartBatch();
        foreach (TypedDocumentSnapshot<User> doc in snapshot.Documents)
        {
            deleteBatch.Delete(doc.Reference.Untyped);
        }

        if (snapshot.Documents.Count > 0)
        {
            await deleteBatch.CommitAsync();
        }
    }

    private static async Task RunOfficialClient(FirestoreDb db, List<User> users)
    {
        CollectionReference collection = db.Collection(Guid.NewGuid().ToString());

        WriteBatch batch = db.StartBatch();
        foreach (User user in users)
        {
            batch.Create(collection.Document(), user);
        }

        await batch.CommitAsync();

        await collection
            .WhereGreaterThanOrEqualTo("Age", users[0].Age)
            .WhereEqualTo("Location.home_country", users[0].Location.Country)
            .OrderByDescending("Age")
            .GetSnapshotAsync();

        QuerySnapshot snapshot = await collection.GetSnapshotAsync();

        WriteBatch deleteBatch = db.StartBatch();
        foreach (DocumentSnapshot doc in snapshot.Documents)
        {
            deleteBatch.Delete(doc.Reference);
        }

        if (snapshot.Documents.Count > 0)
        {
            await deleteBatch.CommitAsync();
        }
    }
}
