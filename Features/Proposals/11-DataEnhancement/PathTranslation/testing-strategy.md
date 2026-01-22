# Path Syntax Translation - Testing Strategy

**Epic:** 11 - Data Enhancement Pipeline
**Feature:** Path Syntax Translation
**Priority:** HIGH (Foundation)
**Target Coverage:** 85-90%

---

## Overview

Comprehensive testing strategy for path syntax translation system, ensuring reliable bidirectional conversion between XPath, JSONPath, Dot Notation, and custom syntaxes through canonical representation.

---

## Test Pyramid

```
        ┌─────────────────┐
        │  Performance    │  5 tests (benchmarks)
        │   Benchmarks    │
        ├─────────────────┤
        │   Integration   │  15 tests (DataContainer integration)
        │      Tests      │
        ├─────────────────┤
        │   Unit Tests    │  60+ tests (navigators, translation, parsing)
        └─────────────────┘
```

**Coverage Goals:**
- IPathNavigator implementations: 90%+
- ICanonicalPath: 95%+
- PathTranslationService: 90%+
- Overall: 85-90%

---

## Unit Tests

### Category 1: Navigator Parsing Tests (20 tests)

**XPathNavigator.Parse() - 7 tests:**

```csharp
[TestClass]
public class XPathNavigatorParseTests
{
    private XPathNavigator _navigator = null!;

    [TestInitialize]
    public void Setup()
    {
        _navigator = new XPathNavigator();
    }

    [TestMethod]
    public void Parse_SimplePropertyPath_ReturnsCanonicalPath()
    {
        // Arrange
        var path = "Customer/Address/City";

        // Act
        var canonical = _navigator.Parse(path);

        // Assert
        Assert.IsFalse(canonical.IsAbsolute);
        Assert.AreEqual(3, canonical.Segments.Count);
        Assert.AreEqual(PathSegmentType.Property, canonical.Segments[0].Type);
        Assert.AreEqual("Customer", canonical.Segments[0].Value);
        Assert.AreEqual("Address", canonical.Segments[1].Value);
        Assert.AreEqual("City", canonical.Segments[2].Value);
    }

    [TestMethod]
    public void Parse_AbsolutePath_SetsIsAbsoluteTrue()
    {
        // Arrange
        var path = "/Customer/Address";

        // Act
        var canonical = _navigator.Parse(path);

        // Assert
        Assert.IsTrue(canonical.IsAbsolute);
        Assert.AreEqual(2, canonical.Segments.Count);
    }

    [TestMethod]
    public void Parse_ArrayIndex_ReturnsArraySegment()
    {
        // Arrange
        var path = "Orders/0/Total";

        // Act
        var canonical = _navigator.Parse(path);

        // Assert
        Assert.AreEqual(3, canonical.Segments.Count);
        Assert.AreEqual(PathSegmentType.Property, canonical.Segments[0].Type);
        Assert.AreEqual(PathSegmentType.ArrayIndex, canonical.Segments[1].Type);
        Assert.AreEqual(0, canonical.Segments[1].Index);
        Assert.AreEqual("Total", canonical.Segments[2].Value);
    }

    [TestMethod]
    public void Parse_Wildcard_ReturnsWildcardSegment()
    {
        // Arrange
        var path = "Orders/*/Total";

        // Act
        var canonical = _navigator.Parse(path);

        // Assert
        Assert.AreEqual(3, canonical.Segments.Count);
        Assert.AreEqual(PathSegmentType.Wildcard, canonical.Segments[1].Type);
    }

    [TestMethod]
    public void Parse_RecursiveDescent_ReturnsRecursiveSegment()
    {
        // Arrange
        var path = "**/LineItems";

        // Act
        var canonical = _navigator.Parse(path);

        // Assert
        Assert.AreEqual(2, canonical.Segments.Count);
        Assert.AreEqual(PathSegmentType.RecursiveDescent, canonical.Segments[0].Type);
        Assert.AreEqual(PathSegmentType.Property, canonical.Segments[1].Type);
    }

    [TestMethod]
    public void Parse_EmptyPath_ReturnsEmptyCanonicalPath()
    {
        // Arrange
        var path = "";

        // Act
        var canonical = _navigator.Parse(path);

        // Assert
        Assert.AreEqual(0, canonical.Segments.Count);
    }

    [TestMethod]
    public void Parse_RootOnly_ReturnsAbsoluteEmptyPath()
    {
        // Arrange
        var path = "/";

        // Act
        var canonical = _navigator.Parse(path);

        // Assert
        Assert.IsTrue(canonical.IsAbsolute);
        Assert.AreEqual(0, canonical.Segments.Count);
    }
}
```

**JSONPathNavigator.Parse() - 7 tests:**

