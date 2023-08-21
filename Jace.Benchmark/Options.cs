using CommandLine;

namespace Jace.Benchmark;

class Options
{
    [Option("case-sensitivity", HelpText = "Execute in case sensitive mode, case insensitive mode or execute both.")]
    public CaseSensitivity CaseSensitivity { get; set; }

    [Option('m', "mode", HelpText = "Specify the benchmark to execute.")]
    public BenchmarkMode Mode { get; set; }

    [Option('f', "file", HelpText = "If set, the result will be written to the specified path as CSV.")]
    public string FileName { get; set; }
        
}