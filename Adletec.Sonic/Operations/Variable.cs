namespace Adletec.Sonic.Operations
{
    /// <summary>
    /// Represents a variable in a mathematical formula.
    /// </summary>
    public class Variable : Operation
    {
        public Variable(string name)
            : base(DataType.FloatingPoint, true, false)
        {
            this.Name = name;
        }

        public string Name { get; }

        public override bool Equals(object obj)
        {
            var other = obj as Variable;
            return other != null && this.Name.Equals(other.Name);
        }

        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }
    }
}
