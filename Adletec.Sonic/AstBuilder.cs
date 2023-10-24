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
        // the parser context encapsulates the state of the parser; when we encounter a function, we push a new parser
        // context onto the stack, and we'll pop it whenever the function has been parsed;
        // that way we don't take parameters off the result stack that belong to the parent function
        private readonly Stack<ParserContext> parserContextStack = new Stack<ParserContext>();

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
            parserContextStack.Clear();
            parserContextStack.Push(new ParserContext());
            
            var tokenReferences = new List<TokenReference>(tokens.Count);

            for (var i = 0; i < tokens.Count; i++)
            {
                var token = tokens[i];
                switch (token.TokenType)
                {
                    case TokenType.Integer:
                        var integerConstant = new IntegerConstant((int)token.Value);
                        tokenReferences.Add(new TokenReference(token, integerConstant));
                        parserContextStack.Peek().ResultStack.Push(integerConstant);
                        break;
                    case TokenType.FloatingPoint:
                        var floatingPointConstant = new FloatingPointConstant((double)token.Value);
                        tokenReferences.Add(new TokenReference(token, floatingPointConstant));
                        parserContextStack.Peek().ResultStack.Push(floatingPointConstant);
                        break;
                    case TokenType.Function:
                        // todo idea: have a stack of parameter-stacks; when a function is encountered, push a new parameter-stack onto the stack; when the matching right bracket is encountered, pop the parameter-stack from the stack and push the function onto the result stack
                        // might be slow.
                        // the core problem is that we don't really know how many items whe should pop from the result stack,
                        // since the parameter count is only a heuristic (counts the number of argument separators) and we
                        // can't check if there are items between the argument separators because of brackets.
                        if (functionRegistry.IsFunctionName((string)token.Value))
                        {
                            parserContextStack.Push(new ParserContext());
                            parserContextStack.Peek().OperatorStack.Push(token);
                            // the next token is the left bracket, the token after that is the first argument or the right bracket
                            // todo in case of something like function(()), this will assume that there is one parameter - validate that this is the desired behavior
                            if (tokens.Count > i + 1 && tokens[i + 2].TokenType != TokenType.RightBracket)
                            {
                                parserContextStack.Peek().ParameterCountStack.Push(1);
                            }
                            else
                            {
                                parserContextStack.Peek().ParameterCountStack.Push(0);
                            }
                        }
                        else
                        {
                            throw new UnknownFunctionParserException("${token.Value} is not a known function.",
                                token.StartPosition, token.Length, token.Value.ToString());
                        }

                        break;
                    case TokenType.Symbol:
                        var tokenValue = (string)token.Value;
                        if (constantRegistry.IsConstantName(tokenValue))
                        {
                            var registeredConstant =
                                new FloatingPointConstant(constantRegistry.GetConstantInfo(tokenValue).Value);
                            tokenReferences.Add(new TokenReference(token, registeredConstant));
                            parserContextStack.Peek().ResultStack.Push(registeredConstant);
                        }
                        else
                        {
                            var variable = new Variable(tokenValue);
                            tokenReferences.Add(new TokenReference(token, variable));
                            parserContextStack.Peek().ResultStack.Push(variable);
                        }

                        break;
                    case TokenType.LeftBracket:
                        parserContextStack.Peek().OperatorStack.Push(token);
                        break;
                    case TokenType.RightBracket:
                        PopOperations(true, token);
                        break;
                    case TokenType.ArgumentSeparator:
                        PopOperations(false, token);
                        parserContextStack.Peek().ParameterCountStack.Push(parserContextStack.Peek().ParameterCountStack.Pop() + 1);
                        break;
                    case TokenType.Operation:
                        var operation1 = (char)token.Value;

                        while (parserContextStack.Peek().OperatorStack.Count > 0 && 
                               (parserContextStack.Peek().OperatorStack.Peek().TokenType == TokenType.Operation || parserContextStack.Peek().OperatorStack.Peek().TokenType == TokenType.Function))
                        {
                            Token operation2Token = parserContextStack.Peek().OperatorStack.Peek();
                            var isFunctionOnTopOfStack = operation2Token.TokenType == TokenType.Function;

                            if (!isFunctionOnTopOfStack)
                            {
                                var operation2 = (char)operation2Token.Value;

                                if ((IsLeftAssociativeOperation(operation1) &&
                                     operationPrecedence[operation1] <= operationPrecedence[operation2]) ||
                                    operationPrecedence[operation1] < operationPrecedence[operation2])
                                {
                                    parserContextStack.Peek().OperatorStack.Pop();
                                    parserContextStack.Peek().ResultStack.Push(ConvertOperation(operation2Token));
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }

                        parserContextStack.Peek().OperatorStack.Push(token);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(token),
                            $"Unexpected value \"{token}\" for {nameof(token)}.");
                }
            }

            PopOperations(false, null);

            VerifyResultStack(tokenReferences);

            return parserContextStack.Peek().ResultStack.First();
        }

        private void PopOperations(bool untilLeftBracket, Token? currentToken)
        {
            if (untilLeftBracket && !currentToken.HasValue)
                throw new ArgumentNullException(nameof(currentToken),
                    $"If the parameter \"{nameof(untilLeftBracket)}\" is set to true, " +
                    $"the parameter \"${nameof(currentToken)}\" cannot be null.");

            while (parserContextStack.Peek().OperatorStack.Count > 0 && parserContextStack.Peek().OperatorStack.Peek().TokenType != TokenType.LeftBracket)
            {
                Token token = parserContextStack.Peek().OperatorStack.Pop();

                switch (token.TokenType)
                {
                    case TokenType.Operation:
                        parserContextStack.Peek().ResultStack.Push(ConvertOperation(token));
                        break;
                    case TokenType.Function:
                        var function = ConvertFunction(token);
                        parserContextStack.Pop();
                        parserContextStack.Peek().ResultStack.Push(function);
                        break;
                    default:
                        throw new InvalidOperationException("Unexpected token type.");
                }
            }


            if (untilLeftBracket)
            {
                if (parserContextStack.Peek().OperatorStack.Count > 0 && parserContextStack.Peek().OperatorStack.Peek().TokenType == TokenType.LeftBracket)
                    parserContextStack.Peek().OperatorStack.Pop();
                else
                    throw new MissingLeftBracketParserException("No matching left bracket found for the right " +
                                                                $"bracket at position {currentToken.Value.StartPosition}.",
                        currentToken.Value.StartPosition);
            }
            else
            {
                if (parserContextStack.Peek().OperatorStack.Count > 0 && parserContextStack.Peek().OperatorStack.Peek().TokenType == TokenType.LeftBracket
                                            && !(currentToken.HasValue && currentToken.Value.TokenType ==
                                                TokenType.ArgumentSeparator))
                    throw new MissingRightBracketParserException("No matching right bracket found for the left " +
                                                                 $"bracket at position {parserContextStack.Peek().OperatorStack.Peek().StartPosition}.",
                        parserContextStack.Peek().OperatorStack.Peek().StartPosition);
            }
            
            if (parserContextStack.Count >= 1 && parserContextStack.Peek().OperatorStack.Count >= 1 && parserContextStack.Peek().OperatorStack.Peek().TokenType == TokenType.Function)
            {
                var function = ConvertFunction(parserContextStack.Peek().OperatorStack.Pop());
                parserContextStack.Pop();
                parserContextStack.Peek().ResultStack.Push(function);
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
                        argument2 = parserContextStack.Peek().ResultStack.Pop();
                        argument1 = parserContextStack.Peek().ResultStack.Pop();
                        dataType = RequiredDataType(argument1, argument2);

                        return new Addition(dataType, argument1, argument2);
                    case '-':
                        argument2 = parserContextStack.Peek().ResultStack.Pop();
                        argument1 = parserContextStack.Peek().ResultStack.Pop();
                        dataType = RequiredDataType(argument1, argument2);

                        return new Subtraction(dataType, argument1, argument2);
                    case '*':
                        argument2 = parserContextStack.Peek().ResultStack.Pop();
                        argument1 = parserContextStack.Peek().ResultStack.Pop();
                        dataType = RequiredDataType(argument1, argument2);

                        return new Multiplication(dataType, argument1, argument2);
                    case '/':
                        divisor = parserContextStack.Peek().ResultStack.Pop();
                        dividend = parserContextStack.Peek().ResultStack.Pop();

                        return new Division(DataType.FloatingPoint, dividend, divisor);
                    case '%':
                        divisor = parserContextStack.Peek().ResultStack.Pop();
                        dividend = parserContextStack.Peek().ResultStack.Pop();

                        return new Modulo(DataType.FloatingPoint, dividend, divisor);
                    case '_':
                        argument1 = parserContextStack.Peek().ResultStack.Pop();

                        return new UnaryMinus(argument1.DataType, argument1);
                    case '^':
                        Operation exponent = parserContextStack.Peek().ResultStack.Pop();
                        Operation @base = parserContextStack.Peek().ResultStack.Pop();

                        return new Exponentiation(DataType.FloatingPoint, @base, exponent);
                    case '&':
                        argument2 = parserContextStack.Peek().ResultStack.Pop();
                        argument1 = parserContextStack.Peek().ResultStack.Pop();
                        dataType = RequiredDataType(argument1, argument2);

                        return new And(dataType, argument1, argument2);
                    case '|':
                        argument2 = parserContextStack.Peek().ResultStack.Pop();
                        argument1 = parserContextStack.Peek().ResultStack.Pop();
                        dataType = RequiredDataType(argument1, argument2);

                        return new Or(dataType, argument1, argument2);
                    case '<':
                        argument2 = parserContextStack.Peek().ResultStack.Pop();
                        argument1 = parserContextStack.Peek().ResultStack.Pop();
                        dataType = RequiredDataType(argument1, argument2);

                        return new LessThan(dataType, argument1, argument2);
                    case '≤':
                        argument2 = parserContextStack.Peek().ResultStack.Pop();
                        argument1 = parserContextStack.Peek().ResultStack.Pop();
                        dataType = RequiredDataType(argument1, argument2);

                        return new LessOrEqualThan(dataType, argument1, argument2);
                    case '>':
                        argument2 = parserContextStack.Peek().ResultStack.Pop();
                        argument1 = parserContextStack.Peek().ResultStack.Pop();
                        dataType = RequiredDataType(argument1, argument2);

                        return new GreaterThan(dataType, argument1, argument2);
                    case '≥':
                        argument2 = parserContextStack.Peek().ResultStack.Pop();
                        argument1 = parserContextStack.Peek().ResultStack.Pop();
                        dataType = RequiredDataType(argument1, argument2);

                        return new GreaterOrEqualThan(dataType, argument1, argument2);
                    case '=':
                        argument2 = parserContextStack.Peek().ResultStack.Pop();
                        argument1 = parserContextStack.Peek().ResultStack.Pop();
                        dataType = RequiredDataType(argument1, argument2);

                        return new Equal(dataType, argument1, argument2);
                    case '≠':
                        argument2 = parserContextStack.Peek().ResultStack.Pop();
                        argument1 = parserContextStack.Peek().ResultStack.Pop();
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

                    // this is only derived from the number of argument separators and also always assumes that
                    // there is at least one argument, therefore we still need the try catch block below
                    int actualParameterCount;

                    if (functionInfo.IsDynamicFunc)
                    {
                        actualParameterCount = parserContextStack.Peek().ParameterCountStack.Pop();
                    }
                    else
                    {
                        actualParameterCount = parserContextStack.Peek().ParameterCountStack.Pop();
                        var expectedParameterCount = functionInfo.NumberOfParameters;
                        if (actualParameterCount != expectedParameterCount)
                            throw new InvalidNumberOfFunctionArgumentsParserException(
                                $"There is a syntax issue for the function \"{functionToken.Value}\" at expression position {functionToken.StartPosition}. " +
                                $"The number of arguments is {actualParameterCount} but {expectedParameterCount} are expected.",
                                functionToken.StartPosition, functionToken.Length, (string)functionToken.Value);
                    }

                    var operations = new List<Operation>();
                    // todo: we should now be able to pop all operations in the current context
                    for (var i = 0; i < actualParameterCount; i++)
                        operations.Add(parserContextStack.Peek().ResultStack.Pop());
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
                    "The number of arguments does not match with what is expected.", functionToken.StartPosition,
                    functionToken.Length, (string)functionToken.Value);
            }
        }

        private void VerifyResultStack(IList<TokenReference> tokenReferences)
        {
            if (parserContextStack.Peek().ResultStack.Count <= 1) return;

            Operation[] operations = parserContextStack.Peek().ResultStack.ToArray();

            for (var i = 1; i < operations.Length; i++)
            {
                Operation operation = operations[i];

                // todo don't filter for types, the first operation is always ok, the second one is the one that is unexpected
                // just check the second for what it is and throw the appropriate exception
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

        private class ParserContext
        {
            public Stack<Operation> ResultStack { get; } = new Stack<Operation>();
            public Stack<Token> OperatorStack { get; } = new Stack<Token>();
            public Stack<int> ParameterCountStack { get; } = new Stack<int>();
        }
    }
}