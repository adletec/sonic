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

    /// <summary>
    /// This exception is thrown when an invalid token is encountered during parsing.
    /// This also includes tokens which are not valid in the current context (e.g. subsequent numbers "4 3").
    /// </summary>
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

    /// <summary>
    /// This exception is thrown when an invalid number is encountered during parsing (e.g. "4.3.2" or a malformed scientific notation).
    /// </summary>
    public class InvalidFloatingPointNumberParseException : ParseException
    {
        public int TokenPosition { get; }
        public string Token { get; }

        public InvalidFloatingPointNumberParseException(string message, int tokenPosition, string token) :
            base(message)
        {
            TokenPosition = tokenPosition;
            Token = token;
        }
    }

    /// <summary>
    /// This exception is thrown when the parser encounters a right parenthesis without a matching left parenthesis.
    /// </summary>
    public class MissingLeftParenthesisParseException : ParseException
    {
        public int RightParenthesisPosition { get; }

        public MissingLeftParenthesisParseException(string message, int rightParenthesisPosition) : base(message)
        {
            RightParenthesisPosition = rightParenthesisPosition;
        }
    }

    /// <summary>
    /// This exception is thrown when the parser encounters a left parenthesis without a matching right parenthesis.
    /// </summary>
    public class MissingRightParenthesisParseException : ParseException
    {
        public int LeftParenthesisPosition { get; }

        public MissingRightParenthesisParseException(string message, int leftParenthesisPosition) : base(message)
        {
            LeftParenthesisPosition = leftParenthesisPosition;
        }
    }
    
    /// <summary>
    /// This exception is thrown when the parser encounters a function name which is not registered with the engine.
    /// </summary>
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

    /// <summary>
    /// This exception is thrown when the parser encounters a function call with the wrong number of arguments.
    /// </summary>
    public class InvalidArgumentCountParseException : ParseException
    {
        public int FunctionNamePosition { get; }
        public string FunctionName { get; }

        public InvalidArgumentCountParseException(string message, int functionNamePosition, string functionName) : base(message)
        {
            FunctionNamePosition = functionNamePosition;
            FunctionName = functionName;
        }
    }

    /// <summary>
    /// This exception is thrown when an operation is missing an operand (e.g. "4 *").
    /// </summary>
    public class MissingOperandParseException : ParseException
    {
        public int OperatorPosition { get; }
        public string Operator { get; }

        public MissingOperandParseException(string message, int operatorPosition, string @operator)
            : base(message)
        {
            OperatorPosition = operatorPosition;
            Operator = @operator;
        }
    }
}