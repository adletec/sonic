using System;
using System.Collections.Generic;
using System.Linq;
using Adletec.Sonic.Execution;
using Adletec.Sonic.Operations;
using Adletec.Sonic.Tokenizer;

namespace Adletec.Sonic
{
    public class AstBuilder
    {
        private readonly IFunctionRegistry functionRegistry;
        private readonly IConstantRegistry constantRegistry;
        private readonly Dictionary<char, int> operationPrecedence = new Dictionary<char, int>();
        
        private readonly Stack<Operation> resultStack = new Stack<Operation>();
        private readonly Stack<Token> operatorStack = new Stack<Token>();
        private readonly Stack<int> parameterCountStack = new Stack<int>();

        public AstBuilder(IFunctionRegistry functionRegistry, IConstantRegistry constantRegistry)
        {
            this.functionRegistry = functionRegistry ?? throw new ArgumentNullException(nameof(functionRegistry));
            this.constantRegistry = constantRegistry ?? throw new ArgumentNullException(nameof(constantRegistry));

            operationPrecedence.Add('(', 0);
            operationPrecedence.Add('&', 1);
            operationPrecedence.Add('|', 1);
            operationPrecedence.Add('<', 2);
            operationPrecedence.Add('>', 2);
            operationPrecedence.Add('≤', 2);
            operationPrecedence.Add('≥', 2);
            operationPrecedence.Add('≠', 2);
            operationPrecedence.Add('=', 2);
            operationPrecedence.Add('+', 3);
            operationPrecedence.Add('-', 3);
            operationPrecedence.Add('*', 4);
            operationPrecedence.Add('/', 4);
            operationPrecedence.Add('%', 4);
            operationPrecedence.Add('_', 5);
            operationPrecedence.Add('^', 6);
        }

        public Operation Build(IList<Token> tokens)
        {
            for (var i = 0; i < tokens.Count; i++)
            {
                var token = tokens[i];
                switch (token.TokenType)
                {
                    case TokenType.Integer:
                        var integerConstant = new IntegerConstant((int)token.Value);
                        resultStack.Push(integerConstant);
                        break;
                    case TokenType.FloatingPoint:
                        var floatingPointConstant = new FloatingPointConstant((double)token.Value);
                        resultStack.Push(floatingPointConstant);
                        break;
                    case TokenType.Function:
                        // todo idea: have a stack of parameter-stacks; when a function is encountered, push a new parameter-stack onto the stack; when the matching right bracket is encountered, pop the parameter-stack from the stack and push the function onto the result stack
                        // might be slow.
                        // the core problem is that we don't really know how many items whe should pop from the result stack,
                        // since the parameter count is only a heuristic (counts the number of argument separators) and we
                        // can't check if there are items between the argument separators because of brackets.
                        if (functionRegistry.IsFunctionName((string)token.Value))
                        {
                            operatorStack.Push(token);
                            // the next token is the left bracket, the token after that is the first argument or the right bracket
                            // todo in case of something like function(()), this will assume that there is one parameter - validate that this is the desired behavior
                            if (tokens.Count > i + 1 && tokens[i + 2].TokenType != TokenType.RightBracket)
                            {
                                parameterCountStack.Push(1);
                            }
                            else
                            {
                                parameterCountStack.Push(0);
                            }
                        }
                        else
                        {
                            throw new UnknownFunctionParseException("${token.Value} is not a known function.",
                                token.StartPosition, token.Length, token.Value.ToString());
                        }

                        break;
                    case TokenType.Symbol:
                        var tokenValue = (string)token.Value;
                        if (constantRegistry.IsConstantName(tokenValue))
                        {
                            var registeredConstant =
                                new FloatingPointConstant(constantRegistry.GetConstantInfo(tokenValue).Value);
                            resultStack.Push(registeredConstant);
                        }
                        else
                        {
                            var variable = new Variable(tokenValue);
                            resultStack.Push(variable);
                        }

                        break;
                    case TokenType.LeftBracket:
                        operatorStack.Push(token);
                        break;
                    case TokenType.RightBracket:
                        PopOperations(true, token);
                        break;
                    case TokenType.ArgumentSeparator:
                        PopOperations(false, token);
                        parameterCountStack
                            .Push(parameterCountStack.Pop() + 1);
                        break;
                    case TokenType.Operation:
                        var currentOperationValue = (char)token.Value;
                        // Check if the operator on top of the stack has a higher precedence than the current operator
                        // If so, pop the operator from the stack and push it onto the result stack
                        // Repeat until the operator stack is empty or the operator on top of the stack has a lower precedence
                        while (operatorStack.Count > 0 &&
                               operatorStack.Peek().TokenType == TokenType.Operation)
                        {
                            Token operationOnTopOfStack = operatorStack.Peek();

                            var operationOnTopOfStackValue = (char)operationOnTopOfStack.Value;

                            // if the current operation is left associative and has the same or higher precedence as
                            // the operation on top of the stack, pop the operation from the stack and push it onto the
                            // result stack
                            if ((IsLeftAssociativeOperation(currentOperationValue) &&
                                 operationPrecedence[currentOperationValue] <=
                                 operationPrecedence[operationOnTopOfStackValue]) ||
                                operationPrecedence[currentOperationValue] <
                                operationPrecedence[operationOnTopOfStackValue])
                            {
                                operatorStack.Pop();
                                resultStack.Push(ConvertOperation(operationOnTopOfStack));
                            }
                            else
                            {
                                break;
                            }
                        }

                        operatorStack.Push(token);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(token),
                            $"Unexpected value \"{token}\" for {nameof(token)}.");
                }
            }

