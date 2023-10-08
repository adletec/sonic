using System;
using System.Globalization;
using Adletec.Sonic.Execution;
using Adletec.Sonic.Tokenizer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Adletec.Sonic.Tests;

[TestClass]
public class EvaluatorBuilderTests
{
    [TestMethod]
    public void TestCopyConstructor()
    {

        // change all values from their default values.
        var baseBuilder = Evaluator.Create()
            .UseCulture(CultureInfo.CurrentCulture)
            .UseArgumentSeparator('\\')
            .UseExecutionMode(ExecutionMode.Interpreted)
            .DisableCache()
            .DisableOptimizer()
            .DisableCaseSensitivity()
            .DisableDefaultFunctions()
            .DisableDefaultConstants()
            .UseCacheMaximumSize(12345)
            .UseCacheReductionSize(123)
            .AddConstant("a", 12345)
            .AddFunction("b", x => x);
            
        var copiedBuilder = new EvaluatorBuilder(baseBuilder);
        
        // values must match in copied object
        Assert.AreEqual(baseBuilder, copiedBuilder);
    }

    [TestMethod]
    public void TestIllegalArgumentSeparator()
    {
        AssertExtensions.ThrowsException<ArgumentException>(() =>
        {
            Evaluator.Create().UseArgumentSeparator('a');
        });
    }

}