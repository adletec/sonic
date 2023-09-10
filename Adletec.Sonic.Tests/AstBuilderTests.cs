#if NETFX_CORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#elif __ANDROID__
using NUnit.Framework;
using TestClass = NUnit.Framework.TestFixtureAttribute;
using TestMethod = NUnit.Framework.TestAttribute;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif
using System.Collections.Generic;
using System.Linq;
using Adletec.Sonic.Execution;
using Adletec.Sonic.Operations;
using Adletec.Sonic.Tests.Mocks;
using Adletec.Sonic.Tokenizer;

namespace Adletec.Sonic.Tests;

[TestClass]
public class AstBuilderTests
{
    [TestMethod]
    public void TestBuildFormula1()
    {
        IFunctionRegistry registry = new MockFunctionRegistry();

        var builder = new AstBuilder(registry, new ConstantRegistry(false, false));
        var operation = builder.Build(new List<Token>
        {
            new() { Value = '(', TokenType = TokenType.LeftBracket },
            new() { Value = 42, TokenType = TokenType.Integer },
            new() { Value = '+', TokenType = TokenType.Operation },
            new() { Value = 8, TokenType = TokenType.Integer },
            new() { Value = ')', TokenType = TokenType.RightBracket },
            new() { Value = '*', TokenType = TokenType.Operation },
            new() { Value = 2, TokenType = TokenType.Integer }
        });

        var multiplication = (Multiplication)operation;
        var addition = (Addition)multiplication.Argument1;

        Assert.AreEqual(42, ((Constant<int>)addition.Argument1).Value);
        Assert.AreEqual(8, ((Constant<int>)addition.Argument2).Value);
        Assert.AreEqual(2, ((Constant<int>)multiplication.Argument2).Value);
    }

    [TestMethod]
    public void TestBuildFormula2()
    {
        IFunctionRegistry registry = new MockFunctionRegistry();

        var builder = new AstBuilder(registry, new ConstantRegistry(false, false));
        var operation = builder.Build(new List<Token>
        {
            new() { Value = 2, TokenType = TokenType.Integer },
            new() { Value = '+', TokenType = TokenType.Operation },
            new() { Value = 8, TokenType = TokenType.Integer },
            new() { Value = '*', TokenType = TokenType.Operation },
            new() { Value = 3, TokenType = TokenType.Integer }
        });

        Addition addition = (Addition)operation;
        Multiplication multiplication = (Multiplication)addition.Argument2;

        Assert.AreEqual(2, ((Constant<int>)addition.Argument1).Value);
        Assert.AreEqual(8, ((Constant<int>)multiplication.Argument1).Value);
        Assert.AreEqual(3, ((Constant<int>)multiplication.Argument2).Value);
    }

    [TestMethod]
    public void TestBuildFormula3()
    {
        IFunctionRegistry registry = new MockFunctionRegistry();

        var builder = new AstBuilder(registry, new ConstantRegistry(false, false));
        var operation = builder.Build(new List<Token>
        {
            new() { Value = 2, TokenType = TokenType.Integer },
            new() { Value = '*', TokenType = TokenType.Operation },
            new() { Value = 8, TokenType = TokenType.Integer },
            new() { Value = '-', TokenType = TokenType.Operation },
            new() { Value = 3, TokenType = TokenType.Integer }
        });

        var subtraction = (Subtraction)operation;
        var multiplication = (Multiplication)subtraction.Argument1;

        Assert.AreEqual(3, ((Constant<int>)subtraction.Argument2).Value);
        Assert.AreEqual(2, ((Constant<int>)multiplication.Argument1).Value);
        Assert.AreEqual(8, ((Constant<int>)multiplication.Argument2).Value);
    }

    [TestMethod]
    public void TestDivision()
    {
        IFunctionRegistry registry = new MockFunctionRegistry();

        var builder = new AstBuilder(registry, new ConstantRegistry(false, false));
        var operation = builder.Build(new List<Token>
        {
            new() { Value = 10, TokenType = TokenType.Integer },
            new() { Value = '/', TokenType = TokenType.Operation },
            new() { Value = 2, TokenType = TokenType.Integer }
        });

        Assert.AreEqual(typeof(Division), operation.GetType());

        var division = (Division)operation;

        Assert.AreEqual(new IntegerConstant(10), division.Dividend);
        Assert.AreEqual(new IntegerConstant(2), division.Divisor);
    }

