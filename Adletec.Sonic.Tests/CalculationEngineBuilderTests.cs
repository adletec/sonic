using System;
using System.Globalization;
using Adletec.Sonic.Execution;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Adletec.Sonic.Tests;

[TestClass]
public class CalculationEngineBuilderTests
{
    [TestMethod]
    public void TestCopyConstructor()
    {

        // change all values from their default values.
        var baseBuilder = CalculationEngine.Create()
            .UseCulture(CultureInfo.InvariantCulture)
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
            
        var copiedBuilder = new CalculationEngineBuilder(baseBuilder);
        
        // values must match in copied object
        Assert.AreEqual(baseBuilder, copiedBuilder);
    }

}