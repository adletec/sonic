using System;

namespace Adletec.Sonic.Execution
{
    public class FunctionInfo
    {
        public FunctionInfo(string functionName, int numberOfParameters, bool isIdempotent, bool isOverWritable, bool isDynamicFunc, Delegate function)
        {
            this.FunctionName = functionName;
            this.NumberOfParameters = numberOfParameters;
            this.IsIdempotent = isIdempotent;
            this.IsOverWritable = isOverWritable;
            this.IsDynamicFunc = isDynamicFunc;
            this.Function = function;
        }

        public string FunctionName { get; }
        
        public int NumberOfParameters { get; }

        public bool IsOverWritable { get; }

        public bool IsIdempotent { get; }

        public bool IsDynamicFunc { get; }

        public Delegate Function { get; }
    }
}
