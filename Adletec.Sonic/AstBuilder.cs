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
        
        // todo: using class-member stacks prevents this from being thread safe and forces us to create a new instance of this class for each thread
        private readonly Stack<Operation> resultStack = new Stack<Operation>();
        private readonly Stack<Token> operatorStack = new Stack<Token>();
        private readonly Stack<int> parameterCount = new Stack<int>();

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
            // todo this has no performance benefit since the class is re-instantiated for each call of Build
            resultStack.Clear();
            operatorStack.Clear();

            parameterCount.Clear();
            var tokenReferences = new List<TokenReference>(tokens.Count);

            foreach (var token in tokens)
            {
                switch (token.TokenType)
                {
                    case TokenType.Integer:
                        var integerConstant = new IntegerConstant((int)token.Value);
                        tokenReferences.Add(new TokenReference(token, integerConstant));
                        resultStack.Push(integerConstant);
                        break;
                    case TokenType.FloatingPoint:
                        var floatingPointConstant = new FloatingPointConstant((double)token.Value);
                        tokenReferences.Add(new TokenReference(token, floatingPointConstant));
                        resultStack.Push(floatingPointConstant);
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
                            if (constantRegistry.IsConstantName(tokenValue))
                            {
                                var registeredConstant =
                                    new FloatingPointConstant(constantRegistry.GetConstantInfo(tokenValue).Value);
                                tokenReferences.Add(new TokenReference(token, registeredConstant));
                                resultStack.Push(registeredConstant);
                            }
                            else
                            {
                                var variable = new Variable(tokenValue);
                                tokenReferences.Add(new TokenReference(token, variable));
                                resultStack.Push(variable);
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
                        throw new ArgumentOutOfRangeException(nameof(token),
                            $"Unexpected value \"{token}\" for {nameof(token)}.");
                }
            }

            PopOperations(false, null);

            VerifyResultStack(tokenReferences);

            return resultStack.First();
        }

        private void PopOperations(bool untilLeftBracket, Token? currentToken)
        {
            if (untilLeftBracket && !currentToken.HasValue)
                throw new ArgumentNullException(nameof(currentToken),
                    $"If the parameter \"{nameof(untilLeftBracket)}\" is set to true, " +
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
                    throw new MissingLeftBracketParserException("No matching left bracket found for the right " +
                                                                $"bracket at position {currentToken.Value.StartPosition}.",
                        currentToken.Value.StartPosition);
            }
            else
            {
                if (operatorStack.Count > 0 && operatorStack.Peek().TokenType == TokenType.LeftBracket
                                            && !(currentToken.HasValue && currentToken.Value.TokenType ==
                                                TokenType.ArgumentSeparator))
                    throw new MissingRightBracketParserException("No matching right bracket found for the left " +
                                                                 $"bracket at position {operatorStack.Peek().StartPosition}.",
                        operatorStack.Peek().StartPosition);
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
                throw new InvalidNumberOfOperationArgumentsParserException(
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

                    int numberOfParameters;

                    if (functionInfo.IsDynamicFunc)
                    {
                        numberOfParameters = parameterCount.Pop();
                    }
                    else
                    {
                        parameterCount.Pop();
                        numberOfParameters = functionInfo.NumberOfParameters;
                    }

                    var operations = new List<Operation>();
                    for (var i = 0; i < numberOfParameters; i++)
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
                throw new InvalidNumberOfFunctionArgumentsParserException(
                    $"There is a syntax issue for the function \"{functionToken.Value}\" at position {functionToken.StartPosition}. " +
                    "The number of arguments does not match with what is expected. " +
                    "This can also happen if the expression contains a variable with the same name as a function. " +
                    "In that case please rename either the variable or the function.", functionToken.StartPosition,
                    functionToken.Length, (string)functionToken.Value);
            }
        }

        private void VerifyResultStack(IList<TokenReference> tokenReferences)
        {
            if (resultStack.Count <= 1) return;

            Operation[] operations = resultStack.ToArray();

            for (var i = 1; i < operations.Length; i++)
            {
                Operation operation = operations[i];

                if (operation.GetType() == typeof(IntegerConstant))
                {
                    var constant = (IntegerConstant)operation;
                    var tokenReference = tokenReferences.First(t => ReferenceEquals(t.Operation, constant));
                    throw new UnexpectedIntegerConstantParserException(
                        $"Unexpected integer constant \"{constant.Value}\" found.", tokenReference.Token.StartPosition,
                        tokenReference.Token.Length, tokenReference.Token.Value.ToString());
                }

                if (operation.GetType() == typeof(FloatingPointConstant))
                {
                    var constant = (FloatingPointConstant)operation;
                    var tokenReference = tokenReferences.First(t => ReferenceEquals(t.Operation, constant));
                    throw new UnexpectedFloatingPointConstantParserException(
                        $"Unexpected floating point constant \"{constant.Value}\" found.",
                        tokenReference.Token.StartPosition, tokenReference.Token.Length,
                        (string)tokenReference.Token.Value);
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
    }
}