# Firestore .NET Typed Client

![GitHub Workflow Status (with event)](https://img.shields.io/github/actions/workflow/status/mihail-brinza/firestore-dotnet-typed-client/build_and_tests.yml?style=flat-square&label=build%20%26%20tests)
![](https://img.shields.io/github/license/mihail-brinza/firestore-dotnet-typed-client?style=flat-square)
![](https://img.shields.io/nuget/dt/Firestore.Typed.Client?style=flat-square)
![](https://img.shields.io/nuget/v/Firestore.Typed.Client?style=flat-square)

**A light-weight, strongly-typed Firestore client that catches query errors at compile time instead of in production.**

## The Problem

The official Google Firestore client references fields by name using plain strings. This means typos, renamed properties, and wrong field names all compile without errors and only fail at runtime.

```csharp
// Official client — compiles fine, breaks at runtime
query.WhereEqualTo("Locaiton.home_country", "Portugal");

// Typed client — compiler catches the typo immediately
query.WhereEqualTo(u => u.Locaiton.Country, "Portugal");
// CS1061: 'User' does not contain a definition for 'Locaiton'
```

Firestore also lets you define custom storage names via `[FirestoreProperty("home_country")]`. With the official client you have to remember to use `"home_country"` instead of `"Country"` in every query. The typed client resolves this automatically:

```csharp
// Official client — you need to know the Firestore storage name
query.WhereEqualTo("Location.home_country", "Portugal");

// Typed client — uses the C# property, resolves the storage name for you
query.WhereEqualTo(u => u.Location.Country, "Portugal");
```

## Why use the Typed Client?

| | Official Client | Typed Client |
|---|---|---|
| Field references | Magic strings | Lambda expressions |
| Custom field names | Manual lookup | Automatic |
| Wrong field name | Silent runtime failure | Compile error |
| Type mismatch on update | Runtime exception | Compile error |
| API compatibility | — | Full (wraps the official client) |

This library wraps the official `Google.Cloud.Firestore` client. Everything the official client supports (transactions, listeners, batched writes) works the same way.

## Installation

```bash
dotnet add package Firestore.Typed.Client
```

## Compatibility

This library targets **.NET Standard 2.0**, which means it works with:

- .NET Framework 4.6.1+
- .NET Core 2.0+
- .NET 5, 6, 7, 8, 9, 10+

---

## Quick Start

The examples below use the following data model:

```csharp
[FirestoreData]
public class User
{
    [FirestoreProperty]
    public string FirstName { get; set; }

    [FirestoreProperty("second_name")]
    public string SecondName { get; set; }

    [FirestoreProperty]
    public int Age { get; set; }

    [FirestoreProperty]
    public Location Location { get; set; }
}

[FirestoreData]
public class Location
{
    [FirestoreProperty]
    public string City { get; set; }

    [FirestoreProperty("home_country")]
    public string Country { get; set; }
}
```

For more information about data modeling, see the [Official Documentation](https://cloud.google.com/dotnet/docs/reference/Google.Cloud.Firestore/latest/datamodel).

```csharp
FirestoreDb db = await FirestoreDb.CreateAsync("your-project-id");

// Create a document with a random ID in the "users" collection.
TypedCollectionReference<User> collection = db.TypedCollection<User>("users");

User newUser = new User
{
    FirstName  = "John",
    SecondName = "Doe",
    Age        = 10,
    Location = new Location
    {
        City    = "Lisbon",
        Country = "Portugal"
    }
};

TypedDocumentReference<User> document = await collection.AddAsync(newUser);

// A TypedDocumentReference<User> doesn't contain the data - it's just a path.
// Let's fetch the current document.
TypedDocumentSnapshot<User> snapshot = await document.GetSnapshotAsync();

// We can access individual fields by selecting them using a lambda expression
string firstName = snapshot.GetValue(user => user.FirstName);
int age = snapshot.GetValue(user => user.Age);
string country = snapshot.GetValue(user => user.Location.Country);

// Or get the deserialized object
User? createdUser = snapshot.Object;

// Query the collection for all documents
// where doc.Age < 35 && doc.Location.home_country == Portugal.
TypedQuery<User> query = collection
    .WhereLessThan(user => user.Age, 35)
    .WhereEqualTo(user => user.Location.Country, "Portugal");
TypedQuerySnapshot<User> querySnapshot = await query.GetSnapshotAsync();

foreach (TypedDocumentSnapshot<User> queryResult in querySnapshot.Documents)
{
    User? userResult = queryResult.Object;
}
```

---

## API

### Access Collection

```csharp
TypedCollectionReference<User> collection = db.TypedCollection<User>("users");
```

### Creating a Document

`AddAsync` only accepts the document type `TDocument`, so passing the wrong type won't compile.

```csharp
TypedDocumentReference<User> document = await collection.AddAsync(user);
```

### Reading Documents

```csharp
TypedDocumentSnapshot<User> snapshot = await document.GetSnapshotAsync();
bool documentExists = snapshot.Exists;

// Individual fields can be checked and fetched
bool hasAge = snapshot.ContainsField(user => user.Age);
string city = snapshot.GetValue(user => user.Location.City);

// Or get the deserialized object
User user = snapshot.Object;
```

### Updating Specific Fields

The typed client enforces the correct value type for each field. Passing the wrong type won't compile.

#### Typed Client

Multi field update:

```csharp
UpdateDefinition<User> update = new UpdateDefinition<User>()
    .Set(user => user.Age, 18)
    .Set(user => user.Location.Country, "Spain");

WriteResult result = await document.UpdateAsync(update);
```

Single field update:

```csharp
WriteResult result = await document.UpdateAsync(user => user.Age, 18);
```

#### Official Client

Multi field update:

```csharp
// Note that here we need to refer to the custom field name "home_country",
// while with the typed client the reference to the custom name is automatic
Dictionary<FieldPath, object> updates = new Dictionary<FieldPath, object>
{
    { new FieldPath("Age"), 18 },
    { new FieldPath("Location.home_country"), "Spain" }
};
WriteResult result = await document.UpdateAsync(updates);
```

Single field update:

```csharp
WriteResult result = await document.UpdateAsync("Age", 18);
```

### Replace Document with Optional Merge

```csharp
// Replaces document, with non specified fields having the default value
await document.SetAsync(newUser);

// Sets Age and FirstName, keeping everything else as it was
await document.SetAsync(newUser, TypedSetOptions<User>.MergeFields(u => u.Age, u => u.FirstName));

// The untyped way of merging using anonymous classes is also supported
await document.SetAsync(new { Age = 38, FirstName = "John" }, SetOptions.MergeAll);
```

### Querying

#### Typed Client

```csharp
TypedCollectionReference<User> collection = db.TypedCollection<User>("users");

// A TypedCollectionReference<User> is a TypedQuery<User>, so we can fetch everything
TypedQuerySnapshot<User> allUsers = await collection.GetSnapshotAsync();
foreach (TypedDocumentSnapshot<User> document in allUsers.Documents)
{
    User user = document.Object;
}

// Filters, ordering, and pagination are supported
TypedQuery<User> adultsFromPortugalQuery = collection
    .OrderBy(user => user.Age)
    .WhereGreaterThanOrEqualTo(user => user.Age, 18)
    .WhereEqualTo(user => user.Location.Country, "Portugal")
    .OrderByDescending(user => user.Age);

TypedQuerySnapshot<User> results = await adultsFromPortugalQuery.GetSnapshotAsync();
foreach (TypedDocumentSnapshot<User> document in results.Documents)
{
    User user = document.Object;
    Console.WriteLine($"{user.FirstName}: {user.SecondName}");
}
```

#### Official Client

```csharp
CollectionReference collection = db.Collection("users");

QuerySnapshot allUsers = await collection.GetSnapshotAsync();
foreach (DocumentSnapshot document in allUsers.Documents)
{
    User user = document.ConvertTo<User>();
}

Query adultsFromPortugalQuery = collection
    .OrderBy("Age")
    .WhereGreaterThanOrEqualTo("Age", 18)
    .WhereEqualTo("Location.home_country", "Portugal")
    .OrderByDescending("Age");

QuerySnapshot results = await adultsFromPortugalQuery.GetSnapshotAsync();
foreach (DocumentSnapshot document in results.Documents)
{
    User user = document.ConvertTo<User>();
    Console.WriteLine($"{user.FirstName}: {user.SecondName}");
}
```

### Deleting Documents

Works the same as the official client. See the [Official Documentation](https://cloud.google.com/dotnet/docs/reference/Google.Cloud.Firestore/latest/userguide#deleting-a-document).

```csharp
await document.DeleteAsync();
```

---

Everything else (transactions, listeners, batched writes) works exactly as the official client. For more details, see the [Official Documentation](https://cloud.google.com/dotnet/docs/reference/Google.Cloud.Firestore/latest).

## Benchmarks

This section compares the Official Client performance against the Typed client.
The benchmarks were performed using the Firestore Emulator with the following setup:

```
Windows 11, AMD Ryzen 5 7600X 4.70GHz, 6 cores
.NET SDK 10.0.101, .NET 10.0.1 (x64 RyuJIT x86-64-v4)
BenchmarkDotNet v0.15.8
```

### 1. Lambda Field Translator Benchmark

The only real overhead of the Typed Client is translating lambda expressions into Firestore field names.
Below are the results of translating a **SimpleField** `u => u.FirstName` and a **NestedField** `u => u.Location.Country`.

```
| Method      | Mean       | Error   | StdDev  | Gen0   | Allocated |
|------------ |-----------:|--------:|--------:|-------:|----------:|
| SimpleField |   445.7 ns | 5.11 ns | 4.53 ns | 0.0401 |     672 B |
| NestedField | 1,017.5 ns | 6.40 ns | 5.00 ns | 0.0725 |    1232 B |
```

### 2. Single Entity Benchmark

This benchmark consists of creating a user, getting it by id, and querying by country.
Each operation is run 50 times with interleaved typed/official calls for fair comparison.

```
| Method         |     Mean |
|--------------- |---------:|
| TypedClient    | 5.69 ms  |
| OfficialClient | 6.07 ms  |
```

### 3. Multiple Entities Benchmark

This benchmark consists of inserting users in batch, querying by `Age` and `Country`,
fetching all documents, and cleaning up. Each size is run 10 times with interleaved calls.

```
| Users | TypedClient |  OfficialClient |  Diff |
|------:|------------:|----------------:|------:|
|     1 |     7.22 ms |         6.44 ms | +12%  |
|     5 |     8.02 ms |         7.60 ms |  +6%  |
|    10 |     7.41 ms |         7.35 ms |  +1%  |
|    50 |    75.25 ms |        83.21 ms | -10%  |
|   100 |   101.01 ms |        97.14 ms |  +4%  |
```

The differences between the TypedClient and the Official one fluctuate in both directions
and fall within normal variance for I/O-bound emulator operations. The TypedClient uses the
OfficialClient underneath, so the performance overhead is negligible.
