using System.Globalization;

namespace Adletec.Sonic.Benchmark.Expressions.Fixed;

/// <summary>
/// Provides three default expressions to be evaluated.
///
/// Expression A: A simple expression with basic arithmetics and variables
/// Expression B: A balanced expression using functions and constants
/// Expression C: An expression which can be folded (simplified) to a single constant
/// </summary>
class DefaultFixedExpressionProvider : IFixedExpressionProvider
{
    public const string ExpressionA = "var1 + var2 * var3 / 2";
    public const string ExpressionB = "sin(var1) + cos(var2) + pi^2";

    public const string ExpressionC =
        "(var1 + var2 * var3 / 2) * 0 + 0 / (var1 + var2 * var3 / 2) + (var1 + var2 * var3 / 2)^0";

    public IEnumerable<BenchmarkExpression> GetExpressions()
    {
        return new[]
        {
            // Simple: A simple expression with basic arithmetics and variables
            new BenchmarkExpression
            (
                "A: Simple", ExpressionA, new List<string> { "var1", "var2", "var3" }
            ),

            // Balanced: A balanced expression using functions and constants
            new BenchmarkExpression
                (
                    "B: Balanced", ExpressionB, new List<string> { "var1", "var2" }
                ).WithDialect(ExpressionDialect.NCalc,
                    string.Create(CultureInfo.InvariantCulture, $"Sin(var1) + Cos(var2) + Pow({Math.PI},2)"))
                // Due to a bug in Jace, the constant "pi" cannot be found in interpreted mode.
                // This is a workaround which shouldn't cost any performance
                .WithDialect(ExpressionDialect.Jace, $"sin(var1) + cos(var2) + {Math.PI}^2")
                .WithDialect(ExpressionDialect.DynamicExpresso, $"Math.Sin(var1) + Math.Cos(var2) + Math.Pow(Math.PI,2)"),

            // Foldable: An expression which can be folded (simplified) to a single constant
            new BenchmarkExpression
                (
                    "C: Foldable", ExpressionC,
                    new List<string> { "var1", "var2", "var3" }
                ).WithDialect(ExpressionDialect.NCalc,
                    "(var1 + var2 * var3 / 2) * 0 + 0 / (var1 + var2 * var3 / 2) + Pow(var1 + var2 * var3 / 2,0)")
                .WithDialect(ExpressionDialect.DynamicExpresso,
                    "(var1 + var2 * var3 / 2) * 0 + 0 / (var1 + var2 * var3 / 2) + Math.Pow(var1 + var2 * var3 / 2,0)")
        };
    }

    public IEnumerable<BenchmarkExpression> GetExpressions(int count)
    {
        // not relevant for this kind of provider
        throw new NotImplementedException();
    }
}