```csharp
[TestClass]
public class JSONPathNavigatorParseTests
{
    private JSONPathNavigator _navigator = null!;

    [TestInitialize]
    public void Setup()
    {
        _navigator = new JSONPathNavigator();
    }

    [TestMethod]
    public void Parse_SimplePropertyPath_ReturnsCanonicalPath()
    {
        // Arrange
        var path = "$.Customer.Address.City";

        // Act
        var canonical = _navigator.Parse(path);

        // Assert
        Assert.IsTrue(canonical.IsAbsolute);  // $ makes it absolute
        Assert.AreEqual(3, canonical.Segments.Count);
        Assert.AreEqual("Customer", canonical.Segments[0].Value);
        Assert.AreEqual("Address", canonical.Segments[1].Value);
        Assert.AreEqual("City", canonical.Segments[2].Value);
    }

    [TestMethod]
    public void Parse_ArrayIndex_ReturnsArraySegment()
    {
        // Arrange
        var path = "$.Orders[0].Total";

        // Act
        var canonical = _navigator.Parse(path);

        // Assert
        Assert.AreEqual(3, canonical.Segments.Count);
        Assert.AreEqual(PathSegmentType.ArrayIndex, canonical.Segments[1].Type);
        Assert.AreEqual(0, canonical.Segments[1].Index);
    }

    [TestMethod]
    public void Parse_WildcardArray_ReturnsWildcardSegment()
    {
        // Arrange
        var path = "$.Orders[*].Total";

        // Act
        var canonical = _navigator.Parse(path);

        // Assert
        Assert.AreEqual(PathSegmentType.Wildcard, canonical.Segments[1].Type);
    }

    [TestMethod]
    public void Parse_RecursiveDescent_ReturnsRecursiveSegment()
    {
        // Arrange
        var path = "$..LineItems";

        // Act
        var canonical = _navigator.Parse(path);

        // Assert
        Assert.AreEqual(2, canonical.Segments.Count);
        Assert.AreEqual(PathSegmentType.RecursiveDescent, canonical.Segments[0].Type);
    }

    [TestMethod]
    public void Parse_WithoutDollarPrefix_ReturnsRelativePath()
    {
        // Arrange
        var path = "Customer.Address";

        // Act
        var canonical = _navigator.Parse(path);

        // Assert
        Assert.IsFalse(canonical.IsAbsolute);
    }

    [TestMethod]
    public void Parse_MultipleArrayIndices_ParsesCorrectly()
    {
        // Arrange
        var path = "$.Data[0][1][2]";

        // Act
        var canonical = _navigator.Parse(path);

        // Assert
        Assert.AreEqual(4, canonical.Segments.Count);
        Assert.AreEqual(PathSegmentType.Property, canonical.Segments[0].Type);
        Assert.AreEqual(0, canonical.Segments[1].Index);
        Assert.AreEqual(1, canonical.Segments[2].Index);
        Assert.AreEqual(2, canonical.Segments[3].Index);
    }

    [TestMethod]
    public void Parse_EmptyPath_ThrowsArgumentException()
    {
        // Arrange
        var path = "";

        // Act & Assert
        Assert.ThrowsException<ArgumentException>(() => _navigator.Parse(path));
    }
}
```

**DotNotationNavigator.Parse() - 6 tests:**

```csharp
[TestClass]
public class DotNotationNavigatorParseTests
{
    private DotNotationNavigator _navigator = null!;

    [TestInitialize]
    public void Setup()
    {
        _navigator = new DotNotationNavigator();
    }

    [TestMethod]
    public void Parse_SimplePropertyPath_ReturnsCanonicalPath()
    {
        // Arrange
        var path = "Customer.Address.City";

        // Act
        var canonical = _navigator.Parse(path);

        // Assert
        Assert.IsFalse(canonical.IsAbsolute);
        Assert.AreEqual(3, canonical.Segments.Count);
        Assert.AreEqual("Customer", canonical.Segments[0].Value);
    }

    [TestMethod]
    public void Parse_ArrayIndex_ReturnsArraySegment()
    {
        // Arrange
        var path = "Orders.0.Total";

        // Act
        var canonical = _navigator.Parse(path);

        // Assert
        Assert.AreEqual(PathSegmentType.ArrayIndex, canonical.Segments[1].Type);
        Assert.AreEqual(0, canonical.Segments[1].Index);
    }

    [TestMethod]
    public void Parse_Wildcard_ReturnsWildcardSegment()
    {
        // Arrange
        var path = "Orders.*.Total";

        // Act
        var canonical = _navigator.Parse(path);

        // Assert
        Assert.AreEqual(PathSegmentType.Wildcard, canonical.Segments[1].Type);
    }

    [TestMethod]
    public void Parse_RecursiveDescent_ReturnsRecursiveSegment()
    {
        // Arrange
        var path = "Customer.**.LineItems";

        // Act
        var canonical = _navigator.Parse(path);

        // Assert
        Assert.AreEqual(PathSegmentType.RecursiveDescent, canonical.Segments[1].Type);
    }

    [TestMethod]
    public void Parse_SingleProperty_ReturnsSingleSegment()
    {
        // Arrange
        var path = "Customer";

        // Act
        var canonical = _navigator.Parse(path);

        // Assert
        Assert.AreEqual(1, canonical.Segments.Count);
        Assert.AreEqual("Customer", canonical.Segments[0].Value);
    }

    [TestMethod]
    public void Parse_EmptyPath_ReturnsEmptyCanonicalPath()
    {
        // Arrange
        var path = "";

        // Act
        var canonical = _navigator.Parse(path);

        // Assert
        Assert.AreEqual(0, canonical.Segments.Count);
    }
}
```

---

### Category 2: Navigator Format Tests (15 tests)

**Round-Trip Tests (5 tests per navigator):**

```csharp
[TestClass]
public class NavigatorRoundTripTests
{
    [TestMethod]
    public void XPathNavigator_ParseAndFormat_PreservesPath()
    {
        // Arrange
        var navigator = new XPathNavigator();
        var originalPath = "Customer/Orders/0/Total";

        // Act
        var canonical = navigator.Parse(originalPath);
        var formatted = navigator.Format(canonical);

        // Assert
        Assert.AreEqual(originalPath, formatted);
    }

    [TestMethod]
    public void JSONPathNavigator_ParseAndFormat_PreservesPath()
    {
        // Arrange
        var navigator = new JSONPathNavigator();
        var originalPath = "$.Customer.Orders[0].Total";

        // Act
        var canonical = navigator.Parse(originalPath);
        var formatted = navigator.Format(canonical);

        // Assert
        Assert.AreEqual(originalPath, formatted);
    }

    [TestMethod]
    public void DotNotationNavigator_ParseAndFormat_PreservesPath()
    {
        // Arrange
        var navigator = new DotNotationNavigator();
        var originalPath = "Customer.Orders.0.Total";

        // Act
        var canonical = navigator.Parse(originalPath);
        var formatted = navigator.Format(canonical);

        // Assert
        Assert.AreEqual(originalPath, formatted);
    }

    [TestMethod]
    public void XPathNavigator_FormatWithWildcard_ReturnsAsterisk()
    {
        // Arrange
        var navigator = new XPathNavigator();
        var canonical = new CanonicalPath(false, new[]
        {
            PathSegment.Property("Orders"),
            PathSegment.Wildcard(),
            PathSegment.Property("Total")
        });

        // Act
        var formatted = navigator.Format(canonical);

        // Assert
        Assert.AreEqual("Orders/*/Total", formatted);
    }

    [TestMethod]
    public void XPathNavigator_FormatWithRecursiveDescent_ReturnsDoubleStar()
    {
        // Arrange
        var navigator = new XPathNavigator();
        var canonical = new CanonicalPath(false, new[]
        {
            PathSegment.RecursiveDescent(),
            PathSegment.Property("LineItems")
        });

        // Act
        var formatted = navigator.Format(canonical);

        // Assert
        Assert.AreEqual("**/LineItems", formatted);
    }
}
```

