# Firestore .NET Typed Client

**A light-weight, strongly-typed Firestore client that allows you to catch query errors before your clients catch
them :)**

Since this project wraps the official Firestore Client,
please read the [Official Documentation](https://cloud.google.com/dotnet/docs/reference/Google.Cloud.Firestore/latest)
if you have any question.

---
This .NET Firestore client wraps the official Firestore client made available by Google, adding typed queries (similar
to how the .NET MongoDb Driver does it).
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

Assuming we have a collection of users with the above schema, we now compare a few examples with the typed and official
client:

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

A frequent use-case when dealing with collection is to have always the same type of object, for this reason, when we
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

