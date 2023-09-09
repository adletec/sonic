namespace Adletec.Sonic.Benchmark2;

public class BenchmarkExpression
{
    public BenchmarkExpression(string expression, List<string> variableNames)
    {
        Expression = expression;
        VariableNames = variableNames;
    }
    public string Expression { get; }
    
    public List<string> VariableNames { get; }
    
    public override string ToString()
    {
        return Expression;
    }
}