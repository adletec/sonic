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

        public string Token { get; }

        public InvalidTokenParseException(string message, int tokenPosition, string token) :
            base(message)
        {
            TokenPosition = tokenPosition;
            Token = token;
        }
    }

    public class InvalidFloatingPointNumberException : ParseException
    {
        public int TokenPosition { get; }
        public string Token { get; }

        public InvalidFloatingPointNumberException(string message, int tokenPosition, string token) :
            base(message)
        {
            TokenPosition = tokenPosition;
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
        public string FunctionName { get; }

        public UnknownFunctionParseException(string message, int functionNamePosition,
            string functionName) : base(message)
        {
            FunctionNamePosition = functionNamePosition;
            FunctionName = functionName;
        }
    }

    public class InvalidFunctionArgumentCountParseException : ParseException
    {
        public int FunctionNamePosition { get; }
        public string FunctionName { get; }

        public InvalidFunctionArgumentCountParseException(string message, int functionNamePosition, string functionName) : base(message)
        {
            FunctionNamePosition = functionNamePosition;
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
}