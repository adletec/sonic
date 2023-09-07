using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using Adletec.Sonic.Execution;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Adletec.Sonic.Tests;

[TestClass]
public class CalculationEngineTests
{
    [TestMethod]
    public void TestCalculationFormula1FloatingPointCompiled()
    {
        var engine = CalculationEngine.Create()
            .UseCulture(CultureInfo.InvariantCulture)
            .UseExecutionMode(ExecutionMode.Compiled)
            .Build();

        double result = engine.Evaluate("2.0+3.0");

        Assert.AreEqual(5.0, result);
    }

    [TestMethod]
    public void TestCalculationFormula1IntegersCompiled()
    {
        var engine = CalculationEngine.Create()
            .UseCulture(CultureInfo.InvariantCulture)
            .UseExecutionMode(ExecutionMode.Compiled)
            .Build();

        double result = engine.Evaluate("2+3");

        Assert.AreEqual(5.0, result);
    }

    [TestMethod]
    public void TestCalculateFormula1()
    {
        var engine = CalculationEngine.CreateWithDefaults();
        double result = engine.Evaluate("2+3");

        Assert.AreEqual(5.0, result);
    }

    [TestMethod]
    public void TestCalculateModuloCompiled()
    {
        var engine = SonicEngines.CompiledNoCacheNoOptimizerCaseInsensitive();
        double result = engine.Evaluate("5 % 3.0");

        Assert.AreEqual(2.0, result);
    }

    [TestMethod]
    public void TestCalculateModuloInterpreted()
    {
        var engine = SonicEngines.InterpretedNoCacheNoOptimizerCaseInsensitive();
        double result = engine.Evaluate("5 % 3.0");

        Assert.AreEqual(2.0, result);
    }

    [TestMethod]
    public void TestCalculatePowCompiled()
    {
        var engine = SonicEngines.Compiled();
        double result = engine.Evaluate("2^3.0");

        Assert.AreEqual(8.0, result);
    }

    [TestMethod]
    public void TestCalculatePowInterpreted()
    {
        var engine = SonicEngines.Interpreted();
        double result = engine.Evaluate("2^3.0");

        Assert.AreEqual(8.0, result);
    }

    [TestMethod]
    public void TestCalculateFormulaWithVariables()
    {
        Dictionary<string, double> variables = new Dictionary<string, double>();
        variables.Add("var1", 2.5);
        variables.Add("var2", 3.4);

        var engine = CalculationEngine.CreateWithDefaults();
        double result = engine.Evaluate("var1*var2", variables);

        Assert.AreEqual(8.5, result);
    }

    [TestMethod]
    public void TestCalculateFormulaWithCaseSensitiveVariables1Compiled()
    {
        Dictionary<string, double> variables = new Dictionary<string, double>();
        variables.Add("VaR1", 2.5);
        variables.Add("vAr2", 3.4);
        variables.Add("var1", 1);
        variables.Add("var2", 1);

        var engine = SonicEngines.CompiledNoCacheNoOptimizer();
        double result = engine.Evaluate("VaR1*vAr2", variables);

        Assert.AreEqual(8.5, result);
    }

    [TestMethod]
    public void TestCalculateFormulaWithCaseSensitiveThrows()
    {
        var variables = new Dictionary<string, double>
        {
            { "var1", 1 },
            { "var2", 1 }
        };

        var engine = CalculationEngine.Create()
            .EnableCaseSensitivity()
            .Build();

        var ex = AssertExtensions.ThrowsException<VariableNotDefinedException>(() =>
            engine.Evaluate("VaR1*vAr2", variables));
        Assert.AreEqual("The variable \"VaR1\" used is not defined.", ex.Message);
    }

    [TestMethod]
    public void TestCalculateFormulaWithCaseSensitiveVariables1Interpreted()
    {
        var variables = new Dictionary<string, double>
        {
            { "VaR1", 2.5 },
            { "vAr2", 3.4 },
            { "var1", 1 },
            { "var2", 1 }
        };

        var engine = SonicEngines.InterpretedNoCacheNoOptimizer();
        double result = engine.Evaluate("VaR1*vAr2", variables);

        Assert.AreEqual(8.5, result);
    }

    [TestMethod]
    public void TestCalculateFormulaVariableNotDefinedInterpreted()
    {
        var variables = new Dictionary<string, double> { { "var1", 2.5 } };

        AssertExtensions.ThrowsException<VariableNotDefinedException>(() =>
        {
            var engine = SonicEngines.Interpreted();
            double result = engine.Evaluate("var1*var2", variables);
        });
    }

    [TestMethod]
    public void TestCalculateFormulaVariableNotDefinedCompiled()
    {
        Dictionary<string, double> variables = new Dictionary<string, double>();
        variables.Add("var1", 2.5);

        AssertExtensions.ThrowsException<VariableNotDefinedException>(() =>
        {
            var engine = SonicEngines.Compiled();
            double result = engine.Evaluate("var1*var2", variables);
        });
    }

    [TestMethod]
    public void TestCalculateSineFunctionInterpreted()
    {
        var engine = SonicEngines.Interpreted();
        double result = engine.Evaluate("sin(14)");

        Assert.AreEqual(Math.Sin(14.0), result);
    }

    [TestMethod]
    public void TestCalculateSineFunctionCompiled()
    {
        var engine = SonicEngines.CompiledNoOptimizerCaseInsensitive();
        double result = engine.Evaluate("sin(14)");

        Assert.AreEqual(Math.Sin(14.0), result);
    }

    [TestMethod]
    public void TestCalculateCosineFunctionInterpreted()
    {
        var engine = SonicEngines.Interpreted();
        double result = engine.Evaluate("cos(41)");

        Assert.AreEqual(Math.Cos(41.0), result);
    }

