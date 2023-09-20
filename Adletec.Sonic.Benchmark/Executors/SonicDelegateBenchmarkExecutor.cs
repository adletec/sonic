using Adletec.Sonic.Benchmark.Expressions;
using Adletec.Sonic.Benchmark.Values;
using Adletec.Sonic.Execution;

namespace Adletec.Sonic.Benchmark.Executors;

/// <summary>
/// A benchmark executor that uses Sonic's CreateDelegate-method to create a delegate for each expression.
/// </summary>
public class SonicDelegateBenchmarkExecutor : IBenchmarkExecutor
{
    private readonly CalculationEngine engine;

    public SonicDelegateBenchmarkExecutor()
    {
        engine = CalculationEngine.CreateWithDefaults();
    }

    public SonicDelegateBenchmarkExecutor(bool caseSensitive, bool interpreted, bool cached, bool optimize = true, bool guardedMode = false)
    {
        var engineBuilder = CalculationEngine.Create();
        engineBuilder = caseSensitive ? engineBuilder.EnableCaseSensitivity() : engineBuilder.DisableCaseSensitivity();
        engineBuilder = interpreted ? engineBuilder.UseExecutionMode(ExecutionMode.Interpreted) : engineBuilder.UseExecutionMode(ExecutionMode.Compiled);
        engineBuilder = cached ? engineBuilder.EnableCache() : engineBuilder.DisableCache();
        engineBuilder = guardedMode ? engineBuilder.EnableGuardedMode() : engineBuilder.DisableGuardedMode();
        engineBuilder = optimize ? engineBuilder.EnableOptimizer() : engineBuilder.DisableOptimizer();
        engine = engineBuilder.Build();
    }

    public void RunBenchmark(BenchmarkExpression expression, IValueProvider valueProvider, long iterations)
    {
        var calculate = engine.CreateDelegate(expression.GetExpression(Dialect));
        var variables = new Dictionary<string, double>();
        for (var i = 0; i < iterations; i++)
        {
            foreach (var variableName in expression.VariableNames)
            {
                variables[variableName] = valueProvider.GetNextValue();
            }
            calculate(variables);
        }
    }

    public void RunBenchmark(IEnumerable<BenchmarkExpression> expressions, IValueProvider valueProvider,
        long iterations)
    {
        foreach (var benchmarkExpression in expressions)
        {
            var calculate = engine.CreateDelegate(benchmarkExpression.GetExpression(Dialect));
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
}