using System.Linq.Expressions;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

using Firestore.Typed.Client.Tests.Model;
using Firestore.Typed.Client.Visitor;

namespace Firestore.Typed.Client.Benchmarks;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class FieldVisitorBenchmark
{

    [Benchmark]
    public string SimpleField()
    {
        var fieldNameVisitor = new FieldNameVisitor();
        Expression<Func<User, string>> expr = user => user.FirstName;
        fieldNameVisitor.Visit(expr);
        return fieldNameVisitor.FieldName;
    }

    [Benchmark]
    public string NestedField()
    {
        var fieldNameVisitor = new FieldNameVisitor();
        Expression<Func<User, string>> expr = user => user.Location.Country;
        fieldNameVisitor.Visit(expr);
        return fieldNameVisitor.FieldName;
    }
}