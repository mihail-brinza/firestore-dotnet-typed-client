using System.Collections.Generic;
using System.Threading.Tasks;

using Firestore.Typed.Client.Tests.Model;
using Firestore.Typed.Client.Tests.Utils;

using FluentAssertions;

using Google.Cloud.Firestore;

using Grpc.Core;

using Xunit;
// ReSharper disable UseConfigureAwaitFalse

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
    public async Task Test_UpdateAsync_SingleValue()
    {
        const int newAge = 83;
        TypedDocumentReference<User> documentReference = Collection.Document(RandUser.Id);
        WriteResult _ = await documentReference.UpdateAsync(user => user.Age, newAge);
        TypedDocumentSnapshot<User> snapshot = await documentReference.GetSnapshotAsync();
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

        WriteResult _ = await documentReference.UpdateAsync(update);

        TypedDocumentSnapshot<User> snapshot = await documentReference.GetSnapshotAsync();

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

        WriteResult _ = await documentReference.SetAsync(userToReplace);

        TypedDocumentSnapshot<User> replacedUser = await documentReference.GetSnapshotAsync();
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
        }, TypedSetOptions<User>.MergeFields(u => u.Age));

        TypedDocumentSnapshot<User> mergedUser = await documentReference.GetSnapshotAsync();
        RandUser.Age = newAge;
        mergedUser.RequiredObject.Should().BeEquivalentTo(RandUser);
    }

    [Fact]
    public async Task Test_NewDocument_CreateAsync()
    {
        // Create typed user
        TypedDocumentReference<User> typedDoc = Collection.Document();
        User typedUser = new UserFaker().FinishWith(((faker, user) => user.Id = typedDoc.Id)).Generate();
        WriteResult typedWriteResult = await typedDoc.CreateAsync(typedUser);

        // Create untyped user
        DocumentReference untypedDoc = NonTypedCollection.Document();
        User untypedUser = new UserFaker().FinishWith(((_, user) => user.Id = untypedDoc.Id)).Generate();
        WriteResult untypedWriteResult = await untypedDoc.CreateAsync(untypedUser);

        // Assert document references and write results
        typedWriteResult.Should().NotBeNull();
        untypedWriteResult.Should().NotBeNull();

        // Get snapshot to ensure that object was created
        TypedDocumentSnapshot<User> typedSnapshot = await typedDoc.GetSnapshotAsync();
        typedSnapshot.Exists.Should().BeTrue();
        typedSnapshot.RequiredObject.Should().BeEquivalentTo(typedUser);

        // Get snapshot to ensure that object was created
        DocumentSnapshot untypedSnapshot = await untypedDoc.GetSnapshotAsync();
        untypedSnapshot.Exists.Should().BeTrue();
        untypedSnapshot.ConvertTo<User>().Should().BeEquivalentTo(untypedUser);
    }


    [Fact]
    public async Task Test_NewDocument_DeleteAsync_Precondition_None()
    {
        // Create typed user
        TypedDocumentReference<User> typedDoc = Collection.Document();

        await typedDoc
            .Invoking(doc => doc.DeleteAsync(Precondition.None)).Should()
            .NotThrowAsync()
;

        // Create untyped user
        DocumentReference untypedDoc = NonTypedCollection.Document();
        await untypedDoc
            .Invoking(doc => doc.DeleteAsync(Precondition.None)).Should()
            .NotThrowAsync()
;
    }

    [Fact]
    public async Task Test_NewDocument_DeleteAsync_Precondition_MustExist_When_User_Does_Not_Exist()
    {
        // Create typed user
        TypedDocumentReference<User> typedDoc = Collection.Document();

        await typedDoc
            .Invoking(doc => doc.DeleteAsync(Precondition.MustExist)).Should()
            .ThrowExactlyAsync<RpcException>()
;

        // Create untyped user
        DocumentReference untypedDoc = NonTypedCollection.Document();
        await untypedDoc
            .Invoking(doc => doc.DeleteAsync(Precondition.MustExist)).Should()
            .ThrowExactlyAsync<RpcException>()
;
    }

    /// <summary>
    /// Gets an existing user and then deletes the user and ensures that it does not exist anymore,
    /// comparing the result with both libraries
    /// </summary>
    [Fact]
    public async Task Test_NewDocument_DeleteAsync_Precondition_MustExist()
    {
        // Typed case
        TypedDocumentReference<User> typedDoc = Collection.Document(RandUser.Id);
        TypedDocumentSnapshot<User> typedSnapshot = await typedDoc.GetSnapshotAsync();
        typedSnapshot.Exists.Should().BeTrue();
        await typedDoc
            .Invoking(doc => doc.DeleteAsync(Precondition.MustExist)).Should()
            .NotThrowAsync()
;

        typedSnapshot = await typedDoc.GetSnapshotAsync();
        typedSnapshot.Exists.Should().BeFalse();

        // UnTyped case
        DocumentReference untypedDoc = NonTypedCollection.Document(RandUser.Id);
        DocumentSnapshot untypedSnapshot = await untypedDoc.GetSnapshotAsync();
        untypedSnapshot.Exists.Should().BeTrue();

        await untypedDoc
            .Invoking(doc => doc.DeleteAsync(Precondition.MustExist)).Should()
            .NotThrowAsync()
;

        untypedSnapshot = await untypedDoc.GetSnapshotAsync();
        untypedSnapshot.Exists.Should().BeFalse();
    }
}