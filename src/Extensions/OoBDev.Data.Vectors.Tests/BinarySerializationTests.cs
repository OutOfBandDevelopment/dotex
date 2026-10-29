using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using System.IO;

namespace OoBDev.Data.Vectors.Tests;

/// <summary>
/// SQL Server stores the types with <c>IBinarySerialize</c>; what is written must read back unchanged.
/// </summary>
[TestClass]
public class BinarySerializationTests
{
    private static MemoryStream Written(global::System.Action<BinaryWriter> write)
    {
        var stream = new MemoryStream();
        using (var writer = new BinaryWriter(stream, global::System.Text.Encoding.UTF8, leaveOpen: true))
            write(writer);
        stream.Position = 0;
        return stream;
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Matrix_WriteThenRead_KeepsDoubleValues()
    {
        var source = SqlMatrix.Parse("1.5,2,3|4,5.25,6");
        using var stream = Written(source.Write);
        var copy = new SqlMatrix();
        using (var reader = new BinaryReader(stream)) copy.Read(reader);

        Assert.AreEqual(4d, copy.Row(1).Values[0], 1e-12);
        Assert.AreEqual(5.25d, copy.Element(1, 1).Value, 1e-12);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void MatrixF_WriteThenRead_KeepsValues()
    {
        var source = SqlMatrixF.Parse("1.5,2,3|4,5.25,6");
        using var stream = Written(source.Write);
        var copy = new SqlMatrixF();
        using (var reader = new BinaryReader(stream)) copy.Read(reader);

        Assert.AreEqual(4d, copy.Row(1).Values[0], 1e-6);
        Assert.AreEqual(6d, copy.Column(2).Values[1], 1e-6);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Vectors_WriteThenRead_KeepValues()
    {
        var vector = SqlVector.Parse("3,4");
        using var vectorStream = Written(vector.Write);
        var vectorCopy = new SqlVector();
        using (var reader = new BinaryReader(vectorStream)) vectorCopy.Read(reader);

        var floats = SqlVectorF.Parse("3,4");
        using var floatStream = Written(floats.Write);
        var floatCopy = new SqlVectorF();
        using (var reader = new BinaryReader(floatStream)) floatCopy.Read(reader);

        Assert.AreEqual(5d, vectorCopy.Magnitude().Value, 1e-9);
        Assert.AreEqual(5d, floatCopy.Magnitude().Value, 1e-6);
    }
}