    [TestMethod]
    public void TestCalculateCosineFunctionCompiled()
    {
        var engine = SonicEngines.CompiledNoOptimizerCaseInsensitive();
        double result = engine.Evaluate("cos(41)");

        Assert.AreEqual(Math.Cos(41.0), result);
    }

    [TestMethod]
    public void TestCalculateLognFunctionInterpreted()
    {
        var engine = SonicEngines.InterpretedNoOptimizerCaseInsensitive();
        double result = engine.Evaluate("logn(14, 3)");

        Assert.AreEqual(Math.Log(14.0, 3.0), result);
    }

    [TestMethod]
    public void TestCalculateLognFunctionCompiled()
    {
        var engine = SonicEngines.CompiledNoOptimizerCaseInsensitive();
        double result = engine.Evaluate("logn(14, 3)");

        Assert.AreEqual(Math.Log(14.0, 3.0), result);
    }

    [TestMethod]
    public void TestNegativeConstant()
    {
        var engine = SonicEngines.CompiledNoOptimizerCaseInsensitive();
        double result = engine.Evaluate("-100");

        Assert.AreEqual(-100.0, result);
    }

    [TestMethod]
    public void TestMultiplicationWithNegativeConstant()
    {
        var engine = SonicEngines.CompiledNoOptimizerCaseInsensitive();
        double result = engine.Evaluate("5*-100");

        Assert.AreEqual(-500.0, result);
    }

    [TestMethod]
    public void TestUnaryMinus1Compiled()
    {
        var engine = SonicEngines.CompiledNoOptimizerCaseInsensitive();
        double result = engine.Evaluate("-(1+2+(3+4))");

        Assert.AreEqual(-10.0, result);
    }

    [TestMethod]
    public void TestUnaryMinus1Interpreted()
    {
        var engine = SonicEngines.InterpretedNoOptimizerCaseInsensitive();
        double result = engine.Evaluate("-(1+2+(3+4))");

        Assert.AreEqual(-10.0, result);
    }

    [TestMethod]
    public void TestUnaryMinus2Compiled()
    {
        var engine = SonicEngines.CompiledNoOptimizerCaseInsensitive();
        double result = engine.Evaluate("5+(-(1*2))");

        Assert.AreEqual(3.0, result);
    }

    [TestMethod]
    public void TestUnaryMinus2Interpreted()
    {
        var engine = SonicEngines.InterpretedNoOptimizerCaseInsensitive();
        double result = engine.Evaluate("5+(-(1*2))");

        Assert.AreEqual(3.0, result);
    }

    [TestMethod]
    public void TestUnaryMinus3Compiled()
    {
        var engine = SonicEngines.CompiledNoOptimizerCaseInsensitive();
        double result = engine.Evaluate("5*(-(1*2)*3)");

        Assert.AreEqual(-30.0, result);
    }

    [TestMethod]
    public void TestUnaryMinus3Interpreted()
    {
        var engine = SonicEngines.InterpretedNoOptimizerCaseInsensitive();
        double result = engine.Evaluate("5*(-(1*2)*3)");

        Assert.AreEqual(-30.0, result);
    }

    [TestMethod]
    public void TestUnaryMinus4Compiled()
    {
        var engine = SonicEngines.CompiledNoOptimizerCaseInsensitive();
        double result = engine.Evaluate("5* -(1*2)");

        Assert.AreEqual(-10.0, result);
    }

    [TestMethod]
    public void TestUnaryMinus4Interpreted()
    {
        var engine = SonicEngines.InterpretedNoOptimizerCaseInsensitive();
        double result = engine.Evaluate("5* -(1*2)");

        Assert.AreEqual(-10.0, result);
    }

    [TestMethod]
    public void TestUnaryMinus5Compiled()
    {
        var engine = SonicEngines.CompiledNoOptimizerCaseInsensitive();
        double result = engine.Evaluate("-(1*2)^3");

        Assert.AreEqual(-8.0, result);
    }

    [TestMethod]
    public void TestUnaryMinus5Interpreted()
    {
        var engine = SonicEngines.InterpretedNoOptimizerCaseInsensitive();
        double result = engine.Evaluate("-(1*2)^3");

        Assert.AreEqual(-8.0, result);
    }

    [TestMethod]
    public void TestUnaryMinus6Compiled()
    {
        var engine = SonicEngines.CompiledNoOptimizerCaseInsensitive();
        double result = engine.Evaluate("-(1*2)^2");

        Assert.AreEqual(-4.0, result);
    }

    [TestMethod]
    public void TestUnaryMinus6Interpreted()
    {
        var engine = SonicEngines.InterpretedNoOptimizerCaseInsensitive();
        double result = engine.Evaluate("-(1*2)^2");

        Assert.AreEqual(-4.0, result);
    }

    [TestMethod]
    public void TestBuild()
    {
        var engine = CalculationEngine.CreateWithDefaults();
        Func<Dictionary<string, double>, double> function = engine.Build("var1+2*(3*age)");

        Dictionary<string, double> variables = new Dictionary<string, double>();
        variables.Add("var1", 2);
        variables.Add("age", 4);

        double result = function(variables);
        Assert.AreEqual(26.0, result);
    }

    [TestMethod]
    public void TestFormulaBuilder()
    {
        var engine = CalculationEngine.CreateWithDefaults();
        Func<int, double, double> function = (Func<int, double, double>)engine.Formula("var1+2*(3*age)")
            .Parameter("var1", DataType.Integer)
            .Parameter("age", DataType.FloatingPoint)
            .Result(DataType.FloatingPoint)
            .Build();

        double result = function(2, 4);
        Assert.AreEqual(26.0, result);
    }

    [TestMethod]
    public void TestFormulaBuilderCompiled()
    {
        var engine = SonicEngines.Compiled();
        Func<int, double, double> function = (Func<int, double, double>)engine.Formula("var1+2*(3*age)")
            .Parameter("var1", DataType.Integer)
            .Parameter("age", DataType.FloatingPoint)
            .Result(DataType.FloatingPoint)
            .Build();

        double result = function(2, 4);
        Assert.AreEqual(26.0, result);
    }

