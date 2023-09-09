using BenchmarkDotNet.Attributes;

namespace Adletec.Sonic.Benchmark2;

public class CalculationEngineBenchmark
{
    [ParamsSource(nameof(ExpressionValues))]
    public BenchmarkExpression Expression { get; set; }

    [ParamsSource(nameof(BenchmarkExecutors))]
    public IBenchmarkExecutor Executor { get; set; }

    public long Iterations { get; set; } = 2000000L;

    public IEnumerable<BenchmarkExpression> ExpressionValues => new[]
    {
        new BenchmarkExpression
        (
            // A simple expression with variables
            "var1 + var2 * var3 / 2",
            new List<string> { "var1", "var2", "var3" }
        ),
        new BenchmarkExpression
        (
            // An expression which can be simplified to a constant
            // This is to test the performance of the constant folding
            "(var1 + var2 * var3 / 2) * 0 + 0 / (var1 + var2 * var3 / 2) + (var1 + var2 * var3 / 2)^0",
            new List<string> { "var1", "var2", "var3" }
        ),
        new BenchmarkExpression(
            // A balanced expression using functions and constants
            "sin(var1) + cos(var2) + pi^2",
            new List<string> { "var1", "var2"}
        )
    };

    public IEnumerable<IBenchmarkExecutor> BenchmarkExecutors => new List<IBenchmarkExecutor>
    {
        new JaceDefaultsBenchmarkExecutor(),
        new JaceCaseSensitiveCompiledBenchmarkExecutor(),
        new SonicDefaultsBenchmarkExecutor(),
        new NCalcDefaultsBenchmarkExecutor()
    };

    [Benchmark]
    public void RunBenchmark() =>
        Executor.RunBenchmark(Expression.Expression, Expression.VariableNames, Iterations,
            new IncrementingValueProvider());
}