using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Threading.Tasks;
using CommandLine;
using Jace.Benchmark.ResultWriters;
using Jace.Execution;

namespace Jace.Benchmark
{
    static class Program
    {
        private const int NumberOfTests = 1000000;
        private const int NumberOfFunctionsToGenerate = 10000;
        private const int NumberExecutionsPerRandomFunction = 1000;

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(options =>
                {
                    IResultWriter resultWriter = options.FileName != null ? 
                        new CsvResultWriter(options.FileName) :
                        new ConsoleResultWriter();
                    
                    DataTable table = Benchmark(options.Mode, options.CaseSensitivity);

                    resultWriter.Write(table);
                });
        }

        private static DataTable Benchmark(BenchmarkMode mode, CaseSensitivity caseSensitivity)
        {
            TimeSpan duration;

            // setup benchmark scenarios
            var baseOptions = new JaceOptions()
            {
                CultureInfo = CultureInfo.InvariantCulture,
                CacheEnabled = true,
                OptimizerEnabled = true
            };

            var interpretedEngineOptions = new JaceOptions(baseOptions)
            {
                ExecutionMode = ExecutionMode.Interpreted,
                CaseSensitive = false
            };

            var interpretedEngineCaseSensitiveOptions = new JaceOptions(baseOptions)
            {
                ExecutionMode = ExecutionMode.Interpreted,
                CaseSensitive = true
            };

            var compiledEngineOptions = new JaceOptions(baseOptions)
            {
                ExecutionMode = ExecutionMode.Compiled,
                CaseSensitive = false
            };

            var compiledEngineCaseSensitiveOptions = new JaceOptions(baseOptions)
            {
                ExecutionMode = ExecutionMode.Compiled,
                CaseSensitive = true
            };

            // initialize engines with benchmark scenarios
            var interpretedEngine = new CalculationEngine(interpretedEngineOptions);
            var interpretedEngineCaseSensitive = new CalculationEngine(interpretedEngineCaseSensitiveOptions);
            var compiledEngine = new CalculationEngine(compiledEngineOptions);
            var compiledEngineCaseSensitive = new CalculationEngine(compiledEngineCaseSensitiveOptions);

            // define benchmark operations
            BenchMarkOperation[] benchmarks =
            {
                new BenchMarkOperation()
                    { Formula = "2+3*7", Mode = BenchmarkMode.Static, BenchMarkDelegate = BenchMarkCalculationEngine },
                new BenchMarkOperation()
                {
                    Formula = "something2 - (var1 + var2 * 3)/(2+3)", Mode = BenchmarkMode.Simple,
                    BenchMarkDelegate = BenchMarkCalculationEngine,
                    VariableDict = new Dictionary<string, double>()
                        { { "var1", 4.5642 }, { "var2", 845.4235 }, { "something2", 25038.66 } }
                },
                new BenchMarkOperation()
                {
                    Formula = "logn(var1, (2+3) * 500)", Mode = BenchmarkMode.SimpleFunction,
                    BenchMarkDelegate = BenchMarkCalculationEngineFunctionBuild
                },
                new BenchMarkOperation()
                {
                    Formula = "(var1 + var2 * 3)/(2+3) - something", Mode = BenchmarkMode.Simple,
                    BenchMarkDelegate = BenchMarkCalculationEngineFunctionBuild
                },
            };

            // define result layout
            DataTable table = new DataTable();
            table.Columns.Add("Engine");
            table.Columns.Add("Case Sensitive");
            table.Columns.Add("Formula");
            table.Columns.Add("Iterations per Random Formula", typeof(int));
            table.Columns.Add("Total Iterations", typeof(int));
            table.Columns.Add("Total Duration");

            // run benchmark scenarios
            foreach (BenchMarkOperation benchmark in benchmarks)
            {
                if (mode == BenchmarkMode.All || mode == benchmark.Mode)
                {
                    if (caseSensitivity == CaseSensitivity.All || caseSensitivity == CaseSensitivity.CaseInSensitive)
                    {
                        duration = benchmark.BenchMarkDelegate(interpretedEngine, benchmark.Formula,
                            benchmark.VariableDict);
                        table.AddBenchmarkRecord("Interpreted", false, benchmark.Formula, null, NumberOfTests,
                            duration);
                    }

                    if (caseSensitivity == CaseSensitivity.All || caseSensitivity == CaseSensitivity.CaseSensitive)
                    {
                        duration = benchmark.BenchMarkDelegate(interpretedEngineCaseSensitive, benchmark.Formula,
                            benchmark.VariableDict);
                        table.AddBenchmarkRecord("Interpreted", true, benchmark.Formula, null, NumberOfTests, duration);
                    }

                    if (caseSensitivity == CaseSensitivity.All || caseSensitivity == CaseSensitivity.CaseInSensitive)
                    {
                        duration = benchmark.BenchMarkDelegate(compiledEngine, benchmark.Formula,
                            benchmark.VariableDict);
                        table.AddBenchmarkRecord("Compiled", false, benchmark.Formula, null, NumberOfTests, duration);
                    }

                    if (caseSensitivity == CaseSensitivity.All || caseSensitivity == CaseSensitivity.CaseSensitive)
                    {
                        duration = benchmark.BenchMarkDelegate(compiledEngineCaseSensitive, benchmark.Formula,
                            benchmark.VariableDict);
                        table.AddBenchmarkRecord("Compiled", true, benchmark.Formula, null, NumberOfTests, duration);
                    }
                }
            }

            if (mode == BenchmarkMode.All || mode == BenchmarkMode.Random)
            {
                List<string> functions = GenerateRandomFunctions(NumberOfFunctionsToGenerate);

                if (caseSensitivity == CaseSensitivity.All || caseSensitivity == CaseSensitivity.CaseInSensitive)
                {
                    //Interpreted Mode
                    duration = BenchMarkCalculationEngineRandomFunctionBuild(interpretedEngine, functions,
                        NumberExecutionsPerRandomFunction);
                    table.AddBenchmarkRecord("Interpreted", false,
                        string.Format("Random Mode {0} functions 3 variables", NumberOfFunctionsToGenerate),
                        NumberExecutionsPerRandomFunction,
                        NumberExecutionsPerRandomFunction * NumberOfFunctionsToGenerate, duration);
                }

                if (caseSensitivity == CaseSensitivity.All || caseSensitivity == CaseSensitivity.CaseSensitive)
                {
                    //Interpreted Mode(Case Sensitive)
                    duration = BenchMarkCalculationEngineRandomFunctionBuild(interpretedEngineCaseSensitive, functions,
                        NumberExecutionsPerRandomFunction);
                    table.AddBenchmarkRecord("Interpreted", true,
                        string.Format("Random Mode {0} functions 3 variables", NumberOfFunctionsToGenerate),
                        NumberExecutionsPerRandomFunction,
                        NumberExecutionsPerRandomFunction * NumberOfFunctionsToGenerate, duration);
                }

                if (caseSensitivity == CaseSensitivity.All || caseSensitivity == CaseSensitivity.CaseInSensitive)
                {
                    //Compiled Mode
                    duration = BenchMarkCalculationEngineRandomFunctionBuild(compiledEngine, functions,
                        NumberExecutionsPerRandomFunction);
                    table.AddBenchmarkRecord("Compiled", false,
                        string.Format("Random Mode {0} functions 3 variables", NumberOfFunctionsToGenerate),
                        NumberExecutionsPerRandomFunction,
                        NumberExecutionsPerRandomFunction * NumberOfFunctionsToGenerate, duration);
                }

                if (caseSensitivity == CaseSensitivity.All || caseSensitivity == CaseSensitivity.CaseSensitive)
                {
                    //Compiled Mode(Case Sensitive)
                    duration = BenchMarkCalculationEngineRandomFunctionBuild(compiledEngineCaseSensitive, functions,
                        NumberExecutionsPerRandomFunction);
                    table.AddBenchmarkRecord("Compiled", true,
                        string.Format("Random Mode {0} functions 3 variables", NumberOfFunctionsToGenerate),
                        NumberExecutionsPerRandomFunction,
                        NumberExecutionsPerRandomFunction * NumberOfFunctionsToGenerate, duration);
                }
            }

            return table;
        }

