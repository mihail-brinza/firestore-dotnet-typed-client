using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

using Firestore.Typed.Client.Tests.Model;
using Firestore.Typed.Client.Tests.Utils;

using FluentAssertions;

using Google.Cloud.Firestore;

using Grpc.Core;

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
}