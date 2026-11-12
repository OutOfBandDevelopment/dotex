using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;

namespace OoBDev.Documents.Guides.Tests;

[TestClass]
public class XmlDocReferenceRendererTests
{
    private const string _xml = """
        <?xml version="1.0"?>
        <doc>
          <assembly><name>Sample.Lib</name></assembly>
          <members>
            <member name="T:Sample.Zeta"><summary>Last type.</summary></member>
            <member name="T:Sample.Alpha">
              <summary>First type with <c>code</c>, <see cref="T:Sample.Zeta"/> and <paramref name="x"/>.</summary>
              <remarks>More | detail.</remarks>
            </member>
            <member name="M:Sample.Alpha.#ctor(System.String)"><summary>Builds it.</summary></member>
            <member name="M:Sample.Alpha.Run(System.Int32,System.String)"><summary>Runs.</summary></member>
            <member name="P:Sample.Alpha.Name"><summary>The name.</summary></member>
            <member name="P:Sample.Alpha.Hidden"><inheritdoc /></member>
            <member name="F:Sample.Alpha.Max"><summary>Upper bound.</summary></member>
            <member name="M:Other.Orphan"><summary>No owner type.</summary></member>
          </members>
        </doc>
        """;

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Render_SampleDoc_ListsTypesInOrderWithMembers()
    {
        // Test
        var text = XmlDocReferenceRenderer.Render(_xml);

        // Assert
        StringAssert.StartsWith(text, "# Sample.Lib\n");
        Assert.IsLessThan(text.IndexOf("## Sample.Zeta", global::System.StringComparison.Ordinal), text.IndexOf("## Sample.Alpha", global::System.StringComparison.Ordinal));
        StringAssert.Contains(text, "First type with `code`, `Zeta` and `x`.");
        StringAssert.Contains(text, "More \\| detail.");
        StringAssert.Contains(text, "### Fields\n\n- `Max`: Upper bound.");
        StringAssert.Contains(text, "- `Name`: The name.");
        StringAssert.Contains(text, "- `constructor(System.String)`: Builds it.");
        StringAssert.Contains(text, "- `Run(System.Int32,System.String)`: Runs.");
        Assert.DoesNotContain("Hidden", text);
        Assert.DoesNotContain("Orphan", text);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Render_SameInputTwice_IsIdentical()
    {
        // Test
        var first = XmlDocReferenceRenderer.Render(_xml);
        var second = XmlDocReferenceRenderer.Render(_xml);

        // Assert
        Assert.AreEqual(first, second);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Render_NoTypes_SaysSo()
    {
        // Test
        var text = XmlDocReferenceRenderer.Render("<doc><assembly><name>Empty</name></assembly><members/></doc>");

        // Assert
        StringAssert.Contains(text, "No documented types.");
    }
}