    [TestMethod]
    public void TestMultiplication()
    {
        IFunctionRegistry registry = new MockFunctionRegistry();

        var builder = new AstBuilder(registry, new ConstantRegistry(false, false));
        var operation = builder.Build(new List<Token>
        {
            new() { Value = 10, TokenType = TokenType.Integer },
            new() { Value = '*', TokenType = TokenType.Operation },
            new() { Value = 2.0, TokenType = TokenType.FloatingPoint }
        });

        var multiplication = (Multiplication)operation;

        Assert.AreEqual(new IntegerConstant(10), multiplication.Argument1);
        Assert.AreEqual(new FloatingPointConstant(2.0), multiplication.Argument2);
    }

    [TestMethod]
    public void TestExponentiation()
    {
        IFunctionRegistry registry = new MockFunctionRegistry();

        var builder = new AstBuilder(registry, new ConstantRegistry(false, false));
        var operation = builder.Build(new List<Token>
        {
            new() { Value = 2, TokenType = TokenType.Integer },
            new() { Value = '^', TokenType = TokenType.Operation },
            new() { Value = 3, TokenType = TokenType.Integer }
        });

        var exponentiation = (Exponentiation)operation;

        Assert.AreEqual(new IntegerConstant(2), exponentiation.Base);
        Assert.AreEqual(new IntegerConstant(3), exponentiation.Exponent);
    }

    [TestMethod]
    public void TestModulo()
    {
        IFunctionRegistry registry = new MockFunctionRegistry();

        var builder = new AstBuilder(registry, new ConstantRegistry(false, false));
        var operation = builder.Build(new List<Token>
        {
            new() { Value = 2.7, TokenType = TokenType.FloatingPoint },
            new() { Value = '%', TokenType = TokenType.Operation },
            new() { Value = 3, TokenType = TokenType.Integer }
        });

        var modulo = (Modulo)operation;

        Assert.AreEqual(new FloatingPointConstant(2.7), modulo.Dividend);
        Assert.AreEqual(new IntegerConstant(3), modulo.Divisor);
    }

    [TestMethod]
    public void TestVariable()
    {
        IFunctionRegistry registry = new MockFunctionRegistry();

        var builder = new AstBuilder(registry, new ConstantRegistry(false, false));
        var operation = builder.Build(new List<Token>
        {
            new() { Value = 10, TokenType = TokenType.Integer },
            new() { Value = '*', TokenType = TokenType.Operation },
            new() { Value = "var1", TokenType = TokenType.Text }
        });

        var multiplication = (Multiplication)operation;

        Assert.AreEqual(new IntegerConstant(10), multiplication.Argument1);
        Assert.AreEqual(new Variable("var1"), multiplication.Argument2);
    }

    [TestMethod]
    public void TestMultipleVariable()
    {
        IFunctionRegistry registry = new MockFunctionRegistry();

        var builder = new AstBuilder(registry, new ConstantRegistry(false, false));
        var operation = builder.Build(new List<Token>()
        {
            new() { Value = "var1", TokenType = TokenType.Text },
            new() { Value = '+', TokenType = TokenType.Operation },
            new() { Value = 2, TokenType = TokenType.Integer },
            new() { Value = '*', TokenType = TokenType.Operation },
            new() { Value = '(', TokenType = TokenType.LeftBracket },
            new() { Value = 3, TokenType = TokenType.Integer },
            new() { Value = '*', TokenType = TokenType.Operation },
            new() { Value = "age", TokenType = TokenType.Text },
            new() { Value = ')', TokenType = TokenType.RightBracket }
        });

        var addition = (Addition)operation;
        var multiplication1 = (Multiplication)addition.Argument2;
        var multiplication2 = (Multiplication)multiplication1.Argument2;

        Assert.AreEqual(new Variable("var1"), addition.Argument1);
        Assert.AreEqual(new IntegerConstant(2), multiplication1.Argument1);
        Assert.AreEqual(new IntegerConstant(3), multiplication2.Argument1);
        Assert.AreEqual(new Variable("age"), multiplication2.Argument2);
    }

