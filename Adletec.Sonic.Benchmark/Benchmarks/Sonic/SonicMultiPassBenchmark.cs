using Adletec.Sonic.Benchmark.Executors;
using Adletec.Sonic.Benchmark.Expressions;
using Adletec.Sonic.Benchmark.Expressions.Fixed;
using Adletec.Sonic.Benchmark.Values;
using BenchmarkDotNet.Attributes;

namespace Adletec.Sonic.Benchmark.Benchmarks.Sonic;

/// <summary>
/// Compares the performance of the evaluation of expressions using a range of settings.
/// 
/// The benchmarks are run as multi pass, i.e. the expression is parsed once and then evaluated multiple times with
/// different values. The focus of this benchmark is to compare the evaluation performance of Sonic using different
/// settings.
///
/// This is the most common use case for Sonic and the the results are the most relevant for most users.
/// </summary>
[MedianColumn]
public class SonicMultiPassBenchmark
{
    [ParamsSource(nameof(ExpressionValues))]
    public BenchmarkExpression Expression { get; set; } = null!;

    [Params(100000L)] public long Iterations { get; set; }

    [ParamsAllValues]
    public bool CaseSensitive { get; set; }

    [ParamsAllValues]
    public bool Interpreted { get; set; }

    [ParamsAllValues]
    public bool Cached { get; set; }

    [ParamsAllValues]
    public bool Guarded { get; set; }

    [ParamsAllValues]
    public bool Optimize { get; set; }

    /// <summary>
    /// All expressions to run the benchmark with.
    /// </summary>
    public IEnumerable<BenchmarkExpression> ExpressionValues => new DefaultFixedExpressionProvider().GetExpressions();

    [Benchmark(Description = "Delegate [using CreateDelegate]", Baseline = true)]
    public void SonicDelegate() => RunBenchmark(new SonicDelegateBenchmarkExecutor(CaseSensitive, Interpreted, Cached, Optimize, Guarded));

    [Benchmark(Description = "Delegate [using Evaluate]")]
    public void SonicEvaluate() => RunBenchmark(new SonicEvaluateBenchmarkExecutor(CaseSensitive, Interpreted, Cached, Optimize, Guarded));


    private void RunBenchmark(IBenchmarkExecutor executor)
    {
        executor.RunBenchmark(Expression, new IncrementingValueProvider(), Iterations);
    }
}