using Adletec.Sonic.Benchmark2.Executors.Defaults;
using Adletec.Sonic.Benchmark2.Expressions;
using Adletec.Sonic.Benchmark2.Values;

namespace Adletec.Sonic.Benchmark2.Executors.CaseSensitive;

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
    public ExpressionDialect Dialect => ExpressionDialect.Sonic;

    public override string ToString() => "Sonic (C/S)";
}