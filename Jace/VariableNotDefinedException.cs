using System;

namespace Jace
{
    /// <summary>
    /// An exception thrown when a formula must be executed with a variable that is not defined.
    /// </summary>
    public class VariableNotDefinedException : Exception
    {
        public VariableNotDefinedException(string message)
            : base(message)
        {
        }

        public VariableNotDefinedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