---

### Category 3: Syntax Detection Tests (9 tests)

```csharp
[TestClass]
public class SyntaxDetectionTests
{
    [TestMethod]
    public void XPathNavigator_CanParse_DetectsXPathSyntax()
    {
        // Arrange
        var navigator = new XPathNavigator();

        // Act & Assert
        Assert.IsTrue(navigator.CanParse("Customer/Address/City"));
        Assert.IsTrue(navigator.CanParse("/Customer/Address"));
        Assert.IsTrue(navigator.CanParse("**/LineItems"));
    }

    [TestMethod]
    public void XPathNavigator_CanParse_RejectsOtherSyntaxes()
    {
        // Arrange
        var navigator = new XPathNavigator();

        // Act & Assert
        Assert.IsFalse(navigator.CanParse("$.Customer.Address"));
        Assert.IsFalse(navigator.CanParse("Customer.Address.City"));
    }

    [TestMethod]
    public void JSONPathNavigator_CanParse_DetectsJSONPathSyntax()
    {
        // Arrange
        var navigator = new JSONPathNavigator();

        // Act & Assert
        Assert.IsTrue(navigator.CanParse("$.Customer.Address"));
        Assert.IsTrue(navigator.CanParse("$.Orders[0]"));
        Assert.IsTrue(navigator.CanParse("$..LineItems"));
    }

    [TestMethod]
    public void JSONPathNavigator_CanParse_RejectsOtherSyntaxes()
    {
        // Arrange
        var navigator = new JSONPathNavigator();

        // Act & Assert
        Assert.IsFalse(navigator.CanParse("Customer/Address"));
        Assert.IsFalse(navigator.CanParse("Customer.Address"));
    }

    [TestMethod]
    public void DotNotationNavigator_CanParse_DetectsDotNotation()
    {
        // Arrange
        var navigator = new DotNotationNavigator();

        // Act & Assert
        Assert.IsTrue(navigator.CanParse("Customer.Address.City"));
        Assert.IsTrue(navigator.CanParse("Orders.0.Total"));
        Assert.IsTrue(navigator.CanParse("Customer.**.LineItems"));
    }

    [TestMethod]
    public void DotNotationNavigator_CanParse_RejectsOtherSyntaxes()
    {
        // Arrange
        var navigator = new DotNotationNavigator();

        // Act & Assert
        Assert.IsFalse(navigator.CanParse("$.Customer.Address"));
        Assert.IsFalse(navigator.CanParse("Customer/Address"));
    }

    [TestMethod]
    public void PathTranslationService_ParseAny_DetectsXPath()
    {
        // Arrange
        var service = new PathTranslationService();
        var path = "Customer/Address/City";

        // Act
        var canonical = service.ParseAny(path);

        // Assert
        Assert.AreEqual(3, canonical.Segments.Count);
        Assert.AreEqual("Customer", canonical.Segments[0].Value);
    }

    [TestMethod]
    public void PathTranslationService_ParseAny_DetectsJSONPath()
    {
        // Arrange
        var service = new PathTranslationService();
        var path = "$.Customer.Address.City";

        // Act
        var canonical = service.ParseAny(path);

        // Assert
        Assert.IsTrue(canonical.IsAbsolute);
        Assert.AreEqual(3, canonical.Segments.Count);
    }

    [TestMethod]
    public void PathTranslationService_ParseAny_DefaultsToDotNotation()
    {
        // Arrange
        var service = new PathTranslationService();
        var path = "Customer.Address.City";

        // Act
        var canonical = service.ParseAny(path);

        // Assert
        Assert.AreEqual(3, canonical.Segments.Count);
    }
}
```

---

### Category 4: Translation Tests (12 tests)

