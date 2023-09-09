// See https://aka.ms/new-console-template for more information

// using Adletec.Sonic.Benchmark2;
//
// var benchmarkExecutors = new List<IBenchmarkExecutor>
// {
//     new JaceCaseSensitiveCompiledBenchmarkExecutor(),
//     new JaceCaseSensitiveCompiledBenchmarkExecutor(),
//     new SonicDefaultsBenchmarkExecutor(),
//     new SonicDefaultsBenchmarkExecutor(),
//     new JaceCaseSensitiveCompiledBenchmarkExecutor(),
//     new JaceCaseSensitiveCompiledBenchmarkExecutor(),
//     new SonicDefaultsBenchmarkExecutor(),
//     new SonicDefaultsBenchmarkExecutor(),
//     // new JaceDefaultsBenchmarkExecutor()
// };
//
// var iterations = 200000000L;
//
// foreach (var expression in expressions)
// {
//     foreach (var executor in benchmarkExecutors)
//     {
//         var result = MeasuringUtil.MeasureExecutionTime(() =>
//             executor.RunBenchmark(expression.Key, expression.Value, iterations, new IncrementingValueProvider())
//         );
//         Console.WriteLine($"{executor.GetType().Name} - {expression.Key} - {result} ms");
//     }
// }

using BenchmarkDotNet.Running;

namespace Adletec.Sonic.Benchmark2;

public class Program
{
    public static void Main(string[] args)
    {
        BenchmarkRunner.Run<CalculationEngineBenchmark>();
    }
}