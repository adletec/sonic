using Adletec.Sonic.Benchmark2.Expressions;
using Adletec.Sonic.Benchmark2.Values;

namespace Adletec.Sonic.Benchmark2.Executors.Defaults;

public class SonicDefaultsBenchmarkExecutor : IBenchmarkExecutor
{
    public void RunBenchmark(string expression, List<string> variableNames, long iterations, IValueProvider valueProvider)
    {
        var engine = CalculationEngine.CreateWithDefaults();
        var calculate = engine.CreateDelegate(expression);
        var variables = new Dictionary<string, double>();
        for (var i = 0; i < iterations; i++)
        {
            foreach (var variableName in variableNames)
            {
                variables[variableName] = valueProvider.GetNextValue();
            }
            calculate(variables);
        }
    }

    public ExpressionDialect Dialect => ExpressionDialect.Sonic;

    public override string ToString() => "Sonic (Defaults)";
}