using Adletec.Sonic.Benchmark.Executors;
using Adletec.Sonic.Benchmark.Executors.Interpreted;
using Adletec.Sonic.Benchmark.Expressions;
using Adletec.Sonic.Benchmark.Expressions.Defaults;
using Adletec.Sonic.Benchmark.Values;
using Adletec.Sonic.Benchmark.Executors.Defaults;
using BenchmarkDotNet.Attributes;

namespace Adletec.Sonic.Benchmark.Benchmarks;

/// <summary>
/// Compares the performance of the interpreted evaluation of expressions.
/// </summary>
public class CompareInterpretedBenchmark
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
        new JaceInterpretedBenchmarkExecutor(),
        new SonicInterpretedBenchmarkExecutor(),
        new NCalcInterpretedBenchmarkExecutor()
    };


    /// <summary>
    /// All expressions to run the benchmark with.
    /// </summary>
    public IEnumerable<BenchmarkExpression> ExpressionValues => new DefaultExpressionProvider().GetExpressions();

    [Benchmark]
    public void CompareDefaults() =>
        Executor.RunBenchmark(Expression.GetExpression(Executor.Dialect), Expression.VariableNames, Iterations,
            new IncrementingValueProvider());
}