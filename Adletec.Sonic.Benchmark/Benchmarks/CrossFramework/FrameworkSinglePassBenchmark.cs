using Adletec.Sonic.Benchmark.Executors;
using Adletec.Sonic.Benchmark.Executors.CrossFramework;
using Adletec.Sonic.Benchmark.Expressions.Generating;
using Adletec.Sonic.Benchmark.Values;
using BenchmarkDotNet.Attributes;

namespace Adletec.Sonic.Benchmark.Benchmarks.Sonic;

/// <summary>
/// Compares the performance of the evaluation of expressions using a range of settings.
///
/// The benchmarks are run as single pass, i.e. the expression is parsed and evaluated once. The focus of this
/// benchmark is to compare the parsing performance of Sonic with other frameworks using different settings.
/// </summary>
[MedianColumn]
public class FrameworkSinglePassBenchmark
{
    [Params(10000)]
    public int ExpressionCount { get; set; }
    
    [ParamsAllValues]
    public bool Interpreted { get; set; }
    
    [ParamsAllValues]
    public bool CacheEnabled { get; set; }
    
    [ParamsAllValues]
    public bool Optimize{ get; set; }

    [Benchmark (Description = "Sonic [using Evaluate]", Baseline = true)]
    public void SonicEvaluate() => RunBenchmark(new SonicEvaluateBenchmarkExecutor(true, Interpreted, CacheEnabled, Optimize));
    
    [Benchmark (Description = "Sonic [using CreateDelegate]")]
    public void SonicDelegate() => RunBenchmark(new SonicDelegateBenchmarkExecutor(true, Interpreted, CacheEnabled, Optimize));
    
    [Benchmark (Description = "Jace [using Calculate]")]
    public void JaceCalculate() => RunBenchmark(new JaceCalculateBenchmarkExecutor(true, Interpreted, CacheEnabled, Optimize));
    
    [Benchmark (Description = "Jace [using Build]")]
    public void JaceDelegate() => RunBenchmark(new JaceDelegateBenchmarkExecutor(true, Interpreted, CacheEnabled, Optimize));
    
    [Benchmark (Description = "NCalc [using Expression]")]
    public void NCalcExpression() => RunBenchmark(new NCalcBenchmarkExecutor(true, Interpreted, CacheEnabled, Optimize));
    
    private void RunBenchmark(IBenchmarkExecutor executor)
    {
        executor.RunBenchmark(new GeneratingExpressionProvider(12).GetExpressions(ExpressionCount), new IncrementingValueProvider(), 1);
    }
}
    
    