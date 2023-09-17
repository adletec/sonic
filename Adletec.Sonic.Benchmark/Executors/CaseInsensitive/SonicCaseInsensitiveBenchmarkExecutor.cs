using Adletec.Sonic.Benchmark.Expressions;
using Adletec.Sonic.Benchmark.Values;

namespace Adletec.Sonic.Benchmark.Executors.CaseInsensitive;

/// <summary>
/// Executes a benchmark using the sonic library with case-insensitive evaluation.
/// </summary>
public class SonicCaseInsensitiveBenchmarkExecutor : IBenchmarkExecutor
{
    public void RunBenchmark(string expression, List<string> variableNames, long iterations, IValueProvider valueProvider)
    {
        var engine = CalculationEngine.Create().DisableCaseSensitivity().Build();
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

    public void RunBenchmark(IEnumerable<BenchmarkExpression> expressions, IValueProvider valueProvider, long iterations)
    {
        var engine = CalculationEngine.Create().DisableCaseSensitivity().Build();
        foreach (var benchmarkExpression in expressions)
        {
            var calculate = engine.CreateDelegate(benchmarkExpression.GetExpression(ExpressionDialect.Sonic));
            var variables = new Dictionary<string, double>();
            for (var i = 0; i < iterations; i++)
            {
                foreach (var variableName in benchmarkExpression.VariableNames)
                {
                    variables[variableName] = valueProvider.GetNextValue();
                }
                calculate(variables);
            }
        }
    }

    public ExpressionDialect Dialect => ExpressionDialect.Sonic;

    public override string ToString() => "Sonic (C/I)";
}