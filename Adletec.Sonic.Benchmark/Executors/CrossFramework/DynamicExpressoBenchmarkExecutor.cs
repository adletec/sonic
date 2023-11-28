using Adletec.Sonic.Benchmark.Expressions;
using Adletec.Sonic.Benchmark.Values;
using DynamicExpresso;

namespace Adletec.Sonic.Benchmark.Executors.CrossFramework;

public class DynamicExpressoBenchmarkExecutor : IBenchmarkExecutor
{
    private readonly Interpreter interpreter = new();

    public void RunBenchmark(BenchmarkExpression expression, IValueProvider valueProvider, long iterations)
    {
        for (var i = 0; i < iterations; i++)
        {
            foreach (var variableName in expression.VariableNames)
            {
                interpreter.SetVariable(variableName, valueProvider.GetNextValue());
            }

            interpreter.Eval(expression.GetExpression(Dialect));
        }
    }

    public void RunBenchmark(IEnumerable<BenchmarkExpression> expressions, IValueProvider valueProvider,
        long iterations)
    {
        foreach (var benchmarkExpression in expressions)
        {
            for (var i = 0; i < iterations; i++)
            {
                foreach (var variableName in benchmarkExpression.VariableNames)
                {
                    interpreter.SetVariable(variableName, valueProvider.GetNextValue());
                }
                interpreter.Eval(benchmarkExpression.GetExpression(Dialect));
            }
        }
    }

    public ExpressionDialect Dialect => ExpressionDialect.DynamicExpresso;
}