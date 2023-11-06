using System;
using System.Collections.Generic;
using System.Linq;
using Adletec.Sonic.Operations;
using Adletec.Sonic.Util;

namespace Adletec.Sonic.Execution
{
    public class Interpreter : IExecutor
    {
        private readonly bool caseSensitive;
        private readonly bool guardedMode;

        public Interpreter() : this(false, false)
        {
        }

        public Interpreter(bool caseSensitive, bool guardedMode)
        {
            this.caseSensitive = caseSensitive;
            this.guardedMode = guardedMode;
        }

        public Func<IDictionary<string, double>, double> BuildFormula(Operation operation,
            IFunctionRegistry functionRegistry, IConstantRegistry constantRegistry)
        {
            // the multiplication of modes is makes the code a bit more complex, but this way we can avoid pretty
            // much all of the performance penalty for the additional checks in case they are disabled.
            if (caseSensitive)
            {
                if (guardedMode)
                {
                    return variables =>
                    {
                        VariableVerifier.VerifyVariableNames(variables, constantRegistry, functionRegistry);
                        return Execute(operation, functionRegistry, constantRegistry, variables);
                    };
                }

                return variables => Execute(operation, functionRegistry, constantRegistry, variables);
            }

            if (guardedMode)
            {
                return variables =>
                {
                    variables = EngineUtil.ConvertVariableNamesToLowerCase(variables);
                    VariableVerifier.VerifyVariableNames(variables, constantRegistry, functionRegistry);
                    return Execute(operation, functionRegistry, constantRegistry, variables);
                };
            }

            return variables =>
            {
                variables = EngineUtil.ConvertVariableNamesToLowerCase(variables);
                return Execute(operation, functionRegistry, constantRegistry, variables);
            };
        }

        public double Execute(Operation operation, IFunctionRegistry functionRegistry,
            IConstantRegistry constantRegistry)
        {
            return Execute(operation, functionRegistry, constantRegistry, new Dictionary<string, double>());
        }

        public double Execute(Operation operation,
            IFunctionRegistry functionRegistry,
            IConstantRegistry constantRegistry,
            IDictionary<string, double> variables)
        {
            return ExecuteInternal(operation, functionRegistry, variables);
        }

        private double ExecuteInternal(Operation operation,
            IFunctionRegistry functionRegistry,
            IDictionary<string, double> variables)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            if (operation.GetType() == typeof(IntegerConstant))
            {
                var constant = (IntegerConstant)operation;
                return constant.Value;
            }

            if (operation.GetType() == typeof(FloatingPointConstant))
            {
                var constant = (FloatingPointConstant)operation;
                return constant.Value;
            }

            if (operation.GetType() == typeof(Variable))
            {
                var variable = (Variable)operation;

                var variableFound = variables.TryGetValue(variable.Name, out var value);

                if (variableFound)
                    return value;

                throw new VariableNotDefinedException($"The variable \"{variable.Name}\" used is not defined.", variable.Name);
            }

            if (operation.GetType() == typeof(Multiplication))
            {
                var multiplication = (Multiplication)operation;
                return ExecuteInternal(multiplication.Argument1, functionRegistry, variables) *
                       ExecuteInternal(multiplication.Argument2, functionRegistry, variables);
            }

            if (operation.GetType() == typeof(Addition))
            {
                var addition = (Addition)operation;
                return ExecuteInternal(addition.Argument1, functionRegistry, variables) +
                       ExecuteInternal(addition.Argument2, functionRegistry, variables);
            }

            if (operation.GetType() == typeof(Subtraction))
            {
                var addition = (Subtraction)operation;
                return ExecuteInternal(addition.Argument1, functionRegistry, variables) -
                       ExecuteInternal(addition.Argument2, functionRegistry, variables);
            }

            if (operation.GetType() == typeof(Division))
            {
                var division = (Division)operation;
                return ExecuteInternal(division.Dividend, functionRegistry, variables) /
                       ExecuteInternal(division.Divisor, functionRegistry, variables);
            }

            if (operation.GetType() == typeof(Modulo))
            {
                var division = (Modulo)operation;
                return ExecuteInternal(division.Dividend, functionRegistry, variables) %
                       ExecuteInternal(division.Divisor, functionRegistry, variables);
            }

