using System;
using System.Collections.Generic;
using System.Linq;
using Adletec.Sonic.Execution;
using Adletec.Sonic.Operations;
using Adletec.Sonic.Tokenizer;
using Adletec.Sonic.Util;

namespace Adletec.Sonic
{
    public class AstBuilder
    {
        private readonly IFunctionRegistry functionRegistry;
        private readonly IConstantRegistry localConstantRegistry;
        private readonly Dictionary<char, int> operationPrecedence = new Dictionary<char, int>();
        private readonly Stack<Operation> resultStack = new Stack<Operation>();
        private readonly Stack<Token> operatorStack = new Stack<Token>();
        private readonly Stack<int> parameterCount = new Stack<int>();

        public AstBuilder(IFunctionRegistry functionRegistry, bool caseSensitive, IConstantRegistry compiledConstants = null)
        {
            this.functionRegistry = functionRegistry ?? throw new ArgumentNullException(nameof(functionRegistry));
            this.localConstantRegistry = compiledConstants ?? new ConstantRegistry(caseSensitive);

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

        public Operation Build(IEnumerable<Token> tokens)
        {
            resultStack.Clear();
            operatorStack.Clear();

            parameterCount.Clear();

            foreach (var token in tokens)
            {
                switch (token.TokenType)
                {
                    case TokenType.Integer:
                        resultStack.Push(new IntegerConstant((int)token.Value));
                        break;
                    case TokenType.FloatingPoint:
                        resultStack.Push(new FloatingPointConstant((double)token.Value));
                        break;
                    case TokenType.Text:
                        if (functionRegistry.IsFunctionName((string)token.Value))
                        {
                            operatorStack.Push(token);
                            parameterCount.Push(1);
                        }
                        else
                        {
                            var tokenValue = (string)token.Value;
                            if (localConstantRegistry.IsConstantName(tokenValue))
                            {
                                resultStack.Push(new FloatingPointConstant(localConstantRegistry.GetConstantInfo(tokenValue).Value));
                            }
                            else
                            {
                                resultStack.Push(new Variable(tokenValue));
                            }
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
                        parameterCount.Push(parameterCount.Pop() + 1);
                        break;
                    case TokenType.Operation:
                        var operation1 = (char)token.Value;

                        while (operatorStack.Count > 0 && (operatorStack.Peek().TokenType == TokenType.Operation ||
                            operatorStack.Peek().TokenType == TokenType.Text))
                        {
                            Token operation2Token = operatorStack.Peek();
                            var isFunctionOnTopOfStack = operation2Token.TokenType == TokenType.Text;

                            if (!isFunctionOnTopOfStack)
                            {
                                var operation2 = (char)operation2Token.Value;

                                if ((IsLeftAssociativeOperation(operation1) &&
                                        operationPrecedence[operation1] <= operationPrecedence[operation2]) ||
                                    operationPrecedence[operation1] < operationPrecedence[operation2])
                                {
                                    operatorStack.Pop();
                                    resultStack.Push(ConvertOperation(operation2Token));
                                }
                                else
                                {
                                    break;
                                }
                            }
                            else
                            {
                                operatorStack.Pop();
                                resultStack.Push(ConvertFunction(operation2Token));
                            }
                        }

                        operatorStack.Push(token);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(token), $"Unexpected value \"{token}\" for {nameof(token)}.");
                }
            }

            PopOperations(false, null);

            VerifyResultStack();

            return resultStack.First();
        }

        private void PopOperations(bool untilLeftBracket, Token? currentToken)
        {
            if (untilLeftBracket && !currentToken.HasValue)
                throw new ArgumentNullException(nameof(currentToken), $"If the parameter \"{nameof(untilLeftBracket)}\" is set to true, " +
                                                                      $"the parameter \"${nameof(currentToken)}\" cannot be null.");

            while (operatorStack.Count > 0 && operatorStack.Peek().TokenType != TokenType.LeftBracket)
            {
                Token token = operatorStack.Pop();

                switch (token.TokenType)
                {
                    case TokenType.Operation:
                        resultStack.Push(ConvertOperation(token));
                        break;
                    case TokenType.Text:
                        resultStack.Push(ConvertFunction(token));
                        break;
                }
            }

            if (untilLeftBracket)
            {
                if (operatorStack.Count > 0 && operatorStack.Peek().TokenType == TokenType.LeftBracket)
                    operatorStack.Pop();
                else
                    throw new ParseException("No matching left bracket found for the right " +
                                             $"bracket at position {currentToken.Value.StartPosition}.");
            }
            else
            {
                if (operatorStack.Count > 0 && operatorStack.Peek().TokenType == TokenType.LeftBracket 
                    && !(currentToken.HasValue && currentToken.Value.TokenType == TokenType.ArgumentSeparator))
                    throw new ParseException("No matching right bracket found for the left " +
                                             $"bracket at position {operatorStack.Peek().StartPosition}.");
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
                throw new ParseException(
                    $"There is a syntax issue for the operation \"{operationToken.Value}\" at position {operationToken.StartPosition}. " +
                    "The number of arguments does not match with what is expected.");
            }
        }

        private Operation ConvertFunction(Token functionToken)
        {
            try
            {
                var functionName = ((string)functionToken.Value).ToLowerInvariant();

                if (functionRegistry.IsFunctionName(functionName))
                {
                    FunctionInfo functionInfo = functionRegistry.GetFunctionInfo(functionName);

                    int numberOfParameters;

                    if (functionInfo.IsDynamicFunc) {
                        numberOfParameters = parameterCount.Pop();
                    }
                    else {
                        parameterCount.Pop();
                        numberOfParameters = functionInfo.NumberOfParameters;
                    }
                    
                    var operations = new List<Operation>();
                    for (var i = 0; i < numberOfParameters; i++)
                        operations.Add(resultStack.Pop());
                    operations.Reverse();

                    return new Function(DataType.FloatingPoint, functionName, operations, functionInfo.IsIdempotent);
                }
                else
                {
                    throw new ArgumentException($"Unknown function \"{functionToken.Value}\".", nameof(functionToken.Value));
                }
            }
            catch (InvalidOperationException)
            {
                // If we encounter a Stack empty issue this means there is a syntax issue in 
                // the mathematical formula
                throw new ParseException(
                    $"There is a syntax issue for the function \"{functionToken.Value}\" at position {functionToken.StartPosition}. " +
                    "The number of arguments does not match with what is expected.");
            }
        }

        private void VerifyResultStack()
        {
            if (resultStack.Count <= 1) return;
            
            Operation[] operations = resultStack.ToArray();

            for (var i = 1; i < operations.Length; i++)
            {
                Operation operation = operations[i];

                if (operation.GetType() == typeof(IntegerConstant))
                {
                    var constant = (IntegerConstant)operation;
                    throw new ParseException($"Unexpected integer constant \"{constant.Value}\" found.");
                }

                if (operation.GetType() == typeof(FloatingPointConstant))
                {
                    var constant = (FloatingPointConstant)operation;
                    throw new ParseException($"Unexpected floating point constant \"{constant.Value}\" found."); 
                }
            }

            throw new ParseException("The syntax of the provided formula is not valid.");
        }

        private bool IsLeftAssociativeOperation(char character)
        {
            return character == '*' || character == '+' || character == '-' || character == '/';
        }

        private DataType RequiredDataType(Operation argument1, Operation argument2)
        {
            return argument1.DataType == DataType.FloatingPoint || argument2.DataType == DataType.FloatingPoint ? DataType.FloatingPoint : DataType.Integer;
        }
    }
}