    [TestMethod]
    public void TestFormulaBuilderConstantInterpreted()
    {
        var engine = SonicBuilders.Interpreted()
            .AddConstant("age", 18.0)
            .Build();

        Func<int, double> function = (Func<int, double>)engine.Formula("age+var1")
            .Parameter("var1", DataType.Integer)
            .Result(DataType.FloatingPoint)
            .Build();

        double result = function(3);
        Assert.AreEqual(21.0, result);
    }

    [TestMethod]
    public void TestFormulaBuilderConstantCompiled()
    {
        var engine = SonicBuilders.Compiled()
            .AddConstant("age", 18.0)
            .Build();

        Func<int, double> function = (Func<int, double>)engine.Formula("age+var1")
            .Parameter("var1", DataType.Integer)
            .Result(DataType.FloatingPoint)
            .Build();

        double result = function(3);
        Assert.AreEqual(21.0, result);
    }

    [TestMethod]
    public void TestFormulaBuilderInvalidParameterName()
    {
        AssertExtensions.ThrowsException<ArgumentException>(() =>
        {
            var engine = CalculationEngine.CreateWithDefaults();
            Func<int, double, double> function = (Func<int, double, double>)engine.Formula("sin+2")
                .Parameter("sin", DataType.Integer)
                .Build();
        });
    }

    [TestMethod]
    public void TestFormulaBuilderDuplicateParameterName()
    {
        AssertExtensions.ThrowsException<ArgumentException>(() =>
        {
            var engine = CalculationEngine.CreateWithDefaults();
            Func<int, double, double> function = (Func<int, double, double>)engine.Formula("var1+2")
                .Parameter("var1", DataType.Integer)
                .Parameter("var1", DataType.FloatingPoint)
                .Build();
        });
    }

    [TestMethod]
    public void TestPiMultiplication()
    {
        var engine = SonicEngines.CompiledCaseInsensitive();
        double result = engine.Evaluate("2 * pI");

        Assert.AreEqual(2 * Math.PI, result);
    }

    [TestMethod]
    public void TestReservedVariableName()
    {
        AssertExtensions.ThrowsException<ArgumentException>(() =>
        {
            var variables = new Dictionary<string, double> { { "pi", 2.0 } };

            var engine = CalculationEngine.CreateWithDefaults();
            double result = engine.Evaluate("2 * pI", variables);
        });
    }

    [TestMethod]
    public void TestVariableNameCaseSensitivityCompiled()
    {
        var variables = new Dictionary<string, double> { { "blabla", 42.5 } };

        var engine = SonicEngines.CompiledCaseInsensitive();
        double result = engine.Evaluate("2 * BlAbLa", variables);

        Assert.AreEqual(85.0, result);
    }

    [TestMethod]
    public void TestVariableNameCaseSensitivityInterpreted()
    {
        var variables = new Dictionary<string, double> { { "blabla", 42.5 } };

        var engine = SonicEngines.InterpretedCaseInsensitive();
        double result = engine.Evaluate("2 * BlAbLa", variables);

        Assert.AreEqual(85.0, result);
    }

    [TestMethod]
    public void TestVariableNameCaseSensitivityNoToLowerCompiled()
    {
        Dictionary<string, double> variables = new Dictionary<string, double>();
        variables.Add("BlAbLa", 42.5);

        var engine = SonicEngines.CompiledNoCacheNoOptimizer();
        double result = engine.Evaluate("2 * BlAbLa", variables);

        Assert.AreEqual(85.0, result);
    }

    [TestMethod]
    public void TestVariableNameCaseSensitivityNoToLowerInterpreted()
    {
        var variables = new Dictionary<string, double> { { "BlAbLa", 42.5 } };

        var engine = SonicEngines.InterpretedNoCacheNoOptimizer();
        double result = engine.Evaluate("2 * BlAbLa", variables);

        Assert.AreEqual(85.0, result);
    }

    [TestMethod]
    public void TestCustomFunctionInterpreted()
    {
        var engine = SonicBuilders.InterpretedNoCacheNoOptimizer()
            .AddFunction("test", (a, b) => a + b)
            .Build();

        double result = engine.Evaluate("test(2,3)");
        Assert.AreEqual(5.0, result);
    }

    [TestMethod]
    public void TestCustomFunctionCompiled()
    {
        var engine = SonicBuilders.CompiledNoCacheNoOptimizer()
            .AddFunction("test", (a, b) => a + b)
            .Build();

        double result = engine.Evaluate("test(2,3)");
        Assert.AreEqual(5.0, result);
    }

    [TestMethod]
    public void TestComplicatedPrecedence1()
    {
        var engine = CalculationEngine.CreateWithDefaults();

        double result = engine.Evaluate("1+2-3*4/5+6-7*8/9+0");
        Assert.AreEqual(0.378, Math.Round(result, 3));
    }

    [TestMethod]
    public void TestComplicatedPrecedence2()
    {
        var engine = CalculationEngine.CreateWithDefaults();

        double result = engine.Evaluate("1+2-3*4/sqrt(25)+6-7*8/9+0");
        Assert.AreEqual(0.378, Math.Round(result, 3));
    }

    [TestMethod]
    public void TestExpressionArguments1()
    {
        var engine = SonicEngines.Compiled();

        double result = engine.Evaluate("ifless(0.57, (3000-500)/(1500-500), 10, 20)");
        Assert.AreEqual(10, result);
    }

    [TestMethod]
    public void TestExpressionArguments2()
    {
        var engine = SonicEngines.Compiled();

        double result = engine.Evaluate("if(0.57 < (3000-500)/(1500-500), 10, 20)");
        Assert.AreEqual(10, result);
    }

