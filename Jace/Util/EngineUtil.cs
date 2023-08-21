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
            var temp = new Dictionary<string, double>();
            foreach (var keyValuePair in variables)
            {
                temp.Add(keyValuePair.Key.ToLowerFast(), keyValuePair.Value);
            }

            return temp;
        }

        // This is a fast ToLower for strings that are in ASCII
        internal static string ToLowerFast(this string text)
        {
            var buffer = new StringBuilder(text.Length);
            var modified = false;
            for(var i = 0; i < text.Length; i++)
            {
                var c = text[i];

                if (c >= 'A' && c <= 'Z')
                {
                    buffer.Append((char)(c + 32));
                    modified = true;
                }
                else 
                {
                    buffer.Append(c);
                }
            }

            return modified ? buffer.ToString() : text;
        }
    }
}
