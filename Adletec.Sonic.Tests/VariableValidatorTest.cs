using System;
using System.Collections.Generic;
using Adletec.Sonic.Execution;
using Adletec.Sonic.Operations;
using Adletec.Sonic.Parsing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Adletec.Sonic.Tests;

[TestClass]
public class VariableValidatorTest
{
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
    
    [TestMethod]
    public void TestCompleteVariableDefinitionWithConstant()
    {
        var validator = new VariableValidator();
        var variables = new List<string> { "x", "y" };
        var expression = "constant + x + y";
        var ast = GetAst(expression, true);
        validator.Validate(ast, variables);
        // Assert doesn't throw
        Assert.IsTrue(true);
    }

    [TestMethod]
    public void TestCompleteVariableDefinitionWithDifferentOperators()
    {
        var validator = new VariableValidator();
        var variables = new List<string> { "x", "y", "z" };
        var expression = "-constant + sin(x) + y / 4 * x - 12^z";
        var ast = GetAst(expression, true);
        validator.Validate(ast, variables);
        // Assert doesn't throw
        Assert.IsTrue(true);
    }

    static Operation GetAst(string expression, bool optimize = false)
    {
        var tokenList = new TokenReader().Read(expression);
        var functionRegistry = new FunctionRegistry(true, false);
        functionRegistry.RegisterFunction("sin", (Func<double, double>)Math.Sin);
        var constantRegistry = new ConstantRegistry(true, false);
        constantRegistry.RegisterConstant("constant", 1.0);
        var ast = new AstBuilder(functionRegistry, constantRegistry).Build(tokenList);
        if (optimize)
        {
            ast = new Optimizer(new Interpreter(true, false)).Optimize(ast, functionRegistry, constantRegistry);
        }

        return ast;
    }
}