    [TestMethod]
    public void TestNestedFunctions()
    {
        var engine = SonicEngines.Compiled();

        double result = engine.Evaluate("max(sin(67), cos(67))");
        Assert.AreEqual(-0.517769799789505, Math.Round(result, 15));
    }

    [TestMethod]
    public void TestVariableCaseInsensitiveFuncInterpreted()
    {
        var engine = SonicEngines.InterpretedCaseInsensitive();
        Func<Dictionary<string, double>, double> formula = engine.Build("var1+2/(3*otherVariablE)");

        var variables = new Dictionary<string, double>
        {
            { "var1", 2 },
            { "otherVariable", 4.2 }
        };

        double result = formula(variables);
    }

    [TestMethod]
    public void TestConstantBuildCompiled()
    {
        var engine = SonicEngines.Compiled();
        Func<Dictionary<string, double>, double> formula = engine.Build("pi");

        Dictionary<string, double> variables = new Dictionary<string, double>();

        double result = formula(variables);

        Assert.AreEqual(Math.PI, result);
    }

    [TestMethod]
    public void TestConstantBuildInterpreted()
    {
        var engine = SonicEngines.Interpreted();
        Func<Dictionary<string, double>, double> formula = engine.Build("pi");

        Dictionary<string, double> variables = new Dictionary<string, double>();

        double result = formula(variables);

        Assert.AreEqual(Math.PI, result);
    }

    [TestMethod]
    public void TestVariableCaseInsensitiveFuncCompiled()
    {
        var engine = SonicEngines.CompiledCaseInsensitive();
        Func<Dictionary<string, double>, double> formula = engine.Build("var1+2/(3*otherVariablE)");

        var variables = new Dictionary<string, double>
        {
            { "var1", 2 },
            { "otherVariable", 4.2 }
        };

        double result = formula(variables);
    }

    [TestMethod]
    public void TestVariableCaseInsensitiveNonFunc()
    {
        var engine = SonicEngines.CompiledCaseInsensitive();

        var variables = new Dictionary<string, double>
        {
            { "var1", 2 },
            { "otherVariable", 4.2 }
        };

        double result = engine.Evaluate("var1+2/(3*otherVariablE)", variables);
    }

    [TestMethod]
    public void TestLessThanInterpreted()
    {
        var engine = SonicEngines.Interpreted();

        Dictionary<string, double> variables = new Dictionary<string, double>();
        variables.Add("var1", 2);
        variables.Add("var2", 4.2);

        double result = engine.Evaluate("var1 < var2", variables);
        Assert.AreEqual(1.0, result);
    }

    [TestMethod]
    public void TestLessThanCompiled()
    {
        var engine = SonicEngines.Compiled();

        Dictionary<string, double> variables = new Dictionary<string, double>();
        variables.Add("var1", 2);
        variables.Add("var2", 4.2);

        double result = engine.Evaluate("var1 < var2", variables);
        Assert.AreEqual(1.0, result);
    }

    [TestMethod]
    public void TestLessOrEqualThan1Interpreted()
    {
        var engine = SonicEngines.Interpreted();

        Dictionary<string, double> variables = new Dictionary<string, double>();
        variables.Add("var1", 2);
        variables.Add("var2", 2);

        double result = engine.Evaluate("var1 <= var2", variables);
        Assert.AreEqual(1.0, result);
    }

    [TestMethod]
    public void TestLessOrEqualThan1Compiled()
    {
        var engine = SonicEngines.Compiled();

        Dictionary<string, double> variables = new Dictionary<string, double>();
        variables.Add("var1", 2);
        variables.Add("var2", 2);

        double result = engine.Evaluate("var1 <= var2", variables);
        Assert.AreEqual(1.0, result);
    }

    [TestMethod]
    public void TestLessOrEqualThan2Interpreted()
    {
        var engine = SonicEngines.Interpreted();

        Dictionary<string, double> variables = new Dictionary<string, double>();
        variables.Add("var1", 2);
        variables.Add("var2", 2);

        double result = engine.Evaluate("var1 ≤ var2", variables);
        Assert.AreEqual(1.0, result);
    }

    [TestMethod]
    public void TestLessOrEqualThan2Compiled()
    {
        var engine = SonicEngines.Compiled();

        Dictionary<string, double> variables = new Dictionary<string, double>();
        variables.Add("var1", 2);
        variables.Add("var2", 2);

        double result = engine.Evaluate("var1 ≤ var2", variables);
        Assert.AreEqual(1.0, result);
    }

    [TestMethod]
    public void TestGreaterThan1Interpreted()
    {
        var engine = SonicEngines.Interpreted();

        Dictionary<string, double> variables = new Dictionary<string, double>();
        variables.Add("var1", 2);
        variables.Add("var2", 3);

        double result = engine.Evaluate("var1 > var2", variables);
        Assert.AreEqual(0.0, result);
    }

    [TestMethod]
    public void TestGreaterThan1Compiled()
    {
        var engine = SonicEngines.Compiled();

        Dictionary<string, double> variables = new Dictionary<string, double>();
        variables.Add("var1", 2);
        variables.Add("var2", 3);

        double result = engine.Evaluate("var1 > var2", variables);
        Assert.AreEqual(0.0, result);
    }

    [TestMethod]
    public void TestGreaterOrEqualThan1Interpreted()
    {
        var engine = SonicEngines.Interpreted();

        Dictionary<string, double> variables = new Dictionary<string, double>();
        variables.Add("var1", 2);
        variables.Add("var2", 2);

        double result = engine.Evaluate("var1 >= var2", variables);
        Assert.AreEqual(1.0, result);
    }

    [TestMethod]
    public void TestGreaterOrEqualThan1Compiled()
    {
        var engine = SonicEngines.Compiled();

        Dictionary<string, double> variables = new Dictionary<string, double>();
        variables.Add("var1", 2);
        variables.Add("var2", 2);

        double result = engine.Evaluate("var1 >= var2", variables);
        Assert.AreEqual(1.0, result);
    }

