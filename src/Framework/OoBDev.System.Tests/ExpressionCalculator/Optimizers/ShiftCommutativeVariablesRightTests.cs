using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.ExpressionCalculator.Expressions;
using OoBDev.System.ExpressionCalculator.Optimizers;
using OoBDev.TestUtilities;

namespace OoBDev.System.Tests.ExpressionCalculator.Optimizers;

[TestClass]
public class ShiftCommutativeVariablesRightTests
{
    private static ExpressionBase<int> Num(int value) => new NumberExpression<int>(value);
    private static ExpressionBase<int> Var(string name) => new VariableExpression<int>(name);
    private static ExpressionBase<int> Bin(ExpressionBase<int> left, BinaryOperators op, ExpressionBase<int> right) =>
        new BinaryOperatorExpression<int>(left, op, right);

    private static ExpressionBase<int> Optimize(ExpressionBase<int> expression) =>
        new ShiftCommutativeVariablesRight<int>().Optimize(expression);

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Optimize_VariableBeforeNumberInAdd_MovesNumberFirst()
    {
        // Stage
        var expression = Bin(Var("x"), BinaryOperators.Add, Num(2));

        // Test
        var result = Optimize(expression);

        // Assert
        Assert.AreEqual("2 + x", result.ToString());
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Optimize_VariablesInMultiply_OrdersAlphabetically()
    {
        // Stage
        var expression = Bin(Var("y"), BinaryOperators.Multiply, Var("x"));

        // Test
        var result = Optimize(expression);

        // Assert
        Assert.AreEqual("x * y", result.ToString());
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Optimize_NonCommutativeSubtract_KeepsOrder()
    {
        // Stage
        var expression = Bin(Var("x"), BinaryOperators.Subtract, Num(2));

        // Test
        var result = Optimize(expression);

        // Assert
        Assert.AreEqual("x - 2", result.ToString());
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Optimize_NestedSameOperator_FlattensAndSorts()
    {
        // Stage
        var expression = Bin(Bin(Var("b"), BinaryOperators.Add, Num(3)), BinaryOperators.Add, Bin(Var("a"), BinaryOperators.Add, Num(1)));

        // Test
        var result = Optimize(expression);

        // Assert
        Assert.AreEqual("1 + 3 + a + b", result.ToString());
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Optimize_ThreeOperands_BuildsLeftAssociativeTree()
    {
        // Stage
        var expression = Bin(Bin(Var("c"), BinaryOperators.Add, Var("a")), BinaryOperators.Add, Var("b"));

        // Test
        var result = Optimize(expression);

        // Assert
        var top = Assert.IsInstanceOfType<BinaryOperatorExpression<int>>(result);
        Assert.AreEqual("c", top.Right.ToString());
        Assert.AreEqual("a + b", top.Left.ToString());
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Optimize_FiveOperands_SortsAll()
    {
        // Stage
        var expression = Bin(Bin(Bin(Bin(Var("e"), BinaryOperators.Add, Var("d")), BinaryOperators.Add, Var("c")), BinaryOperators.Add, Var("b")), BinaryOperators.Add, Var("a"));

        // Test
        var result = Optimize(expression);

        // Assert
        Assert.AreEqual("a + b + c + d + e", result.ToString());
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Optimize_InsideInnerAndUnary_ReordersChildren()
    {
        // Stage
        var inner = new InnerExpression<int>(Bin(Var("x"), BinaryOperators.Add, Num(1)));
        var expression = new UnaryOperatorExpression<int>(UnaryOperators.Negate, inner);

        // Test
        var result = Optimize(expression);

        // Assert
        var unary = Assert.IsInstanceOfType<UnaryOperatorExpression<int>>(result);
        Assert.AreEqual(UnaryOperators.Negate, unary.Operator);
        Assert.AreEqual("1 + x", Assert.IsInstanceOfType<InnerExpression<int>>(unary.Operand).Expression.ToString());
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Optimize_SingleNumber_ReturnsSameExpression()
    {
        // Stage
        var expression = Num(7);

        // Test
        var result = Optimize(expression);

        // Assert
        Assert.AreSame(expression, result);
    }
}
