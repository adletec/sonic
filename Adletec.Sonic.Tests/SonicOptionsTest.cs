using System.Globalization;
using Adletec.Sonic.Execution;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Adletec.Sonic.Tests;

[TestClass]
public class SonicOptionsTest
{
    [TestMethod]
    public void TestCopyConstructor()
    {
        // change all values from their default values.
        var baseOptions = new SonicOptions
        {
            CultureInfo = CultureInfo.InvariantCulture,
            ExecutionMode = ExecutionMode.Interpreted,
            CacheEnabled = false,
            OptimizerEnabled = false,
            CaseSensitive = true,
            DefaultFunctions = false,
            DefaultConstants = false,
            CacheMaximumSize = 0,
            CacheReductionSize = 0
        };

        // values must match in copied object
        var copiedOptions = new SonicOptions(baseOptions);
        Assert.AreEqual(baseOptions, copiedOptions);
    }
}