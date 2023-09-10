using BenchmarkDotNet.Running;

// This shows a list of all the benchmarks in the project and lets the user select which ones to run.
BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
