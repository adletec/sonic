using System;
using System.Collections.Generic;
using System.Text;

namespace Adletec.Sonic.Util
{
    /// <summary>
    /// Utility methods that can be used throughout the engine.
    /// </summary>
    internal static class EngineUtil
    {
        internal static IDictionary<string, double> ConvertToCaseInsensitiveDictionary(IDictionary<string, double> variables)
        {
            return new Dictionary<string, double>(variables, StringComparer.OrdinalIgnoreCase);
        }
    }
}
