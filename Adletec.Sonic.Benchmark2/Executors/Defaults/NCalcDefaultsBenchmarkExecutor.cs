using Adletec.Sonic.Benchmark2.Expressions;
using Adletec.Sonic.Benchmark2.Values;
using NCalc;

namespace Adletec.Sonic.Benchmark2.Executors.Defaults;

/// <summary>
/// Executes a benchmark using the NCalc library with default settings.
///
/// NCalc is always interpreted, and evaluates expressions case-sensitively by default.
/// </summary>
public class NCalcDefaultsBenchmarkExecutor : IBenchmarkExecutor
{
    public void RunBenchmark(string expression, List<string> variableNames, long iterations, IValueProvider valueProvider)
    {
        var nCalcExpression = new Expression(expression);
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
    
    public override string ToString() => "NCalc (Defaults)";
}