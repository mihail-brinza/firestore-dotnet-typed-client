using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Firestore.Typed.Client.Extensions;
using Firestore.Typed.Client.Tests.Model;
using Firestore.Typed.Client.Tests.Utils;

using FluentAssertions;

using Xunit;

namespace Firestore.Typed.Client.Tests;

public class TypedTransactionTests : IAsyncLifetime
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
    public async Task Test_Transaction_GetSnapshotAsync_Document()
    {
        TypedDocumentReference<User> docRef = Collection.Document(RandUser.Id);

        User result = await _testUtils.Db.RunTypedTransactionAsync<User, User>(async transaction =>
        {
            TypedDocumentSnapshot<User> snapshot = await transaction.GetSnapshotAsync(docRef);
            snapshot.Exists.Should().BeTrue();
            return snapshot.RequiredObject;
        });

        result.Should().BeEquivalentTo(RandUser);
    }

    [Fact]
    public async Task Test_Transaction_GetSnapshotAsync_Query()
    {
        TypedQuery<User> query = Collection
            .WhereEqualTo(user => user.Age, RandUser.Age)
            .OrderBy(user => user.FirstName);

        List<User> result = await _testUtils.Db.RunTypedTransactionAsync<User, List<User>>(async transaction =>
        {
            TypedQuerySnapshot<User> snapshot = await transaction.GetSnapshotAsync(query);
            return snapshot.Documents.Select(d => d.RequiredObject).ToList();
        });

        result.Should().AllSatisfy(u => u.Age.Should().Be(RandUser.Age));
    }

    [Fact]
    public async Task Test_Transaction_GetAllSnapshotsAsync()
    {
        var docRefs = Users.Take(3)
            .Select(u => Collection.Document(u.Id))
            .ToList();

        IList<TypedDocumentSnapshot<User>> result =
            await _testUtils.Db.RunTypedTransactionAsync<User, IList<TypedDocumentSnapshot<User>>>(async transaction => await transaction.GetAllSnapshotsAsync(docRefs));

        result.Should().HaveCount(3);
        result.Should().AllSatisfy(s => s.Exists.Should().BeTrue());
    }

    [Fact]
    public async Task Test_Transaction_Create()
    {
        TypedDocumentReference<User> docRef = Collection.Document();
        User newUser = new UserFaker().FinishWith((_, u) => u.Id = docRef.Id).Generate();

        await _testUtils.Db.RunTypedTransactionAsync<User, object>(transaction =>
        {
            transaction.Create(docRef, newUser);
            return Task.FromResult<object>(null!);
        });

        TypedDocumentSnapshot<User> snapshot = await docRef.GetSnapshotAsync();
        snapshot.Exists.Should().BeTrue();
        snapshot.RequiredObject.Should().BeEquivalentTo(newUser);
    }

    [Fact]
    public async Task Test_Transaction_Update_WithUpdateDefinition()
    {
        const int newAge = 88;
        TypedDocumentReference<User> docRef = Collection.Document(RandUser.Id);

        await _testUtils.Db.RunTypedTransactionAsync<User, object>(async transaction =>
        {
            TypedDocumentSnapshot<User> snapshot = await transaction.GetSnapshotAsync(docRef);
            snapshot.Exists.Should().BeTrue();

            UpdateDefinition<User> update = new UpdateDefinition<User>()
                .Set(user => user.Age, newAge);
            transaction.Update(docRef, update);
            return null!;
        });

        TypedDocumentSnapshot<User> updated = await docRef.GetSnapshotAsync();
        updated.GetValue(user => user.Age).Should().Be(newAge);
    }

    [Fact]
    public async Task Test_Transaction_Set()
    {
        TypedDocumentReference<User> docRef = Collection.Document(RandUser.Id);

        var replacementUser = new User
        {
            Id = RandUser.Id,
            FirstName = "TxnSet",
            LastName = "User",
            Age = 42,
            FullName = "TxnSet User",
            PhoneNumbers = ["111", "222", "333", "444"],
            Location = new Location { City = "Tokyo", Country = "Japan" }
        };

        await _testUtils.Db.RunTypedTransactionAsync<User, object>(transaction =>
        {
            transaction.Set(docRef, replacementUser);
            return Task.FromResult<object>(null!);
        });

        TypedDocumentSnapshot<User> snapshot = await docRef.GetSnapshotAsync();
        snapshot.RequiredObject.Should().BeEquivalentTo(replacementUser);
    }

    [Fact]
    public async Task Test_Transaction_Delete()
    {
        TypedDocumentReference<User> docRef = Collection.Document(RandUser.Id);

        await _testUtils.Db.RunTypedTransactionAsync<User, object>(transaction =>
        {
            transaction.Delete(docRef);
            return Task.FromResult<object>(null!);
        });

        TypedDocumentSnapshot<User> snapshot = await docRef.GetSnapshotAsync();
        snapshot.Exists.Should().BeFalse();
    }

    [Fact]
    public async Task Test_Transaction_Properties()
    {
        await _testUtils.Db.RunTypedTransactionAsync<User, object>(transaction =>
        {
            transaction.Database.Should().Be(_testUtils.Db);
            transaction.CancellationToken.Should().NotBeNull();
            return Task.FromResult<object>(null!);
        });
    }
}