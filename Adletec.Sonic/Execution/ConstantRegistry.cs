using System;
using System.Collections;
using System.Collections.Generic;

namespace Adletec.Sonic.Execution
{
    public class ConstantRegistry : IConstantRegistry
    {
        private readonly Dictionary<string, ConstantInfo> constants;

        public ConstantRegistry(bool caseSensitive)
        {
            constants = caseSensitive
                ? new Dictionary<string, ConstantInfo>()
                : new Dictionary<string, ConstantInfo>(StringComparer.OrdinalIgnoreCase);
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
            RegisterConstant(constantName, value, true);
        }

        public void RegisterConstant(string constantName, double value, bool isOverWritable)
        {
            if(string.IsNullOrEmpty(constantName))
                throw new ArgumentNullException(nameof(constantName));

            if (constants.ContainsKey(constantName) && !constants[constantName].IsOverWritable)
            {
                throw new ArgumentException($"The constant \"{constantName}\" cannot be overwritten.");
            }

            var constantInfo = new ConstantInfo(constantName, value, isOverWritable);

            constants[constantName] = constantInfo;
        }
    }
}