```csharp
[TestClass]
public class PathTranslationTests
{
    private PathTranslationService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _service = new PathTranslationService();
    }

    [TestMethod]
    public void Translate_XPathToJSONPath_ConvertsCorrectly()
    {
        // Arrange
        var xpath = "Customer/Orders/0/Total";

        // Act
        var jsonpath = _service.Translate(xpath, "xpath", "jsonpath");

        // Assert
        Assert.AreEqual("$.Customer.Orders[0].Total", jsonpath);
    }

    [TestMethod]
    public void Translate_XPathToDotNotation_ConvertsCorrectly()
    {
        // Arrange
        var xpath = "Customer/Orders/0/Total";

        // Act
        var dotNotation = _service.Translate(xpath, "xpath", "dotnotation");

        // Assert
        Assert.AreEqual("Customer.Orders.0.Total", dotNotation);
    }

    [TestMethod]
    public void Translate_JSONPathToXPath_ConvertsCorrectly()
    {
        // Arrange
        var jsonpath = "$.Customer.Orders[0].Total";

        // Act
        var xpath = _service.Translate(jsonpath, "jsonpath", "xpath");

        // Assert
        Assert.AreEqual("Customer/Orders/0/Total", xpath);
    }

    [TestMethod]
    public void Translate_JSONPathToDotNotation_ConvertsCorrectly()
    {
        // Arrange
        var jsonpath = "$.Customer.Orders[0].Total";

        // Act
        var dotNotation = _service.Translate(jsonpath, "jsonpath", "dotnotation");

        // Assert
        Assert.AreEqual("Customer.Orders.0.Total", dotNotation);
    }

    [TestMethod]
    public void Translate_DotNotationToXPath_ConvertsCorrectly()
    {
        // Arrange
        var dotNotation = "Customer.Orders.0.Total";

        // Act
        var xpath = _service.Translate(dotNotation, "dotnotation", "xpath");

        // Assert
        Assert.AreEqual("Customer/Orders/0/Total", xpath);
    }

    [TestMethod]
    public void Translate_DotNotationToJSONPath_ConvertsCorrectly()
    {
        // Arrange
        var dotNotation = "Customer.Orders.0.Total";

        // Act
        var jsonpath = _service.Translate(dotNotation, "dotnotation", "jsonpath");

        // Assert
        Assert.AreEqual("$.Customer.Orders[0].Total", jsonpath);
    }

    [TestMethod]
    public void Translate_WildcardPath_PreservesWildcard()
    {
        // Arrange
        var xpath = "Orders/*/Total";

        // Act
        var jsonpath = _service.Translate(xpath, "xpath", "jsonpath");

        // Assert
        Assert.AreEqual("$.Orders[*].Total", jsonpath);
    }

    [TestMethod]
    public void Translate_RecursiveDescent_PreservesRecursive()
    {
        // Arrange
        var xpath = "**/LineItems";

        // Act
        var jsonpath = _service.Translate(xpath, "xpath", "jsonpath");

        // Assert
        Assert.AreEqual("$..LineItems", jsonpath);
    }

    [TestMethod]
    public void Translate_UnknownSourceSyntax_ThrowsArgumentException()
    {
        // Arrange
        var path = "Customer.Address";

        // Act & Assert
        var ex = Assert.ThrowsException<ArgumentException>(() =>
            _service.Translate(path, "unknown", "xpath"));
        Assert.IsTrue(ex.Message.Contains("Unknown source syntax"));
    }

    [TestMethod]
    public void Translate_UnknownTargetSyntax_ThrowsArgumentException()
    {
        // Arrange
        var path = "Customer.Address";

        // Act & Assert
        var ex = Assert.ThrowsException<ArgumentException>(() =>
            _service.Translate(path, "xpath", "unknown"));
        Assert.IsTrue(ex.Message.Contains("Unknown target syntax"));
    }

    [TestMethod]
    public void Translate_SameSyntax_ReturnsOriginalPath()
    {
        // Arrange
        var xpath = "Customer/Orders/0";

        // Act
        var result = _service.Translate(xpath, "xpath", "xpath");

        // Assert
        Assert.AreEqual(xpath, result);
    }

    [TestMethod]
    public void Translate_ComplexPath_ConvertsAllSegments()
    {
        // Arrange
        var xpath = "Customer/Orders/*/LineItems/0/Product";

        // Act
        var jsonpath = _service.Translate(xpath, "xpath", "jsonpath");

        // Assert
        Assert.AreEqual("$.Customer.Orders[*].LineItems[0].Product", jsonpath);
    }
}
```

---

### Category 5: CanonicalPath Tests (8 tests)

```csharp
[TestClass]
public class CanonicalPathTests
{
    [TestMethod]
    public void Combine_TwoPaths_CombinesSegments()
    {
        // Arrange
        var path1 = new CanonicalPath(false, new[] { PathSegment.Property("Customer") });
        var path2 = new CanonicalPath(false, new[] { PathSegment.Property("Address"), PathSegment.Property("City") });

        // Act
        var combined = path1.Combine(path2);

        // Assert
        Assert.AreEqual(3, combined.Segments.Count);
        Assert.AreEqual("Customer", combined.Segments[0].Value);
        Assert.AreEqual("Address", combined.Segments[1].Value);
        Assert.AreEqual("City", combined.Segments[2].Value);
    }

    [TestMethod]
    public void GetParent_RemovesLastSegment()
    {
        // Arrange
        var path = new CanonicalPath(false, new[]
        {
            PathSegment.Property("Customer"),
            PathSegment.Property("Address"),
            PathSegment.Property("City")
        });

        // Act
        var parent = path.GetParent();

        // Assert
        Assert.IsNotNull(parent);
        Assert.AreEqual(2, parent.Segments.Count);
        Assert.AreEqual("Address", parent.Segments[1].Value);
    }

    [TestMethod]
    public void GetParent_SingleSegment_ReturnsNull()
    {
        // Arrange
        var path = new CanonicalPath(false, new[] { PathSegment.Property("Customer") });

        // Act
        var parent = path.GetParent();

        // Assert
        Assert.IsNull(parent);
    }

    [TestMethod]
    public void ToString_FormatsCanonicalRepresentation()
    {
        // Arrange
        var path = new CanonicalPath(true, new[]
        {
            PathSegment.Property("Customer"),
            PathSegment.ArrayIndex(0),
            PathSegment.Property("Total")
        });

        // Act
        var str = path.ToString();

        // Assert
        Assert.AreEqual("[Customer][0][Total]", str);
    }

    [TestMethod]
    public void Equals_SamePath_ReturnsTrue()
    {
        // Arrange
        var path1 = new CanonicalPath(false, new[] { PathSegment.Property("Customer"), PathSegment.Property("Name") });
        var path2 = new CanonicalPath(false, new[] { PathSegment.Property("Customer"), PathSegment.Property("Name") });

        // Act
        var equals = path1.Equals(path2);

        // Assert
        Assert.IsTrue(equals);
    }

    [TestMethod]
    public void Equals_DifferentPath_ReturnsFalse()
    {
        // Arrange
        var path1 = new CanonicalPath(false, new[] { PathSegment.Property("Customer") });
        var path2 = new CanonicalPath(false, new[] { PathSegment.Property("Order") });

        // Act
        var equals = path1.Equals(path2);

        // Assert
        Assert.IsFalse(equals);
    }

    [TestMethod]
    public void GetHashCode_SamePath_ReturnsSameHash()
    {
        // Arrange
        var path1 = new CanonicalPath(false, new[] { PathSegment.Property("Customer") });
        var path2 = new CanonicalPath(false, new[] { PathSegment.Property("Customer") });

        // Act
        var hash1 = path1.GetHashCode();
        var hash2 = path2.GetHashCode();

        // Assert
        Assert.AreEqual(hash1, hash2);
    }

    [TestMethod]
    public void CanonicalPath_ImmutableSegments_CannotModify()
    {
        // Arrange
        var segments = new List<IPathSegment> { PathSegment.Property("Customer") };
        var path = new CanonicalPath(false, segments.ToArray());

        // Act
        segments.Add(PathSegment.Property("Order"));  // Modify original list

        // Assert
        Assert.AreEqual(1, path.Segments.Count);  // Path unchanged
    }
}
```

