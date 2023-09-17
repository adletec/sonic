using Adletec.Sonic.Benchmark.Expressions;
using Adletec.Sonic.Benchmark.Expressions.Fixed;
using Adletec.Sonic.Benchmark.Values;

namespace Adletec.Sonic.Benchmark.Executors
{
    /// <summary>
    /// A reference executor which calculates the default expressions using hard-coded C#.
    /// </summary>
    public class NativeBenchmarkExecutor : IBenchmarkExecutor
    {
        private readonly Dictionary<string, Func<IValueProvider, double>> expressionMethods =
            new()
            {
                { DefaultFixedExpressionProvider.ExpressionA, Calculate_Expression_A },
                { DefaultFixedExpressionProvider.ExpressionB, Calculate_Expression_B },
                { DefaultFixedExpressionProvider.ExpressionC, Calculate_Expression_C },
            };

        public void RunBenchmark(BenchmarkExpression expression,
            IValueProvider valueProvider, long iterations)
        {
            if (expressionMethods.TryGetValue(expression.GetExpression(Dialect), out var func))
            {
                RepeatEvaluation(func, iterations, valueProvider);
            }
            else
            {
                throw new NotImplementedException("Expression not implemented.");
            }
        }

        public void RunBenchmark(IEnumerable<BenchmarkExpression> expressions, IValueProvider valueProvider, long iterations)
        {
            foreach (var benchmarkExpression in expressions)
            {
                if (expressionMethods.TryGetValue(benchmarkExpression.GetExpression(ExpressionDialect.Generic), out var func))
                {
                    RepeatEvaluation(func, iterations, valueProvider);
                }
                else
                {
                    throw new NotImplementedException("Expression not implemented.");
                }
            }
        }

        private double RepeatEvaluation(Func<IValueProvider, double> func, long iterations, IValueProvider valueProvider)
        {
            double result = 0;
            for (var i = 0; i < iterations; i++)
            {
                result = func(valueProvider);
            }

            return result;
        }

        private static double Calculate_Expression_A(IValueProvider valueProvider)
        {
            var var1 = valueProvider.GetNextValue();
            var var2 = valueProvider.GetNextValue();
            var var3 = valueProvider.GetNextValue();
            return var1 + var2 * var3 / 2;
        }

        private static double Calculate_Expression_B(IValueProvider valueProvider)
        {
            var var1 = valueProvider.GetNextValue();
            var var2 = valueProvider.GetNextValue();
            return Math.Sin(var1) + Math.Cos(var2) + Math.Pow(Math.PI, 2);
        }

        private static double Calculate_Expression_C(IValueProvider valueProvider)
        {
            var var1 = valueProvider.GetNextValue();
            var var2 = valueProvider.GetNextValue();
            var var3 = valueProvider.GetNextValue();
            return (var1 + var2 * var3 / 2) * 0 + 0 / (var1 + var2 * var3 / 2) + Math.Pow(var1 + var2 * var3 / 2, 0);
        }

        public ExpressionDialect Dialect => ExpressionDialect.Generic;

        public override string ToString() => "Native C# Code";
    }
}