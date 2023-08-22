using System;
using System.Collections.Generic;
using Adletec.Sonic.Operations;

namespace Adletec.Sonic.Execution
{
    public interface IExecutor
    {
        double Execute(Operation operation, IFunctionRegistry functionRegistry, IConstantRegistry constantRegistry);
        double Execute(Operation operation, IFunctionRegistry functionRegistry, IConstantRegistry constantRegistry, IDictionary<string, double> variables);

        Func<IDictionary<string, double>, double> BuildFormula(Operation operation, IFunctionRegistry functionRegistry, IConstantRegistry constantRegistry);
    }
}
