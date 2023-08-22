using System;
using System.Globalization;
using Adletec.Sonic.Execution;

namespace Adletec.Sonic
{
    public class SonicOptions
    {
        private const int DefaultCacheMaximumSize = 500;
        private const int DefaultCacheReductionSize = 50;

        public SonicOptions()
        {
            CultureInfo = CultureInfo.CurrentCulture;
            ExecutionMode = ExecutionMode.Compiled;
            CacheEnabled = true;
            OptimizerEnabled = true;
            CaseSensitive = true;
            DefaultFunctions = true;
            DefaultConstants = true;
            CacheMaximumSize = DefaultCacheMaximumSize;
            CacheReductionSize = DefaultCacheReductionSize;
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        /// <param name="options">The options to copy from.</param>
        public SonicOptions(SonicOptions options)
        {
            CultureInfo = options.CultureInfo;
            ExecutionMode = options.ExecutionMode;
            CacheEnabled = options.CacheEnabled;
            OptimizerEnabled = options.OptimizerEnabled;
            CaseSensitive = options.CaseSensitive;
            DefaultConstants = options.DefaultConstants;
            DefaultFunctions = options.DefaultFunctions;
            CacheMaximumSize = options.CacheMaximumSize;
            CacheReductionSize = options.CacheReductionSize;
        }

        /// <summary>
        /// The <see cref="CultureInfo"/> required for correctly reading floating point numbers.
        /// </summary>
        public CultureInfo CultureInfo { get; set; }

        /// <summary>
        /// The execution mode that must be used for formula execution.
        /// </summary>
        public ExecutionMode ExecutionMode { get; set; }

        /// <summary>
        /// Enable or disable caching of mathematical formulas.
        /// </summary>
        public bool CacheEnabled { get; set; }

        /// <summary>
        /// Configure the maximum cache size for mathematical formulas.
        /// </summary>
        public int CacheMaximumSize { get; set; }

        /// <summary>
        /// Configure the cache reduction size for mathematical formulas.
        /// </summary>
        public int CacheReductionSize { get; set; }

        /// <summary>
        /// Enable or disable optimizing of formulas.
        /// </summary>
        public bool OptimizerEnabled { get; set; }

        /// <summary>
        /// Enable or disable converting to lower case. This parameter is the inverse of <see cref="CaseSensitive"/>.
        /// </summary>
        [Obsolete]
        public bool AdjustVariableCase { 
            get => !CaseSensitive;
            set => CaseSensitive = !value;
        }

        /// <summary>
        /// Enable case sensitive or case insensitive processing mode.
        /// </summary>
        public bool CaseSensitive { get;  set; }

        /// <summary>
        /// Enable or disable the default functions.
        /// </summary>
        public bool DefaultFunctions { get; set; }

        /// <summary>
        /// Enable or disable the default constants.
        /// </summary>
        public bool DefaultConstants { get; set; }
        
        protected bool Equals(SonicOptions other)
        {
            return Equals(CultureInfo, other.CultureInfo) && ExecutionMode == other.ExecutionMode && CacheEnabled == other.CacheEnabled && CacheMaximumSize == other.CacheMaximumSize && CacheReductionSize == other.CacheReductionSize && OptimizerEnabled == other.OptimizerEnabled && CaseSensitive == other.CaseSensitive && DefaultFunctions == other.DefaultFunctions && DefaultConstants == other.DefaultConstants;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SonicOptions)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (CultureInfo != null ? CultureInfo.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int)ExecutionMode;
                hashCode = (hashCode * 397) ^ CacheEnabled.GetHashCode();
                hashCode = (hashCode * 397) ^ CacheMaximumSize;
                hashCode = (hashCode * 397) ^ CacheReductionSize;
                hashCode = (hashCode * 397) ^ OptimizerEnabled.GetHashCode();
                hashCode = (hashCode * 397) ^ CaseSensitive.GetHashCode();
                hashCode = (hashCode * 397) ^ DefaultFunctions.GetHashCode();
                hashCode = (hashCode * 397) ^ DefaultConstants.GetHashCode();
                return hashCode;
            }
        }

    }
}
