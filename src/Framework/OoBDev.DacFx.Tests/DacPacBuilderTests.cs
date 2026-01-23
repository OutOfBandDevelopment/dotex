using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;

namespace OoBDev.DacFx.Tests;

[TestClass]
public class DacPacBuilderTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod, TestCategory(TestCategories.DevLocal)]
    public void BuildPackageTest()
    {
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Debug);
        });
        var loggerBuilder = loggerFactory.CreateLogger<DacPacBuilder>();
        var loggerValidator = loggerFactory.CreateLogger<DacPacValidator>();
        var builder = new DacPacBuilder(loggerBuilder, new DacPacValidator(loggerValidator));
        builder.BuildDacPac(
            assemblyFileFramework: @"C:\repo\merge-em\dotex\src\Extensions\OoBDev.Data.Vectors.Net481\bin\Debug\net48\OoBDev.Data.Vectors.dll"
            //assemblyPdbFramework: @"C:\Repos\oobdev\dotex\src\Extensions\OoBDev.Data.Vectors\bin\Debug\net481\OoBDev.Data.Vectors.pdb",
            //dacpacFile: @"C:\Repos\oobdev\dotex\src\Extensions\OoBDev.Data.Vectors\bin\Debug\netstandard2.0\OoBDev.Data.Vectors.dacpac",
            //projectName: "OoBDev.Data.Vectors",
            //projectVersion: "0.0.0.1"
            );
    }
}
