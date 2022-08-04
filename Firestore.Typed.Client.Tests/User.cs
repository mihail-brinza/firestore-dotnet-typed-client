using System;
using System.Collections.Generic;

using Google.Cloud.Firestore;

namespace Firestore.Typed.Client.Tests;

[FirestoreData]
public class User
{
    public const string SecondNameCustomField = "second_name";

    [FirestoreProperty]
    public string FirstName { get; set; }

    [FirestoreProperty(SecondNameCustomField)]
    public string SecondName { get; set; }

    [FirestoreProperty]
    public int Age { get; set; }

    [FirestoreProperty]
    public Location Location { get; set; }
}