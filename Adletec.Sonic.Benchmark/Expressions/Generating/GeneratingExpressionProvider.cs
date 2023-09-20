using System.Text;
using Microsoft.Extensions.Primitives;

namespace Adletec.Sonic.Benchmark.Expressions.Generating;

public class GeneratingExpressionProvider: IGeneratingExpressionProvider
{

    private readonly int randomSeed;
    private readonly int maxExpressionElements;
    private readonly int minExpressionElements;

    public GeneratingExpressionProvider(int randomSeed, int maxExpressionElements = 5, int minExpressionElements = 3)
    {
        this.randomSeed = randomSeed;
        this.maxExpressionElements = maxExpressionElements;
        this.minExpressionElements = minExpressionElements;
    }

    public IList<BenchmarkExpression> GetExpressions(int count)
    {
        var expressions = new List<BenchmarkExpression>();
        var random = new Random(randomSeed);
        for (var i = 0; i < count; i++)
        {
            expressions.Add(GenerateExpression(maxExpressionElements, minExpressionElements, random));
        }
        return expressions;
    }
    
    private static BenchmarkExpression GenerateExpression(int maxExpressionElements, int minExpressionElements, Random random)
    {
        var variableNames = new List<string>();
        var expression = new StringBuilder();
        var expressionElements = random.Next(minExpressionElements, maxExpressionElements);
        expression.Append(random.Next());
        for (var i = 0; i < expressionElements; i++)
        {
            var expressionElement = (ExpressionElement) random.Next(0, 8);
            var variableName = $"var{variableNames.Count}";
            switch (expressionElement)
            {
                case ExpressionElement.AddConstant:
                    expression.Append($"+{random.Next()}");
                    break;
                case ExpressionElement.AddVariable:
                    expression.Append($"+{variableName}");
                    variableNames.Add(variableName);
                    break;
                case ExpressionElement.SubtractConstant:
                    expression.Append($"-{random.Next()}");
                    break;
                case ExpressionElement.SubtractVariable:
                    expression.Append($"-{variableName}");
                    variableNames.Add(variableName);
                    break;
                case ExpressionElement.MultiplyConstant:
                    expression.Append($"*{random.Next()}");
                    break;
                case ExpressionElement.MultiplyVariable:
                    expression.Append($"*{variableName}");
                    variableNames.Add(variableName);
                    break;
                case ExpressionElement.DivideConstant:
                    expression.Append($"/{random.Next(1, int.MaxValue)}");
                    break;
                case ExpressionElement.DivideVariable:
                    expression.Append($"/{variableName}");
                    variableNames.Add(variableName);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        return new BenchmarkExpression("Random Expression", expression.ToString(), variableNames);
    }
    
}