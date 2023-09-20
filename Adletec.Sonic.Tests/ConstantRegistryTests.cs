#if NETFX_CORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#elif __ANDROID__
using NUnit.Framework;
using TestClass = NUnit.Framework.TestFixtureAttribute;
using TestMethod = NUnit.Framework.TestAttribute;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif
using System;
using Adletec.Sonic.Execution;

namespace Adletec.Sonic.Tests;

[TestClass]
public class ConstantRegistryTests
{
    [TestMethod]
    public void TestAddConstant()
    {
        var registry = new ConstantRegistry(false, false);
            
        registry.RegisterConstant("test", 42.0);

        ConstantInfo functionInfo = registry.GetConstantInfo("test");
            
        Assert.IsNotNull(functionInfo);
        Assert.AreEqual("test", functionInfo.ConstantName);
        Assert.AreEqual(42.0, functionInfo.Value);
    }


    [TestMethod]
    public void TestNotOverwritable()
    {
        var registry = new ConstantRegistry(false, false);

        registry.RegisterConstant("test", 42.0);

        AssertExtensions.ThrowsException<Exception>(() =>
        {
            registry.RegisterConstant("test", 26.3);
        });
    }
}