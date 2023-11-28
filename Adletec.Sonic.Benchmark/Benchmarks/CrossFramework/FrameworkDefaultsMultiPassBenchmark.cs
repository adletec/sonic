using Adletec.Sonic.Benchmark.Executors;
using Adletec.Sonic.Benchmark.Executors.CrossFramework;
using Adletec.Sonic.Benchmark.Expressions;
using Adletec.Sonic.Benchmark.Expressions.Fixed;
using Adletec.Sonic.Benchmark.Values;
using BenchmarkDotNet.Attributes;

namespace Adletec.Sonic.Benchmark.Benchmarks.CrossFramework;

/// <summary>
/// Compares the performance of the evaluation of expressions using each engines default settings.
/// 
/// The benchmarks are run as multi pass, i.e. the expression is parsed once and then evaluated multiple times with
/// different values. The focus of this benchmark is to compare the evaluation performance of Sonic using different
/// settings.
///
/// This is the most common use case for Sonic and the the results are the most relevant for most users.
/// </summary>
[DryJob]
[MedianColumn]
public class FrameworkDefaultsMultiPassBenchmark
{
    [ParamsSource(nameof(ExpressionValues))]
    public BenchmarkExpression Expression { get; set; } = null!;

    [Params(100000L)]
    public long Iterations { get; set; }

    /// <summary>
    /// All expressions to run the benchmark with.
    /// </summary>
    public IEnumerable<BenchmarkExpression> ExpressionValues => new DefaultFixedExpressionProvider().GetExpressions();

    // Native Expression (as reference point)
    [Benchmark(Description = "Native Expression (C#)")]
    public void Native() => RunBenchmark(new NativeBenchmarkExecutor());

    // Default Settings
    [Benchmark(Description = "Sonic [using CreateDelegate] (Default Settings)", Baseline = true)]
    public void SonicDefaultsDelegate() => RunBenchmark(new SonicDelegateBenchmarkExecutor());
    
    [Benchmark(Description = "Sonic [using Evaluate] (Default Settings)")]
    public void SonicDefaultsEvaluate() => RunBenchmark(new SonicEvaluateBenchmarkExecutor());

    [Benchmark(Description = "Jace [using CreateDelegate] (Default Settings)")]
    public void JaceDefaultsDelegate() => RunBenchmark(new JaceDelegateBenchmarkExecutor());

    [Benchmark(Description = "Jace [using Calculate] (Default Settings)")]
    public void JaceDefaultsCalculate() => RunBenchmark(new JaceCalculateBenchmarkExecutor());

    [Benchmark(Description = "NCalc [using Expression] (Default Settings)")]
    public void NCalcDefaults() => RunBenchmark(new NCalcBenchmarkExecutor());

    [Benchmark(Description = "codingseb ExpressionEvaluator (Default Settings)")]
    public void CodingsebDefaults() => RunBenchmark(new CodingsebBenchmarkExecutor());

    [Benchmark(Description = "DynamicExpresso (Default Settings)")]
    public void DynamicExpressoDefaults() => RunBenchmark(new DynamicExpressoBenchmarkExecutor());

    private void RunBenchmark(IBenchmarkExecutor executor)
    {
        executor.RunBenchmark(Expression, new IncrementingValueProvider(), Iterations);
    }
}