    [TestMethod]
    public void TestNotEqual1Interpreted()
    {
        var engine = SonicEngines.Interpreted();

        Dictionary<string, double> variables = new Dictionary<string, double>();
        variables.Add("var1", 2);
        variables.Add("var2", 2);

        double result = engine.Evaluate("var1 != 2", variables);
        Assert.AreEqual(0.0, result);
    }

    [TestMethod]
    public void TestNotEqual2Interpreted()
    {
        var engine = SonicEngines.Interpreted();

        Dictionary<string, double> variables = new Dictionary<string, double>();
        variables.Add("var1", 2);
        variables.Add("var2", 2);

        double result = engine.Evaluate("var1 ≠ 2", variables);
        Assert.AreEqual(0.0, result);
    }

    [TestMethod]
    public void TestNotEqual2Compiled()
    {
        var engine = SonicEngines.Compiled();

        Dictionary<string, double> variables = new Dictionary<string, double>();
        variables.Add("var1", 2);
        variables.Add("var2", 2);

        double result = engine.Evaluate("var1 ≠ 2", variables);
        Assert.AreEqual(0.0, result);
    }

    [TestMethod]
    public void TestEqualInterpreted()
    {
        var engine = SonicEngines.Interpreted();

        Dictionary<string, double> variables = new Dictionary<string, double>();
        variables.Add("var1", 2);
        variables.Add("var2", 2);

        double result = engine.Evaluate("var1 == 2", variables);
        Assert.AreEqual(1.0, result);
    }

    [TestMethod]
    public void TestEqualCompiled()
    {
        var engine = SonicEngines.Compiled();

        Dictionary<string, double> variables = new Dictionary<string, double>();
        variables.Add("var1", 2);
        variables.Add("var2", 2);

        double result = engine.Evaluate("var1 == 2", variables);
        Assert.AreEqual(1.0, result);
    }

    public void TestVariableUnderscoreInterpreted()
    {
        var engine = SonicEngines.Interpreted();

        Dictionary<string, double> variables = new Dictionary<string, double>();
        variables.Add("var_var_1", 1);
        variables.Add("var_var_2", 2);

        double result = engine.Evaluate("var_var_1 + var_var_2", variables);
        Assert.AreEqual(3.0, result);
    }

    [TestMethod]
    public void TestVariableUnderscoreCompiled()
    {
        var engine = SonicEngines.Compiled();

        Dictionary<string, double> variables = new Dictionary<string, double>();
        variables.Add("var_var_1", 1);
        variables.Add("var_var_2", 2);

        double result = engine.Evaluate("var_var_1 + var_var_2", variables);
        Assert.AreEqual(3.0, result);
    }

