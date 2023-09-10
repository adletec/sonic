using Adletec.Sonic.Benchmark.Expressions;
using Adletec.Sonic.Benchmark.Values;
using Adletec.Sonic.Execution;

namespace Adletec.Sonic.Benchmark.Executors.Interpreted;

public class SonicInterpretedBenchmarkExecutor : IBenchmarkExecutor
{
    public void RunBenchmark(string expression, List<string> variableNames, long iterations, IValueProvider valueProvider)
    {
        var engine = CalculationEngine.Create().UseExecutionMode(ExecutionMode.Interpreted).Build();
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

    public override string ToString() => "Sonic (C/S Interpreted)";
}