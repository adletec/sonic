using Adletec.Sonic.Benchmark2.Expressions;
using Adletec.Sonic.Benchmark2.Values;

namespace Adletec.Sonic.Benchmark2.Executors;

public interface IBenchmarkExecutor
{
    /// <summary>
    /// Run the benchmark for the specified expression, variable names, iterations, and value provider.
    /// </summary>
    /// <param name="expression">The expression to evaluate.</param>
    /// <param name="variableNames">A list of all variable names as referenced in the given expression.</param>
    /// <param name="iterations">How often this expression should be evaluated.</param>
    /// <param name="valueProvider">The value provider for the variables.</param>
    void RunBenchmark(string expression, List<string> variableNames, long iterations, IValueProvider valueProvider);
    
    /// <summary>
    /// The expression dialect this benchmark executor is expecting.
    /// </summary>
    ExpressionDialect Dialect { get; }
}