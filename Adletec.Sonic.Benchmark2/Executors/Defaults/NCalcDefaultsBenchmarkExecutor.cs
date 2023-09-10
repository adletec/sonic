using Adletec.Sonic.Benchmark2.Expressions;
using Adletec.Sonic.Benchmark2.Values;

namespace Adletec.Sonic.Benchmark2.Executors.Defaults;

public class NCalcDefaultsBenchmarkExecutor : IBenchmarkExecutor
{
    public void RunBenchmark(string expression, List<string> variableNames, long iterations, IValueProvider valueProvider)
    {
        var nCalcExpression = new NCalc.Expression(expression);
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