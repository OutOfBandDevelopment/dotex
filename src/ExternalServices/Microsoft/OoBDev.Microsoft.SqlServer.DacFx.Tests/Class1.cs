using Microsoft.SqlServer.Dac;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.Microsoft.SqlServer.DacFx.Tests;

[TestClass]
public class Tests
{
    public required TestContext TestContext { get; set; }

    [TestMethod]
    public void Test()
    {
        var model = new TSqlModel(
            SqlServerVersion.Sql160, new TSqlModelOptions
            {
                ServiceBrokerOption = ServiceBrokerOption.EnableBroker,
            });

        model.AddObjects(@"CREATE TABLE [dbo].[Names](
	[NameID] [bigint] IDENTITY(1,1) NOT NULL,
	[NameValue] [nvarchar](200) NOT NULL)");

        DacPackageExtensions.BuildPackage("test.dacpac", model, new PackageMetadata
        {
            Description = "test project",
            Name = "Test name",
            Version = "1.2.3.4"
        }, new PackageOptions
        {
            IgnoreValidationErrors = ["SQL70557", "SR0025"],             
        });
    }
}
