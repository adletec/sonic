using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Adletec.Sonic.Execution;
using Adletec.Sonic.Tokenizer;

namespace Adletec.Sonic
{
    public class Validator
    {
        private readonly IFunctionRegistry functionRegistry;
        private readonly CultureInfo cultureInfo;

        public Validator(IFunctionRegistry functionRegistry, CultureInfo cultureInfo)
        {
            this.functionRegistry = functionRegistry;
            this.cultureInfo = cultureInfo;
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
                case TokenType.Operation when IsBinaryOperation(firstToken):
                    // if the first token is a binary operation, there is no left argument
                    ThrowMissingOperationArgumentParseException(firstToken, "There is no left argument.");
                    break;
                case TokenType.ArgumentSeparator:
                    // an argument separator must be inside a function, which is not the case for the first token
                    ThrowInvalidTokenParseException(firstToken);
                    break;
                case TokenType.RightBracket:
                    // if the first token is a right bracket, there is no matching left bracket
                    ThrowMissingLeftBracketParseException(firstToken);
                    break;
                case TokenType.LeftBracket:
                    // the first token is a left bracket, so we start a bracketed clause
                    contextStack.Push(new ValidationContext()
                    {
                        IsFunction = false,
                        ActualArgumentCount = 0,
                        ExpectedArgumentCount = 0,
                        // this bracket:
                        RootToken = firstToken
                    });
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

                        // values (and bracketed clauses, see LeftBracket) are valid arguments for functions
                        if (IsStartOfArgument(contextStack, tokenBefore))
                        {
                            contextStack.Peek().ActualArgumentCount++;
                        }

                        break;
                    case TokenType.Operation when IsUnaryOperation(token):
                        // basically the same as a value, but doesn't count as argument for a function
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
                            // the bracket is part of a function call, so we create a function context
                            var functionInfo = functionRegistry.GetFunctionInfo((string)tokenBefore.Value);
                            if (functionInfo == null)
                            {
                                ThrowUnknownFunctionParseException(tokenBefore);
                            }

                            contextStack.Push(
                                new ValidationContext()
                                {
                                    IsFunction = true,
                                    IsDynamic = functionInfo.IsDynamicFunc,
                                    ExpectedArgumentCount = functionInfo.NumberOfParameters,
                                    ActualArgumentCount = 0,
                                    // the function:
                                    RootToken = tokenBefore
                                }
                            );
                        }
                        else
                        {
                            // the bracket is not part of a function call, so it's a bracketed clause
                            if (IsStartOfArgument(contextStack, tokenBefore))
                            {
                                contextStack.Peek().ActualArgumentCount++;
                            }

                            contextStack.Push(new ValidationContext()
                            {
                                IsFunction = false,
                                ActualArgumentCount = 0,
                                ExpectedArgumentCount = 0,
                                // this bracket:
                                RootToken = token
                            });
                        }

                        break;
                    case TokenType.RightBracket:
                        // legal predecessor: value (not including function), right bracket
                        // illegal predecessor: left bracket (unless for parameterless functions), operation, argument separator
                        // technically, also function. but that can never happen since function tokens must be followed
                        // by a left bracket to be identified as such.
                        switch (tokenBefore.TokenType)
                        {
                            case TokenType.Operation:
                                ThrowMissingOperationArgumentParseException(token, "There is no right argument.");
                                break;
                            case TokenType.ArgumentSeparator:
                                ThrowInvalidTokenParseException(token,
                                    "Argument separator without following argument.");
                                break;
                            case TokenType.LeftBracket when !IsFunctionOnTopOfStack(contextStack):
                                ThrowInvalidTokenParseException(token, "Empty brackets are not allowed.");
                                break;
                        }

                        if (contextStack.Count == 0)
                        {
                            ThrowMissingLeftBracketParseException(token);
                        }

                        var context = contextStack.Pop();
                        if (context.IsDynamic)
                        {
                            if (context.ActualArgumentCount < 1)
                            {
                                ThrowInvalidFunctionArgumentCountParseException(context.RootToken, -1,
                                    context.ActualArgumentCount, "Dynamic functions must have at least one argument.");
                            }

                            // dynamic functions can have any number of arguments
                            break;
                        }

