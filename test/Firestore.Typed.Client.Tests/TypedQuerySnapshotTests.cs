using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Firestore.Typed.Client.Tests.Model;
using Firestore.Typed.Client.Tests.Utils;

using FluentAssertions;

using Google.Cloud.Firestore;

using Xunit;

namespace Firestore.Typed.Client.Tests;

public class TypedQuerySnapshotTests : IAsyncLifetime
{
    private readonly TestUtils _testUtils = new();
    private TypedCollectionReference<User> Collection => _testUtils.Collection;
    private IReadOnlyList<User> Users => _testUtils.Users;

    public Task InitializeAsync()
    {
        return _testUtils.Init();
    }

    public async Task DisposeAsync()
    {
        await _testUtils.DisposeAsync().ConfigureAwait(false);
    }

    [Fact]
    public async Task Test_Count_Matches_Documents()
    {
        TypedQuerySnapshot<User> snapshot = await Collection.GetSnapshotAsync();

        snapshot.Count.Should().Be(Users.Count);
        snapshot.Count.Should().Be(snapshot.Documents.Count);
    }

    [Fact]
    public async Task Test_Indexer_Returns_Correct_Document()
    {
        TypedQuerySnapshot<User> snapshot = await Collection
            .OrderBy(user => user.FirstName)
            .GetSnapshotAsync();

        snapshot[0].Should().NotBeNull();
        snapshot[0].Exists.Should().BeTrue();
        snapshot[0].RequiredObject.Should().NotBeNull();
    }

    [Fact]
    public async Task Test_Enumerator()
    {
        TypedQuerySnapshot<User> snapshot = await Collection.GetSnapshotAsync();

        var users = new List<User>();
        foreach (TypedDocumentSnapshot<User> doc in snapshot)
        {
            users.Add(doc.RequiredObject);
        }

        users.Should().HaveCount(Users.Count);
    }

    [Fact]
    public async Task Test_Linq_Works_Via_IReadOnlyList()
    {
        TypedQuerySnapshot<User> snapshot = await Collection.GetSnapshotAsync();

        var names = snapshot.Select(doc => doc.RequiredObject.FirstName).ToList();
        names.Should().HaveCount(Users.Count);
        names.Should().AllSatisfy(n => n.Should().NotBeNullOrEmpty());
    }

    [Fact]
    public async Task Test_Changes_Property()
    {
        TypedQuerySnapshot<User> snapshot = await Collection.GetSnapshotAsync();

        snapshot.Changes.Should().NotBeNull();
        snapshot.Changes.Count.Should().Be(Users.Count);
    }

    [Fact]
    public async Task Test_Changes_Have_ChangeType_Added()
    {
        TypedQuerySnapshot<User> snapshot = await Collection.GetSnapshotAsync();

        snapshot.Changes.Should().AllSatisfy(change =>
        {
            change.ChangeType.Should().Be(DocumentChange.Type.Added);
            change.Document.Should().NotBeNull();
            change.Document.Exists.Should().BeTrue();
            change.NewIndex.Should().NotBeNull();
        });
    }

    [Fact]
    public async Task Test_ReadTime_Is_Set()
    {
        TypedQuerySnapshot<User> snapshot = await Collection.GetSnapshotAsync();

        snapshot.ReadTime.Should().NotBeNull();
    }

    [Fact]
    public async Task Test_Query_Property()
    {
        TypedQuery<User> query = Collection.OrderBy(user => user.Age);
        TypedQuerySnapshot<User> snapshot = await query.GetSnapshotAsync();

        snapshot.Query.Should().NotBeNull();
    }

    [Fact]
    public async Task Test_Empty_Query_Returns_Zero_Count()
    {
        TypedQuerySnapshot<User> snapshot = await Collection
            .WhereEqualTo(user => user.FirstName, "ThisNameDoesNotExist_12345")
            .GetSnapshotAsync();

        snapshot.Count.Should().Be(0);
        snapshot.Documents.Should().BeEmpty();
        snapshot.Changes.Should().BeEmpty();
    }
}