            PopOperations(false, null);

            return resultStack.First();
        }

        private void PopOperations(bool untilLeftBracket, Token? currentToken)
        {
            // Check preconditions -> todo: this prevents a programming error in the lib. should we really check this here?
            if (untilLeftBracket && !currentToken.HasValue)
                throw new ArgumentNullException(nameof(currentToken),
                    $"If the parameter \"{nameof(untilLeftBracket)}\" is set to true, " +
                    $"the parameter \"${nameof(currentToken)}\" cannot be null.");

            // Pop operations until operator stack is empty or left bracket is found
            while (operatorStack.Count > 0 &&
                   operatorStack.Peek().TokenType != TokenType.LeftBracket)
            {
                Token token = operatorStack.Pop();

                switch (token.TokenType)
                {
                    case TokenType.Operation:
                        resultStack.Push(ConvertOperation(token));
                        break;
                    case TokenType.Function:
                        var function = ConvertFunction(token);
                        resultStack.Push(function);
                        break;
                    default:
                        throw new InvalidOperationException("Unexpected token type.");
                }
            }


            if (untilLeftBracket)
            {
                if (operatorStack.Count > 0 &&
                    operatorStack.Peek().TokenType == TokenType.LeftBracket)
                    operatorStack.Pop();
                else
                    throw new MissingLeftBracketParseException("No matching left bracket found for the right " +
                                                               $"bracket at position {currentToken.Value.StartPosition}.",
                        currentToken.Value.StartPosition);
            }
            else
            {
                if (operatorStack.Count > 0 && operatorStack.Peek()
                                                                          .TokenType == TokenType.LeftBracket
                                                                      && !(currentToken.HasValue &&
                                                                           currentToken.Value.TokenType ==
                                                                           TokenType.ArgumentSeparator))
                    throw new MissingRightBracketParseException("No matching right bracket found for the left " +
                                                                $"bracket at position {operatorStack.Peek().StartPosition}.",
                        operatorStack.Peek().StartPosition);
            }

            if (operatorStack.Count >= 1 &&
                operatorStack.Peek().TokenType == TokenType.Function)
            {
                var function = ConvertFunction(operatorStack.Pop());
                resultStack.Push(function);
            }
        }

