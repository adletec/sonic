using Adletec.Sonic.Benchmark.Executors;
using Adletec.Sonic.Benchmark.Executors.CrossFramework;
using Adletec.Sonic.Benchmark.Expressions;
using Adletec.Sonic.Benchmark.Expressions.Fixed;
using Adletec.Sonic.Benchmark.Values;
using BenchmarkDotNet.Attributes;

namespace Adletec.Sonic.Benchmark.Benchmarks.CrossFramework;

/// <summary>
/// Compares the performance of the evaluation of expressions using a range of settings.
/// 
/// The benchmarks are run as multi pass, i.e. the expression is parsed once and then evaluated multiple times with
/// different values. The focus of this benchmark is to compare the evaluation performance of Sonic with other frameworks
/// using different settings.
///
/// This is the most common use case for Sonic and the the results are the most relevant for most users.
/// </summary>
[MedianColumn]
public class FrameworkModesMultiPassBenchmark
{
    [ParamsSource(nameof(ExpressionValues))]
    public BenchmarkExpression Expression { get; set; } = null!;

    [Params(100000L)]
    public long Iterations { get; set; }

    [ParamsAllValues]
    public bool CaseSensitive { get; set; }

    [ParamsAllValues]
    public bool Interpreted { get; set; }

    [ParamsAllValues]
    public bool Cached { get; set; }

    [ParamsAllValues]
    public bool Optimize { get; set; }

    /// <summary>
    /// All expressions to run the benchmark with.
    /// </summary>
    public IEnumerable<BenchmarkExpression> ExpressionValues => new DefaultFixedExpressionProvider().GetExpressions();

    // All frameworks are using a method which creates an evaluable object or delegate from the expression.
    // Disabling the cache _and_ using Evaluate in Sonic or Calculate in Jace will take a lot of time.
    
    [Benchmark(Description = "Sonic [using CreateDelegate]", Baseline = true)]
    public void SonicCaseInsensitive() => RunBenchmark(new SonicEvaluateBenchmarkExecutor(CaseSensitive, Interpreted, Cached, Optimize));

    [Benchmark(Description = "Jace [using Build]")]
    public void JaceCaseInsensitive() => RunBenchmark(new JaceDelegateBenchmarkExecutor(CaseSensitive, Interpreted, Cached, Optimize));

    [Benchmark(Description = "NCalc [using Expression]")]
    public void NCalcCaseInsensitive() => RunBenchmark(new NCalcBenchmarkExecutor(CaseSensitive, Interpreted, Cached, Optimize));

    private void RunBenchmark(IBenchmarkExecutor executor)
    {
        executor.RunBenchmark(Expression, new IncrementingValueProvider(), Iterations);
    }
}