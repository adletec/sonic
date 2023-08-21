using System;
using System.Collections.Generic;
using System.Text;

namespace Jace.Util
{
    /// <summary>
    /// Utility methods of jace that can be used throughout the engine.
    /// </summary>
    internal static class EngineUtil
    {
        internal static IDictionary<string, double> ConvertVariableNamesToLowerCase(IDictionary<string, double> variables)
        {
            return new Dictionary<string, double>(variables, StringComparer.OrdinalIgnoreCase);
        }
    }
}
