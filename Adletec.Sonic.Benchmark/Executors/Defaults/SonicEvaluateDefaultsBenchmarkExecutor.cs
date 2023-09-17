using Adletec.Sonic.Benchmark.Expressions;
using Adletec.Sonic.Benchmark.Values;

namespace Adletec.Sonic.Benchmark.Executors.Defaults;

/// <summary>
/// Executes a benchmark using the sonic library with default settings using the engine evaluator.
/// The evaluator is a simple API but slower than using a delegate directly.
///
/// Sonic compile expressions and evaluates them case-sensitively by default.
/// </summary>
public class SonicEvaluateDefaultsBenchmarkExecutor : IBenchmarkExecutor
{
    public void RunBenchmark(string expression, List<string> variableNames, long iterations, IValueProvider valueProvider)
    {
        var engine = CalculationEngine.CreateWithDefaults();
        var variables = new Dictionary<string, double>();
        for (var i = 0; i < iterations; i++)
        {
            foreach (var variableName in variableNames)
            {
                variables[variableName] = valueProvider.GetNextValue();
            }
            engine.Evaluate(expression, variables);
        }
    }

    public void RunBenchmark(IEnumerable<BenchmarkExpression> expressions, IValueProvider valueProvider, long iterations)
    {
        var engine = CalculationEngine.CreateWithDefaults();
        foreach (var benchmarkExpression in expressions)
        {
            var variables = new Dictionary<string, double>();
            for (var i = 0; i < iterations; i++)
            {
                foreach (var variableName in benchmarkExpression.VariableNames)
                {
                    variables[variableName] = valueProvider.GetNextValue();
                }
                engine.Evaluate(benchmarkExpression.GetExpression(ExpressionDialect.Sonic), variables);
            }
        }
    }

    public ExpressionDialect Dialect => ExpressionDialect.Sonic;

    public override string ToString() => "Sonic (Evaluate Defaults)";
}