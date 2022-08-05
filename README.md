# Firestore .NET Typed Client

**A light-weight, strongly-typed Firestore client that allows you to catch query errors before your clients catch
them :)**

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

Using **FirestoreDb** you can create a **TypedCollection\<TDocument>** by the path from the database root:

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
have a **TypedCollection\<TDocument>** we can only add documents of type **TDocument** to it.
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

#### Official Client

```csharp
CollectionReference collection = db.Collection("users");
// AddAsync accepts any object, not only users
DocumentReference = await collection.AddAsync(user); 
```

#### Typed Client

```csharp
TypedCollectionReference<User> collection = db.TypedCollection<User>("users");
// AddAsync only accepts the User type and will not compile with any other type
TypedDocumentReference<User> document = await collection.AddAsync(user); 
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
// while with the typed client is automatic
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
TypedQuery<User> adultsFromPortugalQuery = collection
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

