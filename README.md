# Firestore .NET Typed Client

**A light-weight, strongly-typed Firestore client that allows you to catch query errors before your clients catch
them :)**

Since this project wraps the official Firestore Client,
please read the [Official Documentation](https://cloud.google.com/dotnet/docs/reference/Google.Cloud.Firestore/latest)
if you have any question.

---
This .NET Firestore client wraps the official Firestore client made available by Google, adding typed queries.
Although Firestore is a NoSQL, schemaless, database, we often have the need for a collection to hold a set of documents
with similar schemas that we define.

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

#### Create Database

Creating the database remains equal to the official client:

```csharp
FirestoreDb db = FirestoreDb.Create(projectId);
```

#### Create Collection

```csharp
// with Typed Client
TypedCollectionReference<User> collection = db.TypedCollection<User>("users"); 

// official way
CollectionReference collection = db.Collection("users");
```