    [TestMethod]
    public void TestSinFunction1()
    {
        IFunctionRegistry registry = new MockFunctionRegistry();

        var builder = new AstBuilder(registry, new ConstantRegistry(false, false));
        var operation = builder.Build(new List<Token>
        {
            new() { Value = "sin", TokenType = TokenType.Text },
            new() { Value = '(', TokenType = TokenType.LeftBracket },
            new() { Value = 2, TokenType = TokenType.Integer },
            new() { Value = ')', TokenType = TokenType.RightBracket }
        });

        var sineFunction = (Function)operation;
        Assert.AreEqual(new IntegerConstant(2), sineFunction.Arguments.Single());
    }

    [TestMethod]
    public void TestSinFunction2()
    {
        IFunctionRegistry registry = new MockFunctionRegistry();

        var builder = new AstBuilder(registry, new ConstantRegistry(false, false));
        var operation = builder.Build(new List<Token>
        {
            new() { Value = "sin", TokenType = TokenType.Text },
            new() { Value = '(', TokenType = TokenType.LeftBracket },
            new() { Value = 2, TokenType = TokenType.Integer },
            new() { Value = '+', TokenType = TokenType.Operation },
            new() { Value = 3, TokenType = TokenType.Integer },
            new() { Value = ')', TokenType = TokenType.RightBracket }
        });

        var sineFunction = (Function)operation;

        var addition = (Addition)sineFunction.Arguments.Single();
        Assert.AreEqual(new IntegerConstant(2), addition.Argument1);
        Assert.AreEqual(new IntegerConstant(3), addition.Argument2);
    }

    [TestMethod]
    public void TestSinFunction3()
    {
        IFunctionRegistry registry = new MockFunctionRegistry();

        var builder = new AstBuilder(registry, new ConstantRegistry(false, false));
        var operation = builder.Build(new List<Token>
        {
            new() { Value = "sin", TokenType = TokenType.Text },
            new() { Value = '(', TokenType = TokenType.LeftBracket },
            new() { Value = 2, TokenType = TokenType.Integer },
            new() { Value = '+', TokenType = TokenType.Operation },
            new() { Value = 3, TokenType = TokenType.Integer },
            new() { Value = ')', TokenType = TokenType.RightBracket },
            new() { Value = '*', TokenType = TokenType.Operation },
            new() { Value = 4.9, TokenType = TokenType.FloatingPoint }
        });

        var multiplication = (Multiplication)operation;

        var sineFunction = (Function)multiplication.Argument1;

        var addition = (Addition)sineFunction.Arguments.Single();
        Assert.AreEqual(new IntegerConstant(2), addition.Argument1);
        Assert.AreEqual(new IntegerConstant(3), addition.Argument2);

        Assert.AreEqual(new FloatingPointConstant(4.9), multiplication.Argument2);
    }

    [TestMethod]
    public void TestUnaryMinus1()
    {
        IFunctionRegistry registry = new MockFunctionRegistry();

        var builder = new AstBuilder(registry, new ConstantRegistry(false, false));
        var operation = builder.Build(new List<Token>
        {
            new() { Value = 5.3, TokenType = TokenType.FloatingPoint },
            new() { Value = '*', TokenType = TokenType.Operation },
            new() { Value = '_', TokenType = TokenType.Operation },
            new() { Value = '(', TokenType = TokenType.LeftBracket },
            new() { Value = 5, TokenType = TokenType.Integer },
            new() { Value = '+', TokenType = TokenType.Operation },
            new() { Value = 42, TokenType = TokenType.Integer },
            new() { Value = ')', TokenType = TokenType.RightBracket },
        });

        var multiplication = (Multiplication)operation;
        Assert.AreEqual(new FloatingPointConstant(5.3), multiplication.Argument1);

        var unaryMinus = (UnaryMinus)multiplication.Argument2;

        var addition = (Addition)unaryMinus.Argument;
        Assert.AreEqual(new IntegerConstant(5), addition.Argument1);
        Assert.AreEqual(new IntegerConstant(42), addition.Argument2);
    }

    [TestMethod]
    public void TestUnaryMinus2()
    {
        IFunctionRegistry registry = new MockFunctionRegistry();

        var builder = new AstBuilder(registry, new ConstantRegistry(false, false));
        var operation = builder.Build(new List<Token>
        {
            new() { Value = '_', TokenType = TokenType.Operation },
            new() { Value = '(', TokenType = TokenType.LeftBracket },
            new() { Value = 1, TokenType = TokenType.Integer },
            new() { Value = ')', TokenType = TokenType.RightBracket },
            new() { Value = '^', TokenType = TokenType.Operation },
            new() { Value = 2, TokenType = TokenType.Integer },
        });

        var unaryMinus = (UnaryMinus)operation;

        var exponentiation = (Exponentiation)unaryMinus.Argument;
        Assert.AreEqual(new IntegerConstant(1), exponentiation.Base);
        Assert.AreEqual(new IntegerConstant(2), exponentiation.Exponent);
    }


