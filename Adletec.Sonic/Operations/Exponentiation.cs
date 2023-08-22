namespace Adletec.Sonic.Operations
{
    public class Exponentiation : Operation
    {
        public Exponentiation(DataType dataType, Operation @base, Operation exponent)
            : base(dataType, @base.DependsOnVariables || exponent.DependsOnVariables, @base.IsIdempotent && exponent.IsIdempotent)
        {
            Base = @base;
            Exponent = exponent;
        }

        public Operation Base { get; internal set; }
        public Operation Exponent { get; internal set; }
    }
}
