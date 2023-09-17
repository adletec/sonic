using Adletec.Sonic.Benchmark.Executors.Defaults;
using Adletec.Sonic.Benchmark.Expressions;
using Adletec.Sonic.Benchmark.Values;

namespace Adletec.Sonic.Benchmark.Executors.CaseInsensitive;


/// <summary>
/// Executes a benchmark using the Sonic library with (case-insensitive) default settings.
/// </summary>
public class JaceCaseInsensitiveBenchmarkExecutor : IBenchmarkExecutor
{
    private readonly IBenchmarkExecutor executor = new JaceDefaultsBenchmarkExecutor();
    public void RunBenchmark(string expression, List<string> variableNames, long iterations, IValueProvider valueProvider)
    {
        executor.RunBenchmark(expression, variableNames, iterations, valueProvider);
    }

    public void RunBenchmark(IEnumerable<BenchmarkExpression> expressions, IValueProvider valueProvider, long iterations)
    {
        executor.RunBenchmark(expressions, valueProvider, iterations);
    }

    public ExpressionDialect Dialect => ExpressionDialect.Sonic;
    
    public override string ToString() => "Jace (C/I)";
}