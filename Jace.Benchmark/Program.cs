using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Threading.Tasks;
using CommandLine;
using Jace.Benchmark.ResultWriters;
using Jace.Execution;

namespace Jace.Benchmark;

public static class Program
{
    private const int NumberOfTests = 1000000;
    private const int NumberOfFunctionsToGenerate = 10000;
    private const int NumberExecutionsPerRandomFunction = 1000;

    public static void Main(string[] args)
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
        var baseOptions = new JaceOptions
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
            new()
            {
                Formula = "2+3*7", 
                Mode = BenchmarkMode.Static, 
                BenchMarkDelegate = BenchMarkCalculationEngine
            },
            new()
            {
                Formula = "something2 - (var1 + var2 * 3)/(2+3)", Mode = BenchmarkMode.Simple,
                BenchMarkDelegate = BenchMarkCalculationEngine,
                VariableDict = new Dictionary<string, double>()
                    { { "var1", 4.5642 }, { "var2", 845.4235 }, { "something2", 25038.66 } }
            },
            new()
            {
                Formula = "logn(var1, (2+3) * 500)", Mode = BenchmarkMode.SimpleFunction,
                BenchMarkDelegate = BenchMarkCalculationEngineFunctionBuild
            },
            new()
            {
                Formula = "(var1 + var2 * 3)/(2+3) - something", Mode = BenchmarkMode.Simple,
                BenchMarkDelegate = BenchMarkCalculationEngineFunctionBuild
            }
        };

        // define result layout
        var table = new DataTable();
        table.Columns.Add("Engine");
        table.Columns.Add("Case Sensitive");
        table.Columns.Add("Formula");
        table.Columns.Add("Iterations per Random Formula", typeof(int));
        table.Columns.Add("Total Iterations", typeof(int));
        table.Columns.Add("Total Duration");

        // run benchmark scenarios
        foreach (var benchmark in benchmarks)
        {
            if (mode != BenchmarkMode.All && mode != benchmark.Mode) continue;
            if (caseSensitivity is CaseSensitivity.All or CaseSensitivity.CaseInSensitive)
            {
                duration = benchmark.BenchMarkDelegate(interpretedEngine, benchmark.Formula,
                    benchmark.VariableDict);
                table.AddBenchmarkRecord("Interpreted", false, benchmark.Formula, null, NumberOfTests,
                    duration);
            }

            if (caseSensitivity is CaseSensitivity.All or CaseSensitivity.CaseSensitive)
            {
                duration = benchmark.BenchMarkDelegate(interpretedEngineCaseSensitive, benchmark.Formula,
                    benchmark.VariableDict);
                table.AddBenchmarkRecord("Interpreted", true, benchmark.Formula, null, NumberOfTests, duration);
            }

            if (caseSensitivity is CaseSensitivity.All or CaseSensitivity.CaseInSensitive)
            {
                duration = benchmark.BenchMarkDelegate(compiledEngine, benchmark.Formula,
                    benchmark.VariableDict);
                table.AddBenchmarkRecord("Compiled", false, benchmark.Formula, null, NumberOfTests, duration);
            }

            if (caseSensitivity is CaseSensitivity.All or CaseSensitivity.CaseSensitive)
            {
                duration = benchmark.BenchMarkDelegate(compiledEngineCaseSensitive, benchmark.Formula,
                    benchmark.VariableDict);
                table.AddBenchmarkRecord("Compiled", true, benchmark.Formula, null, NumberOfTests, duration);
            }
        }

        if (mode is not (BenchmarkMode.All or BenchmarkMode.Random)) return table;
            
        List<string> functions = GenerateRandomFunctions(NumberOfFunctionsToGenerate);

        if (caseSensitivity is CaseSensitivity.All or CaseSensitivity.CaseInSensitive)
        {
            //Interpreted Mode
            duration = BenchMarkCalculationEngineRandomFunctionBuild(interpretedEngine, functions,
                NumberExecutionsPerRandomFunction);
            table.AddBenchmarkRecord("Interpreted", false,
                $"Random Mode {NumberOfFunctionsToGenerate} functions 3 variables",
                NumberExecutionsPerRandomFunction,
                NumberExecutionsPerRandomFunction * NumberOfFunctionsToGenerate, duration);
        }

        if (caseSensitivity is CaseSensitivity.All or CaseSensitivity.CaseSensitive)
        {
            //Interpreted Mode(Case Sensitive)
            duration = BenchMarkCalculationEngineRandomFunctionBuild(interpretedEngineCaseSensitive, functions,
                NumberExecutionsPerRandomFunction);
            table.AddBenchmarkRecord("Interpreted", true,
                $"Random Mode {NumberOfFunctionsToGenerate} functions 3 variables",
                NumberExecutionsPerRandomFunction,
                NumberExecutionsPerRandomFunction * NumberOfFunctionsToGenerate, duration);
        }

        if (caseSensitivity is CaseSensitivity.All or CaseSensitivity.CaseInSensitive)
        {
            //Compiled Mode
            duration = BenchMarkCalculationEngineRandomFunctionBuild(compiledEngine, functions,
                NumberExecutionsPerRandomFunction);
            table.AddBenchmarkRecord("Compiled", false,
                $"Random Mode {NumberOfFunctionsToGenerate} functions 3 variables",
                NumberExecutionsPerRandomFunction,
                NumberExecutionsPerRandomFunction * NumberOfFunctionsToGenerate, duration);
        }

        if (caseSensitivity is CaseSensitivity.All or CaseSensitivity.CaseSensitive)
        {
            //Compiled Mode(Case Sensitive)
            duration = BenchMarkCalculationEngineRandomFunctionBuild(compiledEngineCaseSensitive, functions,
                NumberExecutionsPerRandomFunction);
            table.AddBenchmarkRecord("Compiled", true,
                $"Random Mode {NumberOfFunctionsToGenerate} functions 3 variables",
                NumberExecutionsPerRandomFunction,
                NumberExecutionsPerRandomFunction * NumberOfFunctionsToGenerate, duration);
        }

        return table;
    }

    private static TimeSpan BenchMarkCalculationEngine(CalculationEngine engine, string functionText,
        Dictionary<string, double> variableDict)
    {
        var start = DateTime.Now;

        for (var i = 0; i < NumberOfTests; i++)
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

        var end = DateTime.Now;

        return end - start;
    }

    private static TimeSpan BenchMarkCalculationEngineFunctionBuild(CalculationEngine engine, string functionText,
        Dictionary<string, double> variableDict)
    {
        var start = DateTime.Now;

        var function = (Func<int, int, int, double>)engine.Formula(functionText)
            .Parameter("var1", DataType.Integer)
            .Parameter("var2", DataType.Integer)
            .Parameter("something", DataType.Integer)
            .Result(DataType.FloatingPoint)
            .Build();

        var random = new Random();

        for (var i = 0; i < NumberOfTests; i++)
        {
            function(random.Next(), random.Next(), random.Next());
        }

        var end = DateTime.Now;

        return end - start;
    }

    private static List<string> GenerateRandomFunctions(int numberOfFunctions)
    {
        var result = new List<string>();
        var generator = new FunctionGenerator();

        for (var i = 0; i < numberOfFunctions; i++)
            result.Add(generator.Next());

        return result;
    }

    private static TimeSpan BenchMarkCalculationEngineRandomFunctionBuild(CalculationEngine engine,
        IEnumerable<string> functions, int numberOfTests)
    {
        var random = new Random();

        var start = DateTime.Now;

        Parallel.ForEach(functions, functionText =>
        {
            var function = (Func<int, int, int, double>)engine.Formula(functionText)
                .Parameter("var1", DataType.Integer)
                .Parameter("var2", DataType.Integer)
                .Parameter("var3", DataType.Integer)
                .Result(DataType.FloatingPoint)
                .Build();

            for (var i = 0; i < numberOfTests; i++)
            {
                function(random.Next(), random.Next(), random.Next());
            }
        });

        var end = DateTime.Now;

        return end - start;
    }

    private static void AddBenchmarkRecord(this DataTable table, string engine, bool caseSensitive, string formula,
        int? iterationsPerRandom, int totalIterations, TimeSpan duration)
    {
        table.Rows.Add(engine, caseSensitive, formula, iterationsPerRandom, totalIterations, duration);
    }

}