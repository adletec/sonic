using Adletec.Sonic.Benchmark2.Executors.Defaults;
using Adletec.Sonic.Benchmark2.Expressions;
using Adletec.Sonic.Benchmark2.Values;

namespace Adletec.Sonic.Benchmark2.Executors.CaseInsensitive;


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
    public ExpressionDialect Dialect => ExpressionDialect.Sonic;
    
    public override string ToString() => "Jace (C/I)";
}