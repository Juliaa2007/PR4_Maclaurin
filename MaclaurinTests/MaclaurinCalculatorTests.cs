using Microsoft.VisualStudio.TestTools.UnitTesting;
using MaclaurinConsole;

namespace MaclaurinTests;

[TestClass]
public class MaclaurinCalculatorTests
{
    [TestMethod]
    public void Exp_Zero_ReturnsOne()
    {
        Assert.AreEqual(1.0, MaclaurinCalculator.Calculate(SeriesFunction.Exp, 0, 5), 1e-12);
    }

    [TestMethod]
    public void Exp_OneWithManyTerms_ApproximatesE()
    {
        double actual = MaclaurinCalculator.Calculate(SeriesFunction.Exp, 1, 20);
        Assert.AreEqual(Math.E, actual, 1e-10);
    }

    [TestMethod]
    public void Power_TwoToThird_ApproximatesEight()
    {
        double actual = MaclaurinCalculator.Calculate(SeriesFunction.Power, 3, 30, 2);
        Assert.AreEqual(8.0, actual, 1e-10);
    }

    [TestMethod]
    public void LnOnePlusX_Half_ApproximatesLog()
    {
        double actual = MaclaurinCalculator.Calculate(SeriesFunction.LnOnePlusX, 0.5, 100);
        Assert.AreEqual(Math.Log(1.5), actual, 1e-4);
    }

    [TestMethod]
    public void LnRatio_Half_ApproximatesExpected()
    {
        double actual = MaclaurinCalculator.Calculate(SeriesFunction.LnRatio, 0.5, 100);
        Assert.AreEqual(Math.Log(3.0), actual, 1e-4);
    }

    [TestMethod]
    public void ZeroTerms_Exp_ReturnsFirstTerm()
    {
        Assert.AreEqual(1.0, MaclaurinCalculator.Calculate(SeriesFunction.Exp, 5, 0), 1e-12);
    }

    [TestMethod]
    public void NegativeTermCount_Throws()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(
            () => MaclaurinCalculator.Calculate(SeriesFunction.Exp, 1, -1));
    }

    [TestMethod]
    public void TooManyTerms_Throws()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(
            () => MaclaurinCalculator.Calculate(SeriesFunction.Exp, 1, 10001));
    }

    [DataTestMethod]
    [DataRow(-1.0)]
    [DataRow(-1.5)]
    [DataRow(1.01)]
    public void LnOnePlusX_OutOfDomain_Throws(double x)
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(
            () => MaclaurinCalculator.Calculate(SeriesFunction.LnOnePlusX, x, 20));
    }

    [DataTestMethod]
    [DataRow(-1.0)]
    [DataRow(1.0)]
    [DataRow(1.2)]
    public void LnRatio_OutOfDomain_Throws(double x)
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(
            () => MaclaurinCalculator.Calculate(SeriesFunction.LnRatio, x, 20));
    }

    [TestMethod]
    public void Power_NonPositiveBase_Throws()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(
            () => MaclaurinCalculator.Calculate(SeriesFunction.Power, 2, 10, 0));
    }

    [TestMethod]
    public void InvalidFunction_Throws()
    {
        Assert.ThrowsException<ArgumentException>(
            () => MaclaurinCalculator.Calculate((SeriesFunction)99, 1, 10));
    }
}
