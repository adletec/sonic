namespace Adletec.Sonic.Benchmark.Values;

/// <summary>
/// Provides values for benchmarks. For all instances of any implementation of this interface, it is required that they
/// return identical values when its methods are invoked in the same order with the same parameters.
/// </summary>
public interface IValueProvider
{
    /// <summary>
    /// Gets next value. Every instance of the same implementation returns the same sequence of values.
    /// </summary>
    /// <returns>A double value</returns>
    double GetNextValue();

    /// <summary>
    /// Gets a sequence of values. Every instance of the same implementation returns the same sequence of values.
    /// </summary>
    /// <param name="amount">The required number of values.</param>
    /// <returns>A list containing <paramref name="amount"/> values.</returns>
    List<double> GetValues(long amount);
}