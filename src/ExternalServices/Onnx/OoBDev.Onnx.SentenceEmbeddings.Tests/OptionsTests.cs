using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using System;

namespace OoBDev.Onnx.SentenceEmbeddings.Tests;

[TestClass]
public class OptionsTests
{
    [TestCategory(TestCategories.Unit)]
    [TestMethod]
    public void Validate_Defaults_Pass() => new OnnxSentenceEmbeddingOptions().Validate();

    [TestCategory(TestCategories.Unit)]
    [TestMethod]
    [DataRow(nameof(OnnxSentenceEmbeddingOptions.MaxSequenceLength), 2)]
    [DataRow(nameof(OnnxSentenceEmbeddingOptions.MaxBatchSize), 0)]
    [DataRow(nameof(OnnxSentenceEmbeddingOptions.MaxConcurrentInferences), 0)]
    [DataRow(nameof(OnnxSentenceEmbeddingOptions.IntraOpThreads), -1)]
    public void Validate_OutOfRange_Throws(string property, int value)
    {
        var options = new OnnxSentenceEmbeddingOptions();
        typeof(OnnxSentenceEmbeddingOptions).GetProperty(property)!.SetValue(options, value);

        var ex = Assert.Throws<ArgumentException>(options.Validate);
        StringAssert.Contains(ex.Message, property);
    }

    [TestCategory(TestCategories.Unit)]
    [TestMethod]
    public void Validate_DimensionsWithoutMatryoshka_Throws()
    {
        var options = new OnnxSentenceEmbeddingOptions { Dimensions = 128 };

        Assert.Throws<ArgumentException>(options.Validate);
    }

    [TestCategory(TestCategories.Unit)]
    [TestMethod]
    public void Validate_DimensionsWithMatryoshka_Passes() =>
        new OnnxSentenceEmbeddingOptions { Dimensions = 128, SupportsDimensionTruncation = true }.Validate();
}
