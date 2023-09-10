using Adletec.Sonic.Benchmark2.Executors;
using Adletec.Sonic.Benchmark2.Executors.Defaults;
using Adletec.Sonic.Benchmark2.Expressions;
using Adletec.Sonic.Benchmark2.Expressions.Defaults;
using Adletec.Sonic.Benchmark2.Values;
using BenchmarkDotNet.Attributes;

namespace Adletec.Sonic.Benchmark2.Benchmarks;

public class CompareDefaultsBenchmark
{
    [ParamsSource(nameof(ExpressionValues))]
    public BenchmarkExpression Expression { get; set; }

    [ParamsSource(nameof(BenchmarkExecutors))]
    public IBenchmarkExecutor Executor { get; set; }

    public long Iterations { get; set; } = 1000000L;

    /// <summary>
    /// All benchmark executors to run the benchmark with.
    /// </summary>
    public IEnumerable<IBenchmarkExecutor> BenchmarkExecutors => new List<IBenchmarkExecutor>
    {
        new JaceDefaultsBenchmarkExecutor(),
        new SonicDefaultsBenchmarkExecutor(),
        new NCalcDefaultsBenchmarkExecutor()
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