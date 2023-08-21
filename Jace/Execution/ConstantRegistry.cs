using System;
using System.Collections;
using System.Collections.Generic;
using Jace.Util;

namespace Jace.Execution
{
    public class ConstantRegistry : IConstantRegistry
    {
        private readonly bool caseSensitive;
        private readonly Dictionary<string, ConstantInfo> constants;

        public ConstantRegistry(bool caseSensitive)
        {
            this.caseSensitive = caseSensitive;
            this.constants = new Dictionary<string, ConstantInfo>();
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

            return constants.TryGetValue(ConvertConstantName(constantName), out var constantInfo) ? constantInfo : null;
        }

        public bool IsConstantName(string constantName)
        {
            if (string.IsNullOrEmpty(constantName))
                throw new ArgumentNullException(nameof(constantName));

            return constants.ContainsKey(ConvertConstantName(constantName));
        }

        public void RegisterConstant(string constantName, double value)
        {
            RegisterConstant(constantName, value, true);
        }

        public void RegisterConstant(string constantName, double value, bool isOverWritable)
        {
            if(string.IsNullOrEmpty(constantName))
                throw new ArgumentNullException(nameof(constantName));

            constantName = ConvertConstantName(constantName);

            if (constants.ContainsKey(constantName) && !constants[constantName].IsOverWritable)
            {
                throw new Exception($"The constant \"{constantName}\" cannot be overwritten.");
            }

            var constantInfo = new ConstantInfo(constantName, value, isOverWritable);

            constants[constantName] = constantInfo;
        }

        private string ConvertConstantName(string constantName)
        {
            return caseSensitive ? constantName : constantName.ToLowerFast();
        }
    }
}
