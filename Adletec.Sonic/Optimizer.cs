using System.Collections.Generic;
using System.Linq;
using Adletec.Sonic.Execution;
using Adletec.Sonic.Operations;

namespace Adletec.Sonic
{
    public class Optimizer
    {
        private readonly IExecutor executor;

        public Optimizer(IExecutor executor)
        {
            this.executor = executor;
        }

        public Operation Optimize(Operation operation, IFunctionRegistry functionRegistry, IConstantRegistry constantRegistry)
        {
            if (operation.GetType() == typeof(Addition))
            {
                var addition = (Addition)operation;
                addition.Argument1 = Optimize(addition.Argument1, functionRegistry, constantRegistry);
                addition.Argument2 = Optimize(addition.Argument2, functionRegistry, constantRegistry);
                if (addition.Argument1.DependsOnVariables == false && addition.Argument2.DependsOnVariables == false)
                {
                    addition.DependsOnVariables = false;
                }
                if (addition.Argument1.IsIdempotent && addition.Argument2.IsIdempotent)
                {
                    addition.IsIdempotent = true;
                }
            }
            else if (operation.GetType() == typeof(Subtraction))
            {
                var subtraction = (Subtraction)operation;
                subtraction.Argument1 = Optimize(subtraction.Argument1, functionRegistry, constantRegistry);
                subtraction.Argument2 = Optimize(subtraction.Argument2, functionRegistry, constantRegistry);
                if (subtraction.Argument1.DependsOnVariables == false && subtraction.Argument2.DependsOnVariables == false)
                {
                    subtraction.DependsOnVariables = false;
                }
                if (subtraction.Argument1.IsIdempotent && subtraction.Argument2.IsIdempotent)
                {
                    subtraction.IsIdempotent = true;
                }
            }
            else if (operation.GetType() == typeof(Multiplication))
            {
                var multiplication = (Multiplication)operation;
                multiplication.Argument1 = Optimize(multiplication.Argument1, functionRegistry, constantRegistry);
                multiplication.Argument2 = Optimize(multiplication.Argument2, functionRegistry, constantRegistry);

                if (IsZero(multiplication.Argument1) || IsZero(multiplication.Argument2))
                {
                    return new FloatingPointConstant(0.0);
                }

                if (multiplication.Argument1.DependsOnVariables == false &&
                    multiplication.Argument2.DependsOnVariables == false)
                {
                    multiplication.DependsOnVariables = false;
                }
                if (multiplication.Argument1.IsIdempotent && multiplication.Argument2.IsIdempotent)
                {
                    multiplication.IsIdempotent = true;
                }
            }
            else if (operation.GetType() == typeof(Division))
            {
                var division = (Division)operation;
                division.Dividend = Optimize(division.Dividend, functionRegistry, constantRegistry);
                division.Divisor = Optimize(division.Divisor, functionRegistry, constantRegistry);
                if (IsZero(division.Dividend))
                {
                    return new FloatingPointConstant(0.0);
                }
                if (division.Dividend.DependsOnVariables == false && division.Divisor.DependsOnVariables == false)
                {
                    division.DependsOnVariables = false;
                }
                if (division.Dividend.IsIdempotent && division.Divisor.IsIdempotent)
                {
                    division.IsIdempotent = true;
                }
            }
            else if (operation.GetType() == typeof(Exponentiation))
            {
                var exponentiation = (Exponentiation)operation;
                exponentiation.Base = Optimize(exponentiation.Base, functionRegistry, constantRegistry);
                exponentiation.Exponent = Optimize(exponentiation.Exponent, functionRegistry, constantRegistry);

                if (IsZero(exponentiation.Exponent))
                {
                    return new FloatingPointConstant(1.0);
                }
                
                if (IsZero(exponentiation.Base))
                {
                    return new FloatingPointConstant(0.0);
                }

                if (exponentiation.Base.DependsOnVariables == false && exponentiation.Exponent.DependsOnVariables == false)
                {
                    exponentiation.DependsOnVariables = false;
                }
                if (exponentiation.Base.IsIdempotent && exponentiation.Exponent.IsIdempotent)
                {
                    exponentiation.IsIdempotent = true;
                }
            }
            else if(operation.GetType() == typeof(Function))
            {
                var function = (Function)operation;
                IList<Operation> arguments = function.Arguments.Select(a => Optimize(a, functionRegistry, constantRegistry)).ToList();
                function.Arguments = arguments;
                // todo check if can be evaluated after optimization (as in other cases)
            }
            
            if (!operation.DependsOnVariables && operation.IsIdempotent && operation.GetType() != typeof(IntegerConstant)
                && operation.GetType() != typeof(FloatingPointConstant))
            {
                double result = executor.Execute(operation, functionRegistry, constantRegistry);
                return new FloatingPointConstant(result);
            }


            return operation;
        }

        
        private bool IsZero(Operation operation)
        {
            if (operation.GetType() == typeof(FloatingPointConstant))
            {
                return ((FloatingPointConstant)operation).Value == 0.0;
            }

            if (operation.GetType() == typeof(IntegerConstant))
            {
                return ((IntegerConstant)operation).Value == 0;
            }

            return false;
        }
    }
}
