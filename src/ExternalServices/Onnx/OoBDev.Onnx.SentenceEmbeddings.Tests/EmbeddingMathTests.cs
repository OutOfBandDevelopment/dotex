using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using System;

namespace OoBDev.Onnx.SentenceEmbeddings.Tests;

[TestClass]
public class EmbeddingMathTests
{
    [TestCategory(TestCategories.Unit)]
    [TestMethod]
    public void Pool_Mean_AveragesOnlyMaskedTokens()
    {
        // three tokens of two values; the third is padding
        float[] tokens = [1f, 2f, 3f, 4f, 100f, 100f];
        long[] mask = [1, 1, 0];
        var result = new float[2];

        EmbeddingMath.Pool(tokens, mask, 2, EmbeddingPooling.Mean, result);

        Assert.AreEqual(2f, result[0]);
        Assert.AreEqual(3f, result[1]);
    }

    [TestCategory(TestCategories.Unit)]
    [TestMethod]
    public void Pool_Cls_ReturnsFirstToken()
    {
        float[] tokens = [1f, 2f, 3f, 4f];
        long[] mask = [1, 1];
        var result = new float[2];

        EmbeddingMath.Pool(tokens, mask, 2, EmbeddingPooling.Cls, result);

        CollectionAssert.AreEqual(new[] { 1f, 2f }, result);
    }

    [TestCategory(TestCategories.Unit)]
    [TestMethod]
    public void Normalize_ScalesToUnitLength()
    {
        float[] vector = [3f, 4f];

        EmbeddingMath.Normalize(vector);

        Assert.AreEqual(0.6f, vector[0], 1e-6f);
        Assert.AreEqual(0.8f, vector[1], 1e-6f);
    }

    [TestCategory(TestCategories.Unit)]
    [TestMethod]
    public void Normalize_ZeroVector_StaysZero()
    {
        float[] vector = [0f, 0f];

        EmbeddingMath.Normalize(vector);

        CollectionAssert.AreEqual(new[] { 0f, 0f }, vector);
    }
}
