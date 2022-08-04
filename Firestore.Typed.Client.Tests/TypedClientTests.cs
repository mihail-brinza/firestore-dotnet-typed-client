using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using FluentAssertions;

using Google.Api.Gax;
using Google.Cloud.Firestore;

using Xunit;

namespace Firestore.Typed.Client.Tests;

public class TypedClientTests : IAsyncLifetime
{
    private readonly FirestoreDb _db;
    private readonly Lazy<TypedCollectionReference<User>> _lazyCollection;

    private TypedCollectionReference<User> Collection => _lazyCollection.Value;

    public TypedClientTests()
    {
        _db = new FirestoreDbBuilder
        {
            ProjectId         = "downcast-698d1",
            EmulatorDetection = EmulatorDetection.EmulatorOnly,
        }.Build();

        _lazyCollection = new Lazy<TypedCollectionReference<User>>(GetNewUniqueCollection);
    }

    [Fact]
    public async Task Test_WhereClause_With_CustomPropName()
    {
        TypedQuerySnapshot<User> users = await Collection
            .WhereEqualTo(user => user.SecondName, "Snow")
            .GetSnapshotAsync()
            .ConfigureAwait(false);

        users.Count.Should().Be(1);
    }

    [Fact]
    public async Task Test_WhereClause()
    {
        const string firstName = "Michael";
        TypedQuerySnapshot<User> users = await Collection
            .WhereEqualTo(user => user.FirstName, firstName)
            .GetSnapshotAsync()
            .ConfigureAwait(false);

        users.Count.Should().Be(1);
        users[0].RequiredObject.FirstName.Should().Be(firstName);
    }

    [Fact]
    public async Task Test_WhereInClause()
    {
        TypedQuerySnapshot<User> users = await Collection
            .WhereIn(user => user.FirstName, new[] { "Michael", "Ana" })
            .GetSnapshotAsync()
            .ConfigureAwait(false);

        users.Count.Should().Be(2);
    }

    [Fact]
    public async Task Test_Multiple_WhereClauses()
    {
        TypedQuerySnapshot<User> users = await Collection
            .WhereIn(user => user.FirstName, new[] { "Michael", "Ana" })
            .WhereNotEqualTo(user => user.SecondName, "Smith")
            .GetSnapshotAsync()
            .ConfigureAwait(false);

        users.Count.Should().Be(1);
    }

    [Fact]
    public async Task Test_LowerThan_Clause()
    {
        TypedQuerySnapshot<User> users = await Collection
            .WhereLessThan(user => user.Age, 28)
            .GetSnapshotAsync()
            .ConfigureAwait(false);

        users.Count.Should().Be(3);
    }

    [Fact]
    public async Task Test_NestedObject_WhereClause_WithCustom_Name()
    {
        TypedQuerySnapshot<User> users = await Collection
            .WhereEqualTo(user => user.Location.Country, "Portugal")
            .GetSnapshotAsync()
            .ConfigureAwait(false);

        users.Count.Should().Be(2);
    }

    private TypedCollectionReference<User> GetNewUniqueCollection()
    {
        return _db.TypedCollection<User>(Guid.NewGuid() + " - " + DateTime.Now.Ticks);
    }

    public async Task InitializeAsync()
    {
        await Task.WhenAll(_initialUsers.Select(user => Collection.AddAsync(user))).ConfigureAwait(false);
    }

    private readonly List<User> _initialUsers = new()
    {
        new()
        {
            FirstName  = "John",
            SecondName = "Doe",
            Age        = 10,
            Location = new Location
            {
                City    = "Lisbon",
                Country = "Portugal"
            }
        },
        new()
        {
            FirstName  = "John",
            SecondName = "Snow",
            Age        = 15,
            Location = new Location
            {
                City    = "Coimbra",
                Country = "Portugal"
            }
        },
        new()
        {
            FirstName  = "Michael",
            SecondName = "Low",
            Age        = 21,
            Location = new Location
            {
                City    = "Madrid",
                Country = "Spain"
            }
        },
        new()
        {
            FirstName  = "Ana",
            SecondName = "Smith",
            Age        = 33,
            Location = new Location
            {
                City    = "London",
                Country = "England"
            }
        },
        new()
        {
            FirstName  = "David",
            SecondName = "Roy",
            Age        = 45,
            Location = new Location
            {
                City    = "Manchester",
                Country = "England"
            }
        }
    };

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
}