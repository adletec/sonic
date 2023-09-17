using Adletec.Sonic.Benchmark.Executors;
using Adletec.Sonic.Benchmark.Executors.CaseInsensitive;
using Adletec.Sonic.Benchmark.Executors.Defaults;
using Adletec.Sonic.Benchmark.Executors.GuardedMode;
using Adletec.Sonic.Benchmark.Executors.Interpreted;
using Adletec.Sonic.Benchmark.Executors.NoCache;
using Adletec.Sonic.Benchmark.Expressions;
using Adletec.Sonic.Benchmark.Expressions.Fixed;
using Adletec.Sonic.Benchmark.Values;
using BenchmarkDotNet.Attributes;

namespace Adletec.Sonic.Benchmark.Benchmarks;

/// <summary>
/// Compares the performance of the evaluation of expressions using each engines default settings.
/// </summary>
[MedianColumn]
public class SonicBenchmark
{
    [ParamsSource(nameof(ExpressionValues))]
    public BenchmarkExpression Expression { get; set; } = null!;

    [ParamsSource(nameof(BenchmarkExecutors))]
    public IBenchmarkExecutor Executor { get; set; } = null!;

    public long Iterations { get; set; } = 2000000L;

    /// <summary>
    /// All benchmark executors to run the benchmark with.
    /// </summary>
    public IEnumerable<IBenchmarkExecutor> BenchmarkExecutors => new List<IBenchmarkExecutor>
    {
        new SonicDefaultsBenchmarkExecutor(),
        new SonicNoCacheBenchmarkExecutor(),
        new SonicGuardedModeBenchmarkExecutor(),
        new SonicCaseInsensitiveBenchmarkExecutor(),
        new SonicInterpretedBenchmarkExecutor(),
        new SonicEvaluateDefaultsBenchmarkExecutor()
    };


    /// <summary>
    /// All expressions to run the benchmark with.
    /// </summary>
    public IEnumerable<BenchmarkExpression> ExpressionValues => new DefaultFixedExpressionProvider().GetExpressions();

    [Benchmark]
    public void CompareDefaults() =>
        Executor.RunBenchmark(Expression.GetExpression(Executor.Dialect), Expression.VariableNames, Iterations,
            new IncrementingValueProvider());
}