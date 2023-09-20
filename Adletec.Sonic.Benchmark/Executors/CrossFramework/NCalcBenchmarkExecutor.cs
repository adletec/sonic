using Adletec.Sonic.Benchmark.Expressions;
using Adletec.Sonic.Benchmark.Values;
using NCalc;

namespace Adletec.Sonic.Benchmark.Executors.CrossFramework;

/// <summary>
/// A benchmark executor that uses NCalc's Expression-class to evaluate expressions.
/// </summary>
public class NCalcBenchmarkExecutor : IBenchmarkExecutor
{
    private readonly EvaluateOptions Options;

    public NCalcBenchmarkExecutor()
    {
        Options = EvaluateOptions.None;
    }

    public NCalcBenchmarkExecutor(bool caseSensitive, bool interpreted, bool cached, bool optimize = false)
    {
        if (!interpreted || optimize)
        {
            throw new NotSupportedException("Mode not supported.");
        }

        if (!cached && !caseSensitive)
        {
            throw new NotSupportedException("Disabling cache only supported with enabled case sensitivity.");
        }

        if (!caseSensitive)
        {
            Options = EvaluateOptions.IgnoreCase;
        }
        else if (!cached)
        {
            Options = EvaluateOptions.NoCache;
        }
        else
        {
            Options = EvaluateOptions.None;
        }
    }


    public void RunBenchmark(BenchmarkExpression expression, IValueProvider valueProvider, long iterations)
    {
        var nCalcExpression = new Expression(expression.GetExpression(Dialect), Options);
        for (var i = 0; i < iterations; i++)
        {
            foreach (var variableName in expression.VariableNames)
            {
                nCalcExpression.Parameters[variableName] = valueProvider.GetNextValue();
            }

            nCalcExpression.Evaluate();
        }
    }

    public void RunBenchmark(IEnumerable<BenchmarkExpression> expressions, IValueProvider valueProvider,
        long iterations)
    {
        foreach (var benchmarkExpression in expressions)
        {
            var nCalcExpression = new Expression(benchmarkExpression.GetExpression(ExpressionDialect.NCalc), Options);
            for (var i = 0; i < iterations; i++)
            {
                foreach (var variableName in benchmarkExpression.VariableNames)
                {
                    nCalcExpression.Parameters[variableName] = valueProvider.GetNextValue();
                }

                nCalcExpression.Evaluate();
            }
        }
    }

    public ExpressionDialect Dialect => ExpressionDialect.NCalc;
}