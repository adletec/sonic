using System.Globalization;
using Jace.Execution;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Jace.Tests;

[TestClass]
public class JaceOptionsTest
{
    [TestMethod]
    public void TestCopyConstructor()
    {
        // change all values from their default values.
        var baseOptions = new JaceOptions
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
        var copiedOptions = new JaceOptions(baseOptions);
        Assert.AreEqual(baseOptions, copiedOptions);
    }
}