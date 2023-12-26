using System;
using System.Collections.Generic;
using Adletec.Sonic.Execution;
using Adletec.Sonic.Operations;
using Adletec.Sonic.Parsing.Tokenizing;

namespace Adletec.Sonic.Parsing
{
    public class AstBuilder
    {
        private readonly IFunctionRegistry functionRegistry;
        private readonly IConstantRegistry constantRegistry;
        private readonly Dictionary<char, int> operationPrecedence = new Dictionary<char, int>();

        private readonly Stack<Operation> resultStack = new Stack<Operation>();
        private readonly Stack<Token> operatorStack = new Stack<Token>();
        private readonly Stack<int> dynamicFunctionArgumentCountStack = new Stack<int>();

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
            // This is based on the Shunting-yard algorithm
            // https://en.wikipedia.org/wiki/Shunting-yard_algorithm
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
                        operatorStack.Push(token);
                        // validation ensures at least one argument for dynamic functions
                        dynamicFunctionArgumentCountStack.Push(1);
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
                    case TokenType.LeftParenthesis:
                        operatorStack.Push(token);
                        break;
                    case TokenType.RightParenthesis:
                        // parenthesised clause is finished, pop all operations in the clause from the operator stack
                        // (including the left parenthesis and the function if there is one)
                        PopOperations();

                        // Pop left parenthesis
                        operatorStack.Pop();

                        // If the operator stack is not empty and the token on top of the stack is a function, we just processed
                        // the all arguments of the function. Therefore, we need to process the function and push it onto the
                        // result stack.
                        if (operatorStack.Count >= 1 && operatorStack.Peek().TokenType == TokenType.Function)
                        {
                            var function = ConvertFunction(operatorStack.Pop());
                            resultStack.Push(function);
                        }

                        break;
                    case TokenType.ArgumentSeparator:
                        // Argument finished, pop all operations in the argument from the operator stack until the
                        // left parenthesis is encountered. Since this is done for every argument, we can assume that
                        // there won't be another argument on the operator stack and this can be repeated
                        // until the right parenthesis is encountered.
                        PopOperations();
                        dynamicFunctionArgumentCountStack.Push(dynamicFunctionArgumentCountStack.Pop() + 1);
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

                            // If the current operation is left associative and has the same or higher precedence as
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

            // We now know all precedences and can process the remaining operations in the right order
            PopOperations();

            return resultStack.Pop();
        }

        /// <summary>
        /// Pops operations from the operator stack and pushes them onto the result stack until the context ends (result
        /// stack is empty or a left parenthesis is encountered).
        /// 
        /// This is done whenever the end of a context is encountered (e.g. an argument separator or a right parenthesis).
        /// </summary>
        private void PopOperations()
        {
            // Pop operations until operator stack is empty or left parenthesis is found
            while (operatorStack.Count > 0 &&
                   operatorStack.Peek().TokenType != TokenType.LeftParenthesis)
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
        }

        private Operation ConvertOperation(Token operationToken)
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

        private Operation ConvertFunction(Token functionToken)
        {
            var functionName = (string)functionToken.Value;
            FunctionInfo functionInfo = functionRegistry.GetFunctionInfo(functionName);

            var dynamicFunctionArgumentCount = dynamicFunctionArgumentCountStack.Pop();
            int actualParameterCount = functionInfo.IsDynamicFunc
                ? dynamicFunctionArgumentCount
                : functionInfo.NumberOfParameters;

            var operations = new List<Operation>();
            for (var i = 0; i < actualParameterCount; i++)
                operations.Add(resultStack.Pop());
            operations.Reverse();

            return new Function(DataType.FloatingPoint, functionName, operations, functionInfo.IsIdempotent);
        }

        private static bool IsLeftAssociativeOperation(char character)
        {
            return character == '*' || character == '+' || character == '-' || character == '/';
        }

        private static DataType RequiredDataType(Operation argument1, Operation argument2)
        {
            return argument1.DataType == DataType.FloatingPoint || argument2.DataType == DataType.FloatingPoint
                ? DataType.FloatingPoint
                : DataType.Integer;
        }
    }
}