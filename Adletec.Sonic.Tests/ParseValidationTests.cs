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
        catch (InvalidTokenParserException e)
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
        catch (MissingLeftBracketParserException e)
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
        catch (MissingRightBracketParserException e)
        {
            Assert.AreEqual(4, e.LeftBracketPosition);
            return;
        }
        Assert.Fail("Expected exception not thrown");
    }
    
    [TestMethod]
    public void TestValidateInvalidNumberOfArguments()
    {
        try
        {
            var evaluator = Evaluator.CreateWithDefaults();
            var expression = "sin(a, b)";
            evaluator.Validate(expression);
        }
        catch (InvalidNumberOfFunctionArgumentsParserException e)
        {
            Assert.AreEqual("sin", e.FunctionName);
            Assert.AreEqual(0, e.FunctionNamePosition);
            Assert.AreEqual(3, e.FunctionNameLength);
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
        catch (InvalidNumberOfOperationArgumentsParserException e)
        {
            Assert.AreEqual("+", e.Operator);
            Assert.AreEqual(2, e.OperatorPosition);
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
        catch (UnexpectedIntegerConstantParserException e)
        {
            Assert.AreEqual("1", e.Constant);
            Assert.AreEqual(0, e.ConstantPosition);
            Assert.AreEqual(1, e.ConstantLength);
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
        catch (UnexpectedIntegerConstantParserException e)
        {
            Assert.AreEqual("34", e.Constant);
            Assert.AreEqual(11, e.ConstantPosition);
            Assert.AreEqual(2, e.ConstantLength);
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
        catch (UnexpectedFloatingPointConstantParserException e)
        {
            Assert.AreEqual("34.4", e.Constant);
            Assert.AreEqual(11, e.ConstantPosition);
            Assert.AreEqual(2, e.ConstantLength);
            return;
        }
        Assert.Fail("Expected exception not thrown");
    }
    
}