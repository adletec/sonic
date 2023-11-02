using System;
using System.Globalization;
using Adletec.Sonic.Execution;
using Adletec.Sonic.Tokenizer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Adletec.Sonic.Tests;

[TestClass]
public class ValidatorTest
{
    [TestMethod]
    public void TestValidExpression()
    {
        var expression = "1 + 2 * (sin(cos(3)) + ifless(1,2,3,4))";
        var functionRegistry = GetPrefilledFunctionRegistry();
        var tokenParser = new TokenReader(CultureInfo.InvariantCulture, ',');
        var tokens = tokenParser.Read(expression);
        var validator = new Validator(functionRegistry);
        validator.Validate(tokens);
        // no exception
        Assert.IsTrue(true);
    }
    
    [TestMethod]
    public void TestFunctionsWithClausesAsArguments()
    {
        var expression = "1 + 2 * (sin(cos(3)) + ifless(1,(2+3),3,4))";
        var functionRegistry = GetPrefilledFunctionRegistry();
        var tokenParser = new TokenReader(CultureInfo.InvariantCulture, ',');
        var tokens = tokenParser.Read(expression);
        var validator = new Validator(functionRegistry);
        validator.Validate(tokens);
        // no exception
        Assert.IsTrue(true);
    }
    
    
    [TestMethod]
    public void TestDoubleValue()
    {
        var expression = "1 a * (sin(cos(3)) + ifless(1,2,3,4))";
        var functionRegistry = GetPrefilledFunctionRegistry();
        var tokenParser = new TokenReader(CultureInfo.InvariantCulture, ',');
        var tokens = tokenParser.Read(expression);
        var validator = new Validator(functionRegistry);
        try
        {
            validator.Validate(tokens);
        }
        catch (InvalidTokenParseException e)
        {
            Assert.AreEqual("a", e.Token);
            Assert.AreEqual(2, e.TokenPosition);
            Assert.AreEqual(1, e.TokenLength);
            return;
        }
        Assert.Fail("Exception not thrown.");
    }
    
    [TestMethod]
    public void TestArgumentSeparatorOutsideFunction()
    {
        var expression = "1, a * (sin(cos(3)) + ifless(1,2,3,4))";
        var functionRegistry = GetPrefilledFunctionRegistry();
        var tokenParser = new TokenReader(CultureInfo.InvariantCulture, ',');
        var tokens = tokenParser.Read(expression);
        var validator = new Validator(functionRegistry);
        try
        {
            validator.Validate(tokens);
        }
        catch (InvalidTokenParseException e)
        {
            Assert.AreEqual(",", e.Token);
            Assert.AreEqual(1, e.TokenPosition);
            Assert.AreEqual(1, e.TokenLength);
            return;
        }
        Assert.Fail("Exception not thrown.");
    }


    [TestMethod]
    public void TestMissingArgument()
    {
        var expression = "1 + a * (sin(cos(3)) + ifless(1,2,3))";
        var functionRegistry = GetPrefilledFunctionRegistry();
        var tokenParser = new TokenReader(CultureInfo.InvariantCulture, ',');
        var tokens = tokenParser.Read(expression);
        var validator = new Validator(functionRegistry);
        try
        {
            validator.Validate(tokens);
        }
        catch (InvalidFunctionArgumentCountParseException e)
        {
            Assert.AreEqual("ifless", e.FunctionName);
            Assert.AreEqual(23, e.FunctionNamePosition);
            Assert.AreEqual(6, e.FunctionNameLength);
            return;
        }
        Assert.Fail("Exception not thrown.");
    }

    [TestMethod]
    public void TestTooManyArguments()
    {
        var expression = "1 + a * (sin(cos(3)) + ifless(1,2,3,4,5))";
        var functionRegistry = GetPrefilledFunctionRegistry();
        var tokenParser = new TokenReader(CultureInfo.InvariantCulture, ',');
        var tokens = tokenParser.Read(expression);
        var validator = new Validator(functionRegistry);
        try
        {
            validator.Validate(tokens);
        }
        catch (InvalidFunctionArgumentCountParseException e)
        {
            Assert.AreEqual("ifless", e.FunctionName);
            Assert.AreEqual(23, e.FunctionNamePosition);
            Assert.AreEqual(6, e.FunctionNameLength);
            return;
        }
        Assert.Fail("Exception not thrown.");
    }

    [TestMethod]
    public void TestStartWithUnaryMinus()
    {
        var expression = "-1 + a * (sin(cos(3)) + ifless(1,2,3,4))";
        var functionRegistry = GetPrefilledFunctionRegistry();
        var tokenParser = new TokenReader(CultureInfo.InvariantCulture, ',');
        var tokens = tokenParser.Read(expression);
        var validator = new Validator(functionRegistry);
        validator.Validate(tokens);
        // assert no exception
        Assert.IsTrue(true);
    }

    [TestMethod]
    public void TestStartWithOperator()
    {
        var expression = "* 1 + a * (sin(cos(3)) + ifless(1,2,3,4))";
        var functionRegistry = GetPrefilledFunctionRegistry();
        var tokenParser = new TokenReader(CultureInfo.InvariantCulture, ',');
        var tokens = tokenParser.Read(expression);
        var validator = new Validator(functionRegistry);
        try
        {
            validator.Validate(tokens);
        }
        catch (MissingOperationArgumentParseException e)
        {
            Assert.AreEqual("*", e.Operator);
            Assert.AreEqual(0, e.OperatorPosition);
            return;
        }
        Assert.Fail("Exception not thrown.");
    }

