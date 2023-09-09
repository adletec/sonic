namespace Adletec.Sonic.Benchmark2;

public interface IBenchmarkExecutor
{
    void RunBenchmark(string expression, List<string> variableNames, long iterations, IValueProvider valueProvider);
}