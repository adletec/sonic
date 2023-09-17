using Adletec.Sonic.Benchmark.Executors.Defaults;
using Adletec.Sonic.Benchmark.Expressions;
using Adletec.Sonic.Benchmark.Values;

namespace Adletec.Sonic.Benchmark.Executors.CaseSensitive;

/// <summary>
/// Executes a benchmark using the NCalc library with (case-sensitive) default settings.
/// </summary>
public class NCalcCaseSensitiveBenchmarkExecutor : IBenchmarkExecutor
{
    private readonly IBenchmarkExecutor executor = new NCalcDefaultsBenchmarkExecutor();
    public void RunBenchmark(string expression, List<string> variableNames, long iterations, IValueProvider valueProvider)
    {
        executor.RunBenchmark(expression, variableNames, iterations, valueProvider);
    }

    public void RunBenchmark(IEnumerable<BenchmarkExpression> expressions, IValueProvider valueProvider, long iterations)
    {
        executor.RunBenchmark(expressions, valueProvider, iterations);
    }

    public ExpressionDialect Dialect => ExpressionDialect.NCalc;
    
    public override string ToString() => "NCalc (C/S)";
}