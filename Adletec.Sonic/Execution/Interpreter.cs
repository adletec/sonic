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

        public Interpreter(): this(false) { }

        public Interpreter(bool caseSensitive)
        {
            this.caseSensitive = caseSensitive;
        }
        public Func<IDictionary<string, double>, double> BuildFormula(Operation operation, 
            IFunctionRegistry functionRegistry,
            IConstantRegistry constantRegistry)
        {
            return caseSensitive
              ? (Func<IDictionary<string, double>, double>)(variables => Execute(operation, functionRegistry, constantRegistry, variables))
              : (Func<IDictionary<string, double>, double>)(variables =>
              {
                  variables = EngineUtil.ConvertVariableNamesToLowerCase(variables);
                  return Execute(operation, functionRegistry, constantRegistry, variables);
              });
        }

        public double Execute(Operation operation, IFunctionRegistry functionRegistry, IConstantRegistry constantRegistry)
        {
            return Execute(operation, functionRegistry, constantRegistry, new Dictionary<string, double>());
        }

        public double Execute(Operation operation,
            IFunctionRegistry functionRegistry,
            IConstantRegistry constantRegistry, 
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
                
                if (constantRegistry.IsConstantName(variable.Name))
                    return constantRegistry.GetConstantInfo(variable.Name).Value;

                throw new VariableNotDefinedException($"The variable \"{variable.Name}\" used is not defined.");
            }
            if (operation.GetType() == typeof(Multiplication))
            {
                var multiplication = (Multiplication)operation;
                return Execute(multiplication.Argument1, functionRegistry, constantRegistry,  variables) * Execute(multiplication.Argument2, functionRegistry, constantRegistry,  variables);
            }

            if (operation.GetType() == typeof(Addition))
            {
                var addition = (Addition)operation;
                return Execute(addition.Argument1, functionRegistry, constantRegistry,  variables) + Execute(addition.Argument2, functionRegistry, constantRegistry,  variables);
            }

            if (operation.GetType() == typeof(Subtraction))
            {
                var addition = (Subtraction)operation;
                return Execute(addition.Argument1, functionRegistry, constantRegistry,  variables) - Execute(addition.Argument2, functionRegistry, constantRegistry,  variables);
            }

            if (operation.GetType() == typeof(Division))
            {
                var division = (Division)operation;
                return Execute(division.Dividend, functionRegistry, constantRegistry,  variables) / Execute(division.Divisor, functionRegistry, constantRegistry,  variables);
            }

            if (operation.GetType() == typeof(Modulo))
            {
                var division = (Modulo)operation;
                return Execute(division.Dividend, functionRegistry, constantRegistry,  variables) % Execute(division.Divisor, functionRegistry, constantRegistry,  variables);
            }

            if (operation.GetType() == typeof(Exponentiation))
            {
                var exponentiation = (Exponentiation)operation;
                return Math.Pow(Execute(exponentiation.Base, functionRegistry, constantRegistry,  variables), Execute(exponentiation.Exponent, functionRegistry, constantRegistry,  variables));
            }

            if (operation.GetType() == typeof(UnaryMinus))
            {
                var unaryMinus = (UnaryMinus)operation;
                return -Execute(unaryMinus.Argument, functionRegistry, constantRegistry,  variables);
            }

            if (operation.GetType() == typeof(And))
            {
                var and = (And)operation;
                var operation1 = Execute(and.Argument1, functionRegistry, constantRegistry,  variables) != 0;
                var operation2 = Execute(and.Argument2, functionRegistry, constantRegistry,  variables) != 0;

                return operation1 && operation2 ? 1.0 : 0.0;
            }

            if (operation.GetType() == typeof(Or))
            {
                var or = (Or)operation;
                var operation1 = Execute(or.Argument1, functionRegistry, constantRegistry,  variables) != 0;
                var operation2 = Execute(or.Argument2, functionRegistry, constantRegistry,  variables) != 0;

                return operation1 || operation2 ? 1.0 : 0.0;
            }

            if(operation.GetType() == typeof(LessThan))
            {
                var lessThan = (LessThan)operation;
                return Execute(lessThan.Argument1, functionRegistry, constantRegistry,  variables) < Execute(lessThan.Argument2, functionRegistry, constantRegistry,  variables) ? 1.0 : 0.0;
            }

            if (operation.GetType() == typeof(LessOrEqualThan))
            {
                var lessOrEqualThan = (LessOrEqualThan)operation;
                return Execute(lessOrEqualThan.Argument1, functionRegistry, constantRegistry,  variables) <= Execute(lessOrEqualThan.Argument2, functionRegistry, constantRegistry,  variables) ? 1.0 : 0.0;
            }

            if (operation.GetType() == typeof(GreaterThan))
            {
                var greaterThan = (GreaterThan)operation;
                return Execute(greaterThan.Argument1, functionRegistry, constantRegistry,  variables) > Execute(greaterThan.Argument2, functionRegistry, constantRegistry,  variables) ? 1.0 : 0.0;
            }

            if (operation.GetType() == typeof(GreaterOrEqualThan))
            {
                var greaterOrEqualThan = (GreaterOrEqualThan)operation;
                return Execute(greaterOrEqualThan.Argument1, functionRegistry, constantRegistry,  variables) >= Execute(greaterOrEqualThan.Argument2, functionRegistry, constantRegistry,  variables) ? 1.0 : 0.0;
            }

            if (operation.GetType() == typeof(Equal))
            {
                var equal = (Equal)operation;
                return Execute(equal.Argument1, functionRegistry, constantRegistry,  variables) == Execute(equal.Argument2, functionRegistry, constantRegistry,  variables) ? 1.0 : 0.0;
            }

            if (operation.GetType() == typeof(NotEqual))
            {
                var notEqual = (NotEqual)operation;
                return Execute(notEqual.Argument1, functionRegistry, constantRegistry,  variables) != Execute(notEqual.Argument2, functionRegistry, constantRegistry,  variables) ? 1.0 : 0.0;
            }

            if (operation.GetType() == typeof(Function))
            {
                var function = (Function)operation;

                var functionInfo = functionRegistry.GetFunctionInfo(function.FunctionName);

                var arguments = new double[functionInfo.IsDynamicFunc ? function.Arguments.Count : functionInfo.NumberOfParameters];
                for (var i = 0; i < arguments.Length; i++)
                    arguments[i] = Execute(function.Arguments[i], functionRegistry, constantRegistry,  variables);

                return Invoke(functionInfo.Function, arguments);
            }

            throw new ArgumentException($"Unsupported operation \"{operation.GetType().FullName}\".", nameof(operation));
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
                return func6Arguments.Invoke(arguments[0], arguments[1], arguments[2], arguments[3], arguments[4], arguments[5]);
            }

            if (function is Func<double, double, double, double, double, double, double, double> func7Arguments)
            {
                return func7Arguments.Invoke(arguments[0], arguments[1], arguments[2], arguments[3], arguments[4], arguments[5], arguments[6]);
            }

            if (function is Func<double, double, double, double, double, double, double, double, double> func8Arguments)
            {
                return func8Arguments.Invoke(arguments[0], arguments[1], arguments[2], arguments[3], arguments[4], arguments[5], arguments[6], arguments[7]);
            }

            if (function is Func<double, double, double, double, double, double, double, double, double, double> func9Arguments)
            {
                return func9Arguments.Invoke(arguments[0], arguments[1], arguments[2], arguments[3], arguments[4], arguments[5], arguments[6], arguments[7], arguments[8]);
            }

            if (function is Func<double, double, double, double, double, double, double, double, double, double, double> func10Arguments)
            {
                return func10Arguments.Invoke(arguments[0], arguments[1], arguments[2], arguments[3], arguments[4], arguments[5], arguments[6], arguments[7], arguments[8], arguments[9]);
            }

            if (function is Func<double, double, double, double, double, double, double, double, double, double, double, double> func11Arguments)
            {
                return func11Arguments.Invoke(arguments[0], arguments[1], arguments[2], arguments[3], arguments[4], arguments[5], arguments[6], arguments[7], arguments[8], arguments[9], arguments[10]);
            }

            if (function is Func<double, double, double, double, double, double, double, double, double, double, double, double, double> func12Arguments)
            {
                return func12Arguments.Invoke(arguments[0], arguments[1], arguments[2], arguments[3], arguments[4], arguments[5], arguments[6], arguments[7], arguments[8], arguments[9], arguments[10], arguments[11]);
            }

            if (function is Func<double, double, double, double, double, double, double, double, double, double, double, double, double, double> func13Arguments)
            {
                return func13Arguments.Invoke(arguments[0], arguments[1], arguments[2], arguments[3], arguments[4], arguments[5], arguments[6], arguments[7], arguments[8], arguments[9], arguments[10], arguments[11], arguments[12]);
            }

            if (function is Func<double, double, double, double, double, double, double, double, double, double, double, double, double, double, double> func14Arguments)
            {
                return func14Arguments.Invoke(arguments[0], arguments[1], arguments[2], arguments[3], arguments[4], arguments[5], arguments[6], arguments[7], arguments[8], arguments[9], arguments[10], arguments[11], arguments[12], arguments[13]);
            }

            if (function is Func<double, double, double, double, double, double, double, double, double, double, double, double, double, double, double, double> func15Arguments)
            {
                return func15Arguments.Invoke(arguments[0], arguments[1], arguments[2], arguments[3], arguments[4], arguments[5], arguments[6], arguments[7], arguments[8], arguments[9], arguments[10], arguments[11], arguments[12], arguments[13], arguments[14]);
            }

            if (function is Func<double, double, double, double, double, double, double, double, double, double, double, double, double, double, double, double, double> func16Arguments)
            {
                return func16Arguments.Invoke(arguments[0], arguments[1], arguments[2], arguments[3], arguments[4], arguments[5], arguments[6], arguments[7], arguments[8], arguments[9], arguments[10], arguments[11], arguments[12], arguments[13], arguments[14], arguments[15]);
            }

            if (function is DynamicFunc<double, double> dynamicFunc)
            {
                return dynamicFunc.Invoke(arguments);
            }

            return (double)function.DynamicInvoke((from s in arguments select (object)s).ToArray());
        }
    }
}
