using System;
using System.Collections.Generic;
using Adletec.Sonic.Execution;
using Adletec.Sonic.Tokenizer;

namespace Adletec.Sonic
{
    public class Validator
    {
        private readonly IFunctionRegistry functionRegistry;

        public Validator(IFunctionRegistry functionRegistry)
        {
            this.functionRegistry = functionRegistry;
        }

        public void Validate(IList<Token> tokenList)
        {
            var contextStack = new Stack<ValidationContext>();

            if (tokenList.Count == 0)
            {
                throw new ArgumentException("Token list cannot be empty", nameof(tokenList));
            }

            var firstToken = tokenList[0];
            // we'll validate all validate all tokens on the base of their predecessor and check for illegal combinations

            // the first token is a special case, since it has no predecessor
            // legal first token: value (function, integer, floating point, symbol), left bracket
            // illegal predecessor: binary operation, argument separator, right bracket
            switch (firstToken.TokenType)
            {
                case TokenType.Operation:
                    if (IsBinaryOperation(firstToken))
                    {
                        // if the first token is a binary operation, there is no left argument
                        ThrowMissingOperationArgumentParseException(firstToken, "There is no left argument.");
                    }

                    break;
                case TokenType.ArgumentSeparator:
                    // an argument separator must be inside a function, which is not the case for the first token
                    ThrowInvalidTokenParseException(firstToken);
                    break;
                case TokenType.RightBracket:
                    // if the first token is a right bracket, there is no matching left bracket
                    ThrowMissingLeftBracketParseException(firstToken);
                    break;
            }

            // for all following tokens, the same rules apply
            for (var index = 1; index < tokenList.Count; index++)
            {
                var token = tokenList[index];
                var tokenBefore = tokenList[index - 1];
                switch (token.TokenType)
                {
                    // these are all basically values in the context of an expression;
                    case TokenType.Function:
                    case TokenType.Integer:
                    case TokenType.FloatingPoint:
                    case TokenType.Symbol:
                        switch (tokenBefore.TokenType)
                        {
                            // legal predecessor: operation, left bracket, argument separator
                            // illegal predecessor: another value (function, integer, floating point, symbol), right bracket
                            case TokenType.Function:
                            case TokenType.Integer:
                            case TokenType.FloatingPoint:
                            case TokenType.Symbol:
                            case TokenType.RightBracket:
                                ThrowInvalidTokenParseException(token);
                                break;
                        }

                        break;
                    case TokenType.Operation:
                        // legal predecessor: value, function, right bracket
                        // illegal: consecutive operations, left bracket, argument separator
                        switch (tokenBefore.TokenType)
                        {
                            case TokenType.LeftBracket:
                            case TokenType.ArgumentSeparator:
                                ThrowInvalidTokenParseException(token);
                                break;
                            case TokenType.Operation:
                                ThrowMissingOperationArgumentParseException(tokenBefore, "There is no right argument.");
                                break;
                        }

                        break;
                    case TokenType.ArgumentSeparator:
                        // legal predecessor: values, functions, right bracket
                        // illegal predecessor: consecutive argument separators, left bracket, operation
                        switch (tokenBefore.TokenType)
                        {
                            case TokenType.ArgumentSeparator:
                            case TokenType.LeftBracket:
                                ThrowInvalidTokenParseException(token);
                                break;
                            case TokenType.Operation:
                                if (IsBinaryOperation(tokenBefore))
                                {
                                    ThrowMissingOperationArgumentParseException(token,
                                        "There is no right argument.");
                                }

                                break;
                        }

                        // additional constraints: must be inside a function, count must match the
                        // number of arguments of the function (checked on context exit)
                        if (contextStack.Count == 0)
                        {
                            ThrowInvalidTokenParseException(token, "Argument separator is outside of function.");
                        }

                        var validationContext = contextStack.Peek();
                        if (!validationContext.IsFunction)
                        {
                            ThrowInvalidTokenParseException(token,
                                "Argument separator must be a direct child of a function.");
                        }

                        validationContext.ArgumentSeparatorCount++;
                        break;
                    case TokenType.LeftBracket:
                        // legal predecessor: argument separator, function, operation, left bracket
                        // illegal predecessor: right bracket, value
                        switch (tokenBefore.TokenType)
                        {
                            case TokenType.Integer:
                            case TokenType.FloatingPoint:
                            case TokenType.Symbol:
                            case TokenType.RightBracket:
                                ThrowInvalidTokenParseException(token);
                                break;
                        }

                        var isFunction = tokenBefore.TokenType == TokenType.Function;
                        if (isFunction)
                        {
                            var functionInfo = functionRegistry.GetFunctionInfo((string)tokenBefore.Value);
                            contextStack.Push(
                                new ValidationContext()
                                {
                                    IsFunction = true,
                                    ExpectedArgumentSeparatorCount = functionInfo.NumberOfParameters - 1,
                                    ArgumentSeparatorCount = 0,
                                    RootToken = tokenBefore
                                }
                            );
                        }
                        else
                        {
                            contextStack.Push(new ValidationContext()
                            {
                                IsFunction = false,
                                ArgumentSeparatorCount = 0,
                                ExpectedArgumentSeparatorCount = 0
                            });
                        }

                        break;
                    case TokenType.RightBracket:
                        // legal predecessor: value, function, right bracket
                        // illegal predecessor: left bracket, operation, argument separator
                        switch (tokenBefore.TokenType)
                        {
                            case TokenType.Operation:
                                if (IsBinaryOperation(tokenBefore))
                                {
                                    ThrowMissingOperationArgumentParseException(token,
                                        "There is no right argument.");
                                }

                                break;
                            case TokenType.ArgumentSeparator:
                                ThrowInvalidTokenParseException(token,
                                    "Argument separator without following argument.");
                                break;
                            case TokenType.LeftBracket:
                                ThrowInvalidTokenParseException(token, "Empty brackets are not allowed.");
                                break;
                        }

                        if (contextStack.Count == 0)
                        {
                            ThrowMissingLeftBracketParseException(token);
                        }
                        var context = contextStack.Pop();
                        if (context.ExpectedArgumentSeparatorCount != context.ArgumentSeparatorCount)
                        {
                            ThrowInvalidFunctionArgumentCountParseException(context.RootToken,
                                context.ExpectedArgumentSeparatorCount, context.ArgumentSeparatorCount);
                        }

                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private static void ThrowInvalidTokenParseException(Token token, string message = null)
        {
            var tokenString = token.Value.ToString();
            var tokenPosition = token.StartPosition;
            throw new InvalidTokenParseException(
                $"Unexpected token at position {tokenPosition} in expression: \"{tokenString}\". {message}",
                tokenPosition, token.Length, tokenString);
        }

        private static void ThrowMissingOperationArgumentParseException(Token token, string message = null)
        {
            var tokenString = token.Value.ToString();
            var tokenPosition = token.StartPosition;
            throw new MissingOperationArgumentParseException(
                $"Missing argument for operation \"{tokenString}\" at position {tokenPosition}. {message}",
                tokenPosition, tokenString);
        }

        private static void ThrowInvalidFunctionArgumentCountParseException(Token rootToken, int expectedArguments,
            int foundArguments, string message = null)
        {
            var functionName = rootToken.Value.ToString();
            var functionNamePosition = rootToken.StartPosition;
            var functionNameLength = rootToken.Length;
            throw new InvalidFunctionArgumentCountParseException(
                $"Invalid argument count for function \"{functionName}\" at position {functionNamePosition}. Expected {expectedArguments + 1}, but found {foundArguments + 1}. {message}",
                functionNamePosition, functionNameLength, functionName);
        }

        private static void ThrowMissingLeftBracketParseException(Token token, string message = null)
        {
            var tokenPosition = token.StartPosition;
            throw new MissingLeftBracketParseException(
                $"Missing left bracket for right bracket at position {tokenPosition}. {message}",
                tokenPosition);
        }

        private static bool IsBinaryOperation(Token token)
        {
            // the only unary operation is the negation
            return token.TokenType == TokenType.Operation && (char)token.Value != '_';
        }

        private class ValidationContext
        {
            public bool IsFunction { get; set; }
            public int ArgumentSeparatorCount { get; set; }
            public int ExpectedArgumentSeparatorCount { get; set; }
            public Token RootToken { get; set; }
        }
    }
}