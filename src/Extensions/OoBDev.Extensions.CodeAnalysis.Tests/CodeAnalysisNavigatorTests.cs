using System.IO;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Extensions.CodeAnalysis.CSharp;
using OoBDev.Extensions.CodeAnalysis.VisualBasic;

namespace OoBDev.Extensions.CodeAnalysis.Tests;

[TestClass]
public class CodeAnalysisNavigatorTests
{
    private const string CSharpSource = """
        namespace Sample;

        // a comment
        public interface IShape { double Area(); }
        public class Circle : IShape
        {
            public double Radius { get; set; }
            public double Area(int scale = 2) => 3 * Radius * scale;
        }
        public struct Point { public int X; }
        public enum Color { Red, Green }
        """;

    private const string VbSource = """
        Public Class Counter
            Public Property Count As Integer
            Public Sub Increment()
                Count += 1
            End Sub
        End Class
        Public Module Helpers
        End Module
        """;

    private static XPathNavigator Navigator(string source, string lang)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(source));
        return (lang == "cs" ? new CSharpNavigator().ToNavigable(stream) : new VisualBasicNavigator().ToNavigable(stream))!.CreateNavigator()!;
    }

    private static XmlNamespaceManager Manager(XPathNavigator navigator)
    {
        var manager = new XmlNamespaceManager(navigator.NameTable);
        manager.AddNamespace("n", "oobdev:CodeAnalysis/Node");
        manager.AddNamespace("t", "oobdev:CodeAnalysis/Token");
        manager.AddNamespace("v", "oobdev:CodeAnalysis/Trivia");
        manager.AddNamespace("s", "oobdev:CodeAnalysis/Symbol");
        return manager;
    }

    private static int Count(XPathNavigator navigator, string xpath)
    {
        var expression = navigator.Compile(xpath);
        expression.SetContext(Manager(navigator));
        return navigator.Select(expression).Count;
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void CSharpNavigator_Stream_FindsTypeDeclarations()
    {
        // Stage
        var navigator = Navigator(CSharpSource, "cs");

        // Test
        var types = Count(navigator, "//n:*[self::n:ClassDeclaration or self::n:InterfaceDeclaration or self::n:StructDeclaration or self::n:EnumDeclaration]");

        // Assert
        Assert.AreEqual(4, types);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void CSharpNavigator_Stream_ExposesTokenTextAsValue()
    {
        var navigator = Navigator(CSharpSource, "cs");
        var expression = navigator.Compile("//n:ClassDeclaration/t:IdentifierToken");
        expression.SetContext(Manager(navigator));

        var selected = navigator.Select(expression);

        Assert.AreEqual(1, selected.Count);
        selected.MoveNext();
        Assert.AreEqual("Circle", selected.Current!.Value);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void CSharpNavigator_Stream_PutsTriviaUnderTokens()
    {
        var navigator = Navigator(CSharpSource, "cs");

        Assert.IsTrue(Count(navigator, "//t:*/v:SingleLineCommentTrivia") >= 1);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void CSharpNavigator_Stream_AddsKindAndLocationAttributes()
    {
        var navigator = Navigator(CSharpSource, "cs");

        Assert.AreEqual(1, Count(navigator, "/*[@Language='C#'][@RawKind]/n:CompilationUnit[@Location.Start='0']"));
        Assert.AreEqual(4, Count(navigator, "//n:*[@Location.End > @Location.Start][self::n:ClassDeclaration or self::n:InterfaceDeclaration or self::n:StructDeclaration or self::n:EnumDeclaration]"));
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void CSharpNavigator_Stream_HasSingleRootAndTreeElement()
    {
        var navigator = Navigator(CSharpSource, "cs");

        Assert.AreEqual(1, Count(navigator, "/"));
        Assert.AreEqual(1, Count(navigator, "/*[local-name()='Tree']"));
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void CSharpNavigator_File_ReadsFromDisk()
    {
        var path = Path.Combine(Path.GetTempPath(), $"Test_{(global::System.Guid.NewGuid()):N}.cs");
        File.WriteAllText(path, CSharpSource);
        try
        {
            var navigator = new CSharpNavigator().ToNavigable(path)!.CreateNavigator()!;

            Assert.AreEqual(4, Count(navigator, "//n:*[self::n:ClassDeclaration or self::n:InterfaceDeclaration or self::n:StructDeclaration or self::n:EnumDeclaration]"));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void VisualBasicNavigator_Stream_FindsClassAndModule()
    {
        var navigator = Navigator(VbSource, "vb");

        Assert.AreEqual(1, Count(navigator, "//n:ClassBlock"));
        Assert.AreEqual(1, Count(navigator, "//n:ModuleBlock"));
        Assert.AreEqual(1, Count(navigator, "/*[@Language='Visual Basic']/n:CompilationUnit"));
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void VisualBasicNavigator_File_ReadsFromDisk()
    {
        var path = Path.Combine(Path.GetTempPath(), $"Test_{(global::System.Guid.NewGuid()):N}.vb");
        File.WriteAllText(path, VbSource);
        try
        {
            var navigator = new VisualBasicNavigator().ToNavigable(path)!.CreateNavigator()!;

            Assert.AreEqual(1, Count(navigator, "//n:SubBlock"));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void CSharpSemanticNavigator_Stream_AddsSymbolsAndKeepsSyntax()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(CSharpSource));
        var navigator = new CSharpSemanticNavigator().ToNavigable(stream)!.CreateNavigator()!;

        Assert.AreEqual(4, Count(navigator, "//n:*[self::n:ClassDeclaration or self::n:InterfaceDeclaration or self::n:StructDeclaration or self::n:EnumDeclaration]"));
        Assert.IsTrue(Count(navigator, "//s:*") > 0);
        Assert.IsTrue(Count(navigator, "//s:*[@s:DisplayString]") > 0);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void CSharpSemanticNavigator_File_ReadsFromDisk()
    {
        var path = Path.Combine(Path.GetTempPath(), $"Test_{(global::System.Guid.NewGuid()):N}.cs");
        File.WriteAllText(path, CSharpSource);
        try
        {
            var navigator = new CSharpSemanticNavigator().ToNavigable(path)!.CreateNavigator()!;

            Assert.AreEqual(1, Count(navigator, "//n:CompilationUnit"));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void SemanticModelNavigator_ParameterSymbol_ReportsOptionalDefault()
    {
        var tree = CSharpSyntaxTree.ParseText("class C { int M(int a = 5) => a; }");
        var model = CSharpCompilation.Create("t").AddSyntaxTrees(tree).GetSemanticModel(tree);
        var navigator = model.ToNavigable().CreateNavigator()!;

        Assert.IsTrue(Count(navigator, "//s:*[@s:IsOptional='True'][@s:ExplicitDefaultValue='5']") >= 1);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void SyntaxNames_Categories_MapToNamespaces()
    {
        var tree = CSharpSyntaxTree.ParseText("class C { }");
        var root = tree.GetRoot();

        Assert.AreEqual("oobdev:CodeAnalysis/Tree", tree.ToNamespaceUri().NamespaceName);
        Assert.AreEqual("oobdev:CodeAnalysis/Node", root.ToNamespaceUri().NamespaceName);
        Assert.AreEqual("oobdev:CodeAnalysis/Token", root.GetFirstToken().ToNamespaceUri().NamespaceName);
        Assert.AreEqual("CompilationUnit", root.ToLocalName());
        Assert.AreEqual("Tree", tree.ToLocalName());
        Assert.AreEqual("String", "x".ToLocalName());
    }
}
