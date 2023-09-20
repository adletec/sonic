using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Adletec.Sonic.Execution;

namespace Adletec.Sonic
{
    /// <summary>
    /// A builder for the <see cref="Evaluator"/> class.
    /// </summary>
    public class EvaluatorBuilder
    {
        private const int DefaultCacheMaximumSize = 500;
        private const int DefaultCacheReductionSize = 50;

        internal CultureInfo CultureInfo { get; private set; } = CultureInfo.CurrentCulture;
        internal ExecutionMode ExecutionMode { get; private set; } = ExecutionMode.Compiled;

        internal bool CacheEnabled { get; private set; } = true;

        internal bool OptimizerEnabled { get; private set; } = true;

        internal bool CaseSensitive { get; private set; } = true;

        internal bool DefaultFunctions { get; private set; } = true;

        internal bool DefaultConstants { get; private set; } = true;

        internal int CacheMaximumSize { get; private set; } = DefaultCacheMaximumSize;

        internal int CacheReductionSize { get; private set; } = DefaultCacheReductionSize;
        
        internal bool GuardedMode { get; private set; } = false;

        internal List<FunctionDraft> Functions { get; }

        internal List<ConstantDraft> Constants { get; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public EvaluatorBuilder()
        {
            Functions = new List<FunctionDraft>();
            Constants = new List<ConstantDraft>();
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        /// <param name="evaluatorBuilder">The builder to copy from.</param>
        public EvaluatorBuilder(EvaluatorBuilder evaluatorBuilder)
        {
            CultureInfo = evaluatorBuilder.CultureInfo;
            ExecutionMode = evaluatorBuilder.ExecutionMode;
            CacheEnabled = evaluatorBuilder.CacheEnabled;
            OptimizerEnabled = evaluatorBuilder.OptimizerEnabled;
            CaseSensitive = evaluatorBuilder.CaseSensitive;
            DefaultConstants = evaluatorBuilder.DefaultConstants;
            DefaultFunctions = evaluatorBuilder.DefaultFunctions;
            CacheMaximumSize = evaluatorBuilder.CacheMaximumSize;
            CacheReductionSize = evaluatorBuilder.CacheReductionSize;

            // Functions and Constants are immutable, so we can just copy the references.
            Functions = new List<FunctionDraft>(evaluatorBuilder.Functions);
            Constants = new List<ConstantDraft>(evaluatorBuilder.Constants);
        }

        /// <summary>
        /// Use the provided <see cref="CultureInfo"/> for correctly reading floating point numbers.
        /// Default: <see cref="CultureInfo.CurrentCulture"/>.
        /// </summary>
        /// <param name="cultureInfo"></param>
        /// <returns></returns>
        public EvaluatorBuilder UseCulture(CultureInfo cultureInfo)
        {
            CultureInfo = cultureInfo;
            return this;
        }

        /// <summary>
        /// Use the provided <see cref="ExecutionMode"/> for formula execution.
        /// Default: <see cref="ExecutionMode.Compiled"/>.
        /// </summary>
        /// <param name="executionMode"></param>
        /// <returns></returns>
        public EvaluatorBuilder UseExecutionMode(ExecutionMode executionMode)
        {
            ExecutionMode = executionMode;
            return this;
        }

        /// <summary>
        /// Enable caching of already parsed expressions. This improves performance if the same string-expression is evaluated multiple times.
        /// Default: cache enabled.
        /// </summary>
        /// <returns></returns>
        public EvaluatorBuilder EnableCache()
        {
            CacheEnabled = true;
            return this;
        }

        /// <summary>
        /// Disable caching of already parsed expressions. This reduces memory consumption and slightly improves performance if every string-expression is only evaluated once.
        /// In most cases, caching should be enabled. However, if you have a very large number of different string-expressions you only evaluate once, you may want to disable caching.
        /// Default: cache enabled.
        /// </summary>
        /// <returns></returns>
        public EvaluatorBuilder DisableCache()
        {
            CacheEnabled = false;
            return this;
        }

        /// <summary>
        /// Optimize the parsed expressions. This includes simplification of expressions and the pre-calculation of idempotent expression parts.
        /// This improves performance if the same string-expression is evaluated multiple times, since idempotent expression parts are only evaluated once.
        /// Default: optimization enabled.
        /// </summary>
        /// <returns></returns>
        public EvaluatorBuilder EnableOptimizer()
        {
            OptimizerEnabled = true;
            return this;
        }

        /// <summary>
        /// Disable optimization of the parsed expressions. This skips an additional step during parsing which takes some time but will usually improve the performance for subsequent evaluations.
        /// In most cases, optimization should be enabled. However, if you have a very large number of different expressions you only evaluate once, you may want to disable optimization.
        /// Default: optimization enabled.
        /// </summary>
        /// <returns></returns>
        public EvaluatorBuilder DisableOptimizer()
        {
            OptimizerEnabled = false;
            return this;
        }

        /// <summary>
        /// Enables case sensitivity for function, variable, and constant names. This means that the names "a" and "A" are treated as different names. This improves performance.
        /// Default: case sensitivity enabled.
        /// </summary>
        /// <returns></returns>
        public EvaluatorBuilder EnableCaseSensitivity()
        {
            CaseSensitive = true;
            return this;
        }

        /// <summary>
        /// Disables case sensitivity for function, variable, and constant names. This means that the names "a" and "A" are treated as the same name. This negatively impacts performance. However, if user input is expected to be case insensitive, this will be significantly faster than converting all user input to the same case.
        /// Default: case sensitivity enabled.
        /// </summary>
        /// <returns></returns>
        public EvaluatorBuilder DisableCaseSensitivity()
        {
            CaseSensitive = false;
            return this;
        }

        /// <summary>
        /// Makes all default functions available. This includes functions like sin, cos, tan, pow, if, ... (see documentation for full list). These functions cannot be redefined if enabled.
        /// Default: default functions enabled.
        /// </summary>
        /// <returns></returns>
        public EvaluatorBuilder EnableDefaultFunctions()
        {
            DefaultFunctions = true;
            return this;
        }

        /// <summary>
        /// Disables all default functions. This means that the functions sin, cos, tan, pow, if, ... (see documentation for full list) are not available. These functions can be redefined if disabled.
        /// Default: default functions enabled.
        /// </summary>
        /// <returns></returns>
        public EvaluatorBuilder DisableDefaultFunctions()
        {
            DefaultFunctions = false;
            return this;
        }

        /// <summary>
        /// Enables all default constants. This includes constants like pi, e, ... (see documentation for full list). These constants cannot be redefined if enabled.
        /// Default: default constants enabled.
        /// </summary>
        /// <returns></returns>
        public EvaluatorBuilder EnableDefaultConstants()
        {
            DefaultConstants = true;
            return this;
        }

        /// <summary>
        /// Disables all default constants. This means that the constants pi, e, ... (see documentation for full list) are not available. These constants can be redefined if disabled.
        /// Default: default constants enabled.
        /// </summary>
        /// <returns></returns>
        public EvaluatorBuilder DisableDefaultConstants()
        {
            DefaultConstants = false;
            return this;
        }

        /// <summary>
        /// Enables guarded mode. This means that the engine will throw exceptions for non-fatal errors, i.e. if it
        /// receives ambiguous input for which a sane default exist, but which is possibly not what the user intended.
        ///
        /// This includes:
        ///  - the variable dictionary contains a variable with the same name as a constant (default: constant is used).
        ///  - the same constant is defined multiple times (default: last definition is used).
        ///  - the same function is defined multiple times (default: last definition is used).
        ///
        /// Default: guarded mode disabled.
        /// </summary>
        /// <returns></returns>
        public EvaluatorBuilder EnableGuardedMode()
        {
            GuardedMode = true;
            return this;
        }
        
        /// <summary>
        /// Disables guarded mode. This means that the engine will not throw exceptions for non-fatal errors, i.e. if it
        /// receives ambiguous input for which a sane default exist, but which is possibly not what the user intended.
        ///
        /// This includes:
        ///  - the variable dictionary contains a variable with the same name as a constant (default: constant is used).
        ///  - the same constant is defined multiple times (default: last definition is used).
        ///  - the same function is defined multiple times (default: last definition is used).
        ///
        /// Default: guarded mode disabled.
        /// </summary>
        /// <returns></returns>
        public EvaluatorBuilder DisableGuardedMode()
        {
            GuardedMode = false;
            return this;
        }

        /// <summary>
        /// The amount of parsed expressions that are cached. If the cache is full, the least recently used expressions are removed from the cache. This value must be greater than 0, if caching is enabled.
        /// The cache size should be chosen based on the number of different expressions that are evaluated. A good cache size is slightly larger than the number of different expressions that are expected to be repeatedly evaluated by this engine.
        /// If the cache is too small, the performance will be negatively impacted. However, if the cache is too large, the memory consumption will be unnecessarily increased.
        /// Default: 500.
        /// </summary>
        /// <param name="cacheMaximumSize">the number of expressions to cache</param>
        /// <returns></returns>
        public EvaluatorBuilder UseCacheMaximumSize(int cacheMaximumSize)
        {
            CacheMaximumSize = cacheMaximumSize;
            return this;
        }

        /// <summary>
        /// The amount of (least recently used) parsed expressions that are removed from the cache when the cache is full. This value must be greater than 0, if caching is enabled.
        /// Ideally, the cache reduction should happen rarely. If the cache is reduced too often, the performance will be negatively impacted. However, if the cache reduction is too large, it might also remove expressions which will be evaluated again.
        /// Default: 50.
        /// </summary>
        /// <param name="cacheReductionSize"></param>
        /// <returns></returns>
        public EvaluatorBuilder UseCacheReductionSize(int cacheReductionSize)
        {
            CacheReductionSize = cacheReductionSize;
            return this;
        }

        /// <summary>
        /// Add a new function to the evaluator. The function must be a .NET func (but can be defined as lambda).
        /// </summary>
        /// <param name="functionName">The name of the function. This is how the function will be called in the expression (e.g. 'a' if the function should be called by 'a(x)').</param>
        /// <param name="function">The .NET func which is to be executed.</param>
        /// <param name="isIdempotent">Whether the function is idempotent, i.e. always returns the same result if called with the same parameters.</param>
        /// <returns></returns>
        public EvaluatorBuilder AddFunction(string functionName, Func<double> function,
            bool isIdempotent = true)
        {
            Functions.Add(new FunctionDraft(functionName, isIdempotent, function));
            return this;
        }

        /// <summary>
        /// Add a new function to the evaluator. The function must be a .NET func (but can be defined as lambda).
        /// </summary>
        /// <param name="functionName">The name of the function. This is how the function will be called in the expression (e.g. 'a' if the function should be called by 'a(x)').</param>
        /// <param name="function">The .NET func which is to be executed.</param>
        /// <param name="isIdempotent">Whether the function is idempotent, i.e. always returns the same result if called with the same parameters.</param>
        /// <returns></returns>
        public EvaluatorBuilder AddFunction(string functionName, Func<double, double> function,
            bool isIdempotent = true)
        {
            Functions.Add(new FunctionDraft(functionName, isIdempotent, function));
            return this;
        }

        /// <summary>
        /// Add a new function to the evaluator. The function must be a .NET func (but can be defined as lambda).
        /// </summary>
        /// <param name="functionName">The name of the function. This is how the function will be called in the expression (e.g. 'a' if the function should be called by 'a(x)').</param>
        /// <param name="function">The .NET func which is to be executed.</param>
        /// <param name="isIdempotent">Whether the function is idempotent, i.e. always returns the same result if called with the same parameters.</param>
        /// <returns></returns>
        public EvaluatorBuilder AddFunction(string functionName, Func<double, double, double> function,
            bool isIdempotent = true)
        {
            Functions.Add(new FunctionDraft(functionName, isIdempotent, function));
            return this;
        }

        /// <summary>
        /// Add a new function to the evaluator. The function must be a .NET func (but can be defined as lambda).
        /// </summary>
        /// <param name="functionName">The name of the function. This is how the function will be called in the expression (e.g. 'a' if the function should be called by 'a(x)').</param>
        /// <param name="function">The .NET func which is to be executed.</param>
        /// <param name="isIdempotent">Whether the function is idempotent, i.e. always returns the same result if called with the same parameters.</param>
        /// <returns></returns>
        public EvaluatorBuilder AddFunction(string functionName, Func<double, double, double, double> function,
            bool isIdempotent = true)
        {
            Functions.Add(new FunctionDraft(functionName, isIdempotent, function));
            return this;
        }

        /// <summary>
        /// Add a new function to the evaluator. The function must be a .NET func (but can be defined as lambda).
        /// </summary>
        /// <param name="functionName">The name of the function. This is how the function will be called in the expression (e.g. 'a' if the function should be called by 'a(x)').</param>
        /// <param name="function">The .NET func which is to be executed.</param>
        /// <param name="isIdempotent">Whether the function is idempotent, i.e. always returns the same result if called with the same parameters.</param>
        /// <returns></returns>
        public EvaluatorBuilder AddFunction(string functionName,
            Func<double, double, double, double, double> function, bool isIdempotent = true)
        {
            Functions.Add(new FunctionDraft(functionName, isIdempotent, function));
            return this;
        }

        /// <summary>
        /// Add a new function to the evaluator. The function must be a .NET func (but can be defined as lambda).
        /// </summary>
        /// <param name="functionName">The name of the function. This is how the function will be called in the expression (e.g. 'a' if the function should be called by 'a(x)').</param>
        /// <param name="function">The .NET func which is to be executed.</param>
        /// <param name="isIdempotent">Whether the function is idempotent, i.e. always returns the same result if called with the same parameters.</param>
        /// <returns></returns>
        public EvaluatorBuilder AddFunction(string functionName,
            Func<double, double, double, double, double, double> function, bool isIdempotent = true)
        {
            Functions.Add(new FunctionDraft(functionName, isIdempotent, function));
            return this;
        }

        /// <summary>
        /// Add a new function to the evaluator. The function must be a .NET func (but can be defined as lambda).
        /// </summary>
        /// <param name="functionName">The name of the function. This is how the function will be called in the expression (e.g. 'a' if the function should be called by 'a(x)').</param>
        /// <param name="function">The .NET func which is to be executed.</param>
        /// <param name="isIdempotent">Whether the function is idempotent, i.e. always returns the same result if called with the same parameters.</param>
        /// <returns></returns>
        public EvaluatorBuilder AddFunction(string functionName,
            Func<double, double, double, double, double, double, double> function, bool isIdempotent = true)
        {
            Functions.Add(new FunctionDraft(functionName, isIdempotent, function));
            return this;
        }

        /// <summary>
        /// Add a new function to the evaluator. The function must be a .NET func (but can be defined as lambda).
        /// </summary>
        /// <param name="functionName">The name of the function. This is how the function will be called in the expression (e.g. 'a' if the function should be called by 'a(x)').</param>
        /// <param name="function">The .NET func which is to be executed.</param>
        /// <param name="isIdempotent">Whether the function is idempotent, i.e. always returns the same result if called with the same parameters.</param>
        /// <returns></returns>
        public EvaluatorBuilder AddFunction(string functionName,
            Func<double, double, double, double, double, double, double, double> function, bool isIdempotent = true)
        {
            Functions.Add(new FunctionDraft(functionName, isIdempotent, function));
            return this;
        }

        /// <summary>
        /// Add a new function to the evaluator. The function must be a .NET func (but can be defined as lambda).
        /// </summary>
        /// <param name="functionName">The name of the function. This is how the function will be called in the expression (e.g. 'a' if the function should be called by 'a(x)').</param>
        /// <param name="function">The .NET func which is to be executed.</param>
        /// <param name="isIdempotent">Whether the function is idempotent, i.e. always returns the same result if called with the same parameters.</param>
        /// <returns></returns>
        public EvaluatorBuilder AddFunction(string functionName,
            Func<double, double, double, double, double, double, double, double, double> function,
            bool isIdempotent = true)
        {
            Functions.Add(new FunctionDraft(functionName, isIdempotent, function));
            return this;
        }

        /// <summary>
        /// Add a new function to the evaluator. The function must be a .NET func (but can be defined as lambda).
        /// </summary>
        /// <param name="functionName">The name of the function. This is how the function will be called in the expression (e.g. 'a' if the function should be called by 'a(x)').</param>
        /// <param name="function">The .NET func which is to be executed.</param>
        /// <param name="isIdempotent">Whether the function is idempotent, i.e. always returns the same result if called with the same parameters.</param>
        /// <returns></returns>
        public EvaluatorBuilder AddFunction(string functionName,
            Func<double, double, double, double, double, double, double, double, double, double> function,
            bool isIdempotent = true)
        {
            Functions.Add(new FunctionDraft(functionName, isIdempotent, function));
            return this;
        }

        /// <summary>
        /// Add a new function to the evaluator. The function must be a .NET func (but can be defined as lambda).
        /// </summary>
        /// <param name="functionName">The name of the function. This is how the function will be called in the expression (e.g. 'a' if the function should be called by 'a(x)').</param>
        /// <param name="function">The .NET func which is to be executed.</param>
        /// <param name="isIdempotent">Whether the function is idempotent, i.e. always returns the same result if called with the same parameters.</param>
        /// <returns></returns>
        public EvaluatorBuilder AddFunction(string functionName,
            Func<double, double, double, double, double, double, double, double, double, double, double> function,
            bool isIdempotent = true)
        {
            Functions.Add(new FunctionDraft(functionName, isIdempotent, function));
            return this;
        }

        /// <summary>
        /// Add a new function to the evaluator. The function must be a .NET func (but can be defined as lambda).
        /// </summary>
        /// <param name="functionName">The name of the function. This is how the function will be called in the expression (e.g. 'a' if the function should be called by 'a(x)').</param>
        /// <param name="function">The .NET func which is to be executed.</param>
        /// <param name="isIdempotent">Whether the function is idempotent, i.e. always returns the same result if called with the same parameters.</param>
        /// <returns></returns>
        public EvaluatorBuilder AddFunction(string functionName,
            Func<double, double, double, double, double, double, double, double, double, double, double, double>
                function, bool isIdempotent = true)
        {
            Functions.Add(new FunctionDraft(functionName, isIdempotent, function));
            return this;
        }

        /// <summary>
        /// Add a new function to the evaluator. The function must be a .NET func (but can be defined as lambda).
        /// </summary>
        /// <param name="functionName">The name of the function. This is how the function will be called in the expression (e.g. 'a' if the function should be called by 'a(x)').</param>
        /// <param name="function">The .NET func which is to be executed.</param>
        /// <param name="isIdempotent">Whether the function is idempotent, i.e. always returns the same result if called with the same parameters.</param>
        /// <returns></returns>
        public EvaluatorBuilder AddFunction(string functionName,
            Func<double, double, double, double, double, double, double, double, double, double, double, double, double>
                function, bool isIdempotent = true)
        {
            Functions.Add(new FunctionDraft(functionName, isIdempotent, function));
            return this;
        }

        /// <summary>
        /// Add a new function to the evaluator. The function must be a .NET func (but can be defined as lambda).
        /// </summary>
        /// <param name="functionName">The name of the function. This is how the function will be called in the expression (e.g. 'a' if the function should be called by 'a(x)').</param>
        /// <param name="function">The .NET func which is to be executed.</param>
        /// <param name="isIdempotent">Whether the function is idempotent, i.e. always returns the same result if called with the same parameters.</param>
        /// <returns></returns>
        public EvaluatorBuilder AddFunction(string functionName,
            Func<double, double, double, double, double, double, double, double, double, double, double, double, double,
                double> function, bool isIdempotent = true)
        {
            Functions.Add(new FunctionDraft(functionName, isIdempotent, function));
            return this;
        }

        /// <summary>
        /// Add a new function to the evaluator. The function must be a .NET func (but can be defined as lambda).
        /// </summary>
        /// <param name="functionName">The name of the function. This is how the function will be called in the expression (e.g. 'a' if the function should be called by 'a(x)').</param>
        /// <param name="function">The .NET func which is to be executed.</param>
        /// <param name="isIdempotent">Whether the function is idempotent, i.e. always returns the same result if called with the same parameters.</param>
        /// <returns></returns>
        public EvaluatorBuilder AddFunction(string functionName,
            Func<double, double, double, double, double, double, double, double, double, double, double, double, double,
                double, double> function, bool isIdempotent = true)
        {
            Functions.Add(new FunctionDraft(functionName, isIdempotent, function));
            return this;
        }

        /// <summary>
        /// Add a new function to the evaluator. The function must be a .NET func (but can be defined as lambda).
        /// </summary>
        /// <param name="functionName">The name of the function. This is how the function will be called in the expression (e.g. 'a' if the function should be called by 'a(x)').</param>
        /// <param name="function">The .NET func which is to be executed.</param>
        /// <param name="isIdempotent">Whether the function is idempotent, i.e. always returns the same result if called with the same parameters.</param>
        /// <returns></returns>
        public EvaluatorBuilder AddFunction(string functionName,
            Func<double, double, double, double, double, double, double, double, double, double, double, double, double,
                double, double, double> function, bool isIdempotent = true)
        {
            Functions.Add(new FunctionDraft(functionName, isIdempotent, function));
            return this;
        }

        /// <summary>
        /// Add a new function to the evaluator. The function must be a .NET func (but can be defined as lambda).
        /// </summary>
        /// <param name="functionName">The name of the function. This is how the function will be called in the expression (e.g. 'a' if the function should be called by 'a(x)').</param>
        /// <param name="function">The .NET func which is to be executed.</param>
        /// <param name="isIdempotent">Whether the function is idempotent, i.e. always returns the same result if called with the same parameters.</param>
        /// <returns></returns>
        public EvaluatorBuilder AddFunction(string functionName,
            Func<double, double, double, double, double, double, double, double, double, double, double, double, double,
                double, double, double, double> function, bool isIdempotent = true)
        {
            Functions.Add(new FunctionDraft(functionName, isIdempotent, function));
            return this;
        }

        /// <summary>
        /// Add a new function to the evaluator. The function must be a .NET func (but can be defined as lambda).
        /// </summary>
        /// <param name="functionName">The name of the function. This is how the function will be called in the expression (e.g. 'a' if the function should be called by 'a(x)').</param>
        /// <param name="functionDelegate">The .NET func which is to be executed.</param>
        /// <param name="isIdempotent">Whether the function is idempotent, i.e. always returns the same result if called with the same parameters.</param>
        /// <returns></returns>
        public EvaluatorBuilder AddFunction(string functionName, DynamicFunc<double, double> functionDelegate,
            bool isIdempotent = true)
        {
            Functions.Add(new FunctionDraft(functionName, isIdempotent, functionDelegate));
            return this;
        }

        /// <summary>
        /// Add a new constant to the evaluator. If the optimization is enabled, the constant will be replaced its value during parsing in every expression processed by this engine.
        /// </summary>
        /// <param name="constantName">The constant name.</param>
        /// <param name="value">The value of the constant.</param>
        /// <returns></returns>
        public EvaluatorBuilder AddConstant(string constantName, double value)
        {
            Constants.Add(new ConstantDraft(constantName, value));
            return this;
        }

        /// <summary>
        /// Create an instance of the <see cref="Evaluator"/> class with the current configuration.
        /// </summary>
        /// <returns></returns>
        public Evaluator Build()
        {
            return new Evaluator(this);
        }

        protected bool Equals(EvaluatorBuilder other)
        {
            return Equals(CultureInfo, other.CultureInfo) && ExecutionMode == other.ExecutionMode &&
                   CacheEnabled == other.CacheEnabled && OptimizerEnabled == other.OptimizerEnabled &&
                   CaseSensitive == other.CaseSensitive && DefaultFunctions == other.DefaultFunctions &&
                   DefaultConstants == other.DefaultConstants && CacheMaximumSize == other.CacheMaximumSize &&
                   CacheReductionSize == other.CacheReductionSize && Functions.SequenceEqual(other.Functions) &&
                   Constants.SequenceEqual(other.Constants);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((EvaluatorBuilder)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (CultureInfo != null ? CultureInfo.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int)ExecutionMode;
                hashCode = (hashCode * 397) ^ CacheEnabled.GetHashCode();
                hashCode = (hashCode * 397) ^ OptimizerEnabled.GetHashCode();
                hashCode = (hashCode * 397) ^ CaseSensitive.GetHashCode();
                hashCode = (hashCode * 397) ^ DefaultFunctions.GetHashCode();
                hashCode = (hashCode * 397) ^ DefaultConstants.GetHashCode();
                hashCode = (hashCode * 397) ^ CacheMaximumSize;
                hashCode = (hashCode * 397) ^ CacheReductionSize;
                hashCode = (hashCode * 397) ^ (Functions != null ? Functions.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Constants != null ? Constants.GetHashCode() : 0);
                return hashCode;
            }
        }
    }


    /// <summary>
    /// A small helper class to store function information during the configuration of the <see cref="Evaluator"/>.
    /// </summary>
    internal class FunctionDraft
    {
        public string Name { get; }

        public bool IsIdempotent { get; }

        public Delegate Function { get; }

        public FunctionDraft(string name, bool isIdempotent, Delegate function)
        {
            Name = name;
            IsIdempotent = isIdempotent;
            Function = function;
        }

        protected bool Equals(FunctionDraft other)
        {
            return Name == other.Name && IsIdempotent == other.IsIdempotent && Equals(Function, other.Function);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((FunctionDraft)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ IsIdempotent.GetHashCode();
                hashCode = (hashCode * 397) ^ (Function != null ? Function.GetHashCode() : 0);
                return hashCode;
            }
        }
    }

    /// <summary>
    /// A small helper class to store constant information during the configuration of the <see cref="Evaluator"/>.
    /// </summary>
    internal class ConstantDraft
    {
        public string Name { get; }

        public double Value { get; }

        public ConstantDraft(string name, double value)
        {
            Name = name;
            Value = value;
        }

        protected bool Equals(ConstantDraft other)
        {
            return Name == other.Name && Value.Equals(other.Value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ConstantDraft)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Name != null ? Name.GetHashCode() : 0) * 397) ^ Value.GetHashCode();
            }
        }
    }
}