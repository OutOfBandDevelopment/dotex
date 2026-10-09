using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.AI;
using OoBDev.SBert.AllMpnetBaseV2;
using OoBDev.SBert.NomicEmbedTextV1_5;
using OoBDev.TestUtilities;
using System;
using System.Linq;

namespace OoBDev.Onnx.SentenceEmbeddings.Tests;

[TestClass]
public class PresetTests
{
    [TestCategory(TestCategories.Unit)]
    [TestMethod]
    public void LayerNormalize_GivesZeroMeanAndUnitVariance()
    {
        float[] vector = [1f, 2f, 3f, 4f];

        EmbeddingMath.LayerNormalize(vector);

        Assert.AreEqual(0f, vector.Sum(), 1e-5f);
        Assert.AreEqual(1f, vector.Sum(v => v * v) / vector.Length, 1e-3f);
    }

    [TestCategory(TestCategories.Unit)]
    [TestMethod]
    [DataRow(nameof(OnnxSentenceEmbeddingOptions.ClsToken))]
    [DataRow(nameof(OnnxSentenceEmbeddingOptions.SepToken))]
    [DataRow(nameof(OnnxSentenceEmbeddingOptions.UnkToken))]
    [DataRow(nameof(OnnxSentenceEmbeddingOptions.PadToken))]
    [DataRow(nameof(OnnxSentenceEmbeddingOptions.MaskToken))]
    public void Validate_EmptySpecialToken_Throws(string property)
    {
        var options = new OnnxSentenceEmbeddingOptions();
        typeof(OnnxSentenceEmbeddingOptions).GetProperty(property)!.SetValue(options, string.Empty);

        var ex = Assert.Throws<ArgumentException>(options.Validate);

        Assert.AreEqual(property, ex.ParamName);
    }

    [TestCategory(TestCategories.Unit)]
    [TestMethod]
    public void MpnetDefaults_UseMpnetTokensAndPinnedRevision()
    {
        var options = new OnnxSentenceEmbeddingOptions();

        AllMpnetBaseV2Model.ApplyDefaults(options);

        options.Validate();
        Assert.AreEqual("<s>", options.ClsToken);
        Assert.AreEqual("<pad>", options.PadToken);
        Assert.IsNull(options.TokenTypeIdsName);
        Assert.EndsWith(AllMpnetBaseV2Model.Revision, options.ModelPath);
        Assert.AreEqual(2, options.ModelFiles.Count);
        Assert.Contains($"/{AllMpnetBaseV2Model.Revision}/onnx/model.onnx", options.ModelFiles[0].Url);
    }

    [TestCategory(TestCategories.Unit)]
    [TestMethod]
    public void NomicDefaults_UsePrefixLayerNormAndAllowTruncation()
    {
        var options = new OnnxSentenceEmbeddingOptions();

        NomicEmbedTextV1_5Model.ApplyDefaults(options);
        options.Dimensions = 256;

        options.Validate();
        Assert.AreEqual("search_document: ", options.Prefix);
        Assert.IsTrue(options.LayerNormalize);
        Assert.EndsWith(NomicEmbedTextV1_5Model.Revision, options.ModelPath);
    }

    [TestCategory(TestCategories.Unit)]
    [TestMethod]
    public void Registration_AddsKeyedServicesForBothModelsSideBySide()
    {
        var configuration = new ConfigurationBuilder().Build();
        var services = new ServiceCollection();

        services.TryAddAllMpnetBaseV2Services(configuration, OoBDev.SBert.AllMpnetBaseV2.SBertGlobals.DefaultSection);
        services.TryAddNomicEmbedTextV1_5Services(configuration, OoBDev.SBert.NomicEmbedTextV1_5.SBertGlobals.DefaultSection);

        foreach (var key in new[] { OoBDev.SBert.AllMpnetBaseV2.SBertGlobals.AllMpnetBaseV2Key, OoBDev.SBert.NomicEmbedTextV1_5.SBertGlobals.NomicEmbedTextV1_5Key })
            Assert.IsTrue(services.Any(d => d.IsKeyedService && d.ServiceType == typeof(IEmbeddingProvider) && Equals(d.ServiceKey, key)), key);
        Assert.AreEqual(1, services.Count(d => !d.IsKeyedService && d.ServiceType == typeof(IEmbeddingProvider)), "first model is the only default");
    }
}
