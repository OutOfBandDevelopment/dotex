using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace OoBDev.Data.Vectors.Tests;

[TestClass]
public sealed class Test1
{
    [TestMethod]
    public void TestMethod1()
    {
        int length = 384;
        var bytes = BitConverter.GetBytes(length);

        var version = 1;
        var header = (version <<24) | length;
        var bytesh = BitConverter.GetBytes(header);

        var oLength = header & 0x00ffffff;
        var oVersion =( header & 0xff000000) >> 24;
    }
}
