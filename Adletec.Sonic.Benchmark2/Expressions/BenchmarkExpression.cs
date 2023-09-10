namespace Adletec.Sonic.Benchmark2.Expressions;

public class BenchmarkExpression
{
    /// <summary>
    /// A container for an expression and its variable names.
    /// </summary>
    /// <param name="title">A descriptive title for this expression.</param>
    /// <param name="expression">The expression in its most generic version.</param>
    /// <param name="variableNames">All variable names which this expression depends on.</param>
    public BenchmarkExpression(string title, string expression, List<string> variableNames)
    {
        Title = title;
        VariableNames = variableNames;
        Expressions[ExpressionDialect.Generic] = expression;
    }
    
    public string Title { get; }
    
    private Dictionary<ExpressionDialect, string> Expressions { get; } = new();

    /// <summary>
    /// If a specific engine cannot parse the generic expression because of syntactic peculiarities, use this method
    /// to add an alternative representation in another dialect.
    /// </summary>
    /// <param name="dialect"></param>
    /// <param name="expression"></param>
    /// <returns>This object, so the method call can be inlined.</returns>
    public BenchmarkExpression WithDialect(ExpressionDialect dialect, string expression)
    {
        Expressions[dialect] = expression;
        return this;
    }

    /// <summary>
    /// Get the expression in the specified dialect. If there is no equivalent expression in the specified dialect,
    /// the generic expression is returned.
    /// </summary>
    /// <param name="dialect">The desired dialect.</param>
    /// <returns>The expression in the desired dialect, or the generic dialect if no dialect specific version is defined.</returns>
    public string GetExpression(ExpressionDialect dialect)
    {
        return Expressions.TryGetValue(dialect, out var specificExpression) ? specificExpression : Expressions[ExpressionDialect.Generic];
    }

    public List<string> VariableNames { get; }

    public override string ToString()
    {
        return string.IsNullOrWhiteSpace(Title) ? Expressions[ExpressionDialect.Generic]: Title;
    }
}