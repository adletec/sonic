# jace
_jace_ is a high performance calculation engine for the .NET platform. It can interpret and execute strings containing mathematical formulas. _jace_ is a fork of [_Jace.NET_ by Pieter De Rycke](https://github.com/pieterderycke/Jace), which is no longer actively maintained.

Our fork is **considerably faster** (up to 3.5 times) than the upstream version. It also contains numerous **bugfixes** and a lot of **maintenance work** over the latest Jace.NET release (1.0.0). See the changelog for a full list.

_jace_ is also the version of _Jace.NET_ we're using in our commercial products. _jace_ is a core component in a demanding real-time simulation tool for virtual vehicle and ADAS prototyping and continuously stress tested. The development is backed by our product sales and we're sponsoring the maintenance of the open source library.

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

_jace_ can execute formulas in two modes: **dynamic compilation mode** and **interpreted mode**. If **dynamic compilation mode** is used, _jace_ will create a dynamic method at runtime and will generate the necessary MSIL opcodes for native execution of the formula. If a formula is re-executed with other variables, _jace_ will take the dynamically generated method from its cache. Dynamic compilation mode is a lot faster and the sane default for most applications.

For specific use-cases (e.g. Unity with IL2CPP) dynamic code generation can be limited. In those cases, you can use the **interpreted mode** as a fallback.

## Installation
_jace_ is available via [nuget](https://www.nuget.org/packages/adletec-jace):

```bash
dotnet add package adletec-jace --version 1.1.0
```

## Usage
### Evaluating an Expression
#### Directly Evaluate an Expression
Provide the variables in a dictionary and the expression as string:

```csharp
Dictionary<string, double> variables = new Dictionary<string, double>();
variables.Add("var1", 2.5);
variables.Add("var2", 3.4);

CalculationEngine engine = new CalculationEngine();
double result = engine.Calculate("var1*var2", variables);
```

#### Build a Delegate for an Expression

_jace_ can also build a [delegate (Func)](https://learn.microsoft.com/en-us/dotnet/api/system.func-2?view=net-7.0) from your expression which will take the variable dictionary as argument:

```csharp
CalculationEngine engine = new CalculationEngine();
Func<Dictionary<string, double>, double> formula = engine.Build("var1+2/(3*otherVariable)");

Dictionary<string, double> variables = new Dictionary<string, double>();
variables.Add("var1", 2);
variables.Add("otherVariable", 4.2);
	
double result = formula(variables);
```

#### Build a Specific Delegate for an Expression
If you don't want to initialize a dictionary for the evaluation, you can also generate a delegate which will take the individual variable values as arguments instead:

```csharp
CalculationEngine engine = new CalculationEngine();
Func<int, double, double> formula = (Func<int, double, double>)engine.Formula("var1+2/(3*otherVariable)")
	.Parameter("var1", DataType.Integer)
    .Parameter("otherVariable", DataType.FloatingPoint)
    .Result(DataType.FloatingPoint)
    .Build();
	
double result = formula(2, 4.2);
```

### Using Mathematical Functions
You can also use mathematical functions in your expressions:

```csharp
Dictionary<string, double> variables = new Dictionary<string, double>();
variables.Add("var1", 2.5);
variables.Add("var2", 3.4);

CalculationEngine engine = new CalculationEngine();
double result = engine.Calculate("logn(var1,var2)+4", variables);
```

_jace_ supports most common functions out-of-the-box:

| Function |  Signature | Parameters|
|-----|-----|-----|
| Sine |  `sin(a)` | `a` -> Angle in radians |
| Cosine |  `cos(a)` |`a` -> Angle in radians |
| Secant |  `sec(a)` |`a` -> Angle in radians |
| Cosecant |  `csc(a)` |`a` -> Angle in radians |
| Tangent |  `tan(a)` |`a` -> Angle in radians |
| Cotangent |  `cot(a)` |`a` -> Angle in radians |
| Arcsine |  `asin(a)` |`a` -> Angle in radians |
| Arccosine |  `acos(a)` |`a` -> Angle in radians |
| Arctangent |  `atan(a)` |`a` -> Angle in radians |
| Arccotangent |  `acot(a)` |`a` -> Angle in radians |
| Natural logarithm |  `loge(a)` | `a` -> Number whose logarithm is to be found |
| Common logarithm | `log10(a)` |`a` -> Number whose logarithm is to be found |
| Logarithm | `logn(a, base)` | `a` -> Number whose logarithm is to be found<br/>`base` -> base of the logarithm |
| Square root | `sqrt(a)` | `a` -> Number whose square root is to be found |
| Absolute | `abs(a)` | `a` -> Number whose absolute value is to be found |
| If (Condition)| `if(a,b,c)` | `a` -> Boolean expression, e.g. `x > 2`<br/>`b` -> result if true(`!= 0`)<br/>`c` -> result if false (`== 0`)|
| If less | `ifless(a,b,c,d)` | `a` -> first value<br/>`b` -> second value<br/>`c` -> result if `a < b`<br/>`d` -> result if `a >= b`|
| If more | `ifmore(a,b,c,d)` | `a` -> first value<br/>`b` -> second value<br/>`c` -> result if `a > b`<br/>`d` -> result if `a <= b`|
| If equal | `ifequal(a,b,c,d)` | `a` -> first value<br/>`b` -> second value<br/>`c` -> result if `a == b`<br/>`d` -> result if `a != b`|
| Ceiling (rounding towards `+∞`) | `ceiling(a)` | `a` -> Number to be rounded|
| Floor (rounding towards `-∞`) | `floor(a)` | `a` -> Number to be rounded|
| Truncate (integral part) | `truncate(a)` | `a` -> Number to be truncated|
| Round | `round(a)` | `a` -> Number to be rounded (to even)|
| Maximum | `max(a,b,...)` | `a,b,...` -> series of numbers to find the maximum of |
| Minimum | `min(a,b,...)` | `a,b,...` -> series of numbers to find the minimum of |
| Average | `avg(a,b,...)` | `a,b,...` -> series of numbers to find the average of |
| Median | `median(a,b,...)` | `a,b,...` -> series of numbers to find the median of |
| Sum | `median(a,b,...)` | `a,b,...` -> series of numbers to build the sum of |
| Random | `random()` | no parameters, returns random number in `[0..1]` |

Todo:

Define own functions

Constants

Define own constants

## Performance
Below you can find the results of Jace.NET benchmark that show its high performance calculation engine. Tests were done on an Intel i7 2640M laptop.
1000 random formulas were generated, each containing 3 variables and a number of constants (a mix of integers and floating point numbers). Each random generated formula was executed 10 000 times. So in total 10 000 000 calculations are done during the benchmark. You can find the benchmark application in "Jace.Benchmark" if you want to run it on your system.

* Interpreted Mode : 00:00:06.7860119
* Dynamic Compilation Mode: 00:00:02.5584045


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

## Compatibility
If you are using _jace_ inside a Unity project using IL2CPP you must use _jace_ in interpreted mode due to limitations of IL2CPP with dynamic code generation.

## More Information
For more information, you can read the following articles from Pieter de Rycke, original author of Jace.NET:
* http://pieterderycke.wordpress.com/2012/11/04/jace-net-just-another-calculation-engine-for-net/
* http://www.codeproject.com/Articles/682589/Jace-NET-Just-another-calculation-engine-for-NET