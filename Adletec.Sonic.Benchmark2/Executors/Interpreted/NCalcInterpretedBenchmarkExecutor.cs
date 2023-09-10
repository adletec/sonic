using Adletec.Sonic.Benchmark2.Expressions;
using Adletec.Sonic.Benchmark2.Values;

namespace Adletec.Sonic.Benchmark2.Executors.Defaults;

public class NCalcInterpretedBenchmarkExecutor : IBenchmarkExecutor
{
    private readonly IBenchmarkExecutor executor = new NCalcDefaultsBenchmarkExecutor();
    public void RunBenchmark(string expression, List<string> variableNames, long iterations, IValueProvider valueProvider)
    {
        executor.RunBenchmark(expression, variableNames, iterations, valueProvider);
    }
    public ExpressionDialect Dialect => ExpressionDialect.NCalc;
    
    public override string ToString() => "NCalc (C/S Interpreted)";
}