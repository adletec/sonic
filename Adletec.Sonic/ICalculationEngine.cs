using System;
using System.Collections.Generic;
using Adletec.Sonic.Execution;

namespace Adletec.Sonic
{
    
    /// <summary>
    /// The CalculationEngine is the main component of sonic. It parses strings containing
    /// mathematical formulas and to calculates the result.
    /// 
    /// It can be configured to run in a number of modes based on the constructor parameters chosen.
    /// </summary>
    public interface ICalculationEngine
    {
        IEnumerable<FunctionInfo> Functions { get; }
        IEnumerable<ConstantInfo> Constants { get; }
        double Calculate(string formulaText);
        double Calculate(string formulaText, IDictionary<string, double> variables);
        FormulaBuilder Formula(string formulaText);

        /// <summary>
        /// Build a .NET func for the provided formula.
        /// </summary>
        /// <param name="formulaText">The formula that must be converted into a .NET func.</param>
        /// <returns>A .NET func for the provided formula.</returns>
        Func<IDictionary<string, double>, double> Build(string formulaText);

        /// <summary>
        /// Build a .NET func for the provided formula.
        /// </summary>
        /// <param name="formulaText">The formula that must be converted into a .NET func.</param>
        /// <param name="constants">Constant values for variables defined into the formula. They variables will be replaced by the constant value at pre-compilation time.</param>
        /// <returns>A .NET func for the provided formula.</returns>
        Func<IDictionary<string, double>, double> Build(string formulaText, IDictionary<string, double> constants);

        /// <summary>
        /// Add a function to the calculation engine.
        /// </summary>
        /// <param name="functionName">The name of the function. This name can be used in mathematical formulas.</param>
        /// <param name="function">The implementation of the function.</param>
        /// <param name="isIdempotent">Does the function provide the same result when it is executed multiple times.</param>
        void AddFunction(string functionName, Func<double> function, bool isIdempotent = true);

        /// <summary>
        /// Add a function to the calculation engine.
        /// </summary>
        /// <param name="functionName">The name of the function. This name can be used in mathematical formulas.</param>
        /// <param name="function">The implementation of the function.</param>
        /// <param name="isIdempotent">Does the function provide the same result when it is executed multiple times.</param>
        void AddFunction(string functionName, Func<double, double> function, bool isIdempotent = true);

        /// <summary>
        /// Add a function to the calculation engine.
        /// </summary>
        /// <param name="functionName">The name of the function. This name can be used in mathematical formulas.</param>
        /// <param name="function">The implementation of the function.</param>
        /// <param name="isIdempotent">Does the function provide the same result when it is executed multiple times.</param>
        void AddFunction(string functionName, Func<double, double, double> function, bool isIdempotent = true);

        /// <summary>
        /// Add a function to the calculation engine.
        /// </summary>
        /// <param name="functionName">The name of the function. This name can be used in mathematical formulas.</param>
        /// <param name="function">The implementation of the function.</param>
        /// <param name="isIdempotent">Does the function provide the same result when it is executed multiple times.</param>
        void AddFunction(string functionName, Func<double, double, double, double> function, bool isIdempotent = true);

        /// <summary>
        /// Add a function to the calculation engine.
        /// </summary>
        /// <param name="functionName">The name of the function. This name can be used in mathematical formulas.</param>
        /// <param name="function">The implementation of the function.</param>
        /// <param name="isIdempotent">Does the function provide the same result when it is executed multiple times.</param>
        void AddFunction(string functionName, Func<double, double, double, double, double> function, bool isIdempotent = true);

        /// <summary>
        /// Add a function to the calculation engine.
        /// </summary>
        /// <param name="functionName">The name of the function. This name can be used in mathematical formulas.</param>
        /// <param name="function">The implementation of the function.</param>
        /// <param name="isIdempotent">Does the function provide the same result when it is executed multiple times.</param>
        void AddFunction(string functionName, Func<double, double, double, double, double, double> function, bool isIdempotent = true);

        /// <summary>
        /// Add a function to the calculation engine.
        /// </summary>
        /// <param name="functionName">The name of the function. This name can be used in mathematical formulas.</param>
        /// <param name="function">The implementation of the function.</param>
        /// <param name="isIdempotent">Does the function provide the same result when it is executed multiple times.</param>
        void AddFunction(string functionName, Func<double, double, double, double, double, double, double> function, bool isIdempotent = true);

        void AddFunction(string functionName, Func<double, double, double, double, double, double, double, double> function, bool isIdempotent = true);
        void AddFunction(string functionName, Func<double, double, double, double, double, double, double, double, double> function, bool isIdempotent = true);
        void AddFunction(string functionName, Func<double, double, double, double, double, double, double, double, double, double> function, bool isIdempotent = true);
        void AddFunction(string functionName, Func<double, double, double, double, double, double, double, double, double, double, double> function, bool isIdempotent = true);
        void AddFunction(string functionName, Func<double, double, double, double, double, double, double, double, double, double, double, double> function, bool isIdempotent = true);
        void AddFunction(string functionName, Func<double, double, double, double, double, double, double, double, double, double, double, double, double> function, bool isIdempotent = true);
        void AddFunction(string functionName, Func<double, double, double, double, double, double, double, double, double, double, double, double, double, double> function, bool isIdempotent = true);
        void AddFunction(string functionName, Func<double, double, double, double, double, double, double, double, double, double, double, double, double, double, double> function, bool isIdempotent = true);
        void AddFunction(string functionName, Func<double, double, double, double, double, double, double, double, double, double, double, double, double, double, double, double> function, bool isIdempotent = true);
        void AddFunction(string functionName, Func<double, double, double, double, double, double, double, double, double, double, double, double, double, double, double, double, double> function, bool isIdempotent = true);
        void AddFunction(string functionName, DynamicFunc<double, double> functionDelegate, bool isIdempotent = true);

        /// <summary>
        /// Add a constant to the calculation engine.
        /// </summary>
        /// <param name="constantName">The name of the constant. This name can be used in mathematical formulas.</param>
        /// <param name="value">The value of the constant.</param>
        void AddConstant(string constantName, double value);
    }
}