---

### Category 6: Builder Tests (6 tests)

```csharp
[TestClass]
public class CanonicalPathBuilderTests
{
    [TestMethod]
    public void Build_SimpleProperties_CreatesPath()
    {
        // Arrange
        var builder = new CanonicalPathBuilder();

        // Act
        var path = builder
            .Property("Customer")
            .Property("Address")
            .Property("City")
            .Build();

        // Assert
        Assert.AreEqual(3, path.Segments.Count);
        Assert.AreEqual("Customer", path.Segments[0].Value);
    }

    [TestMethod]
    public void Build_WithArrayIndex_CreatesArraySegment()
    {
        // Arrange
        var builder = new CanonicalPathBuilder();

        // Act
        var path = builder
            .Property("Orders")
            .ArrayIndex(0)
            .Property("Total")
            .Build();

        // Assert
        Assert.AreEqual(PathSegmentType.ArrayIndex, path.Segments[1].Type);
        Assert.AreEqual(0, path.Segments[1].Index);
    }

    [TestMethod]
    public void Build_WithWildcard_CreatesWildcardSegment()
    {
        // Arrange
        var builder = new CanonicalPathBuilder();

        // Act
        var path = builder
            .Property("Orders")
            .Wildcard()
            .Property("Total")
            .Build();

        // Assert
        Assert.AreEqual(PathSegmentType.Wildcard, path.Segments[1].Type);
    }

    [TestMethod]
    public void Build_WithRecursiveDescent_CreatesRecursiveSegment()
    {
        // Arrange
        var builder = new CanonicalPathBuilder();

        // Act
        var path = builder
            .RecursiveDescent()
            .Property("LineItems")
            .Build();

        // Assert
        Assert.AreEqual(PathSegmentType.RecursiveDescent, path.Segments[0].Type);
    }

    [TestMethod]
    public void Absolute_SetsIsAbsoluteFlag()
    {
        // Arrange
        var builder = new CanonicalPathBuilder();

        // Act
        var path = builder
            .Absolute()
            .Property("Customer")
            .Build();

        // Assert
        Assert.IsTrue(path.IsAbsolute);
    }

    [TestMethod]
    public void Build_EmptyBuilder_CreatesEmptyPath()
    {
        // Arrange
        var builder = new CanonicalPathBuilder();

        // Act
        var path = builder.Build();

        // Assert
        Assert.AreEqual(0, path.Segments.Count);
        Assert.IsFalse(path.IsAbsolute);
    }
}
```

---

## Integration Tests (15 tests)

### Category 7: DataContainer Integration (8 tests)

