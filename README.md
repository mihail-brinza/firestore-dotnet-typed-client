# Firestore .NET Typed Client

![GitHub Workflow Status (with event)](https://img.shields.io/github/actions/workflow/status/mihail-brinza/firestore-dotnet-typed-client/build_and_tests.yml?style=flat-square&label=build%20%26%20tests)
![](https://img.shields.io/github/license/mihail-brinza/firestore-dotnet-typed-client?style=flat-square)
![](https://img.shields.io/nuget/dt/Firestore.Typed.Client?style=flat-square)
![](https://img.shields.io/nuget/v/Firestore.Typed.Client?style=flat-square)

**A light-weight, strongly-typed Firestore client that allows you to catch query errors before your clients catch
them :)**

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

## Overview

Although Firestore is a NoSQL, schemaless, database, we often have the need for a collection to hold a set of documents
with the same schema that we define using a domain object.

This .NET Firestore client wraps the official Firestore client made available by Google, adding typed queries (similar
to how the .NET MongoDb Driver does it). By having typed queries we no longer need to reference Field Names using
hard-coded strings, which can lead to undetected errors in compile-time.

Instead the Field Names are automatically calculated from a lambda expression, adding type safety to our queries.

Since this project wraps the official Firestore Client, it supports everything that the official client does and uses
all the security and best practices implemented by Google.
For more information about how the official firestore client works please read
the [Official Documentation](https://cloud.google.com/dotnet/docs/reference/Google.Cloud.Firestore/latest).

This documentation compares the Typed Client with the Official one in a few aspects and it has the following data
structure as example:

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

For more information about how DataModeling works, please refer to
the [Data Model Official Documentation](https://cloud.google.com/dotnet/docs/reference/Google.Cloud.Firestore/latest/datamodel)
.

### Sample Code

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
    // access user
    User? userResult = queryResult.Object;
}
```

Furthermore, we can also have custom property names, which are taken care of automatically by this client, and not by
the official one.

Assuming we have a collection of users with the above schema, we now compare a few examples with the typed and official
client:

---

## Create Database

Creating the database remains equal to the official client:

```csharp
FirestoreDb db = FirestoreDb.Create(projectId);
```

## Access Collection

Using `FirestoreDb` you can create a `TypedCollection<TDocument>` by the path from the database root:

#### Typed Client

```csharp
TypedCollectionReference<User> collection = db.TypedCollection<User>("users"); 
```

#### Official Client

```csharp
CollectionReference collection = db.Collection("users");
```

## Creating a document

A frequent use-case when dealing with collections is for each collection to hold one specific type of documents, for
this reason, when we
have a `TypedCollection<TDocument>` we can only add documents of type `TDocument` to it.
Take the next example:

```csharp
User user = new User
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
```

#### Typed Client

```csharp
TypedCollectionReference<User> collection = db.TypedCollection<User>("users");
// AddAsync only accepts the User type and will not compile with any other type
TypedDocumentReference<User> document = await collection.AddAsync(user); 
```

#### Official Client

```csharp
CollectionReference collection = db.Collection("users");
// AddAsync accepts any object, not only users
DocumentReference = await collection.AddAsync(user); 
```

## Updating specific fields

Similarly to the official client, updating only specific field is also supported:

#### Typed Client

Multi Field update:

```csharp
// The methods only allow to Set a value of the type of the property:
// int in the case of Age and string in case of Coutry, not compiling otherwise
UpdateDefinition<User> update = new UpdateDefinition<User>()
.Set(user => user.Age, 18)
.Set(user => user.Location.Country, "Spain");

WriteResult result = await document.UpdateAsync(update);
```

Single Field update:

```csharp
 WriteResult result = await document.UpdateAsync(user => user.Age, 18);
```

#### Official Client

Multi Field update:

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

Single Field update:

```csharp
 WriteResult result = await document.UpdateAsync("Age", 18);
```

## Replace document data with optional merge

The `SetAsync` method replaces all data by default, however, we can also Merge all fields as shown below:

#### Typed Client

```csharp
User newUser = new User
{
    FirstName  = "John",
    SecondName = "DoeDoe",
    Age        = 38
};

// Replaces document, with non specified field having the default value
await document.SetAsync(newUser); 

// Sets Age and FirstName, keeping everything else as it was
await document.SetAsync(newUser, TypedSetOptions<User>.MergeFields(u => u.Age, u => u.FirstName));

// For flexibility, the typed client also keeps the official untyped way of merging, 
// using anonymous classes
await document.SetAsync( new { Age = 38, FirstName = "John" }, SetOptions.MergeAll);
```

## Deleting Documents

Deleting document is works exactly the same in both clients, please refer to
the [Official Documentation](https://cloud.google.com/dotnet/docs/reference/Google.Cloud.Firestore/latest/userguide#deleting-a-document)
for more information.

```csharp
await document.DeleteAsync();
```

## Reading documents

#### Typed Client

```csharp
TypedDocumentSnapshot<User> snapshot = await document.GetSnapshotAsync();
// Even if there's no document in the server, we still get a snapshot
// back - but it knows the document doesn't exist.
bool documentExists = snapshot.Exists;

// Individual fields can be checked and fetched
bool hasAge = snapshot.ContainsField(user => user.Age); 
string city = snapshot.GetValue(user => user.Location.City)); 

// Or you can get an instance of the deserialized data
User user = snapshot.Object;
```

#### Official Client

```csharp
DocumentSnapshot snapshot = await document.GetSnapshotAsync();

bool documentExists = snapshot.Exists;

bool hasAge = snapshot.ContainsField("Age"); 
string city = snapshot.GetValue<string>("Location.City")); 

User user = snapshot.ConvertTo<User>();
```

### Query

#### Typed Client

```csharp
FirestoreDb db = FirestoreDb.Create(projectId);
TypedCollectionReference<User> collection = db.TypedCollection<User>("users");

// A TypedCollectionReference<User> is a TypedQuery<User>, so we can just fetch everything
TypedQuerySnapshot<User> allUsers = await collection.GetSnapshotAsync();
foreach (TypedDocumentSnapshot<User> document in allUsers.Documents)
{
    User user = document.Object; 
}

// Filters, Ordering, etc. are also supported
TypedQuery<User> adultsFromPortugalQuery = collection
    .OrderBy(user => user.Age)
    .WhereGreaterThanOrEqualTo(user => user.Age, 18)
    .WhereEqualTo(user => user.Location.Country, "Portugal")
    .OrderByDescending(user => user.Age);

TypedQuerySnapshot<User> bigCities = await adultsFromPortugalQuery.GetSnapshotAsync();
foreach (TypedDocumentSnapshot<User> document in bigCities.Documents)
{
    // Do anything you'd normally do with a DocumentSnapshot
    User user = document.Object;
    Console.WriteLine($"{user.FirstName}: {user.SecondName}");
}
```

#### Official Client

```csharp
FirestoreDb db = FirestoreDb.Create(projectId);
CollectionReference collection = db.Collection("users");

QuerySnapshot allUsers = await collection.GetSnapshotAsync();
foreach (DocumentSnapshot document in allUsers.Documents)
{
    User user = document.ConvertTo<User>(); 
}

// Filters, Ordering, etc. are also supported
Query adultsFromPortugalQuery = collection
    .OrderBy("Age")
    .WhereGreaterThanOrEqualTo("Age", 18)
    .WhereEqualTo("Location.home_country", "Portugal")
    .OrderByDescending("Age");

QuerySnapshot bigCities = await adultsFromPortugalQuery.GetSnapshotAsync();
foreach (DocumentSnapshot document in bigCities.Documents)
{
    User user = document.ConvertTo<User>();
    Console.WriteLine($"{user.FirstName}: {user.SecondName}");
}
```

---
Everything else (Transactions, Listeners) works exactly as before.

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