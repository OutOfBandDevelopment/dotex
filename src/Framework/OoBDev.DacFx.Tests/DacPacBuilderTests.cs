using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.DacFx.Tests;

[TestClass]
public class DacPacBuilderTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod]
    public void BuildPackageTest()
    {
        var logger = new LoggerFactory().CreateLogger<DacPacBuilder>();
        var builder = new DacPacBuilder(logger);
        builder.BuildDacPac(
            assemblyFileFramework: @"C:\Repos\oobdev\dotex\src\Extensions\OoBDev.Data.Vectors\bin\Debug\net481\OoBDev.Data.Vectors.dll"
            //assemblyPdbFramework: @"C:\Repos\oobdev\dotex\src\Extensions\OoBDev.Data.Vectors\bin\Debug\net481\OoBDev.Data.Vectors.pdb",
            //dacpacFile: @"C:\Repos\oobdev\dotex\src\Extensions\OoBDev.Data.Vectors\bin\Debug\netstandard2.0\OoBDev.Data.Vectors.dacpac",
            //projectName: "OoBDev.Data.Vectors",
            //projectVersion: "0.0.0.1"
            );
    }
}
