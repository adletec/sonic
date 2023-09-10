using Adletec.Sonic.Benchmark2.Expressions;
using Adletec.Sonic.Benchmark2.Values;
using Jace;

namespace Adletec.Sonic.Benchmark2.Executors.CaseSensitive;

/// <summary>
/// Executes a benchmark using the Jace library with case-sensitive settings.
/// </summary>
public class JaceCaseSensitiveCompiledBenchmarkExecutor : IBenchmarkExecutor
{
    public void RunBenchmark(string expression, List<string> variableNames, long iterations, IValueProvider valueProvider)
    {
        var engine = new Jace.CalculationEngine(new JaceOptions {CaseSensitive = true});
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
    
    public override string ToString() => "Jace (C/S)";
}