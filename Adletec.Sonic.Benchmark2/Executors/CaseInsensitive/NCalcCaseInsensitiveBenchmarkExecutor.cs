using Adletec.Sonic.Benchmark2.Expressions;
using Adletec.Sonic.Benchmark2.Values;
using NCalc;

namespace Adletec.Sonic.Benchmark2.Executors.CaseInsensitive;

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
    public ExpressionDialect Dialect => ExpressionDialect.NCalc;
    
    public override string ToString() => "NCalc (C/I)";
}