    [TestMethod]
    public void TestMissingOperatorArgument()
    {
        var expression = "1 + * (sin(cos(3)) + ifless(1,2,3,4))";
        var functionRegistry = GetPrefilledFunctionRegistry();
        var tokenParser = new TokenReader(CultureInfo.InvariantCulture, ',');
        var tokens = tokenParser.Read(expression);
        var validator = new Validator(functionRegistry);
        try
        {
            validator.Validate(tokens);
        }
        catch (MissingOperationArgumentParseException e)
        {
            Assert.AreEqual("+", e.Operator);
            Assert.AreEqual(2, e.OperatorPosition);
            return;
        }
        Assert.Fail("Exception not thrown.");
    }

    [TestMethod]
    public void TestEmptyBrackets()
    {
        var expression = "1 + a * ()";
        var functionRegistry = GetPrefilledFunctionRegistry();
        var tokenParser = new TokenReader(CultureInfo.InvariantCulture, ',');
        var tokens = tokenParser.Read(expression);
        var validator = new Validator(functionRegistry);
        try
        {
            validator.Validate(tokens);
        }
        catch (InvalidTokenParseException e)
        {
            Assert.AreEqual(")", e.Token);
            Assert.AreEqual(9, e.TokenPosition);
            Assert.AreEqual(1, e.TokenLength);
            return;
        }
        Assert.Fail("Exception not thrown.");
    }

    [TestMethod]
    public void TestMissingLeftBracket()
    {
        var expression = "1 + a )";
        var functionRegistry = GetPrefilledFunctionRegistry();
        var tokenParser = new TokenReader(CultureInfo.InvariantCulture, ',');
        var tokens = tokenParser.Read(expression);
        var validator = new Validator(functionRegistry);
        try
        {
            validator.Validate(tokens);
        }
        catch (MissingLeftBracketParseException e)
        {
            Assert.AreEqual(6, e.RightBracketPosition);
            return;
        }
        Assert.Fail("Exception not thrown.");
    }

    [TestMethod]
    public void TestMissingRightBracket()
    {
        var expression = "1 + (a * 2";
        var functionRegistry = GetPrefilledFunctionRegistry();
        var tokenParser = new TokenReader(CultureInfo.InvariantCulture, ',');
        var tokens = tokenParser.Read(expression);
        var validator = new Validator(functionRegistry);
        try
        {
            validator.Validate(tokens);
        }
        catch (MissingRightBracketParseException e)
        {
            Assert.AreEqual(4, e.LeftBracketPosition);
            return;
        }
        Assert.Fail("Exception not thrown.");
    }

    [TestMethod]
    public void TestParameterlessMethod()
    {
        var expression = "1 + random()";
        var functionRegistry = GetPrefilledFunctionRegistry();
        var tokenParser = new TokenReader(CultureInfo.InvariantCulture, ',');
        var tokens = tokenParser.Read(expression);
        var validator = new Validator(functionRegistry);
        validator.Validate(tokens);
        // assert no exception
        Assert.IsTrue(true);
    }
    
    [TestMethod]
    public void TestParameterlessFunctionWithParameter()
    {
        var expression = "1 + random(1)";
        var functionRegistry = GetPrefilledFunctionRegistry();
        var tokenParser = new TokenReader(CultureInfo.InvariantCulture, ',');
        var tokens = tokenParser.Read(expression);
        var validator = new Validator(functionRegistry);
        try
        {
            validator.Validate(tokens);
        }
        catch (InvalidFunctionArgumentCountParseException e)
        {
            Assert.AreEqual("random", e.FunctionName);
            Assert.AreEqual(4, e.FunctionNamePosition);
            Assert.AreEqual(6, e.FunctionNameLength);
            return;
        }
        Assert.Fail("Exception not thrown.");
    }

    [TestMethod]
    public void TestRightBracketAsFirstToken()
    {
        var expression = ")+1";
        var functionRegistry = GetPrefilledFunctionRegistry();
        var tokenParser = new TokenReader(CultureInfo.InvariantCulture, ',');
        var tokens = tokenParser.Read(expression);
        var validator = new Validator(functionRegistry);
        try
        {
            validator.Validate(tokens);
        }
        catch (MissingLeftBracketParseException e)
        {
            Assert.AreEqual(0, e.RightBracketPosition);
            return;
        }
        Assert.Fail("Exception not thrown.");
    }
    
    [TestMethod]
    public void TestRightBracketAfterFunctionName()
    {
        var expression = "sin)";
        var functionRegistry = GetPrefilledFunctionRegistry();
        var tokenParser = new TokenReader(CultureInfo.InvariantCulture, ',');
        var tokens = tokenParser.Read(expression);
        var validator = new Validator(functionRegistry);
        try
        {
            validator.Validate(tokens);
        }
        catch (MissingLeftBracketParseException e)
        {
            Assert.AreEqual(3, e.RightBracketPosition);
            return;
        }
        Assert.Fail("Exception not thrown.");
    }


    private static FunctionRegistry GetPrefilledFunctionRegistry()
    {
        var functionRegistry = new FunctionRegistry(false, false);
        functionRegistry.RegisterFunction("sin", (Func<double, double>)Math.Sin, true);
        functionRegistry.RegisterFunction("cos", (Func<double, double>)Math.Cos, true);
        functionRegistry.RegisterFunction("ifless",
            (Func<double, double, double, double, double>)((a, b, c, d) => a < b ? c : d), true);
        functionRegistry.RegisterFunction("random", (Func<double>)new Random(0).NextDouble, false);
        return functionRegistry;
    }
}