# sonic | rapid expression evaluation for .NET
_sonic_ is a rapid evaluation engine for mathematical expressions. It can parse and execute strings containing mathematical expressions. _sonic_ is an extended fork of [_Jace.NET_ by Pieter De Rycke](https://github.com/pieterderycke/Jace), which is no longer actively maintained.

_sonic_ is **considerably faster** (up to 3.5 times) than _Jace.NET_. It contains numerous **bugfixes** and a lot of **maintenance work** over the latest _Jace.NET_ release (1.0.0). Many of them were originally suggested and developed by the community for _Jace.NET_, but never merged due to the dormant state of the project. See the changelog for details and a complete list.

_sonic_ is also the expression evaluator we use in our commercial products. It is a core component of our real-time simulation tools for virtual vehicle and ADAS prototyping and is continuously stress tested in a demanding environment. Its development and maintenance is funded by our product sales.

## Build Status
| branch | status |
| -----  | -----   |
| master (development) | [![Build status](https://github.com/adletec/jace/actions/workflows/dotnet.yml/badge.svg?branch=master)](https://github.com/adletec/jace/actions/workflows/dotnet.yml?query=branch%3Amaster) |
| release |  ![Build status](https://github.com/adletec/jace/actions/workflows/release.yml/badge.svg)|

## Quick Start
_sonic_ can interpret and execute strings containing mathematical formulas. These formulas can rely on variables. If variables are used, values can be provided for these variables at execution time of the mathematical formula.

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

_sonic_ can execute formulas in two modes: **dynamic compilation mode** and **interpreted mode**. If **dynamic compilation mode** is used, _sonic_ will create a dynamic method at runtime and will generate the necessary MSIL opcodes for native execution of the formula. If a formula is re-executed with other variables, _sonic_ will take the dynamically generated method from its cache. Dynamic compilation mode is a lot faster and the sane default for most applications.

For specific use-cases (e.g. Unity with IL2CPP) dynamic code generation can be limited. In those cases, you can use the **interpreted mode** as a fallback.

## Installation
_sonic_ is available via [nuget](https://www.nuget.org/packages/adletec-sonic):

```bash
dotnet add package adletec-sonic --version 1.1.0
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

_sonic_ can also build a [delegate (Func)](https://learn.microsoft.com/en-us/dotnet/api/system.func-2?view=net-7.0) from your expression which will take the variable dictionary as argument:

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

#### Built-in Functions

_sonic_ supports most common functions out-of-the-box:

| Function |  Signature | Parameters|
|-----|-----|-----|
| Sine |  `sin(a)` | `a`: angle in radians |
| Cosine |  `cos(a)` |`a`: angle in radians |
| Secant |  `sec(a)` |`a`: angle in radians |
| Cosecant |  `csc(a)` |`a`: angle in radians |
| Tangent |  `tan(a)` |`a`: angle in radians |
| Cotangent |  `cot(a)` |`a`: angle in radians |
| Arcsine |  `asin(a)` |`a`: angle in radians |
| Arccosine |  `acos(a)` |`a`: angle in radians |
| Arctangent |  `atan(a)` |`a`: angle in radians |
| Arccotangent |  `acot(a)` |`a`: angle in radians |
| Natural logarithm |  `loge(a)` | `a`: number whose logarithm is to be found |
| Common logarithm | `log10(a)` |`a`: number whose logarithm is to be found |
| Logarithm | `logn(a, b)` | `a`: number whose logarithm is to be found<br/>`b`: base of the logarithm |
| Square root | `sqrt(a)` | `a`: number whose square root is to be found |
| Absolute | `abs(a)` | `a`: number whose absolute value is to be found |
| If | `if(a,b,c)` | `a`: boolean expression, e.g. `x > 2`<br/>`b`: result if true(`!= 0`)<br/>`c`: result if false (`== 0`)|
| If less | `ifless(a,b,c,d)` | `a`: first value<br/>`b`: second value<br/>`c`: result if `a < b`<br/>`d`: result if `a >= b`|
| If more | `ifmore(a,b,c,d)` | `a`: first value<br/>`b`: second value<br/>`c`: result if `a > b`<br/>`d`: result if `a <= b`|
| If equal | `ifequal(a,b,c,d)` | `a`: first value<br/>`b`: second value<br/>`c`: result if `a == b`<br/>`d`: result if `a != b`|
| Ceiling | `ceiling(a)` | `a`: number to be rounded towards `+∞`|
| Floor | `floor(a)` | `a`: number to be rounded towards `-∞`|
| Truncate | `truncate(a)` | `a`: number to be truncated (to integral part)|
| Round | `round(a)` | `a`: number to be rounded (to even)|
| Maximum | `max(a,b,...)` | `a,b,...`: series of numbers to find the maximum of |
| Minimum | `min(a,b,...)` | `a,b,...`: series of numbers to find the minimum of |
| Average | `avg(a,b,...)` | `a,b,...`: series of numbers to find the average of |
| Median | `median(a,b,...)` | `a,b,...`: series of numbers to find the median of |
| Sum | `median(a,b,...)` | `a,b,...`: series of numbers to build the sum of |
| Random | `random()` | no parameters, returns random number in `[0..1]` |

The function names are reserved keywords and cannot be overwritten. If you need to override a function, you can globaly disable the built-in functions using the configuration (see below).

#### Custom Functions
You can define your own functions using the `.AddFunction()`-method of the engine.

```csharp
var engine = new CalculationEngine();
engine.AddFunction("ft2m", (Func<double, double>)((a) => a * 0.3048)):
double result = engine.Calculate("ft2m(30)"); // 9.144
```

The `.AddFunction()`-method provides overloads for functions with up to 16 parameters. If you want to process an arbitrary amount of parameters, you can use dynamic functions:

```csharp
double MyCustomSumFormula(params double[] a)
{
    return a.Sum();
}

var engine = new CalculationEngine();
engine.AddFunction("customSum", MyCustomSumFormula);
double result = engine.Calculate("customSum(1,2,3,4,5,6)"); // 21.0
```

Custom function names are overwritable, so you can re-register the same name with a different implementation.

### Using Constants
_sonic_ provides support for pre-compile constants. These constants are taken into account during the optimazation phase of the compilation process. So if your expression contains an operation like `2 * pi`, this operation is already evaluated when you build a delegate and the result is also cached, even if you're using plain string evaluation.

#### Built-in Constants
| Constant | Name |
| ---- | ---- |
| π | `pi` |
| ⅇ | `e` |

The constant names are reserved keywords and cannot be overwritten. If you need to override a constant, you can globaly disable the built-in constants using the configuration (see below).

#### Custom Constants
You can define your own constants using the `.AddConstant()`-method of the engine.

```csharp
var engine = new CalculationEngine();
engine.AddConstant("g", 9.80665);
double result = engine.Calculate("g*2"); // 19.6133
```

Custom constant names are overwritable, so you can re-register the same name with a different implementation.

## Configuration
The `CalculationEngine`-constructor takes a `SonicOptions`-object to adjust its behavior:

| Option | Values (bold = default)| Comment |
| ---- | ---- | ----|
| `CultureInfo` | **`CultureInfo.CurrentCulture`**<br/>[`CultureInfo.*`](https://learn.microsoft.com/en-us/dotnet/api/system.globalization.cultureinfo?view=net-7.0) | Determines the number format (e.g. decimal separator, thousands separator, etc.); defaults to your system default; | 
| `ExecutionMode` | **`ExecutionMode.Compiled`**<br/>`ExecutionMode.Interpreted` | `Compiled` will dynamically compile the evaluation to MSIL which grants the best performance. If you are using a platform where dynamic compilation is restricted, or want to prevent dynamic compilation for other reasons, you can use `Interpreted` as fallback.|
| `CacheEnabled` | **`true`**<br/>`false` | Can be used to disable the formula cache if set to `false`. The formula cache keeps a copy of the optimized AST for every given formula string, so it can be re-used. This makes subsequent evaluations of the same formula significantly faster.
| `OptimizerEnabled` | **`true`**<br/>`false` | Can be used to disable the optimizer if set to `false`. The optimizer will pre-evaluate parts of the equation which do not depend on variables, including multiplications with 0 or 0 exponents. |
| `CaseSensitive` | **`true`**<br/>`false` | Determines wether the provided variable names will be evaluated case-sensitive (`true`) or case-insensitive (`false`). If you don't absolutely need case-insensitivity, you should keep this option set to case-sensitive since this has a significant performance impact. |
| `DefaultFunctions` | **`true`**<br/>`false` | Can be used to disable the built-in functions if set to `false`. |
| `DefaultConstants` | **`true`**<br/>`false` | Can be used to disable the built-in constants if set to `false`. |
| `CacheMaximumSize` | **`500`**<br/> | The number of expressions to keep in the cache. |
| `CacheReductionSize` | **`50`** | The number of expressions to drop from the cache once it reaches its maximum size ([FIFO](https://en.wikipedia.org/wiki/FIFO_(computing_and_electronics))). |

## Performance
### Benchmark
_sonic_ is primed to deliver great performance out-of-the-box. This also shows when comparing _sonic_ to other common math evaluation libraries.

**Disclaimer**: Keep in mind that all those libraries have unique features and performance is only one aspect when choosing the right library for your project. The following comparison is in no way a statement towards the general superiority/inferiority of any of the listed libraries.

#### Benchmark Setup
We're using a simple benchmark which will take the same three equations and evaluate them using the same values with all libraries:

* Expression A - Simple expression `var1 + var2 * var3 / 2`
* Expression B - Optimizable expression: `(var1 + var2 * var3 / 2) * 0 + 0 / (var1 + var2 * var3 / 2) + (var1 + var2 * var3 / 2)^0`
* Expression C - Functions and Constants: `sin(var1) + cos(var2) + pi^2`

To make sure the expressions will be re-evaluated, we're incrementing each variable on every iteration.

The benchmark runs all iterations on the same machine (MacBook Pro 2021, M1 Max).

#### Benchmark Results
The following table shows the time in seconds it takes the benchmark to complete 100.000 evaluations of each expression:

| Library | Case Sensitive | Expression A | Expression B | Expression C |
|---- | ---- | ---- |  ---- | ---- | 
| _sonic_ | `true` | | | |
| CalcPad | `true` | | | |
| NCalc | `true` |  | | |
| Jace.NET | `true` |  | | |



You can find the benchmark code on GitHub, if you want to re-run it on your machine.

## Architecture
_sonic_ follows a design similar to most of the modern compilers. Parsing and execution is done in a number of phases:

### Tokenizing
During the tokenizing phase, the string is converted into the different kind of tokens: variables, operators, and constants.

### Abstract Syntax Tree Creation
During the abstract syntax tree creation phase, the tokenized input is converted into a hierarchical tree representing the mathematically formula. This tree unambiguously stores the mathematical calculations that must be executed.

### Optimization
During the optimization phase, the abstract syntax tree is optimized for execution.

### Interpreted Execution/Dynamic Compilation
In this phase the abstract syntax tree is executed in either interpreted mode or in dynamic compilation mode.

## Compatibility
If you are using _sonic_ inside a Unity project using IL2CPP you must use _sonic_ in interpreted mode due to limitations of IL2CPP with dynamic code generation.

## More Information
For more information, you can read the following articles from Pieter de Rycke, original author of Jace.NET:
* http://pieterderycke.wordpress.com/2012/11/04/jace-net-just-another-calculation-engine-for-net/
* http://www.codeproject.com/Articles/682589/Jace-NET-Just-another-calculation-engine-for-NET