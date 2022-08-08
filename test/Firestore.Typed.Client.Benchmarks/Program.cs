using BenchmarkDotNet.Running;

namespace Firestore.Typed.Client.Benchmarks;

public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine(Environment.GetEnvironmentVariable("FIRESTORE_EMULATOR_HOST"));
        Console.WriteLine(Environment.GetEnvironmentVariable("FIRESTORE_PROJECT_ID"));
        BenchmarkRunner.Run<ClientsComparerBenchmark>();
        //  BenchmarkRunner.Run<SingleQueryCompareBenchmark>();
        // BenchmarkRunner.Run<FieldVisitorBenchmark>();
    }
}