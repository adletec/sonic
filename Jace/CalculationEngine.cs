using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Jace.Execution;
using Jace.Operations;
using Jace.Tokenizer;
using Jace.Util;

namespace Jace
{
    public delegate TResult DynamicFunc<T, TResult>(params T[] values);

    /// <inheritdoc/>
    public class CalculationEngine : ICalculationEngine
    {
        private readonly IExecutor executor;
        private readonly Optimizer optimizer;
        private readonly CultureInfo cultureInfo;
        private readonly MemoryCache<string, Func<IDictionary<string, double>, double>> executionFormulaCache;
        private readonly bool cacheEnabled;
        private readonly bool optimizerEnabled;
        private readonly bool caseSensitive;

        private readonly Random random;

        public CalculationEngine()
            : this(new JaceOptions())
        {
        }

        public CalculationEngine(CultureInfo cultureInfo)
            : this(new JaceOptions { CultureInfo = cultureInfo })
        {
        }

        public CalculationEngine(CultureInfo cultureInfo, ExecutionMode executionMode)
            : this (new JaceOptions { CultureInfo = cultureInfo, ExecutionMode = executionMode })
        {
        }

        [Obsolete]
        public CalculationEngine(CultureInfo cultureInfo, ExecutionMode executionMode, bool cacheEnabled, bool optimizerEnabled, bool adjustVariableCaseEnabled)
            : this(new JaceOptions { CultureInfo = cultureInfo, ExecutionMode = executionMode, CacheEnabled = cacheEnabled, OptimizerEnabled = optimizerEnabled, CaseSensitive = !adjustVariableCaseEnabled })
        {
        }

        [Obsolete]
        public CalculationEngine(CultureInfo cultureInfo, ExecutionMode executionMode, bool cacheEnabled,
            bool optimizerEnabled, bool adjustVariableCaseEnabled, bool defaultFunctions, bool defaultConstants, int cacheMaximumSize, int cacheReductionSize)
            : this(new JaceOptions
            { CultureInfo = cultureInfo, ExecutionMode = executionMode, CacheEnabled = cacheEnabled, OptimizerEnabled = optimizerEnabled,
                CaseSensitive = !adjustVariableCaseEnabled, DefaultFunctions = defaultFunctions, DefaultConstants = defaultConstants, 
                CacheMaximumSize = cacheMaximumSize, CacheReductionSize = cacheReductionSize })
        {
        }

        public CalculationEngine(JaceOptions options)
        {
            this.caseSensitive = options.CaseSensitive;
            this.executionFormulaCache = new MemoryCache<string, Func<IDictionary<string, double>, double>>(options.CacheMaximumSize, options.CacheReductionSize);
            this.FunctionRegistry = new FunctionRegistry(caseSensitive);
            this.ConstantRegistry = new ConstantRegistry(caseSensitive);
            this.cultureInfo = options.CultureInfo;
            this.cacheEnabled = options.CacheEnabled;
            this.optimizerEnabled = options.OptimizerEnabled;

            this.random = new Random();

            switch (options.ExecutionMode)
            {
                case ExecutionMode.Interpreted:
                    executor = new Interpreter(caseSensitive);
                    break;
                case ExecutionMode.Compiled:
                    executor = new DynamicCompiler(caseSensitive);
                    break;
                default:
                    throw new ArgumentException($"Unsupported execution mode \"{options.ExecutionMode}\".",
                        nameof(options.ExecutionMode));
            }

            optimizer = new Optimizer(new Interpreter()); // We run the optimizer with the interpreter 

            // Register the default constants of jace into the constant registry
            if (options.DefaultConstants)
                RegisterDefaultConstants();

            // Register the default functions of jace into the function registry
            if (options.DefaultFunctions)
                RegisterDefaultFunctions();
        }

        internal IFunctionRegistry FunctionRegistry { get; }

        internal IConstantRegistry ConstantRegistry { get; }

        public IEnumerable<FunctionInfo> Functions => FunctionRegistry;

        public IEnumerable<ConstantInfo> Constants => ConstantRegistry;

        public double Calculate(string formulaText)
        {
            return Calculate(formulaText, new Dictionary<string, double>());
        }

