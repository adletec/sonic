namespace Adletec.Sonic.Benchmark.Expressions;

/// <summary>
/// Generates expressions to be evaluated. This is useful if a large number of expressions should be benchmarked.
/// </summary>
public interface IGeneratingExpressionProvider
{
    /// <summary>
    /// Gets the desired number of expressions.
    /// </summary>
    /// <param name="count">the desired number of expressions</param>
    /// <returns>a collection of expressions with count entries</returns>
    IList<BenchmarkExpression> GetExpressions(int count);
    
}