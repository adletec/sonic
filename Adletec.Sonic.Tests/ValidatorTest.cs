using System;
using System.Globalization;
using System.Linq;
using Adletec.Sonic.Execution;
using Adletec.Sonic.Parsing;
using Adletec.Sonic.Parsing.Tokenizing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Adletec.Sonic.Tests;

[TestClass]
public class ValidatorTest
{
    [TestMethod]
    public void TestValidExpression()
    {
        ValidateExpression("1 + 2 * (sin(cos(3)) + ifless(1,2,3,4))");
        // no exception
        Assert.IsTrue(true);
    }

    [TestMethod]
    public void TestFunctionsWithClausesAsArguments()
    {
        ValidateExpression("1 + 2 * (sin(cos(3)) + ifless(1,(2+3),3,4))");
        // no exception
        Assert.IsTrue(true);
    }

    [TestMethod]
    public void TestUnexpectedFloatingPointConstant()
    {
        try
        {
            ValidateExpression("1 + 3 34.4");
        }
        catch (InvalidTokenParseException e)
        {
            Assert.AreEqual("34.4", e.Token);
            Assert.AreEqual(6, e.TokenPosition);
            return;
        }

        Assert.Fail("Expected exception not thrown");
    }

    [TestMethod]
    public void TestDoubleValue()
    {
        try
        {
            ValidateExpression("1 a * (sin(cos(3)) + ifless(1,2,3,4))");
        }
        catch (InvalidTokenParseException e)
        {
            Assert.AreEqual("a", e.Token);
            Assert.AreEqual(2, e.TokenPosition);
            return;
        }

        Assert.Fail("Exception not thrown.");
    }

    [TestMethod]
    public void TestArgumentSeparatorOutsideFunction()
    {
        try
        {
            ValidateExpression("1, a * (sin(cos(3)) + ifless(1,2,3,4))");
        }
        catch (InvalidTokenParseException e)
        {
            Assert.AreEqual(",", e.Token);
            Assert.AreEqual(1, e.TokenPosition);
            return;
        }

        Assert.Fail("Exception not thrown.");
    }

    [TestMethod]
    public void TestMissingArgument()
    {
        try
        {
            ValidateExpression("1 + a * (sin(cos(3)) + ifless(1,2,3))");
        }
        catch (InvalidArgumentCountParseException e)
        {
            Assert.AreEqual("ifless", e.FunctionName);
            Assert.AreEqual(23, e.FunctionNamePosition);
            return;
        }

        Assert.Fail("Exception not thrown.");
    }

    [TestMethod]
    public void TestTooManyArguments()
    {
        try
        {
            ValidateExpression("1 + a * (sin(cos(3)) + ifless(1,2,3,4,5))");
        }
        catch (InvalidArgumentCountParseException e)
        {
            Assert.AreEqual("ifless", e.FunctionName);
            Assert.AreEqual(23, e.FunctionNamePosition);
            return;
        }

        Assert.Fail("Exception not thrown.");
    }

    [TestMethod]
    public void TestStartWithUnaryMinus()
    {
        ValidateExpression("-1 + a * (sin(cos(3)) + ifless(1,2,3,4))");
        // assert no exception
        Assert.IsTrue(true);
    }

    [TestMethod]
    public void TestStartWithOperator()
    {
        try
        {
            ValidateExpression("* 1 + a * (sin(cos(3)) + ifless(1,2,3,4))");
        }
        catch (MissingOperandParseException e)
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
        try
        {
            ValidateExpression("1 + * (sin(cos(3)) + ifless(1,2,3,4))");
        }
        catch (MissingOperandParseException e)
        {
            Assert.AreEqual("+", e.Operator);
            Assert.AreEqual(2, e.OperatorPosition);
            return;
        }

        Assert.Fail("Exception not thrown.");
    }

    [TestMethod]
    public void TestEmptyParentheses()
    {
        try
        {
            ValidateExpression("1 + a * ()");
        }
        catch (InvalidTokenParseException e)
        {
            Assert.AreEqual(")", e.Token);
            Assert.AreEqual(9, e.TokenPosition);
            return;
        }

        Assert.Fail("Exception not thrown.");
    }

    [TestMethod]
    public void TestMissingLeftParenthesis()
    {
        try
        {
            ValidateExpression("1 + a )");
        }
        catch (MissingLeftParenthesisParseException e)
        {
            Assert.AreEqual(6, e.RightParenthesisPosition);
            return;
        }

        Assert.Fail("Exception not thrown.");
    }

    [TestMethod]
    public void TestMissingRightParenthesis()
    {
        try
        {
            ValidateExpression("1 + (a * 2");
        }
        catch (MissingRightParenthesisParseException e)
        {
            Assert.AreEqual(4, e.LeftParenthesisPosition);
            return;
        }

        Assert.Fail("Exception not thrown.");
    }

    [TestMethod]
    public void TestParameterlessMethod()
    {
        ValidateExpression("1 + random()");
        // assert no exception
        Assert.IsTrue(true);
    }

    [TestMethod]
    public void TestParameterlessFunctionWithParameter()
    {
        try
        {
            ValidateExpression("1 + random(1)");
        }
        catch (InvalidArgumentCountParseException e)
        {
            Assert.AreEqual("random", e.FunctionName);
            Assert.AreEqual(4, e.FunctionNamePosition);
            return;
        }

        Assert.Fail("Exception not thrown.");
    }

    [TestMethod]
    public void TestRightParenthesisAsFirstToken()
    {
        try
        {
            ValidateExpression(")+1");
        }
        catch (MissingLeftParenthesisParseException e)
        {
            Assert.AreEqual(0, e.RightParenthesisPosition);
            return;
        }

        Assert.Fail("Exception not thrown.");
    }

