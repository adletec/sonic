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
        
        /// <summary>
        /// Evaluate the provided expression. The expression may only contain constants and known functions.
        /// </summary>
        /// <param name="expression">an expression string, e.g. "22+3*(4.2-2)</param>
        /// <returns>the result of the evaluation as double</returns>
        double Evaluate(string expression);
        
        /// <summary>
        /// Evaluate the provided expression. All referenced variables must be provided as a dictionary.
        /// </summary>
        /// <param name="expression">an expression string, e.g. "a+b*(2.4+c)</param>
        /// <param name="variables">a dictionary containing the values of all variables referenced in the expression</param>
        /// <returns>the result of the evaluation as double</returns>
        double Evaluate(string expression, IDictionary<string, double> variables);
        
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
    }
}