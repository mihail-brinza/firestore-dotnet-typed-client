using System;
using System.Linq.Expressions;

using Firestore.Typed.Client.Tests.Model;
using Firestore.Typed.Client.Visitor;

using FluentAssertions;

using Xunit;

namespace Firestore.Typed.Client.Tests
{
    public class FieldVisitorTests
    {
        [Fact]
        public void SimpleFieldAccessTest()
        {
            Expression<Func<User, string>> expr = user => user.FirstName;
            var visitor = new FieldNameVisitor();
            visitor.Visit(expr);

            visitor.FieldName.Should().Be(nameof(User.FirstName));
        }

        [Fact]
        public void CustomFieldNameTest()
        {
            Expression<Func<User, string>> expr = user => user.SecondName;
            var visitor = new FieldNameVisitor();
            visitor.Visit(expr);

            visitor.FieldName.Should().Be(User.SecondNameCustomField);
        }

        [Fact]
        public void NestedFieldNameTest()
        {
            Expression<Func<User, string>> expr = user => user.Location.City;
            var visitor = new FieldNameVisitor();
            visitor.Visit(expr);

            visitor.FieldName.Should().Be("Location.City");
        }

        [Fact]
        public void NestedFieldNameWithCustomNameTest()
        {
            Expression<Func<User, string>> expr = user => user.Location.Country;
            var visitor = new FieldNameVisitor();
            visitor.Visit(expr);

            visitor.FieldName.Should().Be($"Location.{Location.CountryCustomName}");
        }
    }
}