using System.Collections.Generic;
using Adletec.Sonic.Execution;
using Adletec.Sonic.Operations;
using Adletec.Sonic.Parsing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Adletec.Sonic.Tests;

[TestClass]
public class VariableValidatorTest
{
    // todo test with constants to make refactorings safer
    
    [TestMethod]
    public void TestCompleteVariableDefinition()
    {
        var validator = new VariableValidator();
        var variables = new List<string> { "x", "y", "z" };
        var expression = "x + y + z";
        var ast = GetAst(expression);
        validator.Validate(ast, variables);
        // Assert doesn't throw
        Assert.IsTrue(true);
    }

    [TestMethod]
    public void TestIncompleteVariableDefinition()
    {
        var validator = new VariableValidator();
        var variables = new List<string> { "x", "y" };
        var expression = "x + y + z";
        var ast = GetAst(expression);
        try
        {
            validator.Validate(ast, variables);
        }
        catch (VariableNotDefinedException e)
        {
            Assert.AreEqual("Variable 'z' is not defined.", e.Message);
            Assert.AreEqual("z", e.VariableName);
            return;
        }

        Assert.Fail("Expected exception was not thrown.");
    }

    [TestMethod]
    public void TestFoldedVariableDefinition()
    {
        var validator = new VariableValidator();
        var variables = new List<string> { "x", "y" };
        var expression = "x + y + 0 * z";
        var ast = GetAst(expression, true);
        validator.Validate(ast, variables);
        // Assert doesn't throw
        Assert.IsTrue(true);
    }
    
    [TestMethod]
    public void TestUnfoldedVariableDefinition()
    {
        var validator = new VariableValidator();
        var variables = new List<string> { "x", "y" };
        var expression = "x + y + 0 * z";
        var ast = GetAst(expression);
        try
        {
            validator.Validate(ast, variables);
        }
        catch (VariableNotDefinedException e)
        {
            Assert.AreEqual("Variable 'z' is not defined.", e.Message);
            Assert.AreEqual("z", e.VariableName);
            return;
        }
        // Assert doesn't throw
        Assert.Fail("Expected exception was not thrown.");
    }

    static Operation GetAst(string expression, bool optimize = false)
    {
        var tokenList = new TokenReader().Read(expression);
        var functionRegistry = new FunctionRegistry(true, false);
        var constantRegistry = new ConstantRegistry(true, false);
        var ast = new AstBuilder(functionRegistry, constantRegistry).Build(tokenList);
        if (optimize)
        {
            ast = new Optimizer(new Interpreter(true, false)).Optimize(ast, functionRegistry, constantRegistry);
        }

        return ast;
    }
}