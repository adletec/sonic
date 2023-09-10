using Adletec.Sonic.Benchmark2.Expressions;
using Adletec.Sonic.Benchmark2.Values;

namespace Adletec.Sonic.Benchmark2.Executors.Defaults;

/// <summary>
/// Executes a benchmark using the Jace library with default settings.
///
/// Jace compiles expressions and evaluates them case-insensitively by default.
/// </summary>
public class JaceDefaultsBenchmarkExecutor : IBenchmarkExecutor
{
    public void RunBenchmark(string expression, List<string> variableNames, long iterations, IValueProvider valueProvider)
    {
        var engine = new Jace.CalculationEngine();
        var calculate = engine.Build(expression);
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
    
    public ExpressionDialect Dialect => ExpressionDialect.Jace;
    
    public override string ToString() => "Jace (Defaults)";
}