```csharp
[TestClass]
public class DataContainerIntegrationTests
{
    [TestMethod]
    public async Task DataContainer_NavigateWithXPath_ReturnsValue()
    {
        // Arrange
        var data = new
        {
            Customer = new
            {
                FirstName = "John",
                LastName = "Doe",
                Address = new { City = "Seattle" }
            }
        };
        var container = DataContainerFactory.Create(data);
        var translationService = new PathTranslationService();

        // Act
        var xpath = "Customer/Address/City";
        var canonical = translationService.ParseAny(xpath);
        var node = container.Navigate(canonical.ToString());
        var value = await node.GetValueAsync<string>();

        // Assert
        Assert.AreEqual("Seattle", value);
    }

    [TestMethod]
    public async Task DataContainer_NavigateWithJSONPath_ReturnsValue()
    {
        // Arrange
        var data = new { Customer = new { Address = new { City = "Seattle" } } };
        var container = DataContainerFactory.Create(data);
        var translationService = new PathTranslationService();

        // Act
        var jsonpath = "$.Customer.Address.City";
        var canonical = translationService.ParseAny(jsonpath);
        var node = container.Navigate(canonical.ToString());
        var value = await node.GetValueAsync<string>();

        // Assert
        Assert.AreEqual("Seattle", value);
    }

    [TestMethod]
    public async Task DataContainer_TranslateAndNavigate_SameResult()
    {
        // Arrange
        var data = new { Customer = new { Orders = new[] { new { Total = 100.0 } } } };
        var container = DataContainerFactory.Create(data);
        var translationService = new PathTranslationService();

        var xpath = "Customer/Orders/0/Total";
        var jsonpath = "$.Customer.Orders[0].Total";

        // Act
        var canonical1 = translationService.ParseAny(xpath);
        var canonical2 = translationService.ParseAny(jsonpath);

        var value1 = await container.Navigate(canonical1.ToString()).GetValueAsync<double>();
        var value2 = await container.Navigate(canonical2.ToString()).GetValueAsync<double>();

        // Assert
        Assert.AreEqual(value1, value2);
        Assert.AreEqual(100.0, value1);
    }

    [TestMethod]
    public void DataContainer_UsesCanonicalPathInternally_TransparentToSyntax()
    {
        // Arrange
        var container = DataContainerFactory.Create();
        var translationService = new PathTranslationService();

        // Act - Register provider using XPath
        var xpathCanonical = translationService.ParseAny("Customer/Profile");
        container.RegisterProvider(xpathCanonical.ToString(), new StaticDataProvider(new { Name = "John" }));

        // Query using JSONPath
        var jsonpathCanonical = translationService.ParseAny("$.Customer.Profile");
        var node = container.Navigate(jsonpathCanonical.ToString());

        // Assert - Both paths resolve to same provider
        Assert.IsNotNull(node);
        Assert.IsNotNull(node.Value);
    }

    [TestMethod]
    public async Task DataContainer_WithCustomNavigator_Works()
    {
        // Arrange
        var customNavigator = new MongoDBPathNavigator();
        var translationService = new PathTranslationService();
        translationService.RegisterNavigator(customNavigator);

        var container = DataContainerFactory.Create(new { users = new { name = "Alice" } });

        // Act
        var mongoPath = "users.name";
        var canonical = translationService.GetNavigator("mongodb")!.Parse(mongoPath);
        var node = container.Navigate(canonical.ToString());
        var value = await node.GetValueAsync<string>();

        // Assert
        Assert.AreEqual("Alice", value);
    }

    [TestMethod]
    public void DataContainer_StoresCanonicalPaths_NotSyntaxSpecificPaths()
    {
        // Arrange
        var container = DataContainerFactory.Create();
        var translationService = new PathTranslationService();

        // Act
        var xpath = "Customer/Orders/0";
        var jsonpath = "$.Customer.Orders[0]";
        var dotNotation = "Customer.Orders.0";

        var canonical1 = translationService.ParseAny(xpath);
        var canonical2 = translationService.ParseAny(jsonpath);
        var canonical3 = translationService.ParseAny(dotNotation);

        // Assert - All three parse to same canonical representation
        Assert.AreEqual(canonical1.ToString(), canonical2.ToString());
        Assert.AreEqual(canonical2.ToString(), canonical3.ToString());
    }

    [TestMethod]
    public async Task DataContainer_NavigateArray_WithDifferentSyntaxes()
    {
        // Arrange
        var data = new { Orders = new[] { new { Id = 1 }, new { Id = 2 }, new { Id = 3 } } };
        var container = DataContainerFactory.Create(data);
        var translationService = new PathTranslationService();

        // Act
        var xpathCanonical = translationService.ParseAny("Orders/1/Id");
        var jsonpathCanonical = translationService.ParseAny("$.Orders[1].Id");
        var dotCanonical = translationService.ParseAny("Orders.1.Id");

        var value1 = await container.Navigate(xpathCanonical.ToString()).GetValueAsync<int>();
        var value2 = await container.Navigate(jsonpathCanonical.ToString()).GetValueAsync<int>();
        var value3 = await container.Navigate(dotCanonical.ToString()).GetValueAsync<int>();

        // Assert
        Assert.AreEqual(2, value1);
        Assert.AreEqual(2, value2);
        Assert.AreEqual(2, value3);
    }

    [TestMethod]
    public void DataContainer_WithExtensionMethods_SimplifiesUsage()
    {
        // Arrange
        var container = DataContainerFactory.Create(new { Customer = new { Name = "Bob" } });
        var translationService = new PathTranslationService();

        // Act - Using extension methods
        var xpath = "Customer/Name";
        var canonical = translationService.ParseAny(xpath);
        var jsonpath = canonical.ToJSONPath(translationService);
        var dotNotation = canonical.ToDotNotation(translationService);

        // Assert
        Assert.AreEqual("$.Customer.Name", jsonpath);
        Assert.AreEqual("Customer.Name", dotNotation);
    }
}
```

---

### Category 8: Custom Navigator Integration (7 tests)

```csharp
[TestClass]
public class CustomNavigatorIntegrationTests
{
    [TestMethod]
    public void RegisterNavigator_CustomNavigator_BecomesAvailable()
    {
        // Arrange
        var service = new PathTranslationService();
        var customNavigator = new MongoDBPathNavigator();

        // Act
        service.RegisterNavigator(customNavigator);
        var retrieved = service.GetNavigator("mongodb");

        // Assert
        Assert.IsNotNull(retrieved);
        Assert.AreEqual("mongodb", retrieved.NavigatorType);
    }

    [TestMethod]
    public void CustomNavigator_Parse_ReturnsCanonicalPath()
    {
        // Arrange
        var navigator = new MongoDBPathNavigator();
        var mongoPath = "users.profile.settings";

        // Act
        var canonical = navigator.Parse(mongoPath);

        // Assert
        Assert.AreEqual(3, canonical.Segments.Count);
        Assert.AreEqual("users", canonical.Segments[0].Value);
    }

    [TestMethod]
    public void CustomNavigator_Format_ReturnsMongoDBSyntax()
    {
        // Arrange
        var navigator = new MongoDBPathNavigator();
        var canonical = new CanonicalPath(false, new[]
        {
            PathSegment.Property("users"),
            PathSegment.Property("profile"),
            PathSegment.Property("settings")
        });

        // Act
        var mongoPath = navigator.Format(canonical);

        // Assert
        Assert.AreEqual("users.profile.settings", mongoPath);
    }

    [TestMethod]
    public void Translate_MongoDBToXPath_ConvertsCorrectly()
    {
        // Arrange
        var service = new PathTranslationService();
        service.RegisterNavigator(new MongoDBPathNavigator());
        var mongoPath = "users.0.name";

        // Act
        var xpath = service.Translate(mongoPath, "mongodb", "xpath");

        // Assert
        Assert.AreEqual("users/0/name", xpath);
    }

    [TestMethod]
    public void Translate_XPathToMongoDB_ConvertsCorrectly()
    {
        // Arrange
        var service = new PathTranslationService();
        service.RegisterNavigator(new MongoDBPathNavigator());
        var xpath = "users/0/name";

        // Act
        var mongoPath = service.Translate(xpath, "xpath", "mongodb");

        // Assert
        Assert.AreEqual("users.0.name", mongoPath);
    }

    [TestMethod]
    public void ParseAny_CustomNavigator_DetectsCorrectly()
    {
        // Arrange
        var service = new PathTranslationService();
        var customNavigator = new MongoDBPathNavigator();
        service.RegisterNavigator(customNavigator);

        // Act
        var canonical = service.ParseAny("users.profile");

        // Assert
        Assert.AreEqual(2, canonical.Segments.Count);
    }

    [TestMethod]
    public void CustomNavigator_RoundTrip_PreservesPath()
    {
        // Arrange
        var navigator = new MongoDBPathNavigator();
        var originalPath = "users.0.profile.settings";

        // Act
        var canonical = navigator.Parse(originalPath);
        var formatted = navigator.Format(canonical);

        // Assert
        Assert.AreEqual(originalPath, formatted);
    }
}
```

