using Adletec.Sonic.Benchmark.Executors;
using Adletec.Sonic.Benchmark.Expressions.Generating;
using Adletec.Sonic.Benchmark.Values;
using BenchmarkDotNet.Attributes;

namespace Adletec.Sonic.Benchmark.Benchmarks.Sonic;

/// <summary>
/// Compares the performance of the evaluation of expressions using a range of settings.
///
/// The benchmarks are run as single pass, i.e. the expression is parsed and evaluated once. The focus of this
/// benchmark is to compare the parsing performance of Sonic using different settings.
/// </summary>
[MedianColumn]
public class SonicSinglePassBenchmark
{
    [Params(10000)]
    public int ExpressionCount { get; set; }
    
    [ParamsAllValues]
    public bool CaseSensitive { get; set; }
    
    [ParamsAllValues]
    public bool Interpreted { get; set; }
    
    [ParamsAllValues]
    public bool Cached { get; set; }
    
    [ParamsAllValues]
    public bool Optimize { get; set; }
    
    [Params (false)]
    public bool Guarded { get; set; }
    
    [Params (false)]
    public bool Validate { get; set; }


    [Benchmark (Description = "Sonic [using Evaluate]", Baseline = true)]
    public void SonicEvaluate() => RunBenchmark(new SonicEvaluateBenchmarkExecutor(CaseSensitive, Interpreted, Cached, Optimize, Guarded, Validate));
    
    [Benchmark (Description = "Sonic [using CreateDelegate]")]
    public void SonicDelegate() => RunBenchmark(new SonicDelegateBenchmarkExecutor(CaseSensitive, Interpreted, Cached, Optimize, Guarded, Validate));
    
    private void RunBenchmark(IBenchmarkExecutor executor)
    {
        executor.RunBenchmark(new GeneratingExpressionProvider(12).GetExpressions(ExpressionCount), new IncrementingValueProvider(), 1);
    }
}
    
    