using System.Collections.Generic;
using System.Threading.Tasks;

using Firestore.Typed.Client.Tests.Model;
using Firestore.Typed.Client.Tests.Utils;

using FluentAssertions;

using Google.Cloud.Firestore;

using Xunit;

namespace Firestore.Typed.Client.Tests;

public class TypedDocumentReferenceTests : IAsyncLifetime
{
    private readonly TestUtils _testUtils = new();
    private TypedCollectionReference<User> Collection => _testUtils.Collection;
    private CollectionReference NonTypedCollection => _testUtils.NonTypedCollection;
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
    public async Task Test_TestUpdateAsync_SingleValue()
    {
        const int newAge = 83;
        TypedDocumentReference<User> documentReference = Collection.Document(RandUser.Id);
        WriteResult _ = await documentReference.UpdateAsync(user => user.Age, newAge).ConfigureAwait(false);

        TypedDocumentSnapshot<User> snapshot = await documentReference.GetSnapshotAsync().ConfigureAwait(false);
        snapshot.GetValue(user => user.Age).Should().Be(newAge);
    }

    [Fact]
    public async Task Test_TestUpdateAsync_MultipleFields()
    {
        const int newAge = 83;
        const string newFirstName = "Hannah";

        UpdateDefinition<User> update = new UpdateDefinition<User>()
            .Set(user => user.Age, newAge)
            .Set(user => user.FirstName, newFirstName);

        TypedDocumentReference<User> documentReference = Collection.Document(RandUser.Id);

        WriteResult _ = await documentReference.UpdateAsync(update).ConfigureAwait(false);

        TypedDocumentSnapshot<User> snapshot = await documentReference.GetSnapshotAsync().ConfigureAwait(false);
        
        snapshot.ContainsField(user => user.FirstName).Should().BeTrue();
        snapshot.ContainsField(user => user.Age).Should().BeTrue();
        snapshot.GetValue(user => user.Age).Should().Be(newAge);
        snapshot.GetValue(user => user.FirstName).Should().Be(newFirstName);

        bool hasAge = snapshot.TryGetValue(user => user.Age, out int age);
        hasAge.Should().BeTrue();
        age.Should().Be(newAge);
    }


    [Fact]
    public async Task Test_SetAsync_Overwrite()
    {
        var userToReplace = new User
        {
            Id        = RandUser.Id,
            FirstName = "John",
            LastName  = "Doe2",
            Age       = 80
        };

        TypedDocumentReference<User> documentReference = Collection.Document(RandUser.Id);

        WriteResult _ = await documentReference.SetAsync(userToReplace).ConfigureAwait(false);

        TypedDocumentSnapshot<User> replacedUser = await documentReference.GetSnapshotAsync().ConfigureAwait(false);
        replacedUser.RequiredObject.Should().BeEquivalentTo(userToReplace);
    }

    [Fact]
    public async Task Test_SetAsync_MergeAll()
    {
        const int newAge = 47;
        TypedDocumentReference<User> documentReference = Collection.Document(RandUser.Id);
        WriteResult _ = await documentReference.SetAsync(new User
        {
            Age = newAge
        }, TypedSetOptions<User>.MergeFields(u => u.Age)).ConfigureAwait(false);

        TypedDocumentSnapshot<User> mergedUser = await documentReference.GetSnapshotAsync().ConfigureAwait(false);
        RandUser.Age = newAge;
        mergedUser.RequiredObject.Should().BeEquivalentTo(RandUser);
    }
}