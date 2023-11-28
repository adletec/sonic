# Changelog
## 1.4.0 (2023-11-28)
### Summary
This release contains an addition to the validation introduced in 1.3.0.

### Features
- Add optional validation of variable completeness.

### Maintenance
- Removal of dead code.

## 1.3.2 (2023-11-16)
### Summary
This is a bugfix release to address an issue with argument count validation.

### Bugfix
- Fixes #37, in which arguments preceded by a unary minus didn't count towards the argument count in the validation step.

## 1.3.1 (2023-11-12)
### Summary
This release contains some minor improvements for the validation introduced in 1.3.0.

### Changes
- `ParseException` and all derived types have a new `Expression` property which contains the complete failed expression.
- The GitHub project now also tracks test coverage. We're starting as is and define < 80% test coverage as "yellow" and < 60% as "red". The aim for the next releases is to consistently improve and then maintain > 80% coverage.

### Bugfix
- Functions are now identified as such if there are white-spaces between the function name and the opening parenthesis.
E.g.: `sin (x)` instead of `sin(x)`.


## 1.3.0 (2023-11-06)
### Summary
This release is all about validation. Jace, and thus _sonic_, has always been a bit lenient when it comes to validating expressions. This is due to the fact that Jace didn't actually
validate the expression, but rather handled errors when they occurred. Improving upon the existing validation quickly proved close to impossible, since all of the existing error handling
was part of the AST Builder, which uses the Shunting Yard algorithm. The Shunting Yard algorithm is a stack-based algorithm, which means that it's not possible to handle things like
mixed up orders of operators and operands, or missing function arguments in a reliable way. This is because the Shunting Yard algorithm doesn't know the context of the expression it's
parsing. It only knows the current token and the tokens it has already seen, but it doesn't know the order of operations and values since they are stored in different stacks.

For example, something like "a sin()" would have been parsed as "sin(a)" while "sin() a" would have thrown an error. Also, typos like "a b +" had been silently parsed as "a + b".

To improve upon this, we've introduced a new validation step before the Shunting Yard algorithm is run. This validation step is able to detect a lot of errors, which previously would
not have been detected. The validation throws a detailed exception, which contains the position of the error in the expression, the token the error is related to, and a human-readable
error message. This makes it a lot easier to debug errors in expressions.

Best of all, we could simplify the AST Builder a lot, since it doesn't have to handle errors anymore. This makes the code a lot easier to read and maintain. As a consequence,
the validation comes at close to no performance cost, since the AST Builder doesn't have to do any additional work. The validation is also completely optional, so if you don't
need it or did already validate your expressions in a previous step, you can disable it.

To enable this, we've also added the "Validate()"-method to the Evaluator. This method will validate the expression without evaluation and throw an exception if the expression is invalid.

### Performance Improvements
- Improve performance of the AST Builder by removing unnecessary operations and simplifying the code.
- Removal of unnecessary case conversion in the function lookup of the AST Builder.
- Slightly faster unary minus check in the AST Builder.

### Features
- Add validation of expressions. Validation of expressions on parsing is enabled by default and can be disabled in the EvaluatorBuilder. Validation can also be triggered manually by calling the "Validate()" method on the Evaluator.
- New option to override the argument separator in the EvaluatorBuilder (also see bugfixes).

### Changes
- (BREAKING) Improved internal architectural structure. Only elements considered public API are now part of the top-level namespace. You can still access elements like the ASTBuilder, but there is no guarantee that there won't be breaking changes in the future.
- (BREAKING) Functions are now treated as their own type of token instead of being treated as operators. As a consequence, function names do no longer collide with variable or constant names. This means that an expression like "a(123) + a" is now valid, whereas it was invalid before. In guarded mode, this will still throw an exception, since guarded mode aims to prevent ambiguous expressions.
- Improved public API documentation.
- Adjusted benchmark to use the new validation feature.
- (INTERNAL) Parantheses are now consistently named "Parentheses" instead of "Brackets" in the code base to prevent AE/BE mixups ([]/()).

### Bugfix
- Fix a bug in which the argument separator could collide with the decimal separator in some cases. Before, the argument separator was taken from the current culture, which could lead to issues if the decimal separator was the same as the argument separator. Now, the argument separator is a comma (",") for cultures which use a colon (".") as decimal separator and a semi colon (";") for cultures which use a comma (",") as decimal separator. This is the same behavior as in Excel, which some might call exptected.
- Fix missing unary minus handling for numerics. The fix for #8 was incomplete and didn't handle unary minus for numerics. This means that the Wikipedia example '-3^2' still evaluated to 9.0 instead of -9.0. This is now fixed. If you want 9.0, you now have to write '(-3)^2' instead.

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
