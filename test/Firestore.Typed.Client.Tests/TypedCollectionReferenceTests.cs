using System.Collections.Generic;
using System.Threading.Tasks;

using Firestore.Typed.Client.Extensions;
using Firestore.Typed.Client.Tests.Model;
using Firestore.Typed.Client.Tests.Utils;

using FluentAssertions;

using Google.Cloud.Firestore;

using Xunit;

namespace Firestore.Typed.Client.Tests;

public class TypedCollectionReferenceTests : IAsyncLifetime
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
    public void Test_NewDocument()
    {
        TypedDocumentReference<User> typedDoc = Collection.Document();
        DocumentReference untypedDoc = NonTypedCollection.Document();

        typedDoc.Id.Should().NotBeNullOrEmpty();
        typedDoc.Id.Should().HaveLength(untypedDoc.Id.Length);
        typedDoc.Path.Should().HaveLength(untypedDoc.Path.Length);
        typedDoc.Database.Should().Be(untypedDoc.Database);
    }

    [Fact]
    public async Task Test_AddAsync()
    {
        User newUser = new UserFaker().Generate();

        TypedDocumentReference<User> docRef = await Collection.AddAsync(newUser);

        docRef.Should().NotBeNull();
        docRef.Id.Should().NotBeNullOrEmpty();

        TypedDocumentSnapshot<User> snapshot = await docRef.GetSnapshotAsync();
        snapshot.Exists.Should().BeTrue();
        snapshot.RequiredObject.FirstName.Should().Be(newUser.FirstName);
        snapshot.RequiredObject.Age.Should().Be(newUser.Age);
    }

    [Fact]
    public async Task Test_ListDocumentsAsync()
    {
        var docRefs = new List<TypedDocumentReference<User>>();
        await foreach (TypedDocumentReference<User> docRef in Collection.ListDocumentsAsync())
        {
            docRefs.Add(docRef);
        }

        docRefs.Should().HaveCount(Users.Count);
        docRefs.Should().AllSatisfy(d => d.Id.Should().NotBeNullOrEmpty());
    }

    [Fact]
    public void Test_Id_Property()
    {
        Collection.Id.Should().Be(_testUtils.CollectionId);
    }

    [Fact]
    public void Test_Path_Property()
    {
        Collection.Path.Should().Contain(_testUtils.CollectionId);
    }

    [Fact]
    public void Test_Database_Property()
    {
        Collection.Database.Should().Be(_testUtils.Db);
    }

    [Fact]
    public void Test_Document_With_Path()
    {
        const string docId = "specific-doc-id";
        TypedDocumentReference<User> docRef = Collection.Document(docId);

        docRef.Id.Should().Be(docId);
    }

    [Fact]
    public void Test_Implicit_Conversion_To_CollectionReference()
    {
        CollectionReference untyped = Collection;
        untyped.Should().BeSameAs(Collection.Untyped);
    }

    [Fact]
    public void Test_Equality()
    {
        TypedCollectionReference<User> col1 = Collection;
        TypedCollectionReference<User> col2 = _testUtils.Db.TypedCollection<User>(_testUtils.CollectionId);

        col1.Equals(col2).Should().BeTrue();
        col1.Equals((object)col2).Should().BeTrue();
        col1.GetHashCode().Should().Be(col2.GetHashCode());
    }

    [Fact]
    public void Test_CompareTo()
    {
        TypedCollectionReference<User> col1 = Collection;
        TypedCollectionReference<User> col2 = _testUtils.Db.TypedCollection<User>(_testUtils.CollectionId);

        col1.CompareTo(col2).Should().Be(0);
    }
}