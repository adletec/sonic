using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Adletec.Sonic.Execution;
using Adletec.Sonic.Parsing.Tokenizing;

namespace Adletec.Sonic.Parsing
{
    /// <summary>
    /// A validator for the token list produced by the <see cref="TokenReader"/>.
    /// </summary>
    public class Validator
    {
        private readonly IFunctionRegistry functionRegistry;
        private readonly CultureInfo cultureInfo;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="functionRegistry">The function registry also used for evaluation.</param>
        /// <param name="cultureInfo">The culture info also used for evaluation.</param>
        public Validator(IFunctionRegistry functionRegistry, CultureInfo cultureInfo)
        {
            this.functionRegistry = functionRegistry;
            this.cultureInfo = cultureInfo;
        }

        /// <summary>
        /// Validates the token list produced by the <see cref="TokenReader"/>.
        /// </summary>
        /// <param name="tokenList">The token list as produced by <see cref="TokenReader"/>.</param>
        /// <exception cref="ArgumentException">If the token list is empty.</exception>
        public void Validate(IList<Token> tokenList, string expression)
        {
            var contextStack = new Stack<ValidationContext>();

            if (tokenList.Count == 0)
            {
                throw new ArgumentException("Token list cannot be empty", nameof(tokenList));
            }

            var firstToken = tokenList[0];
            // we'll validate all validate all tokens on the base of their predecessor and check for illegal combinations

            // the first token is a special case, since it has no predecessor
            // legal first token: value (function, integer, floating point, symbol), left parenthesis
            // illegal predecessor: binary operation, argument separator, right parenthesis
            switch (firstToken.TokenType)
            {
                case TokenType.Operation when IsBinaryOperation(firstToken):
                    // if the first token is a binary operation, there is no left argument
                    ThrowMissingOperationArgumentParseException(firstToken, expression, "There is no left argument.");
                    break;
                case TokenType.ArgumentSeparator:
                    // an argument separator must be inside a function, which is not the case for the first token
                    ThrowInvalidTokenParseException(firstToken, expression);
                    break;
                case TokenType.RightParenthesis:
                    // if the first token is a right parenthesis, there is no matching left parenthesis
                    ThrowMissingLeftParenthesisParseException(firstToken, expression);
                    break;
                case TokenType.LeftParenthesis:
                    // the first token is a left parenthesis, so we start a parenthesised clause
                    contextStack.Push(new ValidationContext()
                    {
                        IsFunction = false,
                        ActualArgumentCount = 0,
                        ExpectedArgumentCount = 0,
                        // this parenthesis:
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
                    case TokenType.Operation when IsUnaryOperation(token):
                        switch (tokenBefore.TokenType)
                        {
                            // legal predecessor: operation, left parenthesis, argument separator
                            // illegal predecessor: another value (function, integer, floating point, symbol), right parenthesis
                            case TokenType.Function:
                            case TokenType.Integer:
                            case TokenType.FloatingPoint:
                            case TokenType.Symbol:
                            case TokenType.RightParenthesis:
                                ThrowInvalidTokenParseException(token, expression);
                                break;
                        }

                        // values (and parenthesised clauses, see LeftParenthesis) are valid arguments for functions
                        if (IsStartOfArgument(contextStack, tokenBefore))
                        {
                            contextStack.Peek().ActualArgumentCount++;
                        }

                        break;
                    case TokenType.Operation:
                        // legal predecessor: value, function, right parenthesis
                        // illegal: consecutive operations, left parenthesis, argument separator
                        switch (tokenBefore.TokenType)
                        {
                            case TokenType.LeftParenthesis:
                            case TokenType.ArgumentSeparator:
                                ThrowInvalidTokenParseException(token, expression);
                                break;
                            case TokenType.Operation:
                                ThrowMissingOperationArgumentParseException(tokenBefore, expression,
                                    "There is no right argument.");
                                break;
                        }

                        break;
                    case TokenType.ArgumentSeparator:
                        // legal predecessor: values, functions, right parenthesis
                        // illegal predecessor: consecutive argument separators, left parenthesis, operation
                        switch (tokenBefore.TokenType)
                        {
                            case TokenType.ArgumentSeparator:
                            case TokenType.LeftParenthesis:
                                ThrowInvalidTokenParseException(token, expression);
                                break;
                            case TokenType.Operation:
                                if (IsBinaryOperation(tokenBefore))
                                {
                                    ThrowMissingOperationArgumentParseException(token, expression,
                                        "There is no right argument.");
                                }

                                break;
                        }

                        // additional constraints: must be inside a function, count must match the
                        // number of arguments of the function (checked on context exit)
                        if (contextStack.Count == 0)
                        {
                            ThrowInvalidTokenParseException(token, expression,
                                "Argument separator is outside of function.");
                        }

                        var validationContext = contextStack.Peek();
                        if (!validationContext.IsFunction)
                        {
                            ThrowInvalidTokenParseException(token, expression,
                                "Argument separator must be a direct child of a function.");
                        }

                        break;
                    case TokenType.LeftParenthesis:
                        // legal predecessor: argument separator, function, operation, left parenthesis
                        // illegal predecessor: right parenthesis, value
                        switch (tokenBefore.TokenType)
                        {
                            case TokenType.Integer:
                            case TokenType.FloatingPoint:
                            case TokenType.Symbol:
                            case TokenType.RightParenthesis:
                                ThrowInvalidTokenParseException(token, expression);
                                break;
                        }

                        var isFunction = tokenBefore.TokenType == TokenType.Function;
                        if (isFunction)
                        {
                            // the parenthesis is part of a function call, so we create a function context
                            var functionInfo = functionRegistry.GetFunctionInfo((string)tokenBefore.Value);
                            if (functionInfo == null)
                            {
                                ThrowUnknownFunctionParseException(tokenBefore, expression);
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
                            // the parenthesis is not part of a function call, so it's a parenthesised clause
                            if (IsStartOfArgument(contextStack, tokenBefore))
                            {
                                contextStack.Peek().ActualArgumentCount++;
                            }

                            contextStack.Push(new ValidationContext()
                            {
                                IsFunction = false,
                                ActualArgumentCount = 0,
                                ExpectedArgumentCount = 0,
                                // this parenthesis:
                                RootToken = token
                            });
                        }

                        break;
                    case TokenType.RightParenthesis:
                        // legal predecessor: value (not including function), right parenthesis
                        // illegal predecessor: left parenthesis (unless for parameterless functions), operation, argument separator
                        // technically, also function. but that can never happen since function tokens must be followed
                        // by a left parenthesis to be identified as such.
                        switch (tokenBefore.TokenType)
                        {
                            case TokenType.Operation:
                                ThrowMissingOperationArgumentParseException(token, expression,
                                    "There is no right argument.");
                                break;
                            case TokenType.ArgumentSeparator:
                                ThrowInvalidTokenParseException(token, expression,
                                    "Argument separator without following argument.");
                                break;
                            case TokenType.LeftParenthesis when !IsFunctionOnTopOfStack(contextStack):
                                ThrowInvalidTokenParseException(token, expression,
                                    "Empty parentheses are not allowed.");
                                break;
                        }

                        if (contextStack.Count == 0)
                        {
                            ThrowMissingLeftParenthesisParseException(token, expression);
                        }

                        var context = contextStack.Pop();
                        if (context.IsDynamic)
                        {
                            if (context.ActualArgumentCount < 1)
                            {
                                ThrowInvalidDynamicFunctionArgumentCountParseException(context.RootToken, expression);
                            }

                            // dynamic functions can have any number of arguments
                            break;
                        }

                        if (context.ExpectedArgumentCount != context.ActualArgumentCount)
                        {
                            ThrowInvalidFunctionArgumentCountParseException(context.RootToken, expression,
                                context.ExpectedArgumentCount, context.ActualArgumentCount);
                        }

                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            if (contextStack.Any())
            {
                ThrowMissingRightParenthesisParseException(contextStack.Peek().RootToken, expression);
            }

            // the last token is a special case, since it has no successor
            // legal last token: value (function, integer, floating point, symbol), right parenthesis
            // illegal last token: operation
            // also illegal, but already checked: argument separator (already checked -> outside function or missing right parenthesis),
            // left parenthesis (already checked -> missing right parenthesis)
            if (tokenList[tokenList.Count - 1].TokenType == TokenType.Operation)
            {
                ThrowMissingOperationArgumentParseException(tokenList.Last(), expression,
                    "There is no right argument.");
            }
        }

        private void ThrowInvalidTokenParseException(Token token, string expression, string message = null)
        {
            var tokenString = token.Value is double d ? d.ToString(cultureInfo) : token.Value.ToString();
            var tokenPosition = token.StartPosition;
            throw new InvalidTokenParseException(
                $"Unexpected token at position {tokenPosition} in expression: \"{tokenString}\". {message}",
                expression, tokenPosition, tokenString);
        }

        private static void ThrowMissingOperationArgumentParseException(Token token, string expression,
            string message = null)
        {
            var tokenString = token.Value.ToString();
            var tokenPosition = token.StartPosition;
            throw new MissingOperandParseException(
                $"Missing argument for operation \"{tokenString}\" at position {tokenPosition}. {message}",
                expression, tokenPosition, tokenString);
        }

        private static void ThrowInvalidDynamicFunctionArgumentCountParseException(Token rootToken, string expression,
            string message = null)
        {
            var functionName = rootToken.Value.ToString();
            var functionNamePosition = rootToken.StartPosition;
            throw new InvalidArgumentCountParseException(
                $"Invalid argument count for dynamic function \"{functionName}\" at position {functionNamePosition}. Expected to find at least one argument, but found none. {message}",
                expression, functionNamePosition, functionName);
        }

        private static void ThrowInvalidFunctionArgumentCountParseException(Token rootToken, string expression,
            int expectedArguments,
            int foundArguments, string message = null)
        {
            var functionName = rootToken.Value.ToString();
            var functionNamePosition = rootToken.StartPosition;
            throw new InvalidArgumentCountParseException(
                $"Invalid argument count for function \"{functionName}\" at position {functionNamePosition}. Expected {expectedArguments}, but found {foundArguments}. {message}",
                expression, functionNamePosition, functionName);
        }

        private static void ThrowMissingLeftParenthesisParseException(Token token, string expression,
            string message = null)
        {
            var tokenPosition = token.StartPosition;
            throw new MissingLeftParenthesisParseException(
                $"Missing left parenthesis for right parenthesis at position {tokenPosition}. {message}",
                expression, tokenPosition);
        }

        private static void ThrowMissingRightParenthesisParseException(Token token, string expression,
            string message = null)
        {
            var tokenPosition = token.StartPosition;
            throw new MissingRightParenthesisParseException(
                $"Missing right parenthesis for left parenthesis at position {tokenPosition}. {message}", expression,
                tokenPosition);
        }

        private static void ThrowUnknownFunctionParseException(Token token, string expression, string message = null)
        {
            var tokenString = token.Value.ToString();
            var tokenPosition = token.StartPosition;
            throw new UnknownFunctionParseException(
                $"Unknown function \"{tokenString}\" at position {tokenPosition}. {message}", expression,
                tokenPosition, token.Value.ToString());
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
                previousToken.TokenType == TokenType.LeftParenthesis
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