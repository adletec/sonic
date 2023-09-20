namespace Adletec.Sonic.Benchmark.Expressions;

/// <summary>
/// Provides a fixed number of expressions to be evaluated.
/// This is useful if the same expressions should be evaluated multiple times with different values.
/// </summary>
public interface IFixedExpressionProvider
{
    /// <summary>
    /// Gets all expressions.
    /// </summary>
    /// <returns></returns>
    IEnumerable<BenchmarkExpression> GetExpressions();
    
}