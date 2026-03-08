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
        await _testUtils.DisposeAsync();
    }

    private async Task CompareAgainstOfficial(
        Func<TypedCollectionReference<User>, TypedQuery<User>> typedQuery,
        Func<CollectionReference, Query> nonTypedQuery
    )
    {
        TypedQuerySnapshot<User> typedQuerySnapshot =
            await typedQuery(Collection).GetSnapshotAsync();

        IEnumerable<User> typedUsers = typedQuerySnapshot.Documents
            .Select(snap => snap.RequiredObject)
            .ToList();

        QuerySnapshot querySnapshot = await nonTypedQuery(NonTypedCollection).GetSnapshotAsync();

        var nonTypedUsers = querySnapshot.Documents
            .Select(snap => snap.ConvertTo<User>())
            .ToList();

        nonTypedUsers.Should().BeEquivalentTo(typedUsers);
        typedQuerySnapshot.Changes.Should().HaveCount(querySnapshot.Changes.Count);
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
        [
            RandUser.PhoneNumbers[0],
            Users[1].PhoneNumbers[1]
        ];
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
        [
            RandUser.FirstName,
            Users[1].FirstName
        ];
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
        [
            RandUser.FirstName,
            Users[1].FirstName
        ];
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

    [Fact]
    public async Task Test_Select_Field_Projection()
    {
        TypedQuerySnapshot<User> snapshot = await Collection
                .Select(user => user.FirstName, user => user.Age)
                .GetSnapshotAsync()
            ;

        QuerySnapshot untypedSnapshot = await _testUtils.NonTypedCollection
                .Select("FirstName", "Age")
                .GetSnapshotAsync()
            ;

        snapshot.Count.Should().Be(untypedSnapshot.Count);

        foreach (TypedDocumentSnapshot<User> doc in snapshot.Documents)
        {
            doc.ContainsField(user => user.FirstName).Should().BeTrue();
            doc.ContainsField(user => user.Age).Should().BeTrue();
            // Non-selected fields should not be present
            doc.ContainsField(user => user.Location).Should().BeFalse();
        }
    }

    [Fact]
    public async Task Test_StreamAsync()
    {
        var streamedUsers = new List<User>();

        await foreach (TypedDocumentSnapshot<User> snapshot in Collection
                           .OrderBy(user => user.Age)
                           .StreamAsync()
                           .ConfigureAwait(false))
        {
            streamedUsers.Add(snapshot.RequiredObject);
        }

        // Compare against GetSnapshotAsync
        TypedQuerySnapshot<User> querySnapshot = await Collection
            .OrderBy(user => user.Age)
            .GetSnapshotAsync();

        var snapshotUsers = querySnapshot.Documents
            .Select(d => d.RequiredObject)
            .ToList();

        streamedUsers.Should().BeEquivalentTo(snapshotUsers, options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task Test_StartAt_With_Snapshot()
    {
        // Get the 3rd document by age ordering
        TypedQuerySnapshot<User> allOrdered = await Collection
                .OrderBy(user => user.Age)
                .GetSnapshotAsync()
            ;

        if (allOrdered.Count < 3)
            return; // Not enough data for this test

        TypedDocumentSnapshot<User> cursorDoc = allOrdered[2];

        TypedQuerySnapshot<User> fromCursor = await Collection
            .OrderBy(user => user.Age)
            .StartAt(cursorDoc)
            .GetSnapshotAsync();

        fromCursor.Count.Should().Be(allOrdered.Count - 2);
        fromCursor[0].RequiredObject.Should().BeEquivalentTo(cursorDoc.RequiredObject);
    }

    [Fact]
    public async Task Test_StartAfter_With_Snapshot()
    {
        TypedQuerySnapshot<User> allOrdered = await Collection
            .OrderBy(user => user.Age)
            .GetSnapshotAsync();

        if (allOrdered.Count < 3)
            return;

        TypedDocumentSnapshot<User> cursorDoc = allOrdered[2];

        TypedQuerySnapshot<User> afterCursor = await Collection
            .OrderBy(user => user.Age)
            .StartAfter(cursorDoc)
            .GetSnapshotAsync();

        afterCursor.Count.Should().Be(allOrdered.Count - 3);
    }

    [Fact]
    public async Task Test_EndBefore_With_Snapshot()
    {
        TypedQuerySnapshot<User> allOrdered = await Collection
            .OrderBy(user => user.Age)
            .GetSnapshotAsync();

        if (allOrdered.Count < 3)
            return;

        TypedDocumentSnapshot<User> cursorDoc = allOrdered[2];

        TypedQuerySnapshot<User> beforeCursor = await Collection
            .OrderBy(user => user.Age)
            .EndBefore(cursorDoc)
            .GetSnapshotAsync();

        beforeCursor.Count.Should().Be(2);
    }

    [Fact]
    public async Task Test_EndAt_With_Snapshot()
    {
        TypedQuerySnapshot<User> allOrdered = await Collection
            .OrderBy(user => user.Age)
            .GetSnapshotAsync();

        if (allOrdered.Count < 3)
            return;

        TypedDocumentSnapshot<User> cursorDoc = allOrdered[2];

        TypedQuerySnapshot<User> atCursor = await Collection
            .OrderBy(user => user.Age)
            .EndAt(cursorDoc)
            .GetSnapshotAsync();

        atCursor.Count.Should().Be(3);
    }

    [Fact]
    public void Test_Equality()
    {
        TypedQuery<User> query1 = Collection.WhereEqualTo(user => user.Age, 25);
        TypedQuery<User> query2 = Collection.WhereEqualTo(user => user.Age, 25);

        query1.Equals(query2).Should().BeTrue();
        query1.Equals((object)query2).Should().BeTrue();
        query1.GetHashCode().Should().Be(query2.GetHashCode());
    }

    [Fact]
    public void Test_Database_Property()
    {
        TypedQuery<User> query = Collection.WhereEqualTo(user => user.Age, 25);
        query.Database.Should().Be(_testUtils.Db);
    }

    [Fact]
    public void Test_Implicit_Conversion_To_Query()
    {
        TypedQuery<User> typedQuery = Collection.WhereEqualTo(user => user.Age, 25);
        Query untypedQuery = typedQuery;
        untypedQuery.Should().BeSameAs(typedQuery.Untyped);
    }
}