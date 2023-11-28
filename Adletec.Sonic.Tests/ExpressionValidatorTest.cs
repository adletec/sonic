using System;
using System.Globalization;
using System.Linq;
using Adletec.Sonic.Execution;
using Adletec.Sonic.Parsing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Adletec.Sonic.Tests;

[TestClass]
public class ExpressionValidatorTest
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
        const string expression = "1 + 3 34.4";
        try
        {
            ValidateExpression(expression);
        }
        catch (InvalidTokenParseException e)
        {
            Assert.AreEqual("34.4", e.Token);
            Assert.AreEqual(6, e.TokenPosition);
            Assert.AreEqual(expression, e.Expression);
            return;
        }

        Assert.Fail("Expected exception not thrown");
    }

    [TestMethod]
    public void TestDoubleValue()
    {
        const string expression = "1 a * (sin(cos(3)) + ifless(1,2,3,4))";
        try
        {
            ValidateExpression(expression);
        }
        catch (InvalidTokenParseException e)
        {
            Assert.AreEqual("a", e.Token);
            Assert.AreEqual(2, e.TokenPosition);
            Assert.AreEqual(expression, e.Expression);
            return;
        }

        Assert.Fail("Exception not thrown.");
    }

    [TestMethod]
    public void TestArgumentSeparatorOutsideFunction()
    {
        const string expression = "1, a * (sin(cos(3)) + ifless(1,2,3,4))";
        try
        {
            ValidateExpression(expression);
        }
        catch (InvalidTokenParseException e)
        {
            Assert.AreEqual(",", e.Token);
            Assert.AreEqual(1, e.TokenPosition);
            Assert.AreEqual(expression, e.Expression);
            return;
        }

        Assert.Fail("Exception not thrown.");
    }

    [TestMethod]
    public void TestMissingArgument()
    {
        const string expression = "1 + a * (sin(cos(3)) + ifless(1,2,3))";
        try
        {
            ValidateExpression(expression);
        }
        catch (InvalidArgumentCountParseException e)
        {
            Assert.AreEqual("ifless", e.FunctionName);
            Assert.AreEqual(23, e.FunctionNamePosition);
            Assert.AreEqual(expression, e.Expression);
            return;
        }

        Assert.Fail("Exception not thrown.");
    }

    [TestMethod]
    public void TestTooManyArguments()
    {
        const string expression = "1 + a * (sin(cos(3)) + ifless(1,2,3,4,5))";
        try
        {
            ValidateExpression(expression);
        }
        catch (InvalidArgumentCountParseException e)
        {
            Assert.AreEqual("ifless", e.FunctionName);
            Assert.AreEqual(23, e.FunctionNamePosition);
            Assert.AreEqual(expression, e.Expression);
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
        const string expression = "* 1 + a * (sin(cos(3)) + ifless(1,2,3,4))";
        try
        {
            ValidateExpression(expression);
        }
        catch (MissingOperandParseException e)
        {
            Assert.AreEqual("*", e.Operator);
            Assert.AreEqual(0, e.OperatorPosition);
            Assert.AreEqual(expression, e.Expression);
            return;
        }

        Assert.Fail("Exception not thrown.");
    }

    [TestMethod]
    public void TestMissingOperatorArgument()
    {
        const string expression = "1 + * (sin(cos(3)) + ifless(1,2,3,4))";
        try
        {
            ValidateExpression(expression);
        }
        catch (MissingOperandParseException e)
        {
            Assert.AreEqual("+", e.Operator);
            Assert.AreEqual(2, e.OperatorPosition);
            Assert.AreEqual(expression, e.Expression);
            return;
        }

        Assert.Fail("Exception not thrown.");
    }

    [TestMethod]
    public void TestEmptyParentheses()
    {
        const string expression = "1 + a * ()";
        try
        {
            ValidateExpression(expression);
        }
        catch (InvalidTokenParseException e)
        {
            Assert.AreEqual(")", e.Token);
            Assert.AreEqual(9, e.TokenPosition);
            Assert.AreEqual(expression, e.Expression);
            return;
        }

        Assert.Fail("Exception not thrown.");
    }

    [TestMethod]
    public void TestMissingLeftParenthesis()
    {
        const string expression = "1 + a )";
        try
        {
            ValidateExpression(expression);
        }
        catch (MissingLeftParenthesisParseException e)
        {
            Assert.AreEqual(6, e.RightParenthesisPosition);
            Assert.AreEqual(expression, e.Expression);
            return;
        }

        Assert.Fail("Exception not thrown.");
    }

    [TestMethod]
    public void TestMissingRightParenthesis()
    {
        const string expression = "1 + (a * 2";
        try
        {
            ValidateExpression(expression);
        }
        catch (MissingRightParenthesisParseException e)
        {
            Assert.AreEqual(4, e.LeftParenthesisPosition);
            Assert.AreEqual(expression, e.Expression);
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
        const string expression = "1 + random(1)";
        try
        {
            ValidateExpression(expression);
        }
        catch (InvalidArgumentCountParseException e)
        {
            Assert.AreEqual("random", e.FunctionName);
            Assert.AreEqual(4, e.FunctionNamePosition);
            Assert.AreEqual(expression, e.Expression);
            return;
        }

        Assert.Fail("Exception not thrown.");
    }

    [TestMethod]
    public void TestRightParenthesisAsFirstToken()
    {
        const string expression = ")+1";
        try
        {
            ValidateExpression(expression);
        }
        catch (MissingLeftParenthesisParseException e)
        {
            Assert.AreEqual(0, e.RightParenthesisPosition);
            Assert.AreEqual(expression, e.Expression);
            return;
        }

        Assert.Fail("Exception not thrown.");
    }

    [TestMethod]
    public void TestRightParenthesisAfterFunctionName()
    {
        const string expression = "sin)";
        try
        {
            ValidateExpression(expression);
        }
        catch (MissingLeftParenthesisParseException e)
        {
            Assert.AreEqual(3, e.RightParenthesisPosition);
            Assert.AreEqual(expression, e.Expression);
            return;
        }

        Assert.Fail("Exception not thrown.");
    }

    [TestMethod]
    public void TestWrongPlaceOfArguments()
    {
        const string expression = "a ifless(,b,c,d)";
        try
        {
            ValidateExpression(expression);
        }
        catch (InvalidTokenParseException e)
        {
            Assert.AreEqual("ifless", e.Token);
            Assert.AreEqual(2, e.TokenPosition);
            Assert.AreEqual(expression, e.Expression);
            return;
        }

        Assert.Fail("Expected exception not thrown");
    }

    [TestMethod]
    public void TestWrongPlaceOfArgumentsEmpty()
    {
        const string expression = "a sin()";
        try
        {
            ValidateExpression(expression);
        }
        catch (InvalidTokenParseException e)
        {
            Assert.AreEqual("sin", e.Token);
            Assert.AreEqual(2, e.TokenPosition);
            Assert.AreEqual(expression, e.Expression);
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
        const string expression = "a +";
        try
        {
            ValidateExpression(expression);
        }
        catch (MissingOperandParseException e)
        {
            Assert.AreEqual("+", e.Operator);
            Assert.AreEqual(2, e.OperatorPosition);
            Assert.AreEqual(expression, e.Expression);
            return;
        }

        Assert.Fail("Expected exception not thrown");
    }

    [TestMethod]
    public void TestDoubleOperators()
    {
        const string expression = "1 2 3 + +";
        try
        {
            ValidateExpression(expression);
        }
        catch (InvalidTokenParseException e)
        {
            Assert.AreEqual("2", e.Token);
            Assert.AreEqual(2, e.TokenPosition);
            Assert.AreEqual(expression, e.Expression);
            return;
        }

        Assert.Fail("Expected exception not thrown");
    }

    [TestMethod]
    public void TestValidateInvalidToken()
    {
        const string expression = "a ! b";
        try
        {
            ValidateExpression(expression);
        }
        catch (InvalidTokenParseException e)
        {
            Assert.AreEqual(2, e.TokenPosition);
            Assert.AreEqual("!", e.Token);
            Assert.AreEqual(expression, e.Expression);
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
        
        const string expression = "max()";
        try
        {
            ValidateExpression(expression);
        }
        catch (InvalidArgumentCountParseException e)
        {
            Assert.AreEqual(0, e.FunctionNamePosition);
            Assert.AreEqual("max", e.FunctionName);
            Assert.AreEqual(expression, e.Expression);
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
    public void TestFunctionCallWithWhiteSpace()
    {
        ValidateExpression("ifless (0.57, (3000-500)/(1500-500), 10, 20)");
        // does not throw
        Assert.IsTrue(true);
    }
    
    
    [TestMethod]
    public void TestUnknownFunction()
    {
        const string expression = "foo(bar)";
        try
        {
            ValidateExpression(expression);
        }
        catch (UnknownFunctionParseException e)
        {
            Assert.AreEqual(0, e.FunctionNamePosition);
            Assert.AreEqual("foo", e.FunctionName);
            Assert.AreEqual(expression, e.Expression);
            return;
        }

        Assert.Fail("Expected exception not thrown");
    }
    
    [TestMethod]
    public void TestInvalidFloatingPointNumber()
    {
        const string expression = "123.456.78";
        try
        {
            ValidateExpression(expression);
        }
        catch (InvalidFloatingPointNumberParseException e)
        {
            Assert.AreEqual(0, e.TokenPosition);
            Assert.AreEqual("123.456.78", e.Token);
            Assert.AreEqual(expression, e.Expression);
            return;
        }

        Assert.Fail("Expected exception not thrown");
    }

    [TestMethod]
    public void TestArgumentWithUnaryMinus()
    {
        const string expression = "ifless(1, -2, 3, 4)";
        ValidateExpression(expression);
        Assert.IsTrue(true);
    }

    [TestMethod]
    public void TestArgumentWithBooleanExpression()
    {
        const string expression = "ifless(1==1,0, 3, 4)";
        ValidateExpression(expression);
        Assert.IsTrue(true);
    }


    private static void ValidateExpression(string expression)
    {
        var functionRegistry = GetPrefilledFunctionRegistry();
        var tokenParser = new TokenReader(CultureInfo.InvariantCulture, ',');
        var tokens = tokenParser.Read(expression);
        var validator = new ExpressionValidator(functionRegistry, CultureInfo.InvariantCulture);
        validator.Validate(tokens, expression);
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