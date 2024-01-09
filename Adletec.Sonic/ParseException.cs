using System;

namespace Adletec.Sonic
{
    /// <summary>
    /// The exception that is thrown when there is a syntax error in the formula provided 
    /// to the evaluator.
    /// </summary>
    public class ParseException : Exception
    {
        
        public string Expression { get; }
        public ParseException(string message, string expression)
            : base(message)
        {
            Expression = expression;
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

        public InvalidTokenParseException(string message, string expression, int tokenPosition, string token) :
            base(message, expression)
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

        public InvalidFloatingPointNumberParseException(string message, string expression, int tokenPosition, string token) :
            base(message, expression)
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

        public MissingLeftParenthesisParseException(string message, string expression, int rightParenthesisPosition) : base(message, expression)
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

        public MissingRightParenthesisParseException(string message, string expression, int leftParenthesisPosition) : base(message, expression)
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

        public UnknownFunctionParseException(string message, string expression, int functionNamePosition,
            string functionName) : base(message, expression)
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

        public InvalidArgumentCountParseException(string message, string expression, int functionNamePosition, string functionName) : base(message, expression)
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

        public MissingOperandParseException(string message, string expression, int operatorPosition, string @operator)
            : base(message, expression)
        {
            OperatorPosition = operatorPosition;
            Operator = @operator;
        }
    }
    
    /// <summary>
    /// This exception is thrown if there is no corresponding "'" for another "'" encountered during parsing.
    /// </summary>
    public class MissingQuoteParseException : ParseException
    {
        public int QuotePosition { get; }

        public MissingQuoteParseException(string message, string expression, int quotePosition)
            : base(message, expression)
        {
            QuotePosition = quotePosition;
        }
    }
}