                        if (context.ExpectedArgumentCount != context.ActualArgumentCount)
                        {
                            ThrowInvalidFunctionArgumentCountParseException(context.RootToken,
                                context.ExpectedArgumentCount, context.ActualArgumentCount);
                        }

                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            if (contextStack.Any())
            {
                ThrowMissingRightBracketParseException(contextStack.Peek().RootToken);
            }

            // the last token is a special case, since it has no successor
            // legal last token: value (function, integer, floating point, symbol), right bracket
            // illegal last token: operation
            // also illegal, but already checked: argument separator (already checked -> outside function or missing right bracket),
            // left bracket (already checked -> missing right bracket)
            if (tokenList[tokenList.Count - 1].TokenType == TokenType.Operation)
            {
                ThrowMissingOperationArgumentParseException(tokenList.Last(), "There is no right argument.");
            }
        }

        private void ThrowInvalidTokenParseException(Token token, string message = null)
        {
            var tokenString = token.Value is double d ? d.ToString(cultureInfo) : token.Value.ToString();
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
        
        private static void ThrowInvalidDynamicFunctionArgumentCountParseException(Token rootToken, string message = null)
        {
            var functionName = rootToken.Value.ToString();
            var functionNamePosition = rootToken.StartPosition;
            var functionNameLength = rootToken.Length;
            throw new InvalidFunctionArgumentCountParseException(
                $"Invalid argument count for dynamic function \"{functionName}\" at position {functionNamePosition}. Expected to find at least one argument, but found none. {message}",
                functionNamePosition, functionNameLength, functionName);
        }

        private static void ThrowInvalidFunctionArgumentCountParseException(Token rootToken, int expectedArguments,
            int foundArguments, string message = null)
        {
            var functionName = rootToken.Value.ToString();
            var functionNamePosition = rootToken.StartPosition;
            var functionNameLength = rootToken.Length;
            throw new InvalidFunctionArgumentCountParseException(
                $"Invalid argument count for function \"{functionName}\" at position {functionNamePosition}. Expected {expectedArguments}, but found {foundArguments}. {message}",
                functionNamePosition, functionNameLength, functionName);
        }

        private static void ThrowMissingLeftBracketParseException(Token token, string message = null)
        {
            var tokenPosition = token.StartPosition;
            throw new MissingLeftBracketParseException(
                $"Missing left bracket for right bracket at position {tokenPosition}. {message}",
                tokenPosition);
        }

        private static void ThrowMissingRightBracketParseException(Token token, string message = null)
        {
            var tokenPosition = token.StartPosition;
            throw new MissingRightBracketParseException(
                $"Missing right bracket for left bracket at position {tokenPosition}. {message}",
                tokenPosition);
        }

        private static void ThrowUnknownFunctionParseException(Token token, string message = null)
        {
            var tokenString = token.Value.ToString();
            var tokenPosition = token.StartPosition;
            throw new UnknownFunctionParseException(
                $"Unknown function \"{tokenString}\" at position {tokenPosition}. {message}",
                tokenPosition, token.Length, token.Value.ToString());
        }

        private static bool IsUnaryOperation(Token token)
        {
            // the only unary operation is the negation
            return token.TokenType == TokenType.Operation && (char)token.Value == '_';
        }

        private static bool IsBinaryOperation(Token token)
        {
            // the only unary operation is the negation
            return token.TokenType == TokenType.Operation && (char)token.Value != '_';
        }

        private static bool IsStartOfArgument(Stack<ValidationContext> contextStack, Token previousToken)
        {
            return IsFunctionOnTopOfStack(contextStack) && (
                previousToken.TokenType == TokenType.ArgumentSeparator ||
                previousToken.TokenType == TokenType.LeftBracket
            );
        }

        private static bool IsFunctionOnTopOfStack(Stack<ValidationContext> contextStack)
        {
            if (contextStack.Count == 0)
            {
                return false;
            }

            var contextOnTop = contextStack.Peek();
            return contextOnTop.IsFunction;
        }

        private class ValidationContext
        {
            public bool IsFunction { get; set; }
            public int ExpectedArgumentCount { get; set; }
            public int ActualArgumentCount { get; set; }
            public Token RootToken { get; set; }
            public bool IsDynamic { get; set; }
        }
    }
}