    [TestMethod]
    public void TestCustomFunctionFunc11Interpreted()
    {
        var engine = SonicBuilders.InterpretedNoCacheNoOptimizer()
            .AddFunction("test", (a, b, c, d, e, f, g, h, i, j, k) => a + b + c + d + e + f + g + h + i + j + k)
            .Build();

        double result = engine.Evaluate("test(1,2,3,4,5,6,7,8,9,10,11)");
        double expected = (11 * (11 + 1)) / 2.0;
        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    public void TestCustomFunctionFunc11Compiled()
    {
        var engine = SonicBuilders.CompiledNoCacheNoOptimizer()
            .AddFunction("test", (a, b, c, d, e, f, g, h, i, j, k) => a + b + c + d + e + f + g + h + i + j + k)
            .Build();
        double result = engine.Evaluate("test(1,2,3,4,5,6,7,8,9,10,11)");
        double expected = (11 * (11 + 1)) / 2.0;
        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    public void TestCustomFunctionDynamicFuncInterpreted()
    {
        double DoSomething(params double[] a)
        {
            return a.Sum();
        }

        var engine = SonicBuilders.InterpretedNoCacheNoOptimizer()
            .AddFunction("test", DoSomething)
            .Build();

        double result = engine.Evaluate("test(1,2,3,4,5,6,7,8,9,10,11)");
        double expected = (11 * (11 + 1)) / 2.0;
        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    public void TestCustomFunctionDynamicFuncCompiled()
    {
        double DoSomething(params double[] a)
        {
            return a.Sum();
        }

        var engine = SonicBuilders.CompiledNoCacheNoOptimizer()
            .AddFunction("test", DoSomething)
            .Build();
        
        double result = engine.Evaluate("test(1,2,3,4,5,6,7,8,9,10,11)");
        double expected = (11 * (11 + 1)) / 2.0;
        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    public void TestCustomFunctionDynamicFuncNestedInterpreted()
    {
        double DoSomething(params double[] a)
        {
            return a.Sum();
        }

        var engine = SonicBuilders.InterpretedNoCacheNoOptimizer()
            .AddFunction("test", DoSomething)
            .Build();
        
        double result = engine.Evaluate("test(1,2,3,test(4,5,6)) + test(7,8,9,10,11)");
        double expected = (11 * (11 + 1)) / 2.0;
        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    public void TestCustomFunctionDynamicFuncNestedDynamicCompiled()
    {
        double DoSomething(params double[] a)
        {
            return a.Sum();
        }

        var engine = SonicBuilders.CompiledNoCacheNoOptimizer()
            .AddFunction("test", DoSomething)
            .Build();
        
        double result = engine.Evaluate("test(1,2,3,test(4,5,6)) + test(7,8,9,10,11)");
        double expected = (11 * (11 + 1)) / 2.0;
        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    public void TestAndCompiled()
    {
        var engine = SonicEngines.CompiledNoOptimizerCaseInsensitive();
        double result = engine.Evaluate("(1 && 0)");
        Assert.AreEqual(0, result);
    }

    [TestMethod]
    public void TestAndInterpreted()
    {
        var engine = SonicEngines.InterpretedNoOptimizerCaseInsensitive();
        double result = engine.Evaluate("(1 && 0)");
        Assert.AreEqual(0, result);
    }

    [TestMethod]
    public void TestOr1Compiled()
    {
        var engine = SonicEngines.CompiledNoOptimizerCaseInsensitive();
        double result = engine.Evaluate("(1 || 0)");
        Assert.AreEqual(1, result);
    }

    [TestMethod]
    public void TestOr1Interpreted()
    {
        var engine = SonicEngines.InterpretedNoOptimizerCaseInsensitive();
        double result = engine.Evaluate("(1 || 0)");
        Assert.AreEqual(1, result);
    }

    [TestMethod]
    public void TestOr2Compiled()
    {
        var engine = SonicEngines.CompiledNoOptimizerCaseInsensitive();
        double result = engine.Evaluate("(0 || 0)");
        Assert.AreEqual(0, result);
    }

    [TestMethod]
    public void TestOr2Interpreted()
    {
        var engine = SonicEngines.InterpretedNoOptimizerCaseInsensitive();
        double result = engine.Evaluate("(0 || 0)");
        Assert.AreEqual(0, result);
    }

    [TestMethod]
    public void TestMedian1Compiled()
    {
        var engine = SonicEngines.CompiledNoOptimizerCaseInsensitive();
        double result = engine.Evaluate("median(3,1,5,4,2)");
        Assert.AreEqual(3, result);
    }

    [TestMethod]
    public void TestMedian1Interpreted()
    {
        var engine = SonicEngines.InterpretedNoOptimizerCaseInsensitive();
        double result = engine.Evaluate("median(3,1,5,4,2)");
        Assert.AreEqual(3, result);
    }

    [TestMethod]
    public void TestMedian2Compiled()
    {
        var engine = SonicEngines.CompiledNoOptimizerCaseInsensitive();
        double result = engine.Evaluate("median(3,1,5,4)");
        Assert.AreEqual(3, result);
    }

    [TestMethod]
    public void TestMedian2Interpreted()
    {
        var engine = SonicEngines.InterpretedNoOptimizerCaseInsensitive();
        double result = engine.Evaluate("median(3,1,5,4)");
        Assert.AreEqual(3, result);
    }

    [TestMethod]
    public void TestCalculationFormulaBuildingWithConstants1Compiled()
    {
        var engine = SonicEngines.Compiled();
        var fn = engine.Build("a+b+c", new Dictionary<string, double> { { "a", 1 } });
        double result = fn(new Dictionary<string, double> { { "b", 2 }, { "c", 2 } });
        Assert.AreEqual(5.0, result);
    }

    [TestMethod]
    public void TestCalculationFormulaBuildingWithConstants1Interpreted()
    {
        var engine = SonicEngines.Interpreted();
        var fn = engine.Build("a+b+c", new Dictionary<string, double> { { "a", 1 } });
        double result = fn(new Dictionary<string, double> { { "b", 2 }, { "c", 2 } });
        Assert.AreEqual(5.0, result);
    }

    [TestMethod]
    public void TestCalculationFormulaBuildingWithConstants2Compiled()
    {
        var engine = SonicEngines.Compiled();

        Func<double, double, double> formula = (Func<double, double, double>)engine.Formula("a+b+c")
            .Parameter("b", DataType.FloatingPoint)
            .Parameter("c", DataType.FloatingPoint)
            .Constant("a", 1)
            .Result(DataType.FloatingPoint)
            .Build();

        double result = formula(2.0, 2.0);
        Assert.AreEqual(5.0, result);
    }

    [TestMethod]
    public void TestCalculationFormulaBuildingWithConstants2Interpreted()
    {
        var engine = SonicEngines.Interpreted();

        Func<double, double, double> formula = (Func<double, double, double>)engine.Formula("a+b+c")
            .Parameter("b", DataType.FloatingPoint)
            .Parameter("c", DataType.FloatingPoint)
            .Constant("a", 1)
            .Result(DataType.FloatingPoint)
            .Build();

        double result = formula(2.0, 2.0);
        Assert.AreEqual(5.0, result);
    }

    [TestMethod]
    public void TestCalculationFormulaBuildingWithConstants3Compiled()
    {
        var engine = SonicEngines.Compiled();

        Func<double, double> formula = (Func<double, double>)engine.Formula("a+A")
            .Parameter("A", DataType.FloatingPoint)
            .Constant("a", 1)
            .Result(DataType.FloatingPoint)
            .Build();

        double result = formula(2.0);
        Assert.AreEqual(3.0, result);
    }

    [TestMethod]
    public void TestCalculationFormulaBuildingWithConstants3Interpreted()
    {
        var engine = SonicEngines.Interpreted();

        Func<double, double> formula = (Func<double, double>)engine.Formula("a+A")
            .Parameter("A", DataType.FloatingPoint)
            .Constant("a", 1)
            .Result(DataType.FloatingPoint)
            .Build();

        double result = formula(2.0);
        Assert.AreEqual(3.0, result);
    }

    [TestMethod]
    public void TestCalculationFormulaBuildingWithConstantsCache1()
    {
        var engine = SonicEngines.Compiled();

        var fn = engine.Build("a+b+c", new Dictionary<string, double> { { "a", 1 } });
        double result = fn(new Dictionary<string, double> { { "b", 2 }, { "c", 2 } });
        Assert.AreEqual(5.0, result);

        AssertExtensions.ThrowsException<VariableNotDefinedException>(() =>
        {
            var fn1 = engine.Build("a+b+c");
            double result1 = fn1(new Dictionary<string, double> { { "b", 3 }, { "c", 3 } });
        });
    }

    [TestMethod]
    public void TestCalculationFormulaBuildingWithConstantsCache2()
    {
        var engine = SonicEngines.Compiled();
        
        var fn = engine.Build("a+b+c");
        double result = fn(new Dictionary<string, double> { { "a", 1 }, { "b", 2 }, { "c", 2 } });
        Assert.AreEqual(5.0, result);


        var fn1 = engine.Build("a+b+c", new Dictionary<string, double> { { "a", 2 } });
        double result1 = fn1(new Dictionary<string, double> { { "b", 2 }, { "c", 2 } });
        Assert.AreEqual(6.0, result1);
    }


    [TestMethod]
    public void TestCalculationFormulaBuildingWithConstantsCache3()
    {
        var engine = SonicEngines.Interpreted();

        Func<double, double> formula = (Func<double, double>)engine.Formula("a+A")
            .Parameter("A", DataType.FloatingPoint)
            .Constant("a", 1)
            .Result(DataType.FloatingPoint)
            .Build();

        double result = formula(2.0);
        Assert.AreEqual(3.0, result);

        Func<double, double, double> formula1 = (Func<double, double, double>)engine.Formula("a+A")
            .Parameter("A", DataType.FloatingPoint)
            .Parameter("a", DataType.FloatingPoint)
            .Result(DataType.FloatingPoint)
            .Build();

        double result1 = formula1(2.0, 2.0);
        Assert.AreEqual(4.0, result1);
    }


    [TestMethod]
    public void TestCalculationFormulaBuildingWithConstantsCache4()
    {
        var engine = SonicEngines.Compiled();

        Func<double, double> formula = (Func<double, double>)engine.Formula("a+A")
            .Parameter("A", DataType.FloatingPoint)
            .Constant("a", 1)
            .Result(DataType.FloatingPoint)
            .Build();

        double result = formula(2.0);
        Assert.AreEqual(3.0, result);

        Func<double, double, double> formula1 = (Func<double, double, double>)engine.Formula("a+A")
            .Parameter("A", DataType.FloatingPoint)
            .Parameter("a", DataType.FloatingPoint)
            .Result(DataType.FloatingPoint)
            .Build();

        double result1 = formula1(2.0, 2.0);
        Assert.AreEqual(4.0, result1);
    }

    [TestMethod]
    public void TestCalculationFormulaBuildingWithConstantsCache5()
    {
        var engine = SonicEngines.Compiled();
        var fn = engine.Build("a+b+c", new Dictionary<string, double> { { "a", 1 } });
        double result = fn(new Dictionary<string, double> { { "b", 2 }, { "c", 2 } });
        Assert.AreEqual(5.0, result);

        var fn1 = engine.Build("a+b+c", new Dictionary<string, double> { { "a", 2 } });
        double result1 = fn1(new Dictionary<string, double> { { "b", 2 }, { "c", 2 } });
        Assert.AreEqual(6.0, result1);
    }

    [TestMethod]
    public void TestCalculationFormulaBuildingWithConstantsCache6()
    {
        var engine = SonicEngines.Interpreted();
        var fn = engine.Build("a+b+c", new Dictionary<string, double> { { "a", 1 } });
        double result = fn(new Dictionary<string, double> { { "b", 2 }, { "c", 2 } });
        Assert.AreEqual(5.0, result);

        var fn1 = engine.Build("a+b+c", new Dictionary<string, double> { { "a", 2 } });
        double result1 = fn1(new Dictionary<string, double> { { "b", 2 }, { "c", 2 } });
        Assert.AreEqual(6.0, result1);
    }

    [TestMethod]
    public void TestCalculationFormulaBuildingWithConstantsCache7()
    {
        var engine = SonicEngines.Compiled();

        var fn = engine.Build("a+b+c");
        double result = fn(new Dictionary<string, double> { { "a", 1 }, { "b", 2 }, { "c", 2 } });
        Assert.AreEqual(5.0, result);


        var fn1 = engine.Build("a+b+c", new Dictionary<string, double> { { "a", 2 } });
        double result1 = fn1(new Dictionary<string, double> { { "b", 3 }, { "c", 3 } });
        Assert.AreEqual(8.0, result1);
    }

    [TestMethod]
    public void TestCalculationCompiledExpressionCompiled()
    {
        Expression<Func<double, double, double>> expression = (a, b) => a + b;
        expression.Compile();

        var engine = SonicBuilders.Compiled()
            .AddFunction("test", expression.Compile())
            .Build();

        double result = engine.Evaluate("test(2, 3)");
        Assert.AreEqual(5.0, result);
    }

    [TestMethod]
    public void TestCalculationCompiledExpressionInterpreted()
    {
        Expression<Func<double, double, double>> expression = (a, b) => a + b;
        expression.Compile();

        var engine = SonicBuilders.Interpreted()
            .AddFunction("test", expression.Compile())
            .Build();

        double result = engine.Evaluate("test(2, 3)");
        Assert.AreEqual(5.0, result);
    }

    [TestMethod]
    public void TestRerunCalculation()
    {
        const string expression = "var1 + var2 * var3 / 2";
        var values = new Dictionary<string, double>
        {
            { "var1", 1 },
            { "var2", 2 },
            { "var3", 3 }
        };

        var engine = SonicEngines.Compiled();
        for (var i = 0; i < 3; i++)
        {
            engine.Evaluate(expression, values);
        }

        // assert "does not throw an exception"
        Assert.IsTrue(true);
    }

    [TestMethod]
    public void TestRerunCalculationFunc()
    {
        const string expression = "var1 + var2 * var3 / 2";
        var values = new Dictionary<string, double>
        {
            { "var1", 1 },
            { "var2", 2 },
            { "var3", 3 }
        };

        var engine = SonicEngines.Compiled();
        var func = engine.Build(expression);
        for (var i = 0; i < 3; i++)
        {
            func(values);
        }

        // assert "does not throw an exception"
        Assert.IsTrue(true);
    }
}

internal static class SonicEngines
{
    public static CalculationEngine CompiledNoOptimizerCaseInsensitive() => CalculationEngine.Create()
        .UseCulture(CultureInfo.InvariantCulture)
        .UseExecutionMode(ExecutionMode.Compiled)
        .EnableCache()
        .DisableOptimizer()
        .DisableCaseSensitivity()
        .Build();

    public static CalculationEngine CompiledCaseInsensitive() => CalculationEngine.Create()
        .UseCulture(CultureInfo.InvariantCulture)
        .UseExecutionMode(ExecutionMode.Compiled)
        .EnableCache()
        .EnableOptimizer()
        .DisableCaseSensitivity()
        .Build();

    public static CalculationEngine CompiledNoCacheNoOptimizerCaseInsensitive() => CalculationEngine.Create()
        .UseCulture(CultureInfo.InvariantCulture)
        .UseExecutionMode(ExecutionMode.Compiled)
        .DisableCache()
        .DisableOptimizer()
        .DisableCaseSensitivity()
        .Build();

    public static CalculationEngine CompiledNoCacheNoOptimizer() => CalculationEngine.Create()
        .UseCulture(CultureInfo.InvariantCulture)
        .UseExecutionMode(ExecutionMode.Compiled)
        .DisableCache()
        .DisableOptimizer()
        .EnableCaseSensitivity()
        .Build();

    public static CalculationEngine Compiled() => CalculationEngine.Create()
        .UseCulture(CultureInfo.InvariantCulture)
        .UseExecutionMode(ExecutionMode.Compiled)
        .EnableCache()
        .EnableOptimizer()
        .EnableCaseSensitivity()
        .Build();

    public static CalculationEngine InterpretedNoCacheNoOptimizerCaseInsensitive() => CalculationEngine.Create()
        .UseCulture(CultureInfo.InvariantCulture)
        .UseExecutionMode(ExecutionMode.Interpreted)
        .DisableCache()
        .DisableOptimizer()
        .DisableCaseSensitivity()
        .Build();

    public static CalculationEngine InterpretedNoOptimizerCaseInsensitive() => CalculationEngine.Create()
        .UseCulture(CultureInfo.InvariantCulture)
        .UseExecutionMode(ExecutionMode.Interpreted)
        .EnableCache()
        .DisableOptimizer()
        .DisableCaseSensitivity()
        .Build();

    public static CalculationEngine InterpretedNoCacheNoOptimizer() => CalculationEngine.Create()
        .UseCulture(CultureInfo.InvariantCulture)
        .UseExecutionMode(ExecutionMode.Interpreted)
        .DisableCache()
        .DisableOptimizer()
        .EnableCaseSensitivity()
        .Build();

    public static CalculationEngine InterpretedCaseInsensitive() => CalculationEngine.Create()
        .UseCulture(CultureInfo.InvariantCulture)
        .UseExecutionMode(ExecutionMode.Interpreted)
        .EnableCache()
        .EnableOptimizer()
        .DisableCaseSensitivity()
        .Build();

    public static CalculationEngine Interpreted() => CalculationEngine.Create()
        .UseCulture(CultureInfo.InvariantCulture)
        .UseExecutionMode(ExecutionMode.Interpreted)
        .EnableCache()
        .EnableOptimizer()
        .EnableCaseSensitivity()
        .Build();
}

internal static class SonicBuilders
{
    public static CalculationEngineBuilder CompiledNoOptimizerCaseInsensitive() => CalculationEngine.Create()
        .UseCulture(CultureInfo.InvariantCulture)
        .UseExecutionMode(ExecutionMode.Compiled)
        .EnableCache()
        .DisableOptimizer()
        .DisableCaseSensitivity();

    public static CalculationEngineBuilder CompiledNoCacheNoOptimizerCaseInsensitive() => CalculationEngine.Create()
        .UseCulture(CultureInfo.InvariantCulture)
        .UseExecutionMode(ExecutionMode.Compiled)
        .DisableCache()
        .DisableOptimizer()
        .DisableCaseSensitivity();

    public static CalculationEngineBuilder CompiledNoCacheNoOptimizer() => CalculationEngine.Create()
        .UseCulture(CultureInfo.InvariantCulture)
        .UseExecutionMode(ExecutionMode.Compiled)
        .DisableCache()
        .DisableOptimizer()
        .EnableCaseSensitivity();

    public static CalculationEngineBuilder Compiled() => CalculationEngine.Create()
        .UseCulture(CultureInfo.InvariantCulture)
        .UseExecutionMode(ExecutionMode.Compiled)
        .EnableCache()
        .EnableOptimizer()
        .EnableCaseSensitivity();

    public static CalculationEngineBuilder InterpretedNoCacheNoOptimizerCaseInsensitive() => CalculationEngine.Create()
        .UseCulture(CultureInfo.InvariantCulture)
        .UseExecutionMode(ExecutionMode.Interpreted)
        .DisableCache()
        .DisableOptimizer()
        .DisableCaseSensitivity();

    public static CalculationEngineBuilder InterpretedNoOptimizerCaseInsensitive() => CalculationEngine.Create()
        .UseCulture(CultureInfo.InvariantCulture)
        .UseExecutionMode(ExecutionMode.Interpreted)
        .EnableCache()
        .DisableOptimizer()
        .DisableCaseSensitivity();

    public static CalculationEngineBuilder InterpretedNoCacheNoOptimizer() => CalculationEngine.Create()
        .UseCulture(CultureInfo.InvariantCulture)
        .UseExecutionMode(ExecutionMode.Interpreted)
        .DisableCache()
        .DisableOptimizer()
        .EnableCaseSensitivity();

    public static CalculationEngineBuilder Interpreted() => CalculationEngine.Create()
        .UseCulture(CultureInfo.InvariantCulture)
        .UseExecutionMode(ExecutionMode.Interpreted)
        .EnableCache()
        .EnableOptimizer()
        .EnableCaseSensitivity();
}