        public double Calculate(string formulaText, IDictionary<string, double> variables)
        {
            if (string.IsNullOrEmpty(formulaText))
                throw new ArgumentNullException(nameof(formulaText));

            if (variables == null)
                throw new ArgumentNullException(nameof(variables));

            if (!caseSensitive)
            {
                variables = EngineUtil.ConvertVariableNamesToLowerCase(variables);
            }
            VerifyVariableNames(variables);

            // Add the reserved variables to the dictionary
            foreach (ConstantInfo constant in ConstantRegistry)
                variables.Add(constant.ConstantName, constant.Value);

            if (IsInFormulaCache(formulaText, null, out var function))
            {
                return function(variables);
            }

            Operation operation = BuildAbstractSyntaxTree(formulaText, new ConstantRegistry(caseSensitive));
            function = BuildFormula(formulaText, null, operation);
            return function(variables);
        }

        public FormulaBuilder Formula(string formulaText)
        {
            if (string.IsNullOrEmpty(formulaText))
                throw new ArgumentNullException(nameof(formulaText));

            return new FormulaBuilder(formulaText, caseSensitive, this);
        }

        public Func<IDictionary<string, double>, double> Build(string formulaText)
        {
            if (string.IsNullOrEmpty(formulaText))
                throw new ArgumentNullException(nameof(formulaText));

            if (IsInFormulaCache(formulaText, null, out var result))
            {
                return result;
            }

            Operation operation = BuildAbstractSyntaxTree(formulaText, new ConstantRegistry(caseSensitive));
            return BuildFormula(formulaText, null, operation);
        }

        public Func<IDictionary<string, double>, double> Build(string formulaText, IDictionary<string, double> constants)
        {
            if (string.IsNullOrEmpty(formulaText))
                throw new ArgumentNullException(nameof(formulaText));


            ConstantRegistry compiledConstants = new ConstantRegistry(caseSensitive);
            if (constants != null)
            {
                foreach (var constant in constants)
                {
                    compiledConstants.RegisterConstant(constant.Key, constant.Value);
                }
            }

            if (IsInFormulaCache(formulaText, compiledConstants, out var result))
            {
                return result;
            }

            Operation operation = BuildAbstractSyntaxTree(formulaText, compiledConstants);
            return BuildFormula(formulaText, compiledConstants,  operation);
        }

        public void AddFunction(string functionName, Func<double> function, bool isIdempotent = true)
        {
            FunctionRegistry.RegisterFunction(functionName, function, isIdempotent, true);
        }

        public void AddFunction(string functionName, Func<double, double> function, bool isIdempotent = true)
        {
            FunctionRegistry.RegisterFunction(functionName, function, isIdempotent, true); 
        }

        public void AddFunction(string functionName, Func<double, double, double> function, bool isIdempotent = true)
        {
            FunctionRegistry.RegisterFunction(functionName, function, isIdempotent, true);
        }

        public void AddFunction(string functionName, Func<double, double, double, double> function, bool isIdempotent = true)
        {
            FunctionRegistry.RegisterFunction(functionName, function, isIdempotent, true);
        }

        public void AddFunction(string functionName, Func<double, double, double, double, double> function, bool isIdempotent = true)
        {
            FunctionRegistry.RegisterFunction(functionName, function, isIdempotent, true);
        }

        public void AddFunction(string functionName, Func<double, double, double, double, double, double> function, bool isIdempotent = true)
        {
            FunctionRegistry.RegisterFunction(functionName, function, isIdempotent, true);
        }

        public void AddFunction(string functionName, Func<double, double, double, double, double, double, double> function, bool isIdempotent = true)
        {
            FunctionRegistry.RegisterFunction(functionName, function, isIdempotent, true);
        }

        public void AddFunction(string functionName, Func<double, double, double, double, double, double, double, double> function, bool isIdempotent = true)
        {
            FunctionRegistry.RegisterFunction(functionName, function, isIdempotent, true);
        }

        public void AddFunction(string functionName, Func<double, double, double, double, double, double, double, double, double> function, bool isIdempotent = true)
        {
            FunctionRegistry.RegisterFunction(functionName, function, isIdempotent, true);
        }

        public void AddFunction(string functionName, Func<double, double, double, double, double, double, double, double, double, double> function, bool isIdempotent = true)
        {
            FunctionRegistry.RegisterFunction(functionName, function, isIdempotent, true);
        }

