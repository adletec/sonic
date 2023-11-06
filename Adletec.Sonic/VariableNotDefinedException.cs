using System;

namespace Adletec.Sonic
{
    /// <summary>
    /// An exception thrown when an expression references an undefined variable.
    /// </summary>
    public class VariableNotDefinedException : Exception
    {
        public string VariableName { get; }
        
        public VariableNotDefinedException(string message, string variableName)
            : base(message)
        {
            VariableName = variableName;
        }

        public VariableNotDefinedException(string message, Exception innerException, string variableName)
            : base(message, innerException)
        {
            VariableName = variableName;
        }
    }
}