            if (operation.GetType() == typeof(Exponentiation))
            {
                var exponentiation = (Exponentiation)operation;
                return Math.Pow(ExecuteInternal(exponentiation.Base, functionRegistry, variables),
                    ExecuteInternal(exponentiation.Exponent, functionRegistry, variables));
            }

            if (operation.GetType() == typeof(UnaryMinus))
            {
                var unaryMinus = (UnaryMinus)operation;
                return -ExecuteInternal(unaryMinus.Argument, functionRegistry, variables);
            }

            if (operation.GetType() == typeof(And))
            {
                var and = (And)operation;
                var operation1 = ExecuteInternal(and.Argument1, functionRegistry, variables) != 0;
                var operation2 = ExecuteInternal(and.Argument2, functionRegistry, variables) != 0;

                return operation1 && operation2 ? 1.0 : 0.0;
            }

            if (operation.GetType() == typeof(Or))
            {
                var or = (Or)operation;
                var operation1 = ExecuteInternal(or.Argument1, functionRegistry, variables) != 0;
                var operation2 = ExecuteInternal(or.Argument2, functionRegistry, variables) != 0;

                return operation1 || operation2 ? 1.0 : 0.0;
            }

            if (operation.GetType() == typeof(LessThan))
            {
                var lessThan = (LessThan)operation;
                return ExecuteInternal(lessThan.Argument1, functionRegistry, variables) <
                       ExecuteInternal(lessThan.Argument2, functionRegistry, variables)
                    ? 1.0
                    : 0.0;
            }

            if (operation.GetType() == typeof(LessOrEqualThan))
            {
                var lessOrEqualThan = (LessOrEqualThan)operation;
                return ExecuteInternal(lessOrEqualThan.Argument1, functionRegistry, variables) <=
                       ExecuteInternal(lessOrEqualThan.Argument2, functionRegistry, variables)
                    ? 1.0
                    : 0.0;
            }

            if (operation.GetType() == typeof(GreaterThan))
            {
                var greaterThan = (GreaterThan)operation;
                return ExecuteInternal(greaterThan.Argument1, functionRegistry, variables) >
                       ExecuteInternal(greaterThan.Argument2, functionRegistry, variables)
                    ? 1.0
                    : 0.0;
            }

            if (operation.GetType() == typeof(GreaterOrEqualThan))
            {
                var greaterOrEqualThan = (GreaterOrEqualThan)operation;
                return ExecuteInternal(greaterOrEqualThan.Argument1, functionRegistry, variables) >=
                       ExecuteInternal(greaterOrEqualThan.Argument2, functionRegistry, variables)
                    ? 1.0
                    : 0.0;
            }

            if (operation.GetType() == typeof(Equal))
            {
                var equal = (Equal)operation;
                return ExecuteInternal(equal.Argument1, functionRegistry, variables) ==
                       ExecuteInternal(equal.Argument2, functionRegistry, variables)
                    ? 1.0
                    : 0.0;
            }

            if (operation.GetType() == typeof(NotEqual))
            {
                var notEqual = (NotEqual)operation;
                return ExecuteInternal(notEqual.Argument1, functionRegistry, variables) !=
                       ExecuteInternal(notEqual.Argument2, functionRegistry, variables)
                    ? 1.0
                    : 0.0;
            }

            if (operation.GetType() == typeof(Function))
            {
                var function = (Function)operation;

                var functionInfo = functionRegistry.GetFunctionInfo(function.FunctionName);

                var arguments = new double[functionInfo.IsDynamicFunc
                    ? function.Arguments.Count
                    : functionInfo.NumberOfParameters];
                for (var i = 0; i < arguments.Length; i++)
                    arguments[i] = ExecuteInternal(function.Arguments[i], functionRegistry, variables);

                return Invoke(functionInfo.Function, arguments);
            }

            throw new ArgumentException($"Unsupported operation \"{operation.GetType().FullName}\".",
                nameof(operation));
        }

