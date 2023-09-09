using Jace;

namespace Adletec.Sonic.Benchmark2;

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
    
    public override string ToString() => "Jace (CaseSensitive, Compiled)";
}