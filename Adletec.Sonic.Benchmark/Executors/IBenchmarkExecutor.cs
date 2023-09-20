using Adletec.Sonic.Benchmark.Expressions;
using Adletec.Sonic.Benchmark.Values;

namespace Adletec.Sonic.Benchmark.Executors;

public interface IBenchmarkExecutor
{
    /// <summary>
    /// Run the benchmark for the specified expression, variable names, iterations, and value provider.
    /// </summary>
    /// <param name="expression">The expression to evaluate.</param>
    /// <param name="valueProvider">The value provider for the variables.</param>
    /// <param name="iterations">How often this expression should be evaluated.</param>
    void RunBenchmark(BenchmarkExpression expression, IValueProvider valueProvider, long iterations);
    
    void RunBenchmark(IEnumerable<BenchmarkExpression> expressions, IValueProvider valueProvider, long iterations);
    
    /// <summary>
    /// The expression dialect this benchmark executor is expecting.
    /// </summary>
    ExpressionDialect Dialect { get; }
}