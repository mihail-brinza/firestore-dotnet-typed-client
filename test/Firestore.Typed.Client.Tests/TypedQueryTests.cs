using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Firestore.Typed.Client.Tests.Model;
using Firestore.Typed.Client.Tests.Utils;

using FluentAssertions;

using Google.Cloud.Firestore;

using Xunit;

namespace Firestore.Typed.Client.Tests;

public class TypedQueryTests : IAsyncLifetime
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

    private async Task CompareAgainstOfficial(
        Func<TypedCollectionReference<User>, TypedQuery<User>> typedQuery,
        Func<CollectionReference, Query> nonTypedQuery
    )
    {
        TypedQuerySnapshot<User> typedQuerySnapshot =
            await typedQuery(Collection).GetSnapshotAsync().ConfigureAwait(false);
        IEnumerable<User> typedUsers = typedQuerySnapshot.Documents
            .Select(snap => snap.RequiredObject)
            .ToList();

        QuerySnapshot querySnapshot = await nonTypedQuery(NonTypedCollection).GetSnapshotAsync().ConfigureAwait(false);

        var nonTypedUsers = querySnapshot.Documents
            .Select(snap => snap.ConvertTo<User>())
            .ToList();

        nonTypedUsers.Should().BeEquivalentTo(typedUsers);
    }

    [Fact]
    public Task Test_WhereEqualTo()
    {
        return CompareAgainstOfficial(
            col => col.WhereEqualTo(user => user.Age, RandUser.Age)
                .OrderBy(user => user.FirstName),
            col => col.WhereEqualTo("Age", RandUser.Age)
                .OrderBy("FirstName"));
    }

    [Fact]
    public Task Test_WhereEqualTo_NestedData()
    {
        return CompareAgainstOfficial(
            col => col
                .WhereEqualTo(user => user.Location.Country, RandUser.Location.Country)
                .OrderBy(user => user.FirstName),
            col => col
                .WhereEqualTo("Location.home_country", RandUser.Location.Country)
                .OrderBy("FirstName"));
    }

    [Fact]
    public Task Test_WhereNotEqualTo()
    {
        return CompareAgainstOfficial(
            col => col
                .OrderBy(user => user.Age)
                .WhereNotEqualTo(user => user.Age, RandUser.Age)
                .OrderByDescending(user => user.LastName),
            col => col
                .OrderBy("Age")
                .WhereNotEqualTo("Age", RandUser.Age)
                .OrderByDescending("last_name"));
    }

    [Fact]
    public Task Test_WhereLessThanOrEqualTo()
    {
        return CompareAgainstOfficial(
            col => col
                .OrderBy(user => user.Age)
                .WhereLessThanOrEqualTo(user => user.Age, RandUser.Age)
                .OrderByDescending(user => user.FullName),
            col => col
                .OrderBy("Age")
                .WhereLessThanOrEqualTo("Age", RandUser.Age)
                .OrderByDescending("FullName"));
    }

    [Fact]
    public Task Test_WhereLessThan()
    {
        return CompareAgainstOfficial(
            col => col
                .OrderBy(user => user.Age)
                .WhereLessThan(user => user.Age, RandUser.Age)
                .OrderByDescending(user => user.Location.City),
            col => col
                .OrderBy("Age")
                .WhereLessThan("Age", RandUser.Age)
                .OrderByDescending("Location.City"));
    }

    [Fact]
    public Task Test_WhereGreaterThan()
    {
        return CompareAgainstOfficial(
            col => col
                .OrderBy(user => user.Age)
                .WhereGreaterThan(user => user.Age, RandUser.Age)
                .OrderByDescending(user => user.Location.City),
            col => col
                .OrderBy("Age")
                .WhereGreaterThan("Age", RandUser.Age)
                .OrderByDescending("Location.City"));
    }

    [Fact]
    public Task Test_WhereGreaterThanOrEqualTo()
    {
        return CompareAgainstOfficial(
            col => col
                .OrderBy(user => user.Age)
                .WhereGreaterThanOrEqualTo(user => user.Age, RandUser.Age)
                .OrderByDescending(user => user.Location.City),
            col => col
                .OrderBy("Age")
                .WhereGreaterThanOrEqualTo("Age", RandUser.Age)
                .OrderByDescending("Location.City"));
    }

    [Fact]
    public Task Test_WhereArrayContains()
    {
        return CompareAgainstOfficial(
            col => col
                .WhereArrayContains(user => user.PhoneNumbers, RandUser.PhoneNumbers[0])
                .OrderByDescending(user => user.Location.City),
            col => col
                .WhereArrayContains("PhoneNumbers", RandUser.PhoneNumbers[0])
                .OrderByDescending("Location.City"));
    }

    [Fact]
    public Task Test_WhereArrayContainsAny()
    {
        string[] phoneNumbers =
        {
            RandUser.PhoneNumbers[0],
            Users[1].PhoneNumbers[1]
        };
        return CompareAgainstOfficial(
            col => col
                .WhereArrayContainsAny(user => user.PhoneNumbers, phoneNumbers)
                .OrderByDescending(user => user.Location.City),
            col => col
                .WhereArrayContainsAny("PhoneNumbers", phoneNumbers)
                .OrderByDescending("Location.City"));
    }

    [Fact]
    public Task Test_WhereIn()
    {
        string[] names =
        {
            RandUser.FirstName,
            Users[1].FirstName
        };
        return CompareAgainstOfficial(
            col => col
                .WhereIn(user => user.FirstName, names)
                .OrderByDescending(user => user.Location.City),
            col => col
                .WhereIn("FirstName", names)
                .OrderByDescending("Location.City"));
    }

    [Fact]
    public Task Test_WhereNotIn()
    {
        string[] names =
        {
            RandUser.FirstName,
            Users[1].FirstName
        };
        return CompareAgainstOfficial(
            col => col
                .OrderBy(user => user.FirstName)
                .WhereNotIn(user => user.FirstName, names)
                .OrderByDescending(user => user.Location.City),
            col => col
                .OrderBy("FirstName")
                .WhereNotIn("FirstName", names)
                .OrderByDescending("Location.City"));
    }


    [Fact]
    public Task Test_Limit_With_Multiple_Orderings()
    {
        return CompareAgainstOfficial(
            col => col
                .OrderBy(user => user.FirstName)
                .OrderByDescending(user => user.Age)
                .OrderBy(user => user.Location.City)
                .Limit(3),
            col => col
                .OrderBy("FirstName")
                .OrderByDescending("Age")
                .OrderBy("Location.City")
                .Limit(3)
        );
    }

    [Fact]
    public Task Test_LimitToLast_With_Multiple_Orderings()
    {
        return CompareAgainstOfficial(
            col => col
                .OrderBy(user => user.FirstName)
                .OrderByDescending(user => user.Age)
                .OrderBy(user => user.Location.City)
                .LimitToLast(3),
            col => col
                .OrderBy("FirstName")
                .OrderByDescending("Age")
                .OrderBy("Location.City")
                .LimitToLast(3)
        );
    }

    [Fact]
    public Task Test_Offset_With_Multiple_Orderings()
    {
        return CompareAgainstOfficial(
            col => col
                .OrderBy(user => user.FirstName)
                .OrderByDescending(user => user.Age)
                .OrderBy(user => user.Location.City)
                .Offset(3),
            col => col
                .OrderBy("FirstName")
                .OrderByDescending("Age")
                .OrderBy("Location.City")
                .Offset(3)
        );
    }

    [Fact]
    public Task Test_StartAt()
    {
        return CompareAgainstOfficial(
            col => col
                .OrderBy(user => user.Age)
                .StartAt(18),
            col => col
                .OrderBy("Age")
                .StartAt(18)
        );
    }

    [Fact]
    public Task Test_StartAfter()
    {
        return CompareAgainstOfficial(
            col => col
                .OrderBy(user => user.Age)
                .StartAfter(18),
            col => col
                .OrderBy("Age")
                .StartAfter(18)
        );
    }

    [Fact]
    public Task Test_EndBefore()
    {
        return CompareAgainstOfficial(
            col => col
                .OrderBy(user => user.Age)
                .EndBefore(18),
            col => col
                .OrderBy("Age")
                .EndBefore(18)
        );
    }

    [Fact]
    public Task Test_EndAt()
    {
        return CompareAgainstOfficial(
            col => col
                .OrderBy(user => user.Age)
                .EndAt(18),
            col => col
                .OrderBy("Age")
                .EndAt(18)
        );
    }
}