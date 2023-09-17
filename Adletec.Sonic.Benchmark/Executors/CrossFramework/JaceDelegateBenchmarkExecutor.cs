using Adletec.Sonic.Benchmark.Expressions;
using Adletec.Sonic.Benchmark.Values;
using Jace;
using Jace.Execution;

namespace Adletec.Sonic.Benchmark.Executors.CrossFramework;

/// <summary>
/// A benchmark executor that uses Jace's Build-method to create a delegate for each expression.
/// </summary>
public class JaceDelegateBenchmarkExecutor: IBenchmarkExecutor
{
    
    private readonly Jace.CalculationEngine engine;
    
    public JaceDelegateBenchmarkExecutor()
    {
        engine = new Jace.CalculationEngine();
    }
    
    public JaceDelegateBenchmarkExecutor(bool caseSensitive, bool interpreted, bool cached, bool optimize = true)
    {
        engine = new Jace.CalculationEngine(
            new JaceOptions()
            {
                ExecutionMode = interpreted ? ExecutionMode.Interpreted : ExecutionMode.Compiled,
                CaseSensitive = caseSensitive,
                CacheEnabled = cached,
                OptimizerEnabled = optimize
            });
    }
    
    
    public void RunBenchmark(BenchmarkExpression expression, IValueProvider valueProvider, long iterations)
    {
        var calculate = engine.Build(expression.GetExpression(Dialect));
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

    public void RunBenchmark(IEnumerable<BenchmarkExpression> expressions, IValueProvider valueProvider, long iterations)
    {
        foreach (var benchmarkExpression in expressions)
        {
            var calculate = engine.Build(benchmarkExpression.GetExpression(Dialect));
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

    public ExpressionDialect Dialect => ExpressionDialect.Jace;
}