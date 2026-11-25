using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using Qdrant.Client.Grpc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OoBDev.Qdrant.Tests;

[TestClass]
public class QdrantOptionsTests
{
    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void New_Defaults_CosineAndThousandResults()
    {
        // Stage
        var options = new QdrantOptions { Url = "http://localhost:6334" };

        // Test / Assert
        Assert.AreEqual(Distance.Cosine, options.Distance);
        Assert.AreEqual(1000, options.ResultLimit);
        Assert.IsFalse(options.EnsureCollectionExists);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Validate_ZeroResultLimit_Fails()
    {
        // Stage
        var options = new QdrantOptions { Url = "http://localhost:6334", ResultLimit = 0 };
        var results = new List<ValidationResult>();

        // Test
        var valid = Validator.TryValidateObject(options, new ValidationContext(options), results, validateAllProperties: true);

        // Assert
        Assert.IsFalse(valid);
        CollectionAssert.Contains(new List<string>(results[0].MemberNames), nameof(QdrantOptions.ResultLimit));
    }
}
