using Adletec.Sonic.Benchmark.Expressions;
using Adletec.Sonic.Benchmark.Values;
using NCalc;

namespace Adletec.Sonic.Benchmark.Executors.CaseInsensitive;

/// <summary>
/// Executes a benchmark using the NCalc library with case-insensitive evaluation.
/// </summary>
public class NCalcCaseInsensitiveBenchmarkExecutor : IBenchmarkExecutor
{
    public void RunBenchmark(string expression, List<string> variableNames, long iterations, IValueProvider valueProvider)
    {
        var nCalcExpression = new Expression(expression, EvaluateOptions.IgnoreCase);
        for (var i = 0; i < iterations; i++)
        {
            foreach (var variableName in variableNames)
            {
                nCalcExpression.Parameters[variableName] = valueProvider.GetNextValue();
            }
            nCalcExpression.Evaluate();
        }
    }

    public void RunBenchmark(IEnumerable<BenchmarkExpression> expressions, IValueProvider valueProvider, long iterations)
    {
        foreach (var benchmarkExpression in expressions)
        {
            var nCalcExpression = new Expression(benchmarkExpression.GetExpression(ExpressionDialect.NCalc), EvaluateOptions.IgnoreCase);
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
    
    public override string ToString() => "NCalc (C/I)";
}