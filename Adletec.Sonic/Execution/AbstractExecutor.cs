using System;
using System.Collections.Generic;
using System.Linq;

namespace Adletec.Sonic.Execution
{
    public abstract class AbstractExecutor 
    {
        /// <summary>
        /// Verify a collection of variables to ensure that all the variable names are valid.
        /// Users are not allowed to overwrite reserved variables or use function names as variables.
        /// If an invalid variable is detected an exception is thrown.
        /// </summary>
        /// <param name="variables">The collection of variables that must be verified.</param>
        protected void VerifyVariableNames(IDictionary<string, double> variables, IConstantRegistry constantRegistry, IFunctionRegistry functionRegistry)
        {
            var variableNames = variables.Keys.ToArray();
            foreach (var variableName in variableNames)
            {
                if(constantRegistry.IsConstantName(variableName))
                    throw new ArgumentException(
                        $"The name \"{variableName}\" is a reserved variable name that cannot be overwritten.", nameof(variables));

                if (functionRegistry.IsFunctionName(variableName))
                    throw new ArgumentException(
                        $"The name \"{variableName}\" is a function name. Parameters cannot have this name.", nameof(variables));
            }
        }

        
    }
}