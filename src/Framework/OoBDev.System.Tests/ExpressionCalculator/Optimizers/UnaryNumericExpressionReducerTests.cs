using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.ExpressionCalculator.Expressions;
using OoBDev.System.ExpressionCalculator.Optimizers;
using OoBDev.TestUtilities;

namespace OoBDev.System.Tests.ExpressionCalculator.Optimizers;

[TestClass]
public class UnaryNumericExpressionReducerTests
{
    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Optimize_DoubleNegation_DropsBothSigns()
    {
        // Stage
        var expression = new UnaryOperatorExpression<int>(UnaryOperators.Negate,
            new UnaryOperatorExpression<int>(UnaryOperators.Negate, new VariableExpression<int>("x")));

        // Test
        var result = new UnaryNumericExpressionReducer<int>().Optimize(expression);

        // Assert
        Assert.IsInstanceOfType<InnerExpression<int>>(result);
        Assert.AreEqual("x", ((InnerExpression<int>)result).Expression.ToString());
    }

    [TestMethod, TestCategory(TestCategories.Unit), TestCategory(TestCategories.BugRegression)]
    public void Optimize_FactorialOfNegation_KeepsBothOperators()
    {
        // Stage
        var expression = new UnaryOperatorExpression<int>(UnaryOperators.Factorial,
            new UnaryOperatorExpression<int>(UnaryOperators.Negate, new VariableExpression<int>("x")));

        // Test
        var result = new UnaryNumericExpressionReducer<int>().Optimize(expression);

        // Assert
        var outer = Assert.IsInstanceOfType<UnaryOperatorExpression<int>>(result);
        Assert.AreEqual(UnaryOperators.Factorial, outer.Operator);
        var inner = Assert.IsInstanceOfType<UnaryOperatorExpression<int>>(outer.Operand);
        Assert.AreEqual(UnaryOperators.Negate, inner.Operator);
    }
}
