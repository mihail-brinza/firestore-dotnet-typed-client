using Bogus;

using Firestore.Typed.Client.Tests.Model;

namespace Firestore.Typed.Client.Tests.Utils;

public sealed class UserFaker : Faker<User>
{
    public UserFaker()
    {
        RuleFor(o => o.Age, f => f.Random.Number(1, 119));
        RuleFor(o => o.FirstName, f => f.Person.FirstName);
        RuleFor(o => o.LastName, f => f.Person.LastName);
        RuleFor(o => o.PhoneNumbers, f => new[]
        {
            f.Phone.PhoneNumber(),
            f.Phone.PhoneNumber(),
            f.Phone.PhoneNumber(),
            f.Phone.PhoneNumber(),
        });
        RuleFor(o => o.FullName, (f, u) => $"{u.FirstName} {u.LastName}");
        RuleFor(o => o.Location, f => new Location
        {
            City    = f.Address.City(),
            Country = f.Address.Country(),
        });
    }
}