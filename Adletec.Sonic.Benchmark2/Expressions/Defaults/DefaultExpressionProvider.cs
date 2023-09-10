using System.Globalization;

namespace Adletec.Sonic.Benchmark2.Expressions.Defaults;

class DefaultExpressionProvider : IExpressionProvider
{
    public IEnumerable<BenchmarkExpression> GetExpressions()
    {
        return new[]
        {
            // Simple: A simple expression with basic arithmetics and variables
            new BenchmarkExpression
            (
                "Simple", "var1 + var2 * var3 / 2", new List<string> { "var1", "var2", "var3" }
            ),

            // Balanced: A balanced expression using functions and constants
            new BenchmarkExpression
            (
                "Balanced", "sin(var1) + cos(var2) + pi^2", new List<string> { "var1", "var2" }
            ).WithDialect(ExpressionDialect.NCalc,
                string.Create(CultureInfo.InvariantCulture, $"Sin(var1) + Cos(var2) + Pow({Math.PI},2)")),

            // Foldable: An expression which can be folded (simplified) to a single constant
            new BenchmarkExpression
            (
                "Foldable", "(var1 + var2 * var3 / 2) * 0 + 0 / (var1 + var2 * var3 / 2) + (var1 + var2 * var3 / 2)^0",
                new List<string> { "var1", "var2", "var3" }
            ).WithDialect(ExpressionDialect.NCalc,
                "(var1 + var2 * var3 / 2) * 0 + 0 / (var1 + var2 * var3 / 2) + Pow(var1 + var2 * var3 / 2,0)"),
        };
    }
}