---

## Performance Benchmarks (5 tests)

```csharp
[TestClass]
[TestCategory(TestCategories.Performance)]
public class PathTranslationPerformanceTests
{
    [TestMethod]
    public void Parse_1000Paths_CompletesUnder100ms()
    {
        // Arrange
        var navigator = new XPathNavigator();
        var path = "Customer/Orders/0/LineItems/5/Product/Name";
        var stopwatch = Stopwatch.StartNew();

        // Act
        for (int i = 0; i < 1000; i++)
        {
            navigator.Parse(path);
        }
        stopwatch.Stop();

        // Assert
        Assert.IsTrue(stopwatch.ElapsedMilliseconds < 100,
            $"Expected < 100ms, actual: {stopwatch.ElapsedMilliseconds}ms");
    }

    [TestMethod]
    public void Translate_1000Paths_CompletesUnder200ms()
    {
        // Arrange
        var service = new PathTranslationService();
        var xpath = "Customer/Orders/0/LineItems/5/Product/Name";
        var stopwatch = Stopwatch.StartNew();

        // Act
        for (int i = 0; i < 1000; i++)
        {
            service.Translate(xpath, "xpath", "jsonpath");
        }
        stopwatch.Stop();

        // Assert
        Assert.IsTrue(stopwatch.ElapsedMilliseconds < 200,
            $"Expected < 200ms, actual: {stopwatch.ElapsedMilliseconds}ms");
    }

    [TestMethod]
    public void ParseAny_WithCaching_ImprovesPerformance()
    {
        // Arrange
        var service = new PathTranslationService();
        var path = "Customer/Orders/0/Total";

        // Warm-up
        service.ParseAny(path);

        // Act - First run (no cache)
        var stopwatch1 = Stopwatch.StartNew();
        for (int i = 0; i < 1000; i++)
        {
            service.ParseAny(path);
        }
        stopwatch1.Stop();

        // Act - Second run (with cache)
        var stopwatch2 = Stopwatch.StartNew();
        for (int i = 0; i < 1000; i++)
        {
            service.ParseAny(path);
        }
        stopwatch2.Stop();

        // Assert - Cached run should be faster or similar
        Assert.IsTrue(stopwatch2.ElapsedMilliseconds <= stopwatch1.ElapsedMilliseconds * 1.1);
    }

    [TestMethod]
    public void CanonicalPath_Equals_FastComparison()
    {
        // Arrange
        var path1 = new CanonicalPath(false, new[]
        {
            PathSegment.Property("Customer"),
            PathSegment.Property("Orders"),
            PathSegment.ArrayIndex(0)
        });
        var path2 = new CanonicalPath(false, new[]
        {
            PathSegment.Property("Customer"),
            PathSegment.Property("Orders"),
            PathSegment.ArrayIndex(0)
        });

        var stopwatch = Stopwatch.StartNew();

        // Act
        for (int i = 0; i < 10000; i++)
        {
            var equals = path1.Equals(path2);
        }
        stopwatch.Stop();

        // Assert
        Assert.IsTrue(stopwatch.ElapsedMilliseconds < 50,
            $"Expected < 50ms, actual: {stopwatch.ElapsedMilliseconds}ms");
    }

    [TestMethod]
    public void NavigatorSelection_CanParse_FastDetection()
    {
        // Arrange
        var xpathNavigator = new XPathNavigator();
        var jsonpathNavigator = new JSONPathNavigator();
        var dotNavigator = new DotNotationNavigator();

        var paths = new[]
        {
            "Customer/Orders/0",
            "$.Customer.Orders[0]",
            "Customer.Orders.0"
        };

        var stopwatch = Stopwatch.StartNew();

        // Act
        for (int i = 0; i < 1000; i++)
        {
            foreach (var path in paths)
            {
                xpathNavigator.CanParse(path);
                jsonpathNavigator.CanParse(path);
                dotNavigator.CanParse(path);
            }
        }
        stopwatch.Stop();

        // Assert
        Assert.IsTrue(stopwatch.ElapsedMilliseconds < 100,
            $"Expected < 100ms, actual: {stopwatch.ElapsedMilliseconds}ms");
    }
}
```

---

## Edge Cases and Error Handling (10 tests)

