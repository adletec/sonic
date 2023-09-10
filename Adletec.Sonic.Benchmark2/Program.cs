using BenchmarkDotNet.Running;

// Add your own benchmark or framework
// -----------------------------------
// To add a new framework to the benchmark, create a new implementation of IBenchmarkExecutor and add it to the
// BenchmarkExecutors list in the desired benchmark class, e.g. Benchmarks/CompareDefaultBenchmark.cs.
//
// You can also create a new benchmark class by copying an existing one and modifying it to your needs. As long as
// it contains the [Benchmark] attribute, it will be picked up by the BenchmarkSwitcher.
//
// Expressions and values are provided by the corresponding classes in the Expressions and Values folders.
// Implement the IExpressionProvider and IValueProvider interfaces to provide your own expressions and values for
// additional benchmarks.

// This shows a list of all the benchmarks in the project and lets the user select which ones to run.
BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
