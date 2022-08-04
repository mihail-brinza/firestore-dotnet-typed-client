# Firestore .NET Typed Client

**A light-weight, strongly-typed Firestore client that allows you to catch query errors before your clients catch
them :)**

Since this project wraps the official Firestore Client,
please read the [Official Documentation](https://cloud.google.com/dotnet/docs/reference/Google.Cloud.Firestore/latest)
if you have any question.

---

## Overview

This .NET Firestore client wraps the official Firestore client made available by Google, adding typed queries (similar
to how the .NET MongoDb Driver does it). By having typed queries we no longer need to reference Field Names using
hard-coded strings, which can lead to undetected errors in compile-time.

Although Firestore is a NoSQL, schemaless, database, we often have the need for a collection to hold a set of documents
with the same schema that we define using a domain object.

This documentation will have the following data structure as example:

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
Console.WriteLine(snapshot.GetValue(user => user.FirstName));
Console.WriteLine(snapshot.GetValue(user => user.Age));
Console.WriteLine(snapshot.GetValue(user => user.Location.Country));

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

### Create Database

Creating the database remains equal to the official client:

```csharp
FirestoreDb db = FirestoreDb.Create(projectId);
```

### Access Collection

Using **FirestoreDb** you can create a **TypedCollection\<TDocument>** by the path from the database root:

```csharp
// with Typed Client
TypedCollectionReference<User> collection = db.TypedCollection<User>("users"); 

// official client
CollectionReference collection = db.Collection("users");
```

### Creating a document

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

### Updating specific fields




