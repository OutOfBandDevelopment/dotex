using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Caching.Tests.Providers;
using OoBDev.TestUtilities;
using OoBDev.TestUtilities.Logging;
using System;
using System.Threading.Tasks;

namespace OoBDev.Caching.Tests.Examples;

// https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.caching.memory.imemorycache?view=dotnet-plat-ext-5.0
[TestClass]
public class ExampleTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod]
    [TestCategory(TestCategories.Simulate)]
    public async Task CachingDesignTest()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection()
            .Build();

        var services = new ServiceCollection()
            .AddSingleton<IConfiguration>(configuration)
            .AddTestLoggingServices(TestContext)
            .AddOptions() //this needs added to startup projects
            .TryAddCachingServices()
            .AddSingleton<ICachingProvider, NullCachingProvider>() // Default no-op provider for tests
            //.AddMicrosoftCachingServices()
            //.AddRedisCachingServices()
            //.AddToolkitServices()

        // register cacheable classes
            .AddTransient(sp => sp.Cacheable<IExampleRepository, ExampleRepository>())
        ;

        var serviceProvider = services.BuildServiceProvider();

        var example = serviceProvider.GetRequiredService<IExampleRepository>();

        var result1 = await example.GetData("test1", "test2");
        var result2 = await example.GetData("test1", "test3", 1, 2, 3, 4);
        var result3 = await example.GetDataSet("test1", "test2");
        await example.UpdateData("test1", "test2");
        await example.UpdateData(result1);
        await example.UpdateData2("test1", "test2");

        var result4 = example.NotTask("test1", "test2");

        //await example.NotSupported1();
        //example.NotSupported2();
    }
}

public class ReturnModel
{
    public string Property1 { get; set; }
    public string Property2 { get; set; }

    public string Param1 { get; set; }
    public string Param2 { get; set; }
}
public interface IExampleRepository
{
    Task<ReturnModel> GetData(string param1, string param2, int a, int b, int c, int d);
    Task<ReturnModel[]> GetDataSet(string param1, string param2);
    Task<ReturnModel> GetData(string param1, string param2);
    ReturnModel NotTask(string param1, string param2);
    Task UpdateData(ReturnModel model);
    Task UpdateData(string param1, string param2);
    Task UpdateData2(string param1, string param2);

    Task NotSupported1() { throw new NotImplementedException(); }
    void NotSupported2() { throw new NotImplementedException(); }
}
public class ExampleRepository : IExampleRepository
{
    public ReturnModel NotTask(string param1, string param2) =>
       new ReturnModel
       {
           Param1 = param1,
           Param2 = param2,

           Property1 = DateTimeOffset.Now.ToString(),
       };

    public Task<ReturnModel> GetData(string param1, string param2, int a, int b, int c, int d) =>
       Task.FromResult(new ReturnModel
       {
           Param1 = param1,
           Param2 = param2,

           Property1 = DateTimeOffset.Now.ToString(),
           Property2 = string.Join("_", a, b, c, d),
       });

    [IsCacheable("bucket1/set/{param1}/{param2}", "00:05:00")]
    public Task<ReturnModel[]> GetDataSet(string param1, string param2) =>
       Task.FromResult(new[] {new
           ReturnModel
               {
                   Param1 = param1,
                   Param2 = param2,

                   Property1 = DateTimeOffset.Now.ToString(),
                   Property2 = Guid.NewGuid().ToString(),
               }
       });

    [IsCacheable("bucket1/data/{param1}/{param2}", "01:00:00")]
    public Task<ReturnModel> GetData(string param1, string param2) =>
       Task.FromResult(new ReturnModel
       {
           Param1 = param1,
           Param2 = param2,

           Property1 = DateTimeOffset.Now.ToString(),
           Property2 = Guid.NewGuid().ToString(),
       });

    [FlushCache("bucket1/data/{model.Param1}/{model.Param2}")]
    public Task UpdateData(ReturnModel model) => Task.FromResult(0);

    [FlushCache("bucket1/data/{param1}/{param2}")]
    public Task UpdateData(string param1, string param2) => Task.FromResult(0);

    // The intention is to get the key from the related method... but unless the 
    // parameters match I'm not sure how I would fill out the missing key values
    [FlushCache(typeof(ExampleRepository), nameof(GetData))]
    public Task UpdateData2(string param1, string param2) => Task.FromResult(0);

    [IsCacheable("bucket1/data/{param1}/{param2}", "00:05:00")]
    public Task NotSupported1() { throw new NotImplementedException(); }

    [IsCacheable("bucket1/data/{param1}/{param2}", "00:05:00")]
    public void NotSupported2() { throw new NotImplementedException(); }
}
