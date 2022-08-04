using System;

namespace Firestore.Typed.Client.Tests;

public class User
{
    public string FirstName { get; set; }
    public string Surname { get; set; }
    public int Age { get; set; }
    public DateTime BirthDate { get; set; }
}