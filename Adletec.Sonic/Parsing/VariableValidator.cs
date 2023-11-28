using System.Collections.Generic;
using Adletec.Sonic.Operations;

namespace Adletec.Sonic.Parsing
{
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
            if (operation.GetType() == typeof(Addition))
            {
                var addition = (Addition)operation;
                Validate(addition.Argument1, variables);
                Validate(addition.Argument2, variables);
            }
            else if (operation.GetType() == typeof(Subtraction))
            {
                var subtraction = (Subtraction)operation;
                Validate(subtraction.Argument1, variables);
                Validate(subtraction.Argument2, variables);
            }
            else if (operation.GetType() == typeof(Multiplication))
            {
                var multiplication = (Multiplication)operation;
                Validate(multiplication.Argument1, variables);
                Validate(multiplication.Argument2, variables);
            }
            else if (operation.GetType() == typeof(Division))
            {
                var division = (Division)operation;
                Validate(division.Dividend, variables);
                Validate(division.Divisor, variables);
            }
            else if (operation.GetType() == typeof(Exponentiation))
            {
                var exponentiation = (Exponentiation)operation;
                Validate(exponentiation.Base, variables);
                Validate(exponentiation.Exponent, variables);
            }
            else if (operation.GetType() == typeof(Variable))
            {
                var variable = (Variable)operation;
                if (variables.Contains(variable.Name) == false)
                {
                    throw new VariableNotDefinedException($"Variable '{variable.Name}' is not defined.", variable.Name);
                }
            }
            else if (operation.GetType() == typeof(Function))
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