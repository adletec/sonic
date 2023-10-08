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

    public class InvalidTokenParserException : ParseException
    {
        public int TokenPosition { get; }
        
        public int TokenLength { get; }
        
        public string Token { get; }

        public InvalidTokenParserException(string message, int tokenPosition, int tokenLength, string token) :
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

    public class MissingLeftBracketParserException : ParseException
    {
        public int RightBracketPosition { get; }

        public MissingLeftBracketParserException(string message, int rightBracketPosition) : base(message)
        {
            RightBracketPosition = rightBracketPosition;
        }
    }

    public class MissingRightBracketParserException : ParseException
    {
        public int LeftBracketPosition { get; }

        public MissingRightBracketParserException(string message, int leftBracketPosition) : base(message)
        {
            LeftBracketPosition = leftBracketPosition;
        }
    }
    
    public class UnknownFunctionParserException : ParseException
    {
        public int FunctionNamePosition { get; }
        public int FunctionNameLength { get; }
        public string FunctionName { get; }

        public UnknownFunctionParserException(string message, int functionNamePosition, int functionNameLength,
            string functionName) : base(message)
        {
            FunctionNamePosition = functionNamePosition;
            FunctionNameLength = functionNameLength;
            FunctionName = functionName;
        }
    }

    public class InvalidNumberOfFunctionArgumentsParserException : ParseException
    {
        public int FunctionNamePosition { get; }
        public int FunctionNameLength { get; }
        public string FunctionName { get; }

        public InvalidNumberOfFunctionArgumentsParserException(string message, int functionNamePosition,
            int functionNameLength, string functionName) : base(message)
        {
            FunctionNamePosition = functionNamePosition;
            FunctionNameLength = functionNameLength;
            FunctionName = functionName;
        }
    }

    public class InvalidNumberOfOperationArgumentsParserException : ParseException
    {
        public int OperatorPosition { get; }
        public string Operator { get; }

        public InvalidNumberOfOperationArgumentsParserException(string message, int operatorPosition, string @operator)
            : base(message)
        {
            OperatorPosition = operatorPosition;
            Operator = @operator;
        }
    }

    public class UnexpectedIntegerConstantParserException : ParseException
    {
        public int ConstantPosition { get; }
        public int ConstantLength { get; }
        public string Constant { get; }

        public UnexpectedIntegerConstantParserException(string message, int constantPosition, int constantLength,
            string constant) : base(message)
        {
            ConstantPosition = constantPosition;
            ConstantLength = constantLength;
            Constant = constant;
        }
    }

    public class UnexpectedFloatingPointConstantParserException : ParseException
    {
        public int ConstantPosition { get; }
        public int ConstantLength { get; }
        public string Constant { get; }

        public UnexpectedFloatingPointConstantParserException(string message, int constantPosition, int constantLength, string constant) : base(message)
        {
            ConstantPosition = constantPosition;
            ConstantLength = constantLength;
            Constant = constant;
        }
    }

    public class InvalidSyntaxParserException : ParseException
    {
        private int SyntaxPosition { get; }
        private string Expression { get; }

        public InvalidSyntaxParserException(string message, int syntaxPosition, string expression) : base(message)
        {
            SyntaxPosition = syntaxPosition;
            Expression = expression;
        }
    }
}