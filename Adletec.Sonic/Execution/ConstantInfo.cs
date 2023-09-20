namespace Adletec.Sonic.Execution
{
    public class ConstantInfo
    {
        public ConstantInfo(string constantName, double value)
        {
            this.ConstantName = constantName;
            this.Value = value;
        }

        public string ConstantName { get; private set; }

        public double Value { get; private set; }

    }
}