        public void AddFunction(string functionName, Func<double, double, double, double, double, double, double, double, double, double, double> function, bool isIdempotent = true)
        {
            FunctionRegistry.RegisterFunction(functionName, function, isIdempotent, true);
        }

        public void AddFunction(string functionName, Func<double, double, double, double, double, double, double, double, double, double, double, double> function, bool isIdempotent = true)
        {
            FunctionRegistry.RegisterFunction(functionName, function, isIdempotent, true);
        }

        public void AddFunction(string functionName, Func<double, double, double, double, double, double, double, double, double, double, double, double, double> function, bool isIdempotent = true)
        {
            FunctionRegistry.RegisterFunction(functionName, function, isIdempotent, true);
        }

        public void AddFunction(string functionName, Func<double, double, double, double, double, double, double, double, double, double, double, double, double, double> function, bool isIdempotent = true)
        {
            FunctionRegistry.RegisterFunction(functionName, function, isIdempotent, true);
        }

        public void AddFunction(string functionName, Func<double, double, double, double, double, double, double, double, double, double, double, double, double, double, double> function, bool isIdempotent = true)
        {
            FunctionRegistry.RegisterFunction(functionName, function, isIdempotent, true);
        }

        public void AddFunction(string functionName, Func<double, double, double, double, double, double, double, double, double, double, double, double, double, double, double, double> function, bool isIdempotent = true)
        {
            FunctionRegistry.RegisterFunction(functionName, function, isIdempotent, true);
        }

        public void AddFunction(string functionName, Func<double, double, double, double, double, double, double, double, double, double, double, double, double, double, double, double, double> function, bool isIdempotent = true)
        {
            FunctionRegistry.RegisterFunction(functionName, function, isIdempotent, true);
        }

        public void AddFunction(string functionName, DynamicFunc<double, double> functionDelegate, bool isIdempotent = true)
        {
            FunctionRegistry.RegisterFunction(functionName, functionDelegate, isIdempotent, true);
        }

        public void AddConstant(string constantName, double value)
        {
            ConstantRegistry.RegisterConstant(constantName, value);
        }

        private void RegisterDefaultFunctions()
        {
            FunctionRegistry.RegisterFunction("sin", (Func<double, double>)Math.Sin, true, false);
            FunctionRegistry.RegisterFunction("cos", (Func<double, double>)Math.Cos, true, false);
            FunctionRegistry.RegisterFunction("csc", (Func<double, double>)MathUtil.Csc, true, false);
            FunctionRegistry.RegisterFunction("sec", (Func<double, double>)MathUtil.Sec, true, false);
            FunctionRegistry.RegisterFunction("asin", (Func<double, double>)Math.Asin, true, false);
            FunctionRegistry.RegisterFunction("acos", (Func<double, double>)Math.Acos, true, false);
            FunctionRegistry.RegisterFunction("tan", (Func<double, double>)Math.Tan, true, false);
            FunctionRegistry.RegisterFunction("cot", (Func<double, double>)MathUtil.Cot, true, false);
            FunctionRegistry.RegisterFunction("atan", (Func<double, double>)Math.Atan, true, false);
            FunctionRegistry.RegisterFunction("acot", (Func<double, double>)MathUtil.Acot, true, false);
            FunctionRegistry.RegisterFunction("loge", (Func<double, double>)Math.Log, true, false);
            FunctionRegistry.RegisterFunction("log10", (Func<double, double>)Math.Log10, true, false);
            FunctionRegistry.RegisterFunction("logn", (Func<double, double, double>)Math.Log, true, false);
            FunctionRegistry.RegisterFunction("sqrt", (Func<double, double>)Math.Sqrt, true, false);
            FunctionRegistry.RegisterFunction("abs", (Func<double, double>)Math.Abs, true, false);
            FunctionRegistry.RegisterFunction("if", (Func<double, double, double, double>)((a, b, c) => a != 0.0 ? b : c), true, false);
            FunctionRegistry.RegisterFunction("ifless", (Func<double, double, double, double, double>)((a, b, c, d) => a < b ? c : d), true, false);
            FunctionRegistry.RegisterFunction("ifmore", (Func<double, double, double, double, double>)((a, b, c, d) => a > b ? c : d), true, false);
            FunctionRegistry.RegisterFunction("ifequal", (Func<double, double, double, double, double>)((a, b, c, d) => a == b ? c : d), true, false);
            FunctionRegistry.RegisterFunction("ceiling", (Func<double, double>)Math.Ceiling, true, false);
            FunctionRegistry.RegisterFunction("floor", (Func<double, double>)Math.Floor, true, false);
            FunctionRegistry.RegisterFunction("truncate", (Func<double, double>)Math.Truncate, true, false);
            FunctionRegistry.RegisterFunction("round", (Func<double, double>)Math.Round, true, false);

            // Dynamic based arguments Functions
            FunctionRegistry.RegisterFunction("max",  (DynamicFunc<double, double>)(a => a.Max()), true, false);
            FunctionRegistry.RegisterFunction("min", (DynamicFunc<double, double>)(a => a.Min()), true, false);
            FunctionRegistry.RegisterFunction("avg", (DynamicFunc<double, double>)(a => a.Average()), true, false);
            FunctionRegistry.RegisterFunction("median", (DynamicFunc<double, double>)(a => a.Median()), true, false);
            FunctionRegistry.RegisterFunction("sum", (DynamicFunc<double, double>)(a => a.Sum()), true, false);

            // Non Idempotent Functions
            FunctionRegistry.RegisterFunction("random", (Func<double>)random.NextDouble, false, false);
        }

