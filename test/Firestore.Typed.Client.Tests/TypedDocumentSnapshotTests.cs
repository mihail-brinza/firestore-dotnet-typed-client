using System.Collections.Generic;
using System.Threading.Tasks;

using Firestore.Typed.Client.Exceptions;
using Firestore.Typed.Client.Tests.Model;
using Firestore.Typed.Client.Tests.Utils;

using FluentAssertions;

using Google.Cloud.Firestore;

using Xunit;

namespace Firestore.Typed.Client.Tests;

public class TypedDocumentSnapshotTests : IAsyncLifetime
{
    private readonly TestUtils _testUtils = new();
    private TypedCollectionReference<User> Collection => _testUtils.Collection;
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
    public async Task Test_Object_Returns_Deserialized_Data()
    {
        TypedDocumentReference<User> docRef = Collection.Document(RandUser.Id);
        TypedDocumentSnapshot<User> snapshot = await docRef.GetSnapshotAsync();

        User user = snapshot.Object;
        user.Should().NotBeNull();
        user.Should().BeEquivalentTo(RandUser);
    }

    [Fact]
    public async Task Test_RequiredObject_Throws_DocumentNotFoundException_When_Missing()
    {
        TypedDocumentReference<User> docRef = Collection.Document("nonexistent-id");
        TypedDocumentSnapshot<User> snapshot = await docRef.GetSnapshotAsync();

        snapshot.Exists.Should().BeFalse();
        snapshot.Invoking(s => s.RequiredObject)
            .Should().Throw<DocumentNotFoundException>()
            .WithMessage("*nonexistent-id*");
    }

    [Fact]
    public async Task Test_ToDictionary()
    {
        TypedDocumentReference<User> docRef = Collection.Document(RandUser.Id);
        TypedDocumentSnapshot<User> snapshot = await docRef.GetSnapshotAsync();

        var dict = snapshot.ToDictionary();
        dict.Should().NotBeEmpty();
        dict.Should().ContainKey("FirstName");
        dict["FirstName"].Should().Be(RandUser.FirstName);
        dict.Should().ContainKey("Age");
        dict["Age"].Should().Be((long)RandUser.Age);
    }

    [Fact]
    public async Task Test_Reference_Property()
    {
        TypedDocumentReference<User> docRef = Collection.Document(RandUser.Id);
        TypedDocumentSnapshot<User> snapshot = await docRef.GetSnapshotAsync();

        snapshot.Reference.Should().NotBeNull();
        snapshot.Reference.Id.Should().Be(RandUser.Id);
        snapshot.Reference.Path.Should().Be(docRef.Path);
    }

    [Fact]
    public async Task Test_Timestamp_Properties()
    {
        TypedDocumentReference<User> docRef = Collection.Document(RandUser.Id);
        TypedDocumentSnapshot<User> snapshot = await docRef.GetSnapshotAsync();

        snapshot.CreateTime.Should().NotBeNull();
        snapshot.UpdateTime.Should().NotBeNull();
        snapshot.ReadTime.Should().NotBeNull();
    }

    [Fact]
    public async Task Test_Timestamp_Properties_Null_When_Missing()
    {
        TypedDocumentReference<User> docRef = Collection.Document("nonexistent-id");
        TypedDocumentSnapshot<User> snapshot = await docRef.GetSnapshotAsync();

        snapshot.CreateTime.Should().BeNull();
        snapshot.UpdateTime.Should().BeNull();
    }

    [Fact]
    public async Task Test_Id_Property()
    {
        TypedDocumentReference<User> docRef = Collection.Document(RandUser.Id);
        TypedDocumentSnapshot<User> snapshot = await docRef.GetSnapshotAsync();

        snapshot.Id.Should().Be(RandUser.Id);
    }

    [Fact]
    public async Task Test_Database_Property()
    {
        TypedDocumentReference<User> docRef = Collection.Document(RandUser.Id);
        TypedDocumentSnapshot<User> snapshot = await docRef.GetSnapshotAsync();

        snapshot.Database.Should().Be(_testUtils.Db);
    }

    [Fact]
    public async Task Test_GetValue_NestedField_With_CustomName()
    {
        TypedDocumentReference<User> docRef = Collection.Document(RandUser.Id);
        TypedDocumentSnapshot<User> snapshot = await docRef.GetSnapshotAsync();

        string country = snapshot.GetValue(user => user.Location.Country);
        country.Should().Be(RandUser.Location.Country);
    }

    [Fact]
    public async Task Test_TryGetValue_Returns_False_For_Missing_Document()
    {
        TypedDocumentReference<User> docRef = Collection.Document("nonexistent-id");
        TypedDocumentSnapshot<User> snapshot = await docRef.GetSnapshotAsync();

        bool found = snapshot.TryGetValue(user => user.Age, out int _);
        found.Should().BeFalse();
    }

    [Fact]
    public async Task Test_ContainsField_Returns_False_For_Missing_Document()
    {
        TypedDocumentReference<User> docRef = Collection.Document("nonexistent-id");
        TypedDocumentSnapshot<User> snapshot = await docRef.GetSnapshotAsync();

        snapshot.ContainsField(user => user.Age).Should().BeFalse();
    }

    [Fact]
    public async Task Test_Implicit_Conversion_To_DocumentSnapshot()
    {
        TypedDocumentReference<User> docRef = Collection.Document(RandUser.Id);
        TypedDocumentSnapshot<User> typedSnapshot = await docRef.GetSnapshotAsync();

        DocumentSnapshot untypedSnapshot = typedSnapshot;
        untypedSnapshot.Should().BeSameAs(typedSnapshot.Untyped);
    }

    [Fact]
    public async Task Test_Equals()
    {
        TypedDocumentReference<User> docRef = Collection.Document(RandUser.Id);
        TypedDocumentSnapshot<User> snapshot1 = await docRef.GetSnapshotAsync();
        TypedDocumentSnapshot<User> snapshot2 = await docRef.GetSnapshotAsync();

        snapshot1.Equals(snapshot2).Should().BeTrue();
        snapshot1.Equals((object)snapshot2).Should().BeTrue();
        snapshot1.GetHashCode().Should().Be(snapshot2.GetHashCode());
    }
}
