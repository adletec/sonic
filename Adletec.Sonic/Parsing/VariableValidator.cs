using System.Collections.Generic;
using Adletec.Sonic.Operations;

namespace Adletec.Sonic.Parsing
{
    /// <summary>
    /// A validator for an AST as produced by the <see cref="AstBuilder"/>.
    /// Checks if all variables referenced in the AST are defined.
    /// </summary>
    public class VariableValidator
    {
        
        /// <summary>
        /// Validate that all variables referenced in the given AST are defined.
        /// </summary>
        /// <param name="operation">The current root of the AST.</param>
        /// <param name="variables">The list of variable names to check against.</param>
        /// <exception cref="VariableNotDefinedException">If a referenced variable is not defined.</exception>
        public void Validate(Operation operation, IList<string> variables)
        {
            // traverse the AST recursively like we do in the optimizer;
            // there are two possible termination conditions:
            // - we reach a variable, which we check against the list of variables
            // - we reach a constant, which we can safely ignore (no if-branch required)
            //     => constants must be defined in order to be recognized as constants and like variables
            //        can't contain further operations
            
            var operationType = operation.GetType();
            if (operationType == typeof(Addition))
            {
                var addition = (Addition)operation;
                Validate(addition.Argument1, variables);
                Validate(addition.Argument2, variables);
            }
            else if (operationType == typeof(Subtraction))
            {
                var subtraction = (Subtraction)operation;
                Validate(subtraction.Argument1, variables);
                Validate(subtraction.Argument2, variables);
            }
            else if (operationType == typeof(Multiplication))
            {
                var multiplication = (Multiplication)operation;
                Validate(multiplication.Argument1, variables);
                Validate(multiplication.Argument2, variables);
            }
            else if (operationType == typeof(Division))
            {
                var division = (Division)operation;
                Validate(division.Dividend, variables);
                Validate(division.Divisor, variables);
            }
            else if (operationType == typeof(Exponentiation))
            {
                var exponentiation = (Exponentiation)operation;
                Validate(exponentiation.Base, variables);
                Validate(exponentiation.Exponent, variables);
            }
            else if (operationType == typeof(Variable))
            {
                var variable = (Variable)operation;
                if (variables.Contains(variable.Name) == false)
                {
                    throw new VariableNotDefinedException($"Variable '{variable.Name}' is not defined.", variable.Name);
                }
            }
            else if (operationType == typeof(Function))
            {
                var function = (Function)operation;
                foreach (var argument in function.Arguments)
                {
                    Validate(argument, variables);
                }
            }
        }
    }
}