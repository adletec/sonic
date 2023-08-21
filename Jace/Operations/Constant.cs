namespace Jace.Operations
{
    public abstract class Constant<T> : Operation
    {
        protected Constant(DataType dataType, T value)
            : base(dataType, false, true)
        {
            this.Value = value;
        }

        public T Value { get; }

        public override bool Equals(object obj)
        {
            if (obj is Constant<T> other)
                return this.Value.Equals(other.Value);
            return false;
        }

        public override int GetHashCode()
        {
            return this.Value.GetHashCode();
        }
    }

    public class IntegerConstant : Constant<int>
    {
        public IntegerConstant(int value)
            : base(DataType.Integer, value)
        {
        }
    }

    public class FloatingPointConstant : Constant<double>
    {
        public FloatingPointConstant(double value)
            : base(DataType.FloatingPoint, value)
        {
        }
    }
}
