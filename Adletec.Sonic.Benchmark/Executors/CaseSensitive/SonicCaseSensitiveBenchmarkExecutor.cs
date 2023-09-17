using Adletec.Sonic.Benchmark.Executors.Defaults;
using Adletec.Sonic.Benchmark.Expressions;
using Adletec.Sonic.Benchmark.Values;

namespace Adletec.Sonic.Benchmark.Executors.CaseSensitive;

/// <summary>
/// Executes a benchmark using the sonic library with (case-sensitive) default settings.
/// </summary>
public class SonicCaseSensitiveBenchmarkExecutor : IBenchmarkExecutor
{
    private readonly IBenchmarkExecutor executor = new SonicDefaultsBenchmarkExecutor();
    public void RunBenchmark(string expression, List<string> variableNames, long iterations, IValueProvider valueProvider)
    {
        executor.RunBenchmark(expression, variableNames, iterations, valueProvider);
    }

    public void RunBenchmark(IEnumerable<BenchmarkExpression> expressions, IValueProvider valueProvider, long iterations)
    {
        executor.RunBenchmark(expressions, valueProvider, iterations);
    }

    public ExpressionDialect Dialect => ExpressionDialect.Sonic;

    public override string ToString() => "Sonic (C/S)";
}