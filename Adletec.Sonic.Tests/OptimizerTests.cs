using System;
using System.Collections.Generic;
using System.Globalization;
using Adletec.Sonic.Execution;
using Adletec.Sonic.Operations;
using Adletec.Sonic.Tokenizer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Adletec.Sonic.Tests;

[TestClass]
public class OptimizerTests
{
    [TestMethod]
    public void TestOptimizerIdempotentFunction()
    {
        Optimizer optimizer = new Optimizer(new Interpreter());

        TokenReader tokenReader = new TokenReader(CultureInfo.InvariantCulture);
        IList<Token> tokens = tokenReader.Read("test(var1, (2+3) * 500)");

        IFunctionRegistry functionRegistry = new FunctionRegistry(true);
        functionRegistry.RegisterFunction("test", (Func<double, double, double>)((a, b) =>  a + b));

        AstBuilder astBuilder = new AstBuilder(functionRegistry, true);
        Operation operation = astBuilder.Build(tokens);

        Function optimizedFunction = (Function)optimizer.Optimize(operation, functionRegistry, null);

        Assert.AreEqual(typeof(FloatingPointConstant), optimizedFunction.Arguments[1].GetType());
    }

    [TestMethod]
    public void TestOptimizerNonIdempotentFunction()
    {
        Optimizer optimizer = new Optimizer(new Interpreter());

        TokenReader tokenReader = new TokenReader(CultureInfo.InvariantCulture);
        IList<Token> tokens = tokenReader.Read("test(500)");

        IFunctionRegistry functionRegistry = new FunctionRegistry(true);
        functionRegistry.RegisterFunction("test", (Func<double, double>)(a => a), false, true);

        AstBuilder astBuilder = new AstBuilder(functionRegistry, true);
        Operation operation = astBuilder.Build(tokens);

        Operation optimizedFunction = optimizer.Optimize(operation, functionRegistry, null);

        Assert.AreEqual(typeof(Function), optimizedFunction.GetType());
        Assert.AreEqual(typeof(IntegerConstant), ((Function)optimizedFunction).Arguments[0].GetType());
    }

    [TestMethod]
    public void TestOptimizerMultiplicationByZero()
    {
        Optimizer optimizer = new Optimizer(new Interpreter());

        TokenReader tokenReader = new TokenReader(CultureInfo.InvariantCulture);
        IList<Token> tokens = tokenReader.Read("var1 * 0.0");

        IFunctionRegistry functionRegistry = new FunctionRegistry(true);

        AstBuilder astBuilder = new AstBuilder(functionRegistry, true);
        Operation operation = astBuilder.Build(tokens);

        Operation optimizedOperation = optimizer.Optimize(operation, functionRegistry, null);

        Assert.AreEqual(typeof(FloatingPointConstant), optimizedOperation.GetType());
        Assert.AreEqual(0.0, ((FloatingPointConstant)optimizedOperation).Value);
    }
    
    
    [TestMethod]
    public void TestOptimizerDividendZero()
    {
        Optimizer optimizer = new Optimizer(new Interpreter());

        TokenReader tokenReader = new TokenReader(CultureInfo.InvariantCulture);
        IList<Token> tokens = tokenReader.Read("0 / var1");

        IFunctionRegistry functionRegistry = new FunctionRegistry(true);

        AstBuilder astBuilder = new AstBuilder(functionRegistry, true);
        Operation operation = astBuilder.Build(tokens);

        Operation optimizedOperation = optimizer.Optimize(operation, functionRegistry, null);

        Assert.AreEqual(typeof(FloatingPointConstant), optimizedOperation.GetType());
        Assert.AreEqual(0.0, ((FloatingPointConstant)optimizedOperation).Value);
    }
    
    [TestMethod]
    public void TestOptimizerCombined()
    {
        Optimizer optimizer = new Optimizer(new Interpreter());

        TokenReader tokenReader = new TokenReader(CultureInfo.InvariantCulture);
        IList<Token> tokens = tokenReader.Read("(var1 + var2 * var3 / 2) * 0 + 0 / (var1 + var2 * var3 / 2) + (var1 + var2 * var3 / 2)^0");

        IFunctionRegistry functionRegistry = new FunctionRegistry(true);

        AstBuilder astBuilder = new AstBuilder(functionRegistry, true);
        Operation operation = astBuilder.Build(tokens);

        Operation optimizedOperation = optimizer.Optimize(operation, functionRegistry, null);

        Assert.AreEqual(typeof(FloatingPointConstant), optimizedOperation.GetType());
        Assert.AreEqual(1.0, ((FloatingPointConstant)optimizedOperation).Value);
    }
    [TestMethod]
    public void TestOptimizerBaseZero()
    {
        Optimizer optimizer = new Optimizer(new Interpreter());

        TokenReader tokenReader = new TokenReader(CultureInfo.InvariantCulture);
        IList<Token> tokens = tokenReader.Read("0 ^ 2");

        IFunctionRegistry functionRegistry = new FunctionRegistry(true);

        AstBuilder astBuilder = new AstBuilder(functionRegistry, true);
        Operation operation = astBuilder.Build(tokens);

        Operation optimizedOperation = optimizer.Optimize(operation, functionRegistry, null);

        Assert.AreEqual(typeof(FloatingPointConstant), optimizedOperation.GetType());
        Assert.AreEqual(0.0, ((FloatingPointConstant)optimizedOperation).Value);
    }
    
    [TestMethod]
    public void TestOptimizerExponentZero()
    {
        Optimizer optimizer = new Optimizer(new Interpreter());

        TokenReader tokenReader = new TokenReader(CultureInfo.InvariantCulture);
        IList<Token> tokens = tokenReader.Read("0 ^ 0");

        IFunctionRegistry functionRegistry = new FunctionRegistry(true);

        AstBuilder astBuilder = new AstBuilder(functionRegistry, true);
        Operation operation = astBuilder.Build(tokens);

        Operation optimizedOperation = optimizer.Optimize(operation, functionRegistry, null);

        Assert.AreEqual(typeof(FloatingPointConstant), optimizedOperation.GetType());
        Assert.AreEqual(1.0, ((FloatingPointConstant)optimizedOperation).Value);
    }
}