        private static TimeSpan BenchMarkCalculationEngine(CalculationEngine engine, string functionText,
            Dictionary<string, double> variableDict)
        {
            DateTime start = DateTime.Now;

            for (int i = 0; i < NumberOfTests; i++)
            {
                if (variableDict == null)
                {
                    engine.Calculate(functionText);
                }
                else
                {
                    engine.Calculate(functionText, new Dictionary<string, double>(variableDict));
                }
            }

            DateTime end = DateTime.Now;

            return end - start;
        }

        private static TimeSpan BenchMarkCalculationEngineFunctionBuild(CalculationEngine engine, string functionText,
            Dictionary<string, double> variableDict)
        {
            DateTime start = DateTime.Now;

            Func<int, int, int, double> function = (Func<int, int, int, double>)engine.Formula(functionText)
                .Parameter("var1", DataType.Integer)
                .Parameter("var2", DataType.Integer)
                .Parameter("something", DataType.Integer)
                .Result(DataType.FloatingPoint)
                .Build();

            Random random = new Random();

            for (int i = 0; i < NumberOfTests; i++)
            {
                function(random.Next(), random.Next(), random.Next());
            }

            DateTime end = DateTime.Now;

            return end - start;
        }

        private static List<string> GenerateRandomFunctions(int numberOfFunctions)
        {
            List<string> result = new List<string>();
            FunctionGenerator generator = new FunctionGenerator();

            for (int i = 0; i < numberOfFunctions; i++)
                result.Add(generator.Next());

            return result;
        }

        private static TimeSpan BenchMarkCalculationEngineRandomFunctionBuild(CalculationEngine engine,
            List<string> functions,
            int numberOfTests)
        {
            Random random = new Random();

            DateTime start = DateTime.Now;

            Parallel.ForEach(functions, (functionText) =>
            {
                Func<int, int, int, double> function = (Func<int, int, int, double>)engine.Formula(functionText)
                    .Parameter("var1", DataType.Integer)
                    .Parameter("var2", DataType.Integer)
                    .Parameter("var3", DataType.Integer)
                    .Result(DataType.FloatingPoint)
                    .Build();

                for (int i = 0; i < numberOfTests; i++)
                {
                    function(random.Next(), random.Next(), random.Next());
                }
            });

            DateTime end = DateTime.Now;

            return end - start;
        }

        private static void AddBenchmarkRecord(this DataTable table, string engine, bool caseSensitive, string formula,
            int? iterationsPerRandom, int totalIterations, TimeSpan duration)
        {
            table.Rows.Add(engine, caseSensitive, formula, iterationsPerRandom, totalIterations, duration);
        }

    }
}