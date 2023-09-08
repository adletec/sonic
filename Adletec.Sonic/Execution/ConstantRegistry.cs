using System;
using System.Collections;
using System.Collections.Generic;

namespace Adletec.Sonic.Execution
{
    public class ConstantRegistry : IConstantRegistry
    {
        private readonly Dictionary<string, ConstantInfo> constants;
        private readonly bool guardedMode;

        public ConstantRegistry(bool caseSensitive, bool guardedMode)
        {
            constants = caseSensitive
                ? new Dictionary<string, ConstantInfo>()
                : new Dictionary<string, ConstantInfo>(StringComparer.OrdinalIgnoreCase);
            this.guardedMode = guardedMode;
        }

        public IEnumerator<ConstantInfo> GetEnumerator()
        {
            return constants.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public ConstantInfo GetConstantInfo(string constantName)
        {
            if (string.IsNullOrEmpty(constantName))
                throw new ArgumentNullException(nameof(constantName));

            return constants.TryGetValue(constantName, out var constantInfo) ? constantInfo : null;
        }

        public bool IsConstantName(string constantName)
        {
            if (string.IsNullOrEmpty(constantName))
                throw new ArgumentNullException(nameof(constantName));

            return constants.ContainsKey(constantName);
        }

        public void RegisterConstant(string constantName, double value)
        {
            if(string.IsNullOrEmpty(constantName))
                throw new ArgumentNullException(nameof(constantName));

            if (guardedMode && constants.ContainsKey(constantName))
            {
                throw new ArgumentException($"The constant \"{constantName}\" cannot be overwritten.");
            }

            var constantInfo = new ConstantInfo(constantName, value);

            constants[constantName] = constantInfo;
        }
    }
}