        private double Invoke(Delegate function, double[] arguments)
        {
            // DynamicInvoke is slow, so we first try to convert it to a Func
            if (function is Func<double> func)
            {
                return func.Invoke();
            }

            if (function is Func<double, double> func1Argument)
            {
                return func1Argument.Invoke(arguments[0]);
            }

            if (function is Func<double, double, double> func2Arguments)
            {
                return func2Arguments.Invoke(arguments[0], arguments[1]);
            }

            if (function is Func<double, double, double, double> func3Arguments)
            {
                return func3Arguments.Invoke(arguments[0], arguments[1], arguments[2]);
            }

            if (function is Func<double, double, double, double, double> func4Arguments)
            {
                return func4Arguments.Invoke(arguments[0], arguments[1], arguments[2], arguments[3]);
            }

            if (function is Func<double, double, double, double, double, double> func5Arguments)
            {
                return func5Arguments.Invoke(arguments[0], arguments[1], arguments[2], arguments[3], arguments[4]);
            }

            if (function is Func<double, double, double, double, double, double, double> func6Arguments)
            {
                return func6Arguments.Invoke(arguments[0], arguments[1], arguments[2], arguments[3], arguments[4],
                    arguments[5]);
            }

            if (function is Func<double, double, double, double, double, double, double, double> func7Arguments)
            {
                return func7Arguments.Invoke(arguments[0], arguments[1], arguments[2], arguments[3], arguments[4],
                    arguments[5], arguments[6]);
            }

            if (function is Func<double, double, double, double, double, double, double, double, double> func8Arguments)
            {
                return func8Arguments.Invoke(arguments[0], arguments[1], arguments[2], arguments[3], arguments[4],
                    arguments[5], arguments[6], arguments[7]);
            }

            if (function is Func<double, double, double, double, double, double, double, double, double, double>
                func9Arguments)
            {
                return func9Arguments.Invoke(arguments[0], arguments[1], arguments[2], arguments[3], arguments[4],
                    arguments[5], arguments[6], arguments[7], arguments[8]);
            }

            if (function is Func<double, double, double, double, double, double, double, double, double, double, double>
                func10Arguments)
            {
                return func10Arguments.Invoke(arguments[0], arguments[1], arguments[2], arguments[3], arguments[4],
                    arguments[5], arguments[6], arguments[7], arguments[8], arguments[9]);
            }

            if (function is
                Func<double, double, double, double, double, double, double, double, double, double, double, double>
                func11Arguments)
            {
                return func11Arguments.Invoke(arguments[0], arguments[1], arguments[2], arguments[3], arguments[4],
                    arguments[5], arguments[6], arguments[7], arguments[8], arguments[9], arguments[10]);
            }

            if (function is
                Func<double, double, double, double, double, double, double, double, double, double, double, double,
                    double> func12Arguments)
            {
                return func12Arguments.Invoke(arguments[0], arguments[1], arguments[2], arguments[3], arguments[4],
                    arguments[5], arguments[6], arguments[7], arguments[8], arguments[9], arguments[10], arguments[11]);
            }

            if (function is
                Func<double, double, double, double, double, double, double, double, double, double, double, double,
                    double, double> func13Arguments)
            {
                return func13Arguments.Invoke(arguments[0], arguments[1], arguments[2], arguments[3], arguments[4],
                    arguments[5], arguments[6], arguments[7], arguments[8], arguments[9], arguments[10], arguments[11],
                    arguments[12]);
            }

            if (function is
                Func<double, double, double, double, double, double, double, double, double, double, double, double,
                    double, double, double> func14Arguments)
            {
                return func14Arguments.Invoke(arguments[0], arguments[1], arguments[2], arguments[3], arguments[4],
                    arguments[5], arguments[6], arguments[7], arguments[8], arguments[9], arguments[10], arguments[11],
                    arguments[12], arguments[13]);
            }

            if (function is
                Func<double, double, double, double, double, double, double, double, double, double, double, double,
                    double, double, double, double> func15Arguments)
            {
                return func15Arguments.Invoke(arguments[0], arguments[1], arguments[2], arguments[3], arguments[4],
                    arguments[5], arguments[6], arguments[7], arguments[8], arguments[9], arguments[10], arguments[11],
                    arguments[12], arguments[13], arguments[14]);
            }

            if (function is
                Func<double, double, double, double, double, double, double, double, double, double, double, double,
                    double, double, double, double, double> func16Arguments)
            {
                return func16Arguments.Invoke(arguments[0], arguments[1], arguments[2], arguments[3], arguments[4],
                    arguments[5], arguments[6], arguments[7], arguments[8], arguments[9], arguments[10], arguments[11],
                    arguments[12], arguments[13], arguments[14], arguments[15]);
            }

            if (function is DynamicFunc<double, double> dynamicFunc)
            {
                return dynamicFunc.Invoke(arguments);
            }

            return (double)function.DynamicInvoke((from s in arguments select (object)s).ToArray());
        }
    }
}