        private Operation ConvertOperation(Token operationToken)
        {
            try
            {
                DataType dataType;
                Operation argument1;
                Operation argument2;
                Operation divisor;
                Operation dividend;

                switch ((char)operationToken.Value)
                {
                    case '+':
                        argument2 = resultStack.Pop();
                        argument1 = resultStack.Pop();
                        dataType = RequiredDataType(argument1, argument2);

                        return new Addition(dataType, argument1, argument2);
                    case '-':
                        argument2 = resultStack.Pop();
                        argument1 = resultStack.Pop();
                        dataType = RequiredDataType(argument1, argument2);

                        return new Subtraction(dataType, argument1, argument2);
                    case '*':
                        argument2 = resultStack.Pop();
                        argument1 = resultStack.Pop();
                        dataType = RequiredDataType(argument1, argument2);

                        return new Multiplication(dataType, argument1, argument2);
                    case '/':
                        divisor = resultStack.Pop();
                        dividend = resultStack.Pop();

                        return new Division(DataType.FloatingPoint, dividend, divisor);
                    case '%':
                        divisor = resultStack.Pop();
                        dividend = resultStack.Pop();

                        return new Modulo(DataType.FloatingPoint, dividend, divisor);
                    case '_':
                        argument1 = resultStack.Pop();

                        return new UnaryMinus(argument1.DataType, argument1);
                    case '^':
                        Operation exponent = resultStack.Pop();
                        Operation @base = resultStack.Pop();

                        return new Exponentiation(DataType.FloatingPoint, @base, exponent);
                    case '&':
                        argument2 = resultStack.Pop();
                        argument1 = resultStack.Pop();
                        dataType = RequiredDataType(argument1, argument2);

                        return new And(dataType, argument1, argument2);
                    case '|':
                        argument2 = resultStack.Pop();
                        argument1 = resultStack.Pop();
                        dataType = RequiredDataType(argument1, argument2);

                        return new Or(dataType, argument1, argument2);
                    case '<':
                        argument2 = resultStack.Pop();
                        argument1 = resultStack.Pop();
                        dataType = RequiredDataType(argument1, argument2);

                        return new LessThan(dataType, argument1, argument2);
                    case '≤':
                        argument2 = resultStack.Pop();
                        argument1 = resultStack.Pop();
                        dataType = RequiredDataType(argument1, argument2);

                        return new LessOrEqualThan(dataType, argument1, argument2);
                    case '>':
                        argument2 = resultStack.Pop();
                        argument1 = resultStack.Pop();
                        dataType = RequiredDataType(argument1, argument2);

                        return new GreaterThan(dataType, argument1, argument2);
                    case '≥':
                        argument2 = resultStack.Pop();
                        argument1 = resultStack.Pop();
                        dataType = RequiredDataType(argument1, argument2);

                        return new GreaterOrEqualThan(dataType, argument1, argument2);
                    case '=':
                        argument2 = resultStack.Pop();
                        argument1 = resultStack.Pop();
                        dataType = RequiredDataType(argument1, argument2);

                        return new Equal(dataType, argument1, argument2);
                    case '≠':
                        argument2 = resultStack.Pop();
                        argument1 = resultStack.Pop();
                        dataType = RequiredDataType(argument1, argument2);

                        return new NotEqual(dataType, argument1, argument2);
                    default:
                        throw new ArgumentException($"Unknown operation \"{operationToken}\".", nameof(operationToken));
                }
            }
            catch (InvalidOperationException)
            {
                // If we encounter a Stack empty issue this means there is a syntax issue in 
                // the mathematical formula
                throw new MissingOperationArgumentParseException(
                    $"There is a syntax issue for the operation \"{operationToken.Value}\" at position {operationToken.StartPosition}. " +
                    "The number of arguments does not match with what is expected.", operationToken.StartPosition,
                    operationToken.Value.ToString());
            }
        }

        private Operation ConvertFunction(Token functionToken)
        {
            try
            {
                // todo: is the tolowerinvariant really needed?
                var functionName = ((string)functionToken.Value).ToLowerInvariant();

                if (functionRegistry.IsFunctionName(functionName))
                {
                    FunctionInfo functionInfo = functionRegistry.GetFunctionInfo(functionName);

                    // this is only derived from the number of argument separators and also always assumes that
                    // there is at least one argument, therefore we still need the try catch block below
                    int actualParameterCount;

                    if (functionInfo.IsDynamicFunc)
                    {
                        actualParameterCount = parameterCountStack.Pop();
                    }
                    else
                    {
                        actualParameterCount = parameterCountStack.Pop();
                        var expectedParameterCount = functionInfo.NumberOfParameters;
                        if (actualParameterCount != expectedParameterCount)
                            throw new InvalidFunctionArgumentCountParseException(
                                $"There is a syntax issue for the function \"{functionToken.Value}\" at expression position {functionToken.StartPosition}. " +
                                $"The number of arguments is {actualParameterCount} but {expectedParameterCount} are expected.",
                                functionToken.StartPosition, functionToken.Length, (string)functionToken.Value);
                    }

                    var operations = new List<Operation>();
                    // todo: we should now be able to pop all operations in the current context
                    for (var i = 0; i < actualParameterCount; i++)
                        operations.Add(resultStack.Pop());
                    operations.Reverse();

                    return new Function(DataType.FloatingPoint, functionName, operations, functionInfo.IsIdempotent);
                }

                throw new ArgumentException($"Unknown function \"{functionToken.Value}\".",
                    nameof(functionToken.Value));
            }
            catch (InvalidOperationException)
            {
                // If we encounter a Stack empty issue this means there is a syntax issue in 
                // the mathematical formula
                throw new InvalidFunctionArgumentCountParseException(
                    $"There is a syntax issue for the function \"{functionToken.Value}\" at position {functionToken.StartPosition}. " +
                    "The number of arguments does not match with what is expected.", functionToken.StartPosition,
                    functionToken.Length, (string)functionToken.Value);
            }
        }

        private bool IsLeftAssociativeOperation(char character)
        {
            return character == '*' || character == '+' || character == '-' || character == '/';
        }

        private DataType RequiredDataType(Operation argument1, Operation argument2)
        {
            return argument1.DataType == DataType.FloatingPoint || argument2.DataType == DataType.FloatingPoint
                ? DataType.FloatingPoint
                : DataType.Integer;
        }

        private class TokenReference
        {
            public TokenReference(Token token, Operation operation)
            {
                Token = token;
                Operation = operation;
            }

            public Operation Operation { get; }
            public Token Token { get; }
        }

        private class ParserContext
        {
        }
    }
}