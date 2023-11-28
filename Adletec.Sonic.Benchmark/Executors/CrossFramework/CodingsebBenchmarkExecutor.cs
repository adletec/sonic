using Adletec.Sonic.Benchmark.Expressions;
using Adletec.Sonic.Benchmark.Values;
using CodingSeb.ExpressionEvaluator;

namespace Adletec.Sonic.Benchmark.Executors.CrossFramework;

public class CodingsebBenchmarkExecutor: IBenchmarkExecutor
{
    private readonly ExpressionEvaluator evaluator = new();

    public void RunBenchmark(BenchmarkExpression expression, IValueProvider valueProvider, long iterations)
    {
        var expressionString = expression.GetExpression(Dialect);
        var variables = new Dictionary<string, object>();
        for (var i = 0; i < iterations; i++)
        {
            foreach (var variableName in expression.VariableNames)
            {
                variables[variableName] = valueProvider.GetNextValue();
            }
            evaluator.Variables = variables;
            evaluator.Evaluate(expressionString);
        }

    }

    public void RunBenchmark(IEnumerable<BenchmarkExpression> expressions, IValueProvider valueProvider, long iterations)
    {
        foreach (var benchmarkExpression in expressions)
        {
            var expressionString = benchmarkExpression.GetExpression(Dialect);
            var variables = new Dictionary<string, object>();
            for (var i = 0; i < iterations; i++)
            {
                foreach (var variableName in benchmarkExpression.VariableNames)
                {
                    variables[variableName] = valueProvider.GetNextValue();
                }
                evaluator.Variables = variables;
                evaluator.Evaluate(expressionString);
            }
        }
    }

    public ExpressionDialect Dialect => ExpressionDialect.NCalc;
}