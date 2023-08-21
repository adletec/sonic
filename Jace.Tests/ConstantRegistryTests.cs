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
using Jace.Execution;

namespace Jace.Tests;

[TestClass]
public class ConstantRegistryTests
{
    [TestMethod]
    public void TestAddConstant()
    {
        ConstantRegistry registry = new ConstantRegistry(false);
            
        registry.RegisterConstant("test", 42.0);

        ConstantInfo functionInfo = registry.GetConstantInfo("test");
            
        Assert.IsNotNull(functionInfo);
        Assert.AreEqual("test", functionInfo.ConstantName);
        Assert.AreEqual(42.0, functionInfo.Value);
    }

    [TestMethod]
    public void TestOverwritable()
    {
        ConstantRegistry registry = new ConstantRegistry(false);

        registry.RegisterConstant("test", 42.0);
        registry.RegisterConstant("test", 26.3);
    }

    [TestMethod]
    public void TestNotOverwritable()
    {
        ConstantRegistry registry = new ConstantRegistry(false);

        registry.RegisterConstant("test", 42.0, false);

        AssertExtensions.ThrowsException<Exception>(() =>
        {
            registry.RegisterConstant("test", 26.3, false);
        });
    }
}