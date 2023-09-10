using Adletec.Sonic.Benchmark2.Expressions;
using Adletec.Sonic.Benchmark2.Values;
using Jace;
using Jace.Execution;

namespace Adletec.Sonic.Benchmark2.Executors.Defaults;

public class JaceInterpretedBenchmarkExecutor : IBenchmarkExecutor
{
    public void RunBenchmark(string expression, List<string> variableNames, long iterations,
        IValueProvider valueProvider)
    {
        var engine = new Jace.CalculationEngine(
            new JaceOptions()
            {
                ExecutionMode = ExecutionMode.Interpreted,
                CaseSensitive = true
            });
        
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

    public override string ToString() => "Jace (C/S Interpreted)";
}