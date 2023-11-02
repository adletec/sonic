using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Adletec.Sonic.Tests;

[TestClass]
public class ParseValidationTests
{
    [TestMethod]
    public void TestValidateValidExpression()
    {
        var evaluator = Evaluator.CreateWithDefaults();
        var expression = "1 + 2 + sin(x) + 34";
        evaluator.Validate(expression);

        // does not throw
        Assert.IsTrue(true);
    }

    [TestMethod]
    public void TestValidateInvalidToken()
    {
        try
        {
            var evaluator = Evaluator.CreateWithDefaults();
            var expression = "a ! b";
            evaluator.Validate(expression);
        }
        catch (InvalidTokenParseException e)
        {
            Assert.AreEqual(2, e.TokenPosition);
            Assert.AreEqual(1, e.TokenLength);
            Assert.AreEqual("!", e.Token);
            return;
        }

        Assert.Fail("Expected exception not thrown");
    }

    [TestMethod]
    public void TestValidateInvalidFloatingPointNumberWithTwoCommas()
    {
        try
        {
            var evaluator = Evaluator.Create().UseCulture(CultureInfo.InvariantCulture).Build();
            var expression = "0.23748.2";
            evaluator.Validate(expression);
        }
        catch (InvalidFloatingPointNumberException e)
        {
            Assert.AreEqual(0, e.TokenPosition);
            Assert.AreEqual(9, e.TokenLength);
            Assert.AreEqual("0.23748.2", e.Token);
            return;
        }

        Assert.Fail("Expected exception not thrown");
    }

    [TestMethod]
    public void TestValidateMissingLeftBracket()
    {
        try
        {
            var evaluator = Evaluator.Create().UseCulture(CultureInfo.InvariantCulture).Build();
            var expression = "a + b)";
            evaluator.Validate(expression);
        }
        catch (MissingLeftBracketParseException e)
        {
            Assert.AreEqual(5, e.RightBracketPosition);
            return;
        }

        Assert.Fail("Expected exception not thrown");
    }

    [TestMethod]
    public void TestValidateMissingRightBracket()
    {
        try
        {
            var evaluator = Evaluator.Create().UseCulture(CultureInfo.InvariantCulture).Build();
            var expression = "a + (b";
            evaluator.Validate(expression);
        }
        catch (MissingRightBracketParseException e)
        {
            Assert.AreEqual(4, e.LeftBracketPosition);
            return;
        }

        Assert.Fail("Expected exception not thrown");
    }

    [TestMethod]
    public void TestValidateTooManyArguments()
    {
        try
        {
            var evaluator = Evaluator.CreateWithDefaults();
            var expression = "sin(a, b)";
            evaluator.Validate(expression);
        }
        catch (InvalidFunctionArgumentCountParseException e)
        {
            Assert.AreEqual("sin", e.FunctionName);
            Assert.AreEqual(0, e.FunctionNamePosition);
            Assert.AreEqual(3, e.FunctionNameLength);
            return;
        }

        Assert.Fail("Expected exception not thrown");
    }

    [TestMethod]
    public void TestValidateTooFewOfArguments()
    {
        try
        {
            var evaluator = Evaluator.CreateWithDefaults();
            var expression = "sin()";
            evaluator.Validate(expression);
        }
        catch (InvalidFunctionArgumentCountParseException e)
        {
            Assert.AreEqual("sin", e.FunctionName);
            Assert.AreEqual(0, e.FunctionNamePosition);
            Assert.AreEqual(3, e.FunctionNameLength);
            return;
        }

        Assert.Fail("Expected exception not thrown");
    }

    [TestMethod]
    public void TestValidateWrongPlaceOfArgumentsEmpty()
    {
        try
        {
            var evaluator = Evaluator.CreateWithDefaults();
            var expression = "a sin()";
            evaluator.Validate(expression);
        }
        catch (InvalidFunctionArgumentCountParseException e)
        {
            Assert.AreEqual("sin", e.FunctionName);
            Assert.AreEqual(2, e.FunctionNamePosition);
            Assert.AreEqual(3, e.FunctionNameLength);
            return;
        }

        Assert.Fail("Expected exception not thrown");
    }

    [TestMethod]
    public void TestValidateWrongPlaceOfArguments()
    {
        try
        {
            var evaluator = Evaluator.CreateWithDefaults();
            var expression = "a ifless(,b,c,d)";
            evaluator.Validate(expression);
        }
        catch (InvalidFunctionArgumentCountParseException e)
        {
            Assert.AreEqual("ifless", e.FunctionName);
            Assert.AreEqual(2, e.FunctionNamePosition);
            Assert.AreEqual(6, e.FunctionNameLength);
            return;
        }

        Assert.Fail("Expected exception not thrown");
    }

    [TestMethod]
    public void TestInvalidNumberOfOperationArguments()
    {
        try
        {
            var evaluator = Evaluator.CreateWithDefaults();
            var expression = "a +";
            evaluator.Validate(expression);
        }
        catch (MissingOperationArgumentParseException e)
        {
            Assert.AreEqual("+", e.Operator);
            Assert.AreEqual(2, e.OperatorPosition);
            return;
        }

        Assert.Fail("Expected exception not thrown");
    }


    [TestMethod]
    public void TestValidateUnexpectedDoubleOperators()
    {
        try
        {
            var evaluator = Evaluator.CreateWithDefaults();
            var expression = "1 2 3 + +";
            evaluator.Validate(expression);
        }
        catch (InvalidTokenParseException e)
        {
            Assert.AreEqual("2", e.Token);
            Assert.AreEqual(2, e.TokenPosition);
            Assert.AreEqual(1, e.TokenLength);
            return;
        }

        Assert.Fail("Expected exception not thrown");
    }

    [TestMethod]
    public void TestValidateUnexpectedConstantStart()
    {
        try
        {
            var evaluator = Evaluator.CreateWithDefaults();
            var expression = "1 2 + sin(x) + 34";
            evaluator.Validate(expression);
        }
        catch (InvalidTokenParseException e)
        {
            Assert.AreEqual("2", e.Token);
            Assert.AreEqual(2, e.TokenPosition);
            Assert.AreEqual(1, e.TokenLength);
            return;
        }

        Assert.Fail("Expected exception not thrown");
    }

    [TestMethod]
    public void TestValidateUnexpectedConstantEnd()
    {
        try
        {
            var evaluator = Evaluator.CreateWithDefaults();
            var expression = "1 + sin(x) 34";
            evaluator.Validate(expression);
        }
        catch (InvalidTokenParseException e)
        {
            Assert.AreEqual("34", e.Token);
            Assert.AreEqual(11, e.TokenPosition);
            Assert.AreEqual(2, e.TokenLength);
            return;
        }

        Assert.Fail("Expected exception not thrown");
    }

    [TestMethod]
    public void TestUnexpectedFloatingPointConstant()
    {
        try
        {
            var evaluator = Evaluator.Create().UseCulture(CultureInfo.InvariantCulture).Build();
            var expression = "1 + 3 34.4";
            evaluator.Validate(expression);
        }
        catch (InvalidTokenParseException e)
        {
            Assert.AreEqual("34.4", e.Token);
            Assert.AreEqual(11, e.TokenPosition);
            Assert.AreEqual(2, e.TokenLength);
            return;
        }

        Assert.Fail("Expected exception not thrown");
    }
}