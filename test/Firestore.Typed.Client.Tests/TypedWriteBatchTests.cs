using System.Collections.Generic;
using System.Threading.Tasks;

using Firestore.Typed.Client.Extensions;
using Firestore.Typed.Client.Tests.Model;
using Firestore.Typed.Client.Tests.Utils;

using FluentAssertions;

using Google.Cloud.Firestore;

using Xunit;

namespace Firestore.Typed.Client.Tests;

public class TypedWriteBatchTests : IAsyncLifetime
{
    private readonly TestUtils _testUtils = new();
    private TypedCollectionReference<User> Collection => _testUtils.Collection;
    private IReadOnlyList<User> Users => _testUtils.Users;
    private User RandUser => _testUtils.RandUser;

    public Task InitializeAsync()
    {
        return _testUtils.Init();
    }

    public async Task DisposeAsync()
    {
        await _testUtils.DisposeAsync().ConfigureAwait(false);
    }

    [Fact]
    public async Task Test_Batch_Create_Multiple_Documents()
    {
        TypedWriteBatch<User> batch = _testUtils.Db.StartTypedBatch<User>();

        var newUsers = new List<User>();
        var docRefs = new List<TypedDocumentReference<User>>();

        for (int i = 0; i < 3; i++)
        {
            TypedDocumentReference<User> docRef = Collection.Document();
            User user = new UserFaker().FinishWith((_, u) => u.Id = docRef.Id).Generate();
            batch.Create(docRef, user);
            newUsers.Add(user);
            docRefs.Add(docRef);
        }

        IList<WriteResult> results = await batch.CommitAsync();
        results.Count.Should().Be(3);

        for (var i = 0; i < 3; i++)
        {
            TypedDocumentSnapshot<User> snapshot = await docRefs[i].GetSnapshotAsync();
            snapshot.Exists.Should().BeTrue();
            snapshot.RequiredObject.Should().BeEquivalentTo(newUsers[i]);
        }
    }

    [Fact]
    public async Task Test_Batch_Update_SingleField()
    {
        const int newAge = 99;
        TypedDocumentReference<User> docRef = Collection.Document(RandUser.Id);

        TypedWriteBatch<User> batch = _testUtils.Db.StartTypedBatch<User>();
        batch.Update(docRef, user => user.Age, newAge);

        IList<WriteResult> results = await batch.CommitAsync();
        results.Count.Should().Be(1);

        TypedDocumentSnapshot<User> snapshot = await docRef.GetSnapshotAsync();
        snapshot.GetValue(user => user.Age).Should().Be(newAge);
    }

    [Fact]
    public async Task Test_Batch_Update_WithUpdateDefinition()
    {
        const int newAge = 77;
        const string newFirstName = "BatchUpdated";

        TypedDocumentReference<User> docRef = Collection.Document(RandUser.Id);

        UpdateDefinition<User> update = new UpdateDefinition<User>()
            .Set(user => user.Age, newAge)
            .Set(user => user.FirstName, newFirstName);

        TypedWriteBatch<User> batch = _testUtils.Db.StartTypedBatch<User>();
        batch.Update(docRef, update);

        IList<WriteResult> results = await batch.CommitAsync();
        results.Count.Should().Be(1);

        TypedDocumentSnapshot<User> snapshot = await docRef.GetSnapshotAsync();
        snapshot.GetValue(user => user.Age).Should().Be(newAge);
        snapshot.GetValue(user => user.FirstName).Should().Be(newFirstName);
    }

    [Fact]
    public async Task Test_Batch_Set_Document()
    {
        TypedDocumentReference<User> docRef = Collection.Document(RandUser.Id);

        var replacementUser = new User
        {
            Id        = RandUser.Id,
            FirstName = "BatchSet",
            LastName  = "User",
            Age       = 55,
            FullName  = "BatchSet User",
            PhoneNumbers = ["111", "222", "333", "444"],
            Location = new Location { City = "Berlin", Country = "Germany" }
        };

        TypedWriteBatch<User> batch = _testUtils.Db.StartTypedBatch<User>();
        batch.Set(docRef, replacementUser);

        IList<WriteResult> results = await batch.CommitAsync();
        results.Count.Should().Be(1);

        TypedDocumentSnapshot<User> snapshot = await docRef.GetSnapshotAsync();
        snapshot.RequiredObject.Should().BeEquivalentTo(replacementUser);
    }

    [Fact]
    public async Task Test_Batch_Delete_Document()
    {
        TypedDocumentReference<User> docRef = Collection.Document(RandUser.Id);

        TypedWriteBatch<User> batch = _testUtils.Db.StartTypedBatch<User>();
        batch.Delete(docRef);

        IList<WriteResult> results = await batch.CommitAsync();
        results.Count.Should().Be(1);

        TypedDocumentSnapshot<User> snapshot = await docRef.GetSnapshotAsync();
        snapshot.Exists.Should().BeFalse();
    }

    [Fact]
    public async Task Test_Batch_Method_Chaining()
    {
        TypedDocumentReference<User> docToDelete = Collection.Document(RandUser.Id);
        TypedDocumentReference<User> docToCreate = Collection.Document();
        User newUser = new UserFaker().FinishWith((_, u) => u.Id = docToCreate.Id).Generate();

        TypedWriteBatch<User> batch = _testUtils.Db.StartTypedBatch<User>();

        // Chain multiple operations
        batch.Create(docToCreate, newUser)
             .Delete(docToDelete);

        IList<WriteResult> results = await batch.CommitAsync();
        results.Count.Should().Be(2);

        TypedDocumentSnapshot<User> createdSnapshot = await docToCreate.GetSnapshotAsync();
        createdSnapshot.Exists.Should().BeTrue();

        TypedDocumentSnapshot<User> deletedSnapshot = await docToDelete.GetSnapshotAsync();
        deletedSnapshot.Exists.Should().BeFalse();
    }

    [Fact]
    public void Test_Implicit_Conversion_To_WriteBatch()
    {
        TypedWriteBatch<User> typedBatch = _testUtils.Db.StartTypedBatch<User>();
        WriteBatch untypedBatch = typedBatch;
        untypedBatch.Should().BeSameAs(typedBatch.Untyped);
    }

    [Fact]
    public void Test_Implicit_Conversion_From_WriteBatch()
    {
        WriteBatch untypedBatch = _testUtils.Db.StartBatch();
        TypedWriteBatch<User> typedBatch = untypedBatch;
        typedBatch.Untyped.Should().BeSameAs(untypedBatch);
    }
}
