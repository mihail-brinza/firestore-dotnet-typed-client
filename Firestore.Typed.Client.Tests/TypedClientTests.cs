using System;
using System.Threading.Tasks;

using Google.Cloud.Firestore;

using Xunit;

namespace Firestore.Typed.Client.Tests;

public class TypedClientTests
{
    [Fact]
    public async Task Test1()
    {
        FirestoreDb db = FirestoreDb.Create();

        TypedCollectionReference<User> collection = db.Collection<User>("Users");

        TypedQuerySnapshot<User> users = await collection
            .Where
            .EqualTo(user => user.Surname, "Brinza")
            .NotEqualTo(user => user.FirstName, "John")
            .EqualTo(user => user.BirthDate, DateTime.Now.AddYears(-20))
            .GetSnapshotAsync();
    }
}