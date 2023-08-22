namespace Adletec.Sonic.Operations
{
    public abstract class Operation
    {
        protected Operation(DataType dataType, bool dependsOnVariables, bool isIdempotent)
        {
            this.DataType = dataType;
            this.DependsOnVariables = dependsOnVariables;
            this.IsIdempotent = isIdempotent;
        }

        public DataType DataType { get; private set; }

        public bool DependsOnVariables { get; internal set; }

        public bool IsIdempotent { get; private set; }
    }
}
