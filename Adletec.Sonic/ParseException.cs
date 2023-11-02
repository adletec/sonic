using System;

namespace Adletec.Sonic
{
    /// <summary>
    /// The exception that is thrown when there is a syntax error in the formula provided 
    /// to the evaluator.
    /// </summary>
    public class ParseException : Exception
    {
        public ParseException(string message)
            : base(message)
        {
        }
    }

    public class InvalidTokenParseException : ParseException
    {
        public int TokenPosition { get; }
        
        public int TokenLength { get; }
        
        public string Token { get; }

        public InvalidTokenParseException(string message, int tokenPosition, int tokenLength, string token) :
            base(message)
        {
            TokenPosition = tokenPosition;
            TokenLength = tokenLength;
            Token = token;
        }
    }

    public class InvalidFloatingPointNumberException : ParseException
    {
        public int TokenPosition { get; }
        public int TokenLength { get; }
        public string Token { get; }

        public InvalidFloatingPointNumberException(string message, int tokenPosition, int tokenLength, string token) :
            base(message)
        {
            TokenPosition = tokenPosition;
            TokenLength = tokenLength;
            Token = token;
        }
    }

    public class MissingLeftBracketParseException : ParseException
    {
        public int RightBracketPosition { get; }

        public MissingLeftBracketParseException(string message, int rightBracketPosition) : base(message)
        {
            RightBracketPosition = rightBracketPosition;
        }
    }

    public class MissingRightBracketParseException : ParseException
    {
        public int LeftBracketPosition { get; }

        public MissingRightBracketParseException(string message, int leftBracketPosition) : base(message)
        {
            LeftBracketPosition = leftBracketPosition;
        }
    }
    
    public class UnknownFunctionParseException : ParseException
    {
        public int FunctionNamePosition { get; }
        public int FunctionNameLength { get; }
        public string FunctionName { get; }

        public UnknownFunctionParseException(string message, int functionNamePosition, int functionNameLength,
            string functionName) : base(message)
        {
            FunctionNamePosition = functionNamePosition;
            FunctionNameLength = functionNameLength;
            FunctionName = functionName;
        }
    }

    public class InvalidFunctionArgumentCountParseException : ParseException
    {
        public int FunctionNamePosition { get; }
        public int FunctionNameLength { get; }
        public string FunctionName { get; }

        public InvalidFunctionArgumentCountParseException(string message, int functionNamePosition,
            int functionNameLength, string functionName) : base(message)
        {
            FunctionNamePosition = functionNamePosition;
            FunctionNameLength = functionNameLength;
            FunctionName = functionName;
        }
    }

    public class MissingOperationArgumentParseException : ParseException
    {
        public int OperatorPosition { get; }
        public string Operator { get; }

        public MissingOperationArgumentParseException(string message, int operatorPosition, string @operator)
            : base(message)
        {
            OperatorPosition = operatorPosition;
            Operator = @operator;
        }
    }

    public class InvalidSyntaxParseException : ParseException
    {
        private int SyntaxPosition { get; }
        private string Expression { get; }

        public InvalidSyntaxParseException(string message, int syntaxPosition, string expression) : base(message)
        {
            SyntaxPosition = syntaxPosition;
            Expression = expression;
        }
    }
}