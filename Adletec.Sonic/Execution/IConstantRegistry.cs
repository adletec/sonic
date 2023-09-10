using System.Collections.Generic;

namespace Adletec.Sonic.Execution
{
    public interface IConstantRegistry : IEnumerable<ConstantInfo>
    {
        ConstantInfo GetConstantInfo(string constantName);
        bool IsConstantName(string constantName);
        void RegisterConstant(string constantName, double value);
    }
}
