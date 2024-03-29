﻿#if NETFX_CORE
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
public class FunctionRegistryTests
{
    [TestMethod]
    public void TestAddFunc2()
    {
        FunctionRegistry registry = new FunctionRegistry(false, false);
            
        Func<double, double, double> testFunction = (a, b) => a * b;
        registry.RegisterFunction("test", testFunction);

        FunctionInfo functionInfo = registry.GetFunctionInfo("test");
            
        Assert.IsNotNull(functionInfo);
        Assert.AreEqual("test", functionInfo.FunctionName);
        Assert.AreEqual(2, functionInfo.NumberOfParameters);
        Assert.AreEqual(testFunction, functionInfo.Function);

            
    }

    [TestMethod]
    public void TestNotOverwritable()
    {
        FunctionRegistry registry = new FunctionRegistry(false, false);

        Func<double, double, double> testFunction1 = (a, b) => a * b;
        Func<double, double, double> testFunction2 = (a, b) => a * b;

        registry.RegisterFunction("test", testFunction1, true);

        AssertExtensions.ThrowsException<Exception>(() =>
        {
            registry.RegisterFunction("test", testFunction2, true);
        });
    }
}