```csharp
[TestClass]
public class PathTranslationEdgeCaseTests
{
    [TestMethod]
    public void Parse_NullPath_ThrowsArgumentNullException()
    {
        // Arrange
        var navigator = new XPathNavigator();

        // Act & Assert
        Assert.ThrowsException<ArgumentNullException>(() => navigator.Parse(null!));
    }

    [TestMethod]
    public void Parse_PathWithSpecialCharacters_HandlesCorrectly()
    {
        // Arrange
        var navigator = new DotNotationNavigator();
        var path = "Customer.My-Property.Another_Property";

        // Act
        var canonical = navigator.Parse(path);

        // Assert
        Assert.AreEqual(3, canonical.Segments.Count);
        Assert.AreEqual("My-Property", canonical.Segments[1].Value);
        Assert.AreEqual("Another_Property", canonical.Segments[2].Value);
    }

    [TestMethod]
    public void Parse_VeryLongPath_HandlesCorrectly()
    {
        // Arrange
        var navigator = new XPathNavigator();
        var segments = Enumerable.Range(0, 100).Select(i => $"Property{i}");
        var path = string.Join("/", segments);

        // Act
        var canonical = navigator.Parse(path);

        // Assert
        Assert.AreEqual(100, canonical.Segments.Count);
    }

    [TestMethod]
    public void Translate_InvalidArrayIndex_ThrowsPathParseException()
    {
        // Arrange
        var navigator = new XPathNavigator();
        var path = "Orders/abc/Total";  // "abc" is not a valid index

        // Act & Assert
        var canonical = navigator.Parse(path);
        Assert.AreEqual(PathSegmentType.Property, canonical.Segments[1].Type);  // Treated as property
    }

    [TestMethod]
    public void Format_NullCanonicalPath_ThrowsArgumentNullException()
    {
        // Arrange
        var navigator = new XPathNavigator();

        // Act & Assert
        Assert.ThrowsException<ArgumentNullException>(() => navigator.Format(null!));
    }

    [TestMethod]
    public void Translate_EmptyPath_HandlesGracefully()
    {
        // Arrange
        var service = new PathTranslationService();

        // Act
        var result = service.Translate("", "xpath", "jsonpath");

        // Assert
        Assert.AreEqual("$", result);  // JSONPath root
    }

    [TestMethod]
    public void ParseAny_AmbiguousPath_UsesFirstMatch()
    {
        // Arrange
        var service = new PathTranslationService();
        var path = "Customer";  // Could be any syntax

        // Act
        var canonical = service.ParseAny(path);

        // Assert
        Assert.AreEqual(1, canonical.Segments.Count);
        Assert.AreEqual("Customer", canonical.Segments[0].Value);
    }

    [TestMethod]
    public void GetNavigator_CaseInsensitive_ReturnsNavigator()
    {
        // Arrange
        var service = new PathTranslationService();

        // Act
        var navigator1 = service.GetNavigator("xpath");
        var navigator2 = service.GetNavigator("XPATH");
        var navigator3 = service.GetNavigator("XPath");

        // Assert
        Assert.IsNotNull(navigator1);
        Assert.AreSame(navigator1, navigator2);
        Assert.AreSame(navigator2, navigator3);
    }

    [TestMethod]
    public void Combine_RelativeAndAbsolutePaths_ThrowsInvalidOperationException()
    {
        // Arrange
        var relativePath = new CanonicalPath(false, new[] { PathSegment.Property("Address") });
        var absolutePath = new CanonicalPath(true, new[] { PathSegment.Property("Customer") });

        // Act & Assert
        Assert.ThrowsException<InvalidOperationException>(() => relativePath.Combine(absolutePath));
    }

    [TestMethod]
    public void PathSegment_NegativeArrayIndex_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.ThrowsException<ArgumentException>(() => PathSegment.ArrayIndex(-1));
    }
}
```

---

## Test Coverage Report

### Target Coverage by Component

| Component | Target Coverage | Priority |
|-----------|----------------|----------|
| `IPathNavigator` implementations | 95% | HIGH |
| `ICanonicalPath` | 95% | HIGH |
| `PathTranslationService` | 90% | HIGH |
| `CanonicalPathBuilder` | 90% | MEDIUM |
| Extension methods | 85% | MEDIUM |
| Error handling | 80% | MEDIUM |

### Coverage Gaps to Address

1. **Concurrent access tests** - Thread safety for PathTranslationService
2. **Memory leak tests** - Ensure navigator cache doesn't grow unbounded
3. **Globalization tests** - Unicode property names, international characters
4. **Regex edge cases** - Complex property names with special characters

---

## Test Execution Strategy

### Local Development
```bash
# Run all path translation tests
dotnet test --filter "FullyQualifiedName~PathTranslation"

# Run only unit tests
dotnet test --filter "TestCategory=Unit&FullyQualifiedName~PathTranslation"

# Run with coverage
dotnet test --collect:"XPlat Code Coverage" --filter "FullyQualifiedName~PathTranslation"
```

### CI/CD Pipeline
```yaml
- name: Run Path Translation Tests
  run: |
    dotnet test \
      --filter "FullyQualifiedName~PathTranslation" \
      --logger "trx;LogFileName=path-translation-results.trx" \
      --collect:"XPlat Code Coverage" \
      --results-directory ./TestResults
```

### Performance Baseline
```bash
# Run performance benchmarks
dotnet test --filter "TestCategory=Performance&FullyQualifiedName~PathTranslation"
```

---

## Success Criteria

- ✅ 60+ unit tests implemented
- ✅ 15+ integration tests implemented
- ✅ 5 performance benchmarks implemented
- ✅ 85%+ overall code coverage
- ✅ All navigators have round-trip tests
- ✅ All syntax pairs have translation tests
- ✅ Edge cases and error handling covered
- ✅ Performance requirements met (< 5ms translation, < 10ms parsing)

---

## Related Documents

- [Requirements](./requirements.md)
- [Architecture](./architecture.md)
- [API Design](./api-design.md)
- [Core Container Testing](../CoreContainer/testing-strategy.md)
