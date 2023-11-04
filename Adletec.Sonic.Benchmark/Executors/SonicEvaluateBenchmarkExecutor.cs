using Adletec.Sonic.Benchmark.Expressions;
using Adletec.Sonic.Benchmark.Values;
using Adletec.Sonic.Execution;

namespace Adletec.Sonic.Benchmark.Executors;

/// <summary>
/// A benchmark executor that uses Sonic's Calculate-method to evaluate expressions.
/// </summary>
public class SonicEvaluateBenchmarkExecutor : IBenchmarkExecutor
{

    private readonly Evaluator engine;

    public SonicEvaluateBenchmarkExecutor()
    {
        engine = Evaluator.CreateWithDefaults();
    }

    public SonicEvaluateBenchmarkExecutor(bool caseSensitive, bool interpreted, bool cached, bool optimize = true, bool guardedMode = false, bool validate = true)
    {

        var engineBuilder = Evaluator.Create();

        engineBuilder = caseSensitive ? engineBuilder.EnableCaseSensitivity() : engineBuilder.DisableCaseSensitivity();
        engineBuilder = interpreted ? engineBuilder.UseExecutionMode(ExecutionMode.Interpreted) : engineBuilder.UseExecutionMode(ExecutionMode.Compiled);
        engineBuilder = cached ? engineBuilder.EnableCache() : engineBuilder.DisableCache();
        engineBuilder = guardedMode ? engineBuilder.EnableGuardedMode() : engineBuilder.DisableGuardedMode();
        engineBuilder = optimize ? engineBuilder.EnableOptimizer() : engineBuilder.DisableOptimizer();
        engineBuilder = validate ? engineBuilder.EnableValidation() : engineBuilder.DisableValidation();
        engine = engineBuilder.Build();
    }

    public void RunBenchmark(BenchmarkExpression expression, IValueProvider valueProvider, long iterations)
    {
        var expressionString = expression.GetExpression(Dialect);
        var variables = new Dictionary<string, double>();
        for (var i = 0; i < iterations; i++)
        {
            foreach (var variableName in expression.VariableNames)
            {
                variables[variableName] = valueProvider.GetNextValue();
            }

            engine.Evaluate(expressionString, variables);
        }
    }

    public void RunBenchmark(IEnumerable<BenchmarkExpression> expressions, IValueProvider valueProvider,
        long iterations)
    {
        foreach (var benchmarkExpression in expressions)
        {
            var variables = new Dictionary<string, double>();
            for (var i = 0; i < iterations; i++)
            {
                foreach (var variableName in benchmarkExpression.VariableNames)
                {
                    variables[variableName] = valueProvider.GetNextValue();
                }

                engine.Evaluate(benchmarkExpression.GetExpression(Dialect), variables);
            }
        }
    }

    public ExpressionDialect Dialect => ExpressionDialect.Sonic;
}