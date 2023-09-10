namespace Adletec.Sonic.Benchmark2.Expressions;

/// <summary>
/// Provides expressions to be evaluated.
/// </summary>
public interface IExpressionProvider
{
    /// <summary>
    /// Gets the expressions.
    /// </summary>
    /// <returns></returns>
    IEnumerable<BenchmarkExpression> GetExpressions();
}