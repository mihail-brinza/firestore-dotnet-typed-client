using BenchmarkDotNet.Running;

using Firestore.Typed.Client.Benchmarks;

if (args.Contains("--comparer"))
{
    await ClientsComparerBenchmark.Run();
}
else if (args.Contains("--single"))
{
    await SingleQueryCompareBenchmark.Run();
}
else if (args.Contains("--field"))
{
    BenchmarkRunner.Run<FieldVisitorBenchmark>();
}
else
{
    Console.WriteLine("Usage:");
    Console.WriteLine("  --field      Run FieldVisitor micro-benchmark (BenchmarkDotNet)");
    Console.WriteLine("  --comparer   Run multi-entity Typed vs Official comparison");
    Console.WriteLine("  --single     Run single-entity Typed vs Official comparison");
}