    [TestMethod]
    public void TestBuildInvalidFormula1()
    {
        IFunctionRegistry registry = new MockFunctionRegistry();

        var builder = new AstBuilder(registry, new ConstantRegistry(false, false));

        AssertExtensions.ThrowsException<ParseException>(() =>
        {
            var operation = builder.Build(new List<Token>
            {
                new() { Value = '(', TokenType = TokenType.LeftBracket, StartPosition = 0 },
                new() { Value = 42, TokenType = TokenType.Integer, StartPosition = 1 },
                new() { Value = '+', TokenType = TokenType.Operation, StartPosition = 3 },
                new() { Value = 8, TokenType = TokenType.Integer, StartPosition = 4 },
                new() { Value = ')', TokenType = TokenType.RightBracket, StartPosition = 5 },
                new() { Value = '*', TokenType = TokenType.Operation, StartPosition = 6 },
            });
        });
    }

    [TestMethod]
    public void TestBuildInvalidFormula2()
    {
        IFunctionRegistry registry = new MockFunctionRegistry();

        var builder = new AstBuilder(registry, new ConstantRegistry(false, false));

        AssertExtensions.ThrowsException<ParseException>(() =>
        {
            var operation = builder.Build(new List<Token>
            {
                new() { Value = 42, TokenType = TokenType.Integer, StartPosition = 0 },
                new() { Value = '+', TokenType = TokenType.Operation, StartPosition = 2 },
                new() { Value = 8, TokenType = TokenType.Integer, StartPosition = 3 },
                new() { Value = ')', TokenType = TokenType.RightBracket, StartPosition = 4 },
                new() { Value = '*', TokenType = TokenType.Operation, StartPosition = 5 },
                new() { Value = 2, TokenType = TokenType.Integer, StartPosition = 6 },
            });
        });
    }

    [TestMethod]
    public void TestBuildInvalidFormula3()
    {
        IFunctionRegistry registry = new MockFunctionRegistry();

        var builder = new AstBuilder(registry, new ConstantRegistry(false, false));

        AssertExtensions.ThrowsException<ParseException>(() =>
        {
            var operation = builder.Build(new List<Token>
            {
                new() { Value = '(', TokenType = TokenType.LeftBracket, StartPosition = 0 },
                new() { Value = 42, TokenType = TokenType.Integer, StartPosition = 1 },
                new() { Value = '+', TokenType = TokenType.Operation, StartPosition = 3 },
                new() { Value = 8, TokenType = TokenType.Integer, StartPosition = 4 }
            });
        });
    }

    [TestMethod]
    public void TestBuildInvalidFormula4()
    {
        IFunctionRegistry registry = new MockFunctionRegistry();

        var builder = new AstBuilder(registry, new ConstantRegistry(false, false));

        AssertExtensions.ThrowsException<ParseException>(() =>
        {
            var operation = builder.Build(new List<Token>
            {
                new() { Value = 5, TokenType = TokenType.Integer, StartPosition = 0 },
                new() { Value = 42, TokenType = TokenType.Integer, StartPosition = 1 },
                new() { Value = '+', TokenType = TokenType.Operation, StartPosition = 3 },
                new() { Value = 8, TokenType = TokenType.Integer, StartPosition = 4 }
            });
        });
    }

    [TestMethod]
    public void TestBuildInvalidFormula5()
    {
        IFunctionRegistry registry = new MockFunctionRegistry();

        var builder = new AstBuilder(registry, new ConstantRegistry(false, false));

        AssertExtensions.ThrowsException<ParseException>(() =>
        {
            var operation = builder.Build(new List<Token>
            {
                new() { Value = 42, TokenType = TokenType.Integer, StartPosition = 0 },
                new() { Value = '+', TokenType = TokenType.Operation, StartPosition = 2 },
                new() { Value = 8, TokenType = TokenType.Integer, StartPosition = 3 },
                new() { Value = 5, TokenType = TokenType.Integer, StartPosition = 4 }
            });
        });
    }
}