    [TestMethod]
    public void TestRightParenthesisAfterFunctionName()
    {
        try
        {
            ValidateExpression("sin)");
        }
        catch (MissingLeftParenthesisParseException e)
        {
            Assert.AreEqual(3, e.RightParenthesisPosition);
            return;
        }

        Assert.Fail("Exception not thrown.");
    }

    [TestMethod]
    public void TestWrongPlaceOfArguments()
    {
        try
        {
            ValidateExpression("a ifless(,b,c,d)");
        }
        catch (InvalidTokenParseException e)
        {
            Assert.AreEqual("ifless", e.Token);
            Assert.AreEqual(2, e.TokenPosition);
            return;
        }

        Assert.Fail("Expected exception not thrown");
    }

    [TestMethod]
    public void TestWrongPlaceOfArgumentsEmpty()
    {
        try
        {
            ValidateExpression("a sin()");
        }
        catch (InvalidTokenParseException e)
        {
            Assert.AreEqual("sin", e.Token);
            Assert.AreEqual(2, e.TokenPosition);
            return;
        }

        Assert.Fail("Expected exception not thrown");
    }

    [TestMethod]
    public void TestSingleDigitExpression()
    {
        ValidateExpression("1");
        // Assert does not throw
        Assert.IsTrue(true);
    }

    [TestMethod]
    public void TestInvalidNumberOfOperationArguments()
    {
        try
        {
            ValidateExpression("a +");
        }
        catch (MissingOperandParseException e)
        {
            Assert.AreEqual("+", e.Operator);
            Assert.AreEqual(2, e.OperatorPosition);
            return;
        }

        Assert.Fail("Expected exception not thrown");
    }

    [TestMethod]
    public void TestDoubleOperators()
    {
        try
        {
            ValidateExpression("1 2 3 + +");
        }
        catch (InvalidTokenParseException e)
        {
            Assert.AreEqual("2", e.Token);
            Assert.AreEqual(2, e.TokenPosition);
            return;
        }

        Assert.Fail("Expected exception not thrown");
    }

    [TestMethod]
    public void TestValidateInvalidToken()
    {
        try
        {
            ValidateExpression("a ! b");
        }
        catch (InvalidTokenParseException e)
        {
            Assert.AreEqual(2, e.TokenPosition);
            Assert.AreEqual("!", e.Token);
            return;
        }

        Assert.Fail("Expected exception not thrown");
    }

    [TestMethod]
    public void TestExpressionStartingWithLeftParenthesis()
    {
        ValidateExpression("(-3)^2");
        // does not throw
        Assert.IsTrue(true);
    }

    [TestMethod]
    public void TestValidateUnaryAfterBinaryOperatorToken()
    {
        ValidateExpression("a--b");
        // does not throw
        Assert.IsTrue(true);
    }

    [TestMethod]
    public void TestDynamicFunctions()
    {
        ValidateExpression("max(sin(67), cos(67))");
        // does not throw
        Assert.IsTrue(true);
    }
    
    [TestMethod]
    public void TestDynamicFunctionsWithoutArgument()
    {
        
        try
        {
            ValidateExpression("max()");
        }
        catch (InvalidArgumentCountParseException e)
        {
            Assert.AreEqual(0, e.FunctionNamePosition);
            Assert.AreEqual("max", e.FunctionName);
            return;
        }

        Assert.Fail("Expected exception not thrown");
    }
    
    
    [TestMethod]
    public void TestParenthesisedOperationsInFunctions()
    {
        ValidateExpression("ifless(0.57, (3000-500)/(1500-500), 10, 20)");
        // does not throw
        Assert.IsTrue(true);
    }
    
    [TestMethod]
    public void TestUnknownFunction()
    {
        try
        {
            ValidateExpression("foo(bar)");
        }
        catch (UnknownFunctionParseException e)
        {
            Assert.AreEqual(0, e.FunctionNamePosition);
            Assert.AreEqual("foo", e.FunctionName);
            return;
        }

        Assert.Fail("Expected exception not thrown");
    }
    
    [TestMethod]
    public void TestInvalidFloatingPointNumber()
    {
        try
        {
            ValidateExpression("123.456.78");
        }
        catch (InvalidFloatingPointNumberParseException e)
        {
            Assert.AreEqual(0, e.TokenPosition);
            Assert.AreEqual("123.456.78", e.Token);
            return;
        }

        Assert.Fail("Expected exception not thrown");
    }


    private static void ValidateExpression(string expression)
    {
        var functionRegistry = GetPrefilledFunctionRegistry();
        var tokenParser = new TokenReader(CultureInfo.InvariantCulture, ',');
        var tokens = tokenParser.Read(expression);
        var validator = new Validator(functionRegistry, CultureInfo.InvariantCulture);
        validator.Validate(tokens);
    }

    private static FunctionRegistry GetPrefilledFunctionRegistry()
    {
        var functionRegistry = new FunctionRegistry(false, false);
        functionRegistry.RegisterFunction("sin", (Func<double, double>)Math.Sin, true);
        functionRegistry.RegisterFunction("cos", (Func<double, double>)Math.Cos, true);
        functionRegistry.RegisterFunction("ifless",
            (Func<double, double, double, double, double>)((a, b, c, d) => a < b ? c : d), true);
        functionRegistry.RegisterFunction("random", (Func<double>)new Random(0).NextDouble, false);
        functionRegistry.RegisterFunction("max", (DynamicFunc<double, double>)(a => a.Max()), true);
        return functionRegistry;
    }
}