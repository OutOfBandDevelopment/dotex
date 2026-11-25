using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.Execution;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.AspNetCore.Authorization;
using OoBDev.Search;
using OoBDev.Search.Requests;
using OoBDev.System.Security.Claims;
using OoBDev.TestUtilities;

namespace OoBDev.Spike.GraphQl;

[TestClass]
public class GraphQlSpikeTests
{
    public TestContext TestContext { get; set; } = null!;

    private sealed class RecordingProvider : ISearchProvider
    {
        public SearchRequest? Last { get; private set; }

        public IReadOnlySet<string> SupportedFilters { get; } = new HashSet<string> { "ext" };

        public Task<SearchResult> SearchAsync(SearchRequest request, CancellationToken cancellationToken = default)
        {
            Last = request;
            var items = new[] { "a", "b", "c" }.Take(request.PageSize).Select(id => new SearchItem(id, 1, new SearchExplanation())).ToArray();
            return Task.FromResult(new SearchResult(items, request.PageSize < 3 ? "next-1" : null));
        }
    }

    private static ClaimsPrincipal User(params string[] rights)
        => new(new ClaimsIdentity(rights.Select(r => new Claim(CommonClaims.ApplicationRight, r)), "test"));

    private static async Task<(IRequestExecutor Executor, RecordingProvider Provider)> BuildAsync()
    {
        var provider = new RecordingProvider();
        var services = new ServiceCollection()
            .AddSingleton<ISearchProvider>(provider)
            .AddSingleton<IApplicationRightChecker, ApplicationRightChecker>();
        services.AddGraphQL().AddQueryType<SearchQuery>();
        var executor = await services.BuildServiceProvider().GetRequiredService<IRequestExecutorResolver>().GetRequestExecutorAsync();
        return (executor, provider);
    }

    private async Task<string> RunAsync(IRequestExecutor executor, string query, ClaimsPrincipal? principal)
    {
        var builder = OperationRequestBuilder.New().SetDocument(query);
        if (principal is not null)
            builder.SetGlobalState(GraphQlApplicationRightAttribute.PrincipalKey, principal);
        var result = await executor.ExecuteAsync(builder.Build());
        var json = result.ToJson();
        TestContext.WriteLine(json);
        return json;
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task Search_TextSyntaxInOneArgument_ReachesTheSameParser()
    {
        var (executor, provider) = await BuildAsync();

        var json = await RunAsync(executor, "{ search(text: \"ext:pdf -tag:draft plain\", first: 2) { nodes { id } endCursor hasNextPage } }", User("search.read"));

        Assert.IsTrue(json.Replace(" ", "").Contains("\"hasNextPage\":true"));
        Assert.AreEqual(2, provider.Last!.PageSize);
        Assert.AreEqual(2, provider.Last.Filters.Count);
        Assert.AreEqual("plain", provider.Last.Text);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task Search_TypedFilter_BuildsTheSameRequestWithoutTheTextSyntax()
    {
        var (executor, provider) = await BuildAsync();

        await RunAsync(executor, "{ searchTyped(text: \"beach\", ext: \"jpg\", first: 3) { nodes { id } } }", User("search.read"));

        Assert.AreEqual("ext", provider.Last!.Filters.Single().Name);
        Assert.AreEqual("beach", provider.Last.Text);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task Search_CursorArgument_IsPassedThroughUnchanged()
    {
        var (executor, provider) = await BuildAsync();

        await RunAsync(executor, "{ search(first: 2, after: \"next-1\") { nodes { id } } }", User("search.read"));

        Assert.AreEqual("next-1", provider.Last!.Cursor);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task Search_WithoutTheRight_IsForbiddenAndNeverCallsTheProvider()
    {
        var (executor, provider) = await BuildAsync();

        var json = await RunAsync(executor, "{ search(text: \"x\") { nodes { id } } }", User("other.right"));

        Assert.Contains("AUTH_FORBIDDEN", json);
        Assert.IsNull(provider.Last);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task Search_WithoutPrincipal_IsUnauthenticated()
    {
        var (executor, provider) = await BuildAsync();

        var json = await RunAsync(executor, "{ search(text: \"x\") { nodes { id } } }", null);

        Assert.Contains("AUTH_NOT_AUTHENTICATED", json);
        Assert.IsNull(provider.Last);
    }
}
