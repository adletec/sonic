# Changelog
## 1.2.0 (2023-09-20)
### Summary
This is the first public release of _sonic_.
The release version might sound a bit arbitrary, but it's pretty straight forward: 1.0.0 is the _Jace_ version _sonic_ is forked from,
1.1.0-* was the internal development version before public release.

As such, the list of changes is pretty long (over 37,707 tracked changes according to git), it's pretty hard to list all of them in detail.
Instead, we're trying to find the right level of abstraction here and only list fundamental changes or fixed bugs.

### Performance Improvements
- Take case-sensitivity into account in function and constant registry (fix by @aavita in #1)
- Implement a faster case-insensitive mode using `StringComparer.OrdinalIgnoreCase`.
- Vastly improve constant folding (add new folding logic, completely fold constants, return constant instead of evaluator if the only operation is a constant, add support for integer constant folding, update idempotency if arguments of idempotent functions can be folded etc.)
- Remove unnecessary operations. In some cases, there were duplications of logic or checks. Those duplications (or plainly operations with no effect towards the evaluation) were probably artifacts of changes made to Jace over the years, when a new logic was set to replace the old one, but the old logic wasn't entirely removed. This became apparent when consolidating the behavior and architecture, and sometimes even while profiling the library to identify bottlenecks.
- Introduce sane default behavior in case of ambiguous situations (e.g. variable and constant of the same name defaults to constant use) and move additional, expensive checks to explicit guard mode.
- Completely disable use of cache if the Evaluator is set to do so. This improves performance if expressions are evaluated once (single pass), which is the prime use-case of disabling the cache.
- Move logic from evaluation to build time wherever possible. This makes no difference for expressions which are evaluated exactly once, but improves performance on every subsequent evaluation.
- Change the initialization of the Evaluator to a builder pattern. This enables us to know all functions and constants when evaluating an expression, which in turn makes it possible to optimize expressions a lot better. Also, this resolves a lot of ambiguity in the usage and also the code base (e.g., how to optimize already built delegates if you can add constants after the optimizer did already run).
- Lots of small performance improvements...
- Whenever iteration over a dictionary is necessary, use an array for iteration instead. The conversion is usually a lot cheaper than the pretty expensive iteration over the dictionary.


### Features
- Add dynamic sum function (by @Grantmartin2002 in #2)
- Add interface to Evaluator to improve test- and mockability

### Changes
- Replace benchmark with a completely new BenchmarkDotNet-based implementation. The new benchmark also adds comparisons with other frameworks, like the original_Jac.net_. This allows users to compare specific use cases in a reliable benchmarking environment.
- Replace manual release process with automated build, test, and release jobs.
- Replace WPF demo application with cross-platform Avalonia UI demo application.
- Update framewok dependencies in all projects which aren't part of the NuGet package.
- Change project files to SDK style and make all projects cross-platform compatible.
- Change defaults to case-sensitive from case-insensitive. This is faster and should also match expected default behavior in the C# context.
- Change the behavior of `.Evaluate()` to match what built delegates do.
- Introduce newer syntactic sugar where possible thanks to the framework upgrades and also makes use of what's available in .NET Standard 1.6. This is primarily done wherever it improves legibility or maintainability (e.g. the use of nameof(argumentName) instead of string concatenation or type inference where types are part of the assignment, the use of object initializers, string templating, etc.).
- Improve null-handling by adding null-checks to error prone methods (no or very little overhead).
- Extend API documentation.


### Bugfixes
- Add missing constant lookup in interpreted mode (fix by @FabianNitsche in #4)
- Fix for missing error handling in case of invalid scientific numbers (fix by @FabianNitsche in #5)
- Fix IndexOutOfBoundsException when using negative Euler numbers (fix by @FabianNitsche in #6)
- Fix handling of invalid floting point numbers in expressions (fix by @FabianNitsche in #7)
- Fixes an issue with the precedence of powers of negative numbers (fix by @FabianNitsche in #8)
- Prevent evaluation from mutating the given variable dictionary
- Fix indempotency of function tokens if they depend on constants
- Fix missing checks when building a delegate. Previously, this omitted all consistency checks. Now, if guard mode is enabled, it will also be active in delegates. If it is disabled (default) the same sane defaults will prevent errors in case of ambiguous input.
