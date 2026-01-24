using OoBDev.ElectronicScoringMachines.Fencing.Favero;
using OoBDev.TestUtilities;
using OoBDev.ToolKit;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using static OoBDev.ToolKit.DelimiterOptions;

namespace OoBDev.ElectronicScoringMachines.Fencing.Tests.Favero;

[TestClass]
public class FaveroTestDataExtractor
{
    public TestContext TestContext { get; set; }

    [TestMethod, TestCategory(TestCategories.DevLocal)]
    [Ignore]
    public void TestDataExtractor()
    {
        var path = @"C:\Repos\mwwhited\OoBDev\src\OoBDev.ElectronicScoringMachines.Fencing\Favero\RawData.txt";

        var chunks = File.ReadAllText(path)
                         .Where(c => (c >= '0' && c <= '9') || (c >= 'A' && c <= 'F') || (c >= 'a' && c <= 'f'))
                         .AsMemory()
                         .BytesFromHexString()
                         .Split(0xff, Carry)
                         ;

        //var segments = (from c in chunks
        //                select c.ToArray().ToHexString(",0x"))
        //               .Distinct()
        //               .OrderBy(i => i)
        //               .Aggregate(new StringBuilder(), (sb, v) => sb.Append("0x").Append(v).AppendLine())
        //               .ToString();
        // this.TestContext.WriteLine(segments);

        var parser = new FaveroStateParser();
        foreach (var c in chunks.Distinct())
        {
            try
            {
                var state = parser.Parse(c.Span);
                this.TestContext.WriteLine(state.ToString());
            }
            catch
            {
                this.TestContext.WriteLine($"ERROR Decoding {c.ToArray().ToHexString()}");
            }
        }
    }
}
