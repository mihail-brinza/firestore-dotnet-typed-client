using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Bogus;

using Firestore.Typed.Client.Tests.Model;

using FluentAssertions;

using Google.Api.Gax;
using Google.Cloud.Firestore;

namespace Firestore.Typed.Client.Tests.Utils;

public class TestUtils : IAsyncDisposable
{
    public IReadOnlyList<User> Users { get; } = new UserFaker().GenerateBetween(5, 30);
    public User RandUser { get; }
    public string CollectionId { get; } = Guid.NewGuid().ToString();
    public string NonTypedCollectionId { get; } = Guid.NewGuid().ToString();

    public FirestoreDb Db { get; } = new FirestoreDbBuilder
    {
        ProjectId         = "downcast-698d1",
        EmulatorDetection = EmulatorDetection.EmulatorOnly
    }.Build();

    public TypedCollectionReference<User> Collection { get; }
    public CollectionReference NonTypedCollection { get; }

    public TestUtils()
    {
        Collection         = Db.TypedCollection<User>(CollectionId);
        NonTypedCollection = Db.Collection(NonTypedCollectionId);
        RandUser           = Users[Random.Shared.Next(0, Users.Count)];
    }

    public Task Init()
    {
        return Task.WhenAll(AddInitialStateToTypedCollection(), AddInitialStateToNonTypedCollection());
    }

    private async Task AddInitialStateToTypedCollection()
    {
        TypedWriteBatch<User> batch = Db.StartTypedBatch<User>();
        foreach (User user in Users)
        {
            TypedDocumentReference<User> documentRef = Collection.Document();
            user.Id = documentRef.Id;
            batch.Create(documentRef, user);
        }

        IList<WriteResult> writeResults = await batch.CommitAsync().ConfigureAwait(false);
        writeResults.Count.Should().Be(Users.Count);
    }

    private async Task AddInitialStateToNonTypedCollection()
    {
        WriteBatch batch = Db.StartBatch();
        foreach (User user in Users)
        {
            DocumentReference documentRef = NonTypedCollection.Document(user.Id);
            batch.Set(documentRef, user);
        }

        IList<WriteResult> writeResults = await batch.CommitAsync().ConfigureAwait(false);
        writeResults.Count.Should().Be(Users.Count);
    }

    public async ValueTask DisposeAsync()
    {
        await DeleteTypedCollection().ConfigureAwait(false);
        await DeleteNonTypedCollection().ConfigureAwait(false);
    }

    private async Task DeleteTypedCollection()
    {
        TypedQuerySnapshot<User> snapshot = await Collection.GetSnapshotAsync().ConfigureAwait(false);
        IReadOnlyList<TypedDocumentSnapshot<User>> documents = snapshot.Documents;
        while (documents.Count > 0)
        {
            foreach (TypedDocumentSnapshot<User> document in documents)
            {
                await document.Reference.DeleteAsync().ConfigureAwait(false);
            }

            snapshot  = await Collection.GetSnapshotAsync().ConfigureAwait(false);
            documents = snapshot.Documents;
        }
    }

    private async Task DeleteNonTypedCollection()
    {
        QuerySnapshot snapshot = await NonTypedCollection.GetSnapshotAsync().ConfigureAwait(false);
        IReadOnlyList<DocumentSnapshot> documents = snapshot.Documents;
        while (documents.Count > 0)
        {
            foreach (DocumentSnapshot document in documents)
            {
                await document.Reference.DeleteAsync().ConfigureAwait(false);
            }

            snapshot  = await NonTypedCollection.GetSnapshotAsync().ConfigureAwait(false);
            documents = snapshot.Documents;
        }
    }
}