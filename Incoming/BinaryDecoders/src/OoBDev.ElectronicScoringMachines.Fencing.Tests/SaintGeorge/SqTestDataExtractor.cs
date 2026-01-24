using OoBDev.TestUtilities;
using OoBDev.ToolKit;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Text;

using static OoBDev.IO.Bytes;
using static OoBDev.ToolKit.DelimiterOptions;

namespace OoBDev.ElectronicScoringMachines.Fencing.Tests.SaintGeorge;

[TestClass]
public class SqTestDataExtractor
{
    public TestContext TestContext { get; set; }
    
    [TestMethod, TestCategory(TestCategories.DevLocal)]
    [Ignore]
    public void TestDataExtractor()
    {
        var path = @"C:\Repos\mwwhited\OoBDev\src\OoBDev.ElectronicScoringMachines.Fencing\SaintGeorge\outfile.bin";
        var data = File.ReadAllBytes(path);
        var memory = data.AsMemory();

        var chunks = memory.Split(delimiter: Soh, option: Carry);

        var segments = (from c in chunks
                        select c.ToArray().ToHexString(",0x"))
                       .Distinct()
                       .OrderBy(i => i)
                       .Aggregate(new StringBuilder(), (sb, v) => sb.Append("0x").Append(v).AppendLine())
                       .ToString();
        this.TestContext.WriteLine(segments);

    }
}