        private void RegisterDefaultConstants()
        {
            ConstantRegistry.RegisterConstant("e", Math.E, false);
            ConstantRegistry.RegisterConstant("pi", Math.PI, false);
        }

        /// <summary>
        /// Build the abstract syntax tree for a given formula. The formula string will
        /// be first tokenized.
        /// </summary>
        /// <param name="formulaText">A string containing the mathematical formula that must be converted 
        /// into an abstract syntax tree.</param>
        /// <param name="compiledConstants">The constants which are to be available in the given formula.</param>
        /// <returns>The abstract syntax tree of the formula.</returns>
        private Operation BuildAbstractSyntaxTree(string formulaText, IConstantRegistry compiledConstants)
        {
            var tokenReader = new TokenReader(cultureInfo);
            List<Token> tokens = tokenReader.Read(formulaText);
            
            var astBuilder = new AstBuilder(FunctionRegistry, caseSensitive, compiledConstants);
            Operation operation = astBuilder.Build(tokens);

            return optimizerEnabled ? optimizer.Optimize(operation, this.FunctionRegistry, this.ConstantRegistry) : operation;
        }

        private Func<IDictionary<string, double>, double> BuildFormula(string formulaText, ConstantRegistry compiledConstants, Operation operation)
        {
            return executionFormulaCache.GetOrAdd(GenerateFormulaCacheKey(formulaText, compiledConstants), v => executor.BuildFormula(operation, this.FunctionRegistry, this.ConstantRegistry));
        }

        private bool IsInFormulaCache(string formulaText, ConstantRegistry compiledConstants, out Func<IDictionary<string, double>, double> function)
        {
            function = null;
            return cacheEnabled && executionFormulaCache.TryGetValue(GenerateFormulaCacheKey(formulaText, compiledConstants), out function);
        }

        private string GenerateFormulaCacheKey(string formulaText, ConstantRegistry compiledConstants)
        {
            return compiledConstants != null && compiledConstants.Any() ? $"{formulaText}@{string.Join(",", compiledConstants.Select(x => $"{x.ConstantName}:{x.Value}"))}" : formulaText;
        }

        /// <summary>
        /// Verify a collection of variables to ensure that all the variable names are valid.
        /// Users are not allowed to overwrite reserved variables or use function names as variables.
        /// If an invalid variable is detected an exception is thrown.
        /// </summary>
        /// <param name="variables">The collection of variables that must be verified.</param>
        internal void VerifyVariableNames(IDictionary<string, double> variables)
        {
            foreach (var variableName in variables.Keys)
            {
                if(ConstantRegistry.IsConstantName(variableName) && !ConstantRegistry.GetConstantInfo(variableName).IsOverWritable)
                    throw new ArgumentException(
                        $"The name \"{variableName}\" is a reserved variable name that cannot be overwritten.", nameof(variables));

                if (FunctionRegistry.IsFunctionName(variableName))
                    throw new ArgumentException(
                        $"The name \"{variableName}\" is a function name. Parameters cannot have this name.", nameof(variables));
            }
        }
    }
}
