namespace Adletec.Sonic.Benchmark2.Expressions;

public interface IExpressionProvider
{
    IEnumerable<BenchmarkExpression> GetExpressions();
}