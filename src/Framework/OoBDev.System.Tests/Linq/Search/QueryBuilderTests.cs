using OoBDev.System.Linq.Expressions;
using OoBDev.System.Linq.Search;
using OoBDev.System.ResponseModel;
using OoBDev.System.Tests.Linq.TestTargets;
using OoBDev.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace OoBDev.System.Tests.Linq.Search;

[TestClass]
public class QueryBuilderTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void ExecuteByTest_IQueryable()
    {
        //Setup
        var queryable = TestDataBuilder.GetTestData(typeof(TestTargetModel), 0);

        var query = new SearchQuery
        {
            SearchTerm = "Search For",

            Filter = new()
            {
                { nameof(TestTargetModel.Name), new FilterParameter(){
                    EqualTo = "Name1",
                } },
                { "DoesntExist", new FilterParameter(){
                    EqualTo = "DoesntExistValue",
                } },
            },

            OrderBy = new()
            {
                { nameof(TestTargetModel.Name), OrderDirections.Descending },
                { nameof(TestTargetModel.Email), OrderDirections.Descending },
                { "DoesntExist", OrderDirections.Ascending },
            }
        };

        //Mock

        var capture = new CaptureResultMessage();

        var mock = new MockRepository(MockBehavior.Strict);

        var sortBuilder = mock.Create<ISortBuilder<TestTargetModel>>();
        var expressionBuilder = mock.Create<IExpressionTreeBuilder<TestTargetModel>>();
        var postBuildVisitors = Array.Empty<IPostBuildExpressionVisitor>();

        sortBuilder.Setup(s => s.SortBy(
            It.IsAny<IQueryable<TestTargetModel>>(),
            It.IsAny<ISortQuery>(),
            It.IsAny<IExpressionTreeBuilder<TestTargetModel>>(),
            It.IsAny<StringComparison>()
            )).Returns(queryable.Cast<TestTargetModel>().OrderBy(_ => 1));

        expressionBuilder.Setup(s => s.BuildExpression(
            It.Is<object>(o => string.Equals(query.SearchTerm, o as string)),
            It.IsAny<StringComparison>(),
            It.Is<bool>(o => o)
            )).Returns(_ => true);

        expressionBuilder.Setup(s => s.GetPredicateExpression(
            It.Is<string>(o => o == nameof(TestTargetModel.Name)),
            It.IsAny<FilterParameter>(),
            It.IsAny<StringComparison>(),
            It.Is<bool>(o => !o)
            )).Returns(_ => true);

        expressionBuilder.Setup(s => s.GetPredicateExpression(
            It.Is<string>(o => o == "DoesntExist"),
            It.IsAny<FilterParameter>(),
            It.IsAny<StringComparison>(),
            It.Is<bool>(o => !o)
            )).Returns((Expression<Func<TestTargetModel, bool>>?)null);

        var queryBuilder = new QueryBuilder<TestTargetModel>(
            sortBuilder: sortBuilder.Object,
            expressionBuilder: expressionBuilder.Object,
            postBuildVisitors: postBuildVisitors,
            logger: TestLogger.CreateLogger<QueryBuilder>(),
            capture: capture
            );

        // Test
        var result = queryBuilder.ExecuteBy(queryable, query);

        foreach (var item in capture.Capture())
            TestContext.WriteLine(item.ToString());

        // Assert
        Assert.IsNotNull(result);
    }
}
