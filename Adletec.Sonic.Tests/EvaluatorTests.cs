using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using Adletec.Sonic.Execution;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Adletec.Sonic.Tests;

[TestClass]
public class EvaluatorTests
{
    [TestMethod]
    public void TestCalculationFormula1FloatingPointCompiled()
    {
        var engine = Sonic.Evaluator.Create()
            .UseCulture(CultureInfo.InvariantCulture)
            .UseExecutionMode(ExecutionMode.Compiled)
            .Build();

        double result = engine.Evaluate("2.0+3.0");

        Assert.AreEqual(5.0, result);
    }

    [TestMethod]
    public void TestCalculationFormula1IntegersCompiled()
    {
        var engine = Sonic.Evaluator.Create()
            .UseCulture(CultureInfo.InvariantCulture)
            .UseExecutionMode(ExecutionMode.Compiled)
            .Build();

        double result = engine.Evaluate("2+3");

        Assert.AreEqual(5.0, result);
    }

    [TestMethod]
    public void TestCalculateFormula1()
    {
        var engine = Sonic.Evaluator.CreateWithDefaults();
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

        var engine = Sonic.Evaluator.CreateWithDefaults();
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

        var engine = Sonic.Evaluator.Create()
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
    public void TestUnaryMinus7Compiled()
    {
        var engine = SonicEngines.CompiledNoCacheNoOptimizer();
        var result = engine.Evaluate("-1.0^2");
        Assert.AreEqual(-1.0, result);
    }

    [TestMethod]
    public void TestUnaryMinus7Interpreted()
    {
        var engine = SonicEngines.InterpretedNoCacheNoOptimizer();
        var result = engine.Evaluate("-1.0^2");
        Assert.AreEqual(-1.0, result);
    }


    [TestMethod]
    public void TestUnaryMinus8Compiled()
    {
        var engine = SonicEngines.CompiledNoCacheNoOptimizer();
        var result = engine.Evaluate("-(1.0)^2");
        Assert.AreEqual(-1.0, result);
    }

    [TestMethod]
    public void TestUnaryMinus8Interpreted()
    {
        var engine = SonicEngines.InterpretedNoCacheNoOptimizer();
        var result = engine.Evaluate("-(1.0)^2");
        Assert.AreEqual(-1.0, result);
    }


    [TestMethod]
    public void TestUnaryMinus9Compiled()
    {
        var engine = SonicEngines.CompiledNoCacheNoOptimizer();
        var result = engine.Evaluate("1--1");
        Assert.AreEqual(2.0, result);
    }

    [TestMethod]
    public void TestUnaryMinus9Interpreted()
    {
        var engine = SonicEngines.InterpretedNoCacheNoOptimizer();
        var result = engine.Evaluate("1--1");
        Assert.AreEqual(2.0, result);
    }


    [TestMethod]
    public void TestUnaryMinus10Compiled()
    {
        var engine = SonicEngines.CompiledNoCacheNoOptimizer();
        var result = engine.Evaluate("1.0-1.0");
        Assert.AreEqual(0.0, result);
    }

    [TestMethod]
    public void TestUnaryMinus10Interpreted()
    {
        var engine = SonicEngines.InterpretedNoCacheNoOptimizer();
        var result = engine.Evaluate("1.0-1.0");
        Assert.AreEqual(0.0, result);
    }


    [TestMethod]
    public void TestUnaryMinus11Compiled()
    {
        var engine = SonicEngines.CompiledNoCacheNoOptimizer();
        var result = engine.Evaluate("5*-2");
        Assert.AreEqual(-10.0, result);
    }

    [TestMethod]
    public void TestUnaryMinus11Interpreted()
    {
        var engine = SonicEngines.InterpretedNoCacheNoOptimizer();
        var result = engine.Evaluate("5*-2");
        Assert.AreEqual(-10.0, result);
    }

    [TestMethod]
    public void TestUnaryMinus12Compiled()
    {
        var engine = SonicEngines.CompiledNoCacheNoOptimizer();
        var result = engine.Evaluate("(-3)^2");
        Assert.AreEqual(9, result);
    }

    [TestMethod]
    public void TestUnaryMinus12Interpreted()
    {
        var engine = SonicEngines.InterpretedNoCacheNoOptimizer();
        var result = engine.Evaluate("(-3)^2");
        Assert.AreEqual(9, result);
    }


    [TestMethod]
    public void TestBuild()
    {
        var engine = Sonic.Evaluator.CreateWithDefaults();
        Func<Dictionary<string, double>, double> function = engine.CreateDelegate("var1+2*(3*age)");

        Dictionary<string, double> variables = new Dictionary<string, double>();
        variables.Add("var1", 2);
        variables.Add("age", 4);

        double result = function(variables);
        Assert.AreEqual(26.0, result);
    }

    [TestMethod]
    public void TestConstantsCompiled()
    {
        var engine = SonicBuilders.Compiled()
            .AddConstant("age", 18.0)
            .Build();

        double result = engine.Evaluate("age+2");
        Assert.AreEqual(20.0, result);
    }

    [TestMethod]
    public void TestConstantsInterpreted()
    {
        var engine = SonicBuilders.Interpreted()
            .AddConstant("age", 18.0)
            .Build();

        double result = engine.Evaluate("age+2");
        Assert.AreEqual(20.0, result);
    }

    [TestMethod]
    public void TestConstantsAndVariablesCompiled()
    {
        var engine = SonicBuilders.Compiled()
            .AddConstant("age", 18.0)
            .Build();

        double result = engine.Evaluate("age+var1", new Dictionary<string, double> { { "var1", 2.0 } });
        Assert.AreEqual(20.0, result);
    }

    [TestMethod]
    public void TestConstantsAndVariablesInterpreted()
    {
        var engine = SonicBuilders.Interpreted()
            .AddConstant("age", 18.0)
            .Build();

        double result = engine.Evaluate("age+var1", new Dictionary<string, double> { { "var1", 2.0 } });
        Assert.AreEqual(20.0, result);
    }

    [TestMethod]
    public void TestExponentiationWithPositiveExponentCompiled()
    {
        var engine = SonicBuilders.Compiled().Build();

        double result = engine.Evaluate("2*10^2");
        Assert.AreEqual(200.0, result);
    }

    [TestMethod]
    public void TestExponentiationWithNegativeExponentCompiled()
    {
        var engine = SonicBuilders.Compiled().Build();

        double result = engine.Evaluate("2*10^-2");
        Assert.AreEqual(0.02, result);
    }

    [TestMethod]
    public void TestExponentiationWithPositiveExponentInterpreted()
    {
        var engine = SonicBuilders.Interpreted().Build();

        double result = engine.Evaluate("2*10^2");
        Assert.AreEqual(200.0, result);
    }

    [TestMethod]
    public void TestExponentiationWithNegativeExponentInterpreted()
    {
        var engine = SonicBuilders.Interpreted().Build();

        double result = engine.Evaluate("2*10^-2");
        Assert.AreEqual(0.02, result);
    }

    [TestMethod]
    public void TestFormulaBuilderInvalidParameterNameUnguarded()
    {
        var engine = Sonic.Evaluator.Create()
            .Build();
        engine.Evaluate("sin+2", new Dictionary<string, double> { { "sin", 2.0 } });
    }

    [TestMethod]
    public void TestFormulaBuilderInvalidParameterNameGuarded()
    {
        AssertExtensions.ThrowsException<ArgumentException>(() =>
        {
            var engine = Sonic.Evaluator.Create()
                .EnableGuardedMode()
                .Build();
            double result = engine.Evaluate("sin+2", new Dictionary<string, double> { { "sin", 2.0 } });
            Assert.AreEqual(4.0, result);
        });
    }

    [TestMethod]
    public void TestVariableNameCollidesWithConstantNameGuardedInCompiledDelegate()
    {
        AssertExtensions.ThrowsException<ArgumentException>(() =>
        {
            var engine = Sonic.Evaluator.Create()
                .EnableGuardedMode()
                .AddConstant("a", 1.0)
                .Build();

            var func = engine.CreateDelegate("a + 2");
            var result = func(new Dictionary<string, double> { { "a", 2.0 } });
        });
    }

    [TestMethod]
    public void TestVariableNameCollidesWithConstantNameUnguardedInCompiledDelegate()
    {
        var engine = Sonic.Evaluator.Create()
            .AddConstant("a", 1.0)
            .Build();

        var func = engine.CreateDelegate("a + 2");
        var result = func(new Dictionary<string, double> { { "a", 2.0 } });
        Assert.AreEqual(3.0, result);
    }

    [TestMethod]
    public void TestVariableNameCollidesWithConstantNameGuardedInInterpretedDelegate()
    {
        AssertExtensions.ThrowsException<ArgumentException>(() =>
        {
            var engine = Sonic.Evaluator.Create()
                .UseExecutionMode(ExecutionMode.Interpreted)
                .EnableGuardedMode()
                .AddConstant("a", 1.0)
                .Build();

            var func = engine.CreateDelegate("a + 2");
            var result = func(new Dictionary<string, double> { { "a", 2.0 } });
        });
    }

    [TestMethod]
    public void TestVariableNameCollidesWithConstantNameUnguardedInInterpretedDelegate()
    {
        var engine = Sonic.Evaluator.Create()
            .UseExecutionMode(ExecutionMode.Interpreted)
            .AddConstant("a", 1.0)
            .Build();

        var func = engine.CreateDelegate("a + 2");
        var result = func(new Dictionary<string, double> { { "a", 2.0 } });
        Assert.AreEqual(3.0, result);
    }

    // Test variable name collides with function name
    [TestMethod]
    public void TestVariableNameCollidesWithFunctionNameGuardedInCompiledDelegate()
    {
        AssertExtensions.ThrowsException<ArgumentException>(() =>
        {
            var engine = Sonic.Evaluator.Create()
                .EnableGuardedMode()
                .AddFunction("a", x => x)
                .Build();

            var func = engine.CreateDelegate("a(2)");
            var result = func(new Dictionary<string, double> { { "a", 2.0 } });
        });
    }

    [TestMethod]
    public void TestVariableNameCollidesWithFunctionNameUnguardedInCompiledDelegate()
    {
        var engine = Sonic.Evaluator.Create()
            .AddFunction("a", x => x)
            .Build();

        var func = engine.CreateDelegate("a(2)");
        var result = func(new Dictionary<string, double> { { "a", 3.0 } });
        Assert.AreEqual(2.0, result);
    }

    [TestMethod]
    public void TestVariableNameCollidesWithFunctionNameGuardedInInterpretedDelegate()
    {
        AssertExtensions.ThrowsException<ArgumentException>(() =>
        {
            var engine = Sonic.Evaluator.Create()
                .UseExecutionMode(ExecutionMode.Interpreted)
                .EnableGuardedMode()
                .AddFunction("a", x => x)
                .Build();

            var func = engine.CreateDelegate("a(2)");
            var result = func(new Dictionary<string, double> { { "a", 3.0 } });
        });
    }

    [TestMethod]
    public void TestVariableNameCollidesWithFunctionNameUnguardedInInterpretedDelegate()
    {
        var engine = Sonic.Evaluator.Create()
            .UseExecutionMode(ExecutionMode.Interpreted)
            .AddFunction("a", x => x)
            .Build();

        var func = engine.CreateDelegate("a(2)");
        var result = func(new Dictionary<string, double> { { "a", 3.0 } });
        Assert.AreEqual(2.0, result);
    }

    [TestMethod]
    public void TestPiMultiplication()
    {
        var engine = SonicEngines.CompiledCaseInsensitive();
        double result = engine.Evaluate("2 * pI");

        Assert.AreEqual(2 * Math.PI, result);
    }

    [TestMethod]
    public void TestReservedVariableNameCaseInsensitiveGuarded()
    {
        AssertExtensions.ThrowsException<ArgumentException>(() =>
        {
            var variables = new Dictionary<string, double> { { "pi", 2.0 } };

            var engine = Sonic.Evaluator.Create()
                .DisableCaseSensitivity()
                .EnableGuardedMode()
                .Build();

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
        var engine = Sonic.Evaluator.CreateWithDefaults();

        double result = engine.Evaluate("1+2-3*4/5+6-7*8/9+0");
        Assert.AreEqual(0.378, Math.Round(result, 3));
    }

    [TestMethod]
    public void TestComplicatedPrecedence2()
    {
        var engine = Sonic.Evaluator.CreateWithDefaults();

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
        Func<Dictionary<string, double>, double> formula = engine.CreateDelegate("var1+2/(3*otherVariablE)");

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
        Func<Dictionary<string, double>, double> formula = engine.CreateDelegate("pi");

        Dictionary<string, double> variables = new Dictionary<string, double>();

        double result = formula(variables);

        Assert.AreEqual(Math.PI, result);
    }

    [TestMethod]
    public void TestConstantBuildInterpreted()
    {
        var engine = SonicEngines.Interpreted();
        Func<Dictionary<string, double>, double> formula = engine.CreateDelegate("pi");

        Dictionary<string, double> variables = new Dictionary<string, double>();

        double result = formula(variables);

        Assert.AreEqual(Math.PI, result);
    }

    [TestMethod]
    public void TestVariableCaseInsensitiveFuncCompiled()
    {
        var engine = SonicEngines.CompiledCaseInsensitive();
        Func<Dictionary<string, double>, double> formula = engine.CreateDelegate("var1+2/(3*otherVariablE)");

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
        var engine = SonicBuilders.Compiled()
            .AddConstant("a", 1)
            .Build();

        var fn = engine.CreateDelegate("a+b+c");
        double result = fn(new Dictionary<string, double> { { "b", 2 }, { "c", 2 } });
        Assert.AreEqual(5.0, result);
    }

    [TestMethod]
    public void TestCalculationFormulaBuildingWithConstants1Interpreted()
    {
        var engine = SonicBuilders.Interpreted()
            .AddConstant("a", 1)
            .Build();
        var fn = engine.CreateDelegate("a+b+c");
        double result = fn(new Dictionary<string, double> { { "b", 2 }, { "c", 2 } });
        Assert.AreEqual(5.0, result);
    }

    [TestMethod]
    public void TestCalculationFormulaBuildingWithConstants2Compiled()
    {
        var engine = SonicBuilders.Compiled()
            .AddConstant("a", 1)
            .AddConstant("A", 2.0)
            .Build();

        var fn = engine.CreateDelegate("a+A");

        double result = fn(new Dictionary<string, double>());
        Assert.AreEqual(3.0, result);
    }

    [TestMethod]
    public void TestCalculationFormulaBuildingWithConstants2Interpreted()
    {
        var engine = SonicBuilders.Interpreted()
            .AddConstant("a", 1)
            .AddConstant("A", 2.0)
            .Build();

        var fn = engine.CreateDelegate("a+A");

        double result = fn(new Dictionary<string, double>());
        Assert.AreEqual(3.0, result);
    }

    [TestMethod]
    public void TestCalculationFormulaBuildingWithConstants3Compiled()
    {
        var engine = SonicBuilders.Compiled()
            .AddConstant("a", 1.0)
            .Build();

        var fn = engine.CreateDelegate("a+A");

        double result = fn(new Dictionary<string, double> { { "A", 2.0 } });
        Assert.AreEqual(3.0, result);
    }

    [TestMethod]
    public void TestCalculationFormulaBuildingWithConstants3Interpreted()
    {
        var engine = SonicBuilders.Interpreted()
            .AddConstant("a", 1.0)
            .Build();

        var fn = engine.CreateDelegate("a+A");

        double result = fn(new Dictionary<string, double> { { "A", 2.0 } });
        Assert.AreEqual(3.0, result);
    }


    [TestMethod]
    public void TestDuplicateConstantDefaultCompiled()
    {
        var engine = SonicBuilders.Compiled()
            .AddConstant("a", 1)
            .AddConstant("a", 2)
            .Build();

        var result = engine.Evaluate("a");
        Assert.AreEqual(2.0, result);
    }

    [TestMethod]
    public void TestDuplicateConstantDefaultInterpreted()
    {
        var engine = SonicBuilders.Interpreted()
            .AddConstant("a", 1)
            .AddConstant("a", 2)
            .Build();

        var result = engine.Evaluate("a");
        Assert.AreEqual(2.0, result);
    }

    [TestMethod]
    public void TestDuplicateConstantCompiled()
    {
        AssertExtensions.ThrowsException<ArgumentException>(() =>
        {
            SonicBuilders.Compiled()
                .EnableGuardedMode()
                .AddConstant("a", 1)
                .AddConstant("a", 2)
                .Build();
        });
    }

    [TestMethod]
    public void TestConflictingConstantAndVariableUnguardedCompiled()
    {
        var engine = SonicBuilders.Compiled()
            .AddConstant("a", 1)
            .Build();

        var result = engine.Evaluate("a+a", new Dictionary<string, double> { { "a", 2 } });
        Assert.AreEqual(2.0, result);
    }

    [TestMethod]
    public void TestConflictingConstantAndVariableGuardedCompiled()
    {
        AssertExtensions.ThrowsException<ArgumentException>(() =>
        {
            var engine = SonicBuilders.Compiled()
                .EnableGuardedMode()
                .AddConstant("a", 1)
                .Build();

            engine.Evaluate("a+a", new Dictionary<string, double> { { "a", 2 } });
        });
    }

    [TestMethod]
    public void TestDuplicateFunctionCompiledUnguarded()
    {
        var engine = SonicBuilders.Compiled()
            .AddFunction("a", x => x)
            .AddFunction("a", x => 2 * x)
            .Build();

        var result = engine.Evaluate("a(1)");
        Assert.AreEqual(2.0, result);
    }

    [TestMethod]
    public void TestDuplicateFunctionCompiledGuarded()
    {
        AssertExtensions.ThrowsException<ArgumentException>(() =>
        {
            SonicBuilders.Compiled()
                .EnableGuardedMode()
                .AddFunction("a", x => x)
                .AddFunction("a", x => 2 * x)
                .Build();
        });
    }

    [TestMethod]
    public void TestConflictingConstantAndFunctionCompiled()
    {
        AssertExtensions.ThrowsException<ArgumentException>(() =>
        {
            var engine = SonicBuilders.Interpreted()
                .EnableGuardedMode()
                .AddFunction("a", x => x)
                .AddConstant("a", 2.0)
                .Build();
        });
    }

    [TestMethod]
    public void TestDuplicateConstantInterpretedUnguarded()
    {
        var engine = SonicBuilders.Interpreted()
            .AddConstant("a", 1)
            .AddConstant("a", 2)
            .Build();

        var result = engine.Evaluate("a");
        Assert.AreEqual(result, 2.0);
    }

    [TestMethod]
    public void TestDuplicateConstantInterpretedGuarded()
    {
        AssertExtensions.ThrowsException<ArgumentException>(() =>
        {
            SonicBuilders.Interpreted()
                .AddConstant("a", 1)
                .AddConstant("a", 2)
                .EnableGuardedMode()
                .Build();
        });
    }

    [TestMethod]
    public void TestConflictingConstantAndVariableUnguardedInterpreted()
    {
        var engine = SonicBuilders.Interpreted()
            .AddConstant("a", 1)
            .Build();

        var result = engine.Evaluate("a+a", new Dictionary<string, double> { { "a", 2 } });
        Assert.AreEqual(2.0, result);
    }

    [TestMethod]
    public void TestConflictingConstantAndVariableGuardedInterpreted()
    {
        AssertExtensions.ThrowsException<ArgumentException>(() =>
        {
            var engine = SonicBuilders.Interpreted()
                .EnableGuardedMode()
                .AddConstant("a", 1)
                .Build();

            engine.Evaluate("a+a", new Dictionary<string, double> { { "a", 2 } });
        });
    }

    [TestMethod]
    public void TestDuplicateFunctionInterpretedGuarded()
    {
        AssertExtensions.ThrowsException<ArgumentException>(() =>
        {
            SonicBuilders.Interpreted()
                .EnableGuardedMode()
                .AddFunction("a", x => x)
                .AddFunction("a", x => 2 * x)
                .Build();
        });
    }

    [TestMethod]
    public void TestDuplicateFunctionInterpretedUnguarded()
    {
        var engine = SonicBuilders.Interpreted()
            .AddFunction("a", x => x)
            .AddFunction("a", x => 2 * x)
            .Build();

        var result = engine.Evaluate("a(1)");
        Assert.AreEqual(2.0, result);
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
        var func = engine.CreateDelegate(expression);
        for (var i = 0; i < 3; i++)
        {
            func(values);
        }

        // assert "does not throw an exception"
        Assert.IsTrue(true);
    }

    [TestMethod]
    public void TestConstantsInNonIdempotentFunctionsCompiled()
    {
        var engine = SonicBuilders.CompiledNoCacheNoOptimizer()
            .AddConstant("a", 1.0)
            // this is idempotent, but the engine does not know that
            .AddFunction("b", x => x + 1, false)
            .Build();

        var result = engine.Evaluate("b(a)");
        Assert.AreEqual(2.0, result);
    }

    [TestMethod]
    public void TestConstantsInNonIdempotentFunctionsInterpreted()
    {
        var engine = SonicBuilders.InterpretedNoCacheNoOptimizer()
            .AddConstant("a", 1.0)
            // this is idempotent, but the engine does not know that
            .AddFunction("b", x => x + 1, false)
            .Build();

        var result = engine.Evaluate("b(a)");
        Assert.AreEqual(2.0, result);
    }

    [TestMethod]
    public void TestParameterlessFunction()
    {
        var engine = SonicBuilders.InterpretedNoCacheNoOptimizer()
            .AddFunction("two", () => 2.0)
            .Build();

        var result = engine.Evaluate("two()");
        Assert.AreEqual(2.0, result);
    }

    [TestMethod]
    public void TestFunctionHasSameNameAsVariable()
    {
        var engine = SonicBuilders.InterpretedNoCacheNoOptimizer()
            .AddFunction("a", () => 2.0)
            .Build();

        var result = engine.Evaluate("a()+a", new Dictionary<string, double> { { "a", 1.0 } });
        Assert.AreEqual(3.0, result);
    }

    [TestMethod]
    public void TestFunctionHasSameNameAsConstant()
    {
        var engine = SonicBuilders.InterpretedNoCacheNoOptimizer()
            .AddFunction("a", () => 2.0)
            .AddConstant("a", 1.0)
            .Build();

        var result = engine.Evaluate("a()+a");
        Assert.AreEqual(3.0, result);
    }

    [TestMethod]
    public void TestFunctionHasSameNameAsVariableGuarded()
    {
        var engine = SonicBuilders.InterpretedNoCacheNoOptimizer()
            .AddFunction("a", () => 2.0)
            .EnableGuardedMode()
            .Build();


        AssertExtensions.ThrowsException<ArgumentException>(() =>
        {
            engine.Evaluate("a()+a", new Dictionary<string, double> { { "a", 1.0 } });
        });
    }

    [TestMethod]
    public void TestFunctionHasSameNameAsConstantGuarded()
    {
        AssertExtensions.ThrowsException<ArgumentException>(() =>
        {
            SonicBuilders.InterpretedNoCacheNoOptimizer()
                .AddFunction("a", () => 2.0)
                .AddConstant("a", 1.0)
                .EnableGuardedMode()
                .Build();
        });
    }

    [TestMethod]
    public void TestCalculateFoldedExpressionWithoutFoldedVariable()
    {
        var engine = SonicEngines.Interpreted();
        var result = engine.Evaluate("0 * a + 4 * 5");
    }

    [TestMethod]
    public void TestVariableValidationPass()
    {
        var engine = SonicEngines.Interpreted();
        var expression = "a + b + c";
        var variables = new List<string> { "a", "b", "c" };
        engine.Validate(expression, variables);

        // assert "does not throw an exception"
        Assert.IsTrue(true);
    }

    [TestMethod]
    public void TestVariableValidationFail()
    {
        var engine = SonicEngines.Interpreted();
        var expression = "a + b + c";
        var variables = new List<string> { "a", "b" };
        Assert.ThrowsException<VariableNotDefinedException>(() =>
            engine.Validate(expression, variables)
        );
    }
    
    // Issue #42
    [TestMethod]
    public void TestStaticFuncInDynamicFunc()
    {
        var engine = SonicEngines.Interpreted();
        var expression = "min(if(a==1, 2, 3))";
        var variables = new List<string> { "a" };
        engine.Validate(expression, variables);
        
        // assert "does not throw an exception"
        Assert.IsTrue(true);
    }
}

internal static class SonicEngines
{
    public static Sonic.Evaluator CompiledNoOptimizerCaseInsensitive() => Sonic.Evaluator.Create()
        .UseCulture(CultureInfo.InvariantCulture)
        .UseExecutionMode(ExecutionMode.Compiled)
        .EnableCache()
        .DisableOptimizer()
        .DisableCaseSensitivity()
        .Build();

    public static Sonic.Evaluator CompiledCaseInsensitive() => Sonic.Evaluator.Create()
        .UseCulture(CultureInfo.InvariantCulture)
        .UseExecutionMode(ExecutionMode.Compiled)
        .EnableCache()
        .EnableOptimizer()
        .DisableCaseSensitivity()
        .Build();

    public static Sonic.Evaluator CompiledNoCacheNoOptimizerCaseInsensitive() => Sonic.Evaluator.Create()
        .UseCulture(CultureInfo.InvariantCulture)
        .UseExecutionMode(ExecutionMode.Compiled)
        .DisableCache()
        .DisableOptimizer()
        .DisableCaseSensitivity()
        .Build();

    public static Sonic.Evaluator CompiledNoCacheNoOptimizer() => Sonic.Evaluator.Create()
        .UseCulture(CultureInfo.InvariantCulture)
        .UseExecutionMode(ExecutionMode.Compiled)
        .DisableCache()
        .DisableOptimizer()
        .EnableCaseSensitivity()
        .Build();

    public static Sonic.Evaluator Compiled() => Sonic.Evaluator.Create()
        .UseCulture(CultureInfo.InvariantCulture)
        .UseExecutionMode(ExecutionMode.Compiled)
        .EnableCache()
        .EnableOptimizer()
        .EnableCaseSensitivity()
        .Build();

    public static Sonic.Evaluator InterpretedNoCacheNoOptimizerCaseInsensitive() => Sonic.Evaluator.Create()
        .UseCulture(CultureInfo.InvariantCulture)
        .UseExecutionMode(ExecutionMode.Interpreted)
        .DisableCache()
        .DisableOptimizer()
        .DisableCaseSensitivity()
        .Build();

    public static Sonic.Evaluator InterpretedNoOptimizerCaseInsensitive() => Sonic.Evaluator.Create()
        .UseCulture(CultureInfo.InvariantCulture)
        .UseExecutionMode(ExecutionMode.Interpreted)
        .EnableCache()
        .DisableOptimizer()
        .DisableCaseSensitivity()
        .Build();

    public static Sonic.Evaluator InterpretedNoCacheNoOptimizer() => Sonic.Evaluator.Create()
        .UseCulture(CultureInfo.InvariantCulture)
        .UseExecutionMode(ExecutionMode.Interpreted)
        .DisableCache()
        .DisableOptimizer()
        .EnableCaseSensitivity()
        .Build();

    public static Sonic.Evaluator InterpretedCaseInsensitive() => Sonic.Evaluator.Create()
        .UseCulture(CultureInfo.InvariantCulture)
        .UseExecutionMode(ExecutionMode.Interpreted)
        .EnableCache()
        .EnableOptimizer()
        .DisableCaseSensitivity()
        .Build();

    public static Sonic.Evaluator Interpreted() => Sonic.Evaluator.Create()
        .UseCulture(CultureInfo.InvariantCulture)
        .UseExecutionMode(ExecutionMode.Interpreted)
        .EnableCache()
        .EnableOptimizer()
        .EnableCaseSensitivity()
        .Build();
}

internal static class SonicBuilders
{
    public static EvaluatorBuilder CompiledNoOptimizerCaseInsensitive() => Sonic.Evaluator.Create()
        .UseCulture(CultureInfo.InvariantCulture)
        .UseExecutionMode(ExecutionMode.Compiled)
        .EnableCache()
        .DisableOptimizer()
        .DisableCaseSensitivity();

    public static EvaluatorBuilder CompiledNoCacheNoOptimizerCaseInsensitive() => Sonic.Evaluator.Create()
        .UseCulture(CultureInfo.InvariantCulture)
        .UseExecutionMode(ExecutionMode.Compiled)
        .DisableCache()
        .DisableOptimizer()
        .DisableCaseSensitivity();

    public static EvaluatorBuilder CompiledNoCacheNoOptimizer() => Sonic.Evaluator.Create()
        .UseCulture(CultureInfo.InvariantCulture)
        .UseExecutionMode(ExecutionMode.Compiled)
        .DisableCache()
        .DisableOptimizer()
        .EnableCaseSensitivity();

    public static EvaluatorBuilder Compiled() => Sonic.Evaluator.Create()
        .UseCulture(CultureInfo.InvariantCulture)
        .UseExecutionMode(ExecutionMode.Compiled)
        .EnableCache()
        .EnableOptimizer()
        .EnableCaseSensitivity();

    public static EvaluatorBuilder InterpretedNoCacheNoOptimizerCaseInsensitive() => Sonic.Evaluator.Create()
        .UseCulture(CultureInfo.InvariantCulture)
        .UseExecutionMode(ExecutionMode.Interpreted)
        .DisableCache()
        .DisableOptimizer()
        .DisableCaseSensitivity();

    public static EvaluatorBuilder InterpretedNoOptimizerCaseInsensitive() => Sonic.Evaluator.Create()
        .UseCulture(CultureInfo.InvariantCulture)
        .UseExecutionMode(ExecutionMode.Interpreted)
        .EnableCache()
        .DisableOptimizer()
        .DisableCaseSensitivity();

    public static EvaluatorBuilder InterpretedNoCacheNoOptimizer() => Sonic.Evaluator.Create()
        .UseCulture(CultureInfo.InvariantCulture)
        .UseExecutionMode(ExecutionMode.Interpreted)
        .DisableCache()
        .DisableOptimizer()
        .EnableCaseSensitivity();

    public static EvaluatorBuilder Interpreted() => Sonic.Evaluator.Create()
        .UseCulture(CultureInfo.InvariantCulture)
        .UseExecutionMode(ExecutionMode.Interpreted)
        .EnableCache()
        .EnableOptimizer()
        .EnableCaseSensitivity();
}