using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.Math;
using OoBDev.TestUtilities;
using System.IO;

namespace OoBDev.System.Tests.Math;

[TestClass]
public class VectorSerializationTests
{
    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Write_ThenRead_KeepsValuesAndMagnitude()
    {
        // Stage
        var vector = Vector.Create([3d, 4d]);
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);
        using var reader = new BinaryReader(stream);

        // Test
        vector.Write(writer);
        writer.Flush();
        stream.Position = 0;
        var read = Vector.Read(reader);

        // Assert
        CollectionAssert.AreEqual(new[] { 3d, 4d }, read.Value);
        Assert.AreEqual(5d, read.Magnitude);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Read_NoStoredMagnitude_ComputesIt()
    {
        // Stage
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);
        writer.Write(2);
        writer.Write(3d);
        writer.Write(4d);
        writer.Flush();
        stream.Position = 0;
        using var reader = new BinaryReader(stream);

        // Test
        var read = Vector.Read(reader);

        // Assert
        Assert.AreEqual(5d, read.Magnitude);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void ToString_ThenParse_RoundTrips()
    {
        // Stage
        var vector = Vector.Create([1.5d, -2d]);

        // Test
        var parsed = Vector.Parse(vector.ToString());

        // Assert
        CollectionAssert.AreEqual(vector.Value, parsed.Value);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void From_Null_ReturnsNull()
    {
        // Test
        var result = Vector.From(null);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void From_UnsupportedType_Throws()
    {
        // Test / Assert
        Assert.ThrowsExactly<global::System.NotSupportedException>(() => Vector.From(42));
    }
}
