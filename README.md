# jace
_jace_ is a high performance calculation engine for the .NET platform. It can interpret and execute strings containing mathematical formulas. _jace_ is a fork of [_Jace.NET_ by Pieter De Rycke](https://github.com/pieterderycke/Jace), which is no longer actively maintained. Our fork contains bugfixes, performance improvements and a lot of maintenance work when compared to the upstream. See the changelog for a full list.

_jace_ is also the version of _Jace.NET_ we're using in our commercial products. _jace_ is a core component in a demanding real-time simulation for virtual prototyping and continuously stress tested. The development is backed by our product sales and we're sponsoring the maintenance of the open source library.

## Build Status
* [![Build status](https://github.com/adletec/jace/actions/workflows/dotnet.yml/badge.svg?branch=master)](https://github.com/adletec/jace/actions/workflows/dotnet.yml?query=branch%3Amaster) (master)
* [![Build status](https://github.com/adletec/jace/actions/workflows/release.yml/badge.svg)](https://github.com/adletec/jace/actions/workflows/release.yml?query=) (release)

## What does it do?
_jace_ can interpret and execute strings containing mathematical formulas. These formulas can rely on variables. If variables are used, values can be provided for these variables at execution time of the mathematical formula.

Consider this simple example:

```csharp
var variables = new Dictionary<string, double>();
variables.Add("var1", 2.5);
variables.Add("var2", 3.4);

var engine = new CalculationEngine();
double result = engine.Calculate("var1*var2", variables); // 8.5
```

The CalculationEngine comes with out-of-the-box support for many arithmetic functions like trigonometrics (`sin`, `cos`, `atan`, `...`) and statistics (`avg`, `max`, `min`, `median`, `...`), but also simple boolean logic (`if`, `ifless`, `ifequal`, ...).

You can also add your own domain-specific functions. This example adds a conversion function from length in feet (`ft`) to meter (`m`):

```csharp
var engine = new CalculationEngine();
engine.AddFunction("ft2m", (Func<double, double>)((a) => a * 0.3048)):
double result = engine.Calculate("ft2m(30)"); // 9.144
```

You can find more examples below.

_jace_ can execute formulas in two modes: **dynamic compilation mode** and **interpreted mode**. If **dynamic compilation mode **is used, _jace_ will create a dynamic method at runtime and will generate the necessary MSIL opcodes for native execution of the formula. If a formula is re-executed with other variables, _jace_ will take the dynamically generated method from its cache. Dynamic compilation mode is a lot faster and the sane default for most applications.

For specific use-cases (e.g. Unity with IL2CPP) dynamic code generation can be limited. In those cases, you can use the **interpreted mode** as a fallback.

## Architecture
_jace_ follows a design similar to most of the modern compilers. Interpretation and execution is done in a number of phases:

### Tokenizing
During the tokenizing phase, the string is converted into the different kind of tokens: variables, operators, and constants.

### Abstract Syntax Tree Creation
During the abstract syntax tree creation phase, the tokenized input is converted into a hierarchical tree representing the mathematically formula. This tree unambiguously stores the mathematical calculations that must be executed.

### Optimization
During the optimization phase, the abstract syntax tree is optimized for execution.

### Interpreted Execution/Dynamic Compilation
In this phase the abstract syntax tree is executed in either interpreted mode or in dynamic compilation mode.

## Examples
_jace_ can be used in a couple of ways:

To directly execute a given mathematical formula using the provided variables:
```csharp
Dictionary<string, double> variables = new Dictionary<string, double>();
variables.Add("var1", 2.5);
variables.Add("var2", 3.4);

CalculationEngine engine = new CalculationEngine();
double result = engine.Calculate("var1*var2", variables);
```

To build a .NET Func accepting a dictionary as input containing the values for each variable:
```csharp
CalculationEngine engine = new CalculationEngine();
Func<Dictionary<string, double>, double> formula = engine.Build("var1+2/(3*otherVariable)");

Dictionary<string, double> variables = new Dictionary<string, double>();
variables.Add("var1", 2);
variables.Add("otherVariable", 4.2);
	
double result = formula(variables);
```

To build a typed .NET Func:
```csharp
CalculationEngine engine = new CalculationEngine();
Func<int, double, double> formula = (Func<int, double, double>)engine.Formula("var1+2/(3*otherVariable)")
	.Parameter("var1", DataType.Integer)
    .Parameter("otherVariable", DataType.FloatingPoint)
    .Result(DataType.FloatingPoint)
    .Build();
	
double result = formula(2, 4.2);
```

Functions can be used inside the mathemical formulas. _jace_ currently offers four functions accepting one argument (sin, cos, loge and log10) and one function accepting two arguments (logn).

```csharp
Dictionary<string, double> variables = new Dictionary<string, double>();
variables.Add("var1", 2.5);
variables.Add("var2", 3.4);

CalculationEngine engine = new CalculationEngine();
double result = engine.Calculate("logn(var1,var2)+4", variables);
```

## Performance
Below you can find the results of Jace.NET benchmark that show its high performance calculation engine. Tests were done on an Intel i7 2640M laptop.
1000 random formulas were generated, each containing 3 variables and a number of constants (a mix of integers and floating point numbers). Each random generated formula was executed 10 000 times. So in total 10 000 000 calculations are done during the benchmark. You can find the benchmark application in "Jace.Benchmark" if you want to run it on your system.

* Interpreted Mode : 00:00:06.7860119
* Dynamic Compilation Mode: 00:00:02.5584045

## Compatibility
If you are using _jace_ inside a Unity project using IL2CPP you must use _jace_ in interpreted mode due to limitations of IL2CPP with dynamic code generation.

## More Information
For more information, you can read the following articles from Pieter de Rycke, original author of Jace.NET:
* http://pieterderycke.wordpress.com/2012/11/04/jace-net-just-another-calculation-engine-for-net/
* http://www.codeproject.com/Articles/682589/Jace-NET-Just-another-calculation-engine-for-NET