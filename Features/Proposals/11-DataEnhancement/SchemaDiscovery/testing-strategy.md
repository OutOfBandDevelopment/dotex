# Schema Discovery & Translation - Testing Strategy

**Epic:** 11 - Data Enhancement Pipeline
**Feature:** Schema Discovery & Translation
**Priority:** MEDIUM
**Target Coverage:** 85-90%

---

## Overview

Comprehensive testing strategy for schema discovery and translation system, ensuring reliable schema introspection from data containers, accurate translation between XSD, JSON Schema, YAML Schema, and robust validation capabilities.

---

## Test Pyramid

```
        ┌─────────────────┐
        │  Performance    │  5 tests (benchmarks)
        │   Benchmarks    │
        ├─────────────────┤
        │   Integration   │  20 tests (provider integration, DataContainer)
        │      Tests      │
        ├─────────────────┤
        │   Unit Tests    │  70+ tests (providers, inference, translation, validation)
        └─────────────────┘
```

**Coverage Goals:**
- ISchemaProvider implementations: 90%+
- ICanonicalSchema: 95%+
- SchemaDiscoveryService: 90%+
- Schema inference engine: 85%+
- Overall: 85-90%

---

## Unit Tests

### Category 1: Schema Inference Tests (15 tests)

**Infer from CLR Types:**

```csharp
[TestClass]
public class SchemaInferenceFromTypeTests
{
    private SchemaInferenceEngine _engine = null!;

    [TestInitialize]
    public void Setup()
    {
        _engine = new SchemaInferenceEngine();
    }

    [TestMethod]
    public void InferFromType_SimpleClass_CreatesObjectSchema()
    {
        // Arrange
        var type = typeof(SimpleCustomer);

        // Act
        var schema = _engine.InferFromType(type);

        // Assert
        Assert.AreEqual("SimpleCustomer", schema.Title);
        Assert.AreEqual(SchemaTypeKind.Object, schema.RootType.Kind);

        var objType = (IObjectSchemaType)schema.RootType;
        Assert.IsTrue(objType.Properties.ContainsKey("FirstName"));
        Assert.IsTrue(objType.Properties.ContainsKey("LastName"));
        Assert.IsTrue(objType.Properties.ContainsKey("Age"));

        Assert.AreEqual(SchemaTypeKind.String, objType.Properties["FirstName"].Kind);
        Assert.AreEqual(SchemaTypeKind.Integer, objType.Properties["Age"].Kind);
    }

    [TestMethod]
    public void InferFromType_NullableProperties_SetsIsNullable()
    {
        // Arrange
        public class TestClass
        {
            public string Required { get; set; } = "";
            public string? Optional { get; set; }
            public int RequiredInt { get; set; }
            public int? OptionalInt { get; set; }
        }

        // Act
        var schema = _engine.InferFromType(typeof(TestClass));
        var objType = (IObjectSchemaType)schema.RootType;

        // Assert
        Assert.IsFalse(objType.Properties["Required"].IsNullable);
        Assert.IsTrue(objType.Properties["Optional"].IsNullable);
        Assert.IsFalse(objType.Properties["RequiredInt"].IsNullable);
        Assert.IsTrue(objType.Properties["OptionalInt"].IsNullable);
    }

    [TestMethod]
    public void InferFromType_CollectionProperty_CreatesArraySchema()
    {
        // Arrange
        public class TestClass
        {
            public List<string> Tags { get; set; } = new();
            public int[] Numbers { get; set; } = Array.Empty<int>();
        }

        // Act
        var schema = _engine.InferFromType(typeof(TestClass));
        var objType = (IObjectSchemaType)schema.RootType;

        // Assert
        Assert.AreEqual(SchemaTypeKind.Array, objType.Properties["Tags"].Kind);
        Assert.AreEqual(SchemaTypeKind.Array, objType.Properties["Numbers"].Kind);

        var tagsArray = (IArraySchemaType)objType.Properties["Tags"];
        Assert.AreEqual(SchemaTypeKind.String, tagsArray.ItemsSchema.Kind);

        var numbersArray = (IArraySchemaType)objType.Properties["Numbers"];
        Assert.AreEqual(SchemaTypeKind.Integer, numbersArray.ItemsSchema.Kind);
    }

    [TestMethod]
    public void InferFromType_NestedClass_CreatesDefinitions()
    {
        // Arrange
        public class Customer
        {
            public string Name { get; set; } = "";
            public Address BillingAddress { get; set; } = null!;
        }

        public class Address
        {
            public string Street { get; set; } = "";
            public string City { get; set; } = "";
        }

        // Act
        var schema = _engine.InferFromType(typeof(Customer));

        // Assert
        Assert.IsTrue(schema.Definitions.ContainsKey("Address"));

        var addressDef = schema.Definitions["Address"];
        Assert.AreEqual(SchemaTypeKind.Object, addressDef.Kind);

        var addressObj = (IObjectSchemaType)addressDef;
        Assert.IsTrue(addressObj.Properties.ContainsKey("Street"));
        Assert.IsTrue(addressObj.Properties.ContainsKey("City"));
    }

    [TestMethod]
    public void InferFromType_PrimitiveTypes_CreatesCorrectSchemas()
    {
        // Arrange
        public class AllPrimitives
        {
            public string Text { get; set; } = "";
            public int Integer { get; set; }
            public long Long { get; set; }
            public decimal Decimal { get; set; }
            public double Double { get; set; }
            public float Float { get; set; }
            public bool Boolean { get; set; }
            public DateTime DateTime { get; set; }
            public Guid Guid { get; set; }
        }

        // Act
        var schema = _engine.InferFromType(typeof(AllPrimitives));
        var objType = (IObjectSchemaType)schema.RootType;

        // Assert
        Assert.AreEqual(SchemaTypeKind.String, objType.Properties["Text"].Kind);
        Assert.AreEqual(SchemaTypeKind.Integer, objType.Properties["Integer"].Kind);
        Assert.AreEqual(SchemaTypeKind.Integer, objType.Properties["Long"].Kind);
        Assert.AreEqual(SchemaTypeKind.Number, objType.Properties["Decimal"].Kind);
        Assert.AreEqual(SchemaTypeKind.Number, objType.Properties["Double"].Kind);
        Assert.AreEqual(SchemaTypeKind.Boolean, objType.Properties["Boolean"].Kind);

        // DateTime and Guid are strings with format hints
        var dateTimeProp = (IStringSchemaType)objType.Properties["DateTime"];
        Assert.AreEqual("date-time", dateTimeProp.Format);

        var guidProp = (IStringSchemaType)objType.Properties["Guid"];
        Assert.AreEqual("uuid", guidProp.Format);
    }

    [TestMethod]
    public void InferFromType_EnumProperty_CreatesStringWithEnumValues()
    {
        // Arrange
        public enum Status { Active, Inactive, Pending }
        public class TestClass
        {
            public Status Status { get; set; }
        }

        // Act
        var schema = _engine.InferFromType(typeof(TestClass));
        var objType = (IObjectSchemaType)schema.RootType;

        // Assert
        var statusProp = (IStringSchemaType)objType.Properties["Status"];
        Assert.IsNotNull(statusProp.EnumValues);
        Assert.AreEqual(3, statusProp.EnumValues.Count);
        CollectionAssert.AreEquivalent(
            new[] { "Active", "Inactive", "Pending" },
            statusProp.EnumValues.ToArray());
    }

    [TestMethod]
    public void InferFromData_StringValue_DetectsFormatHints()
    {
        // Arrange
        var emailData = new { email = "test@example.com" };
        var uriData = new { uri = "https://example.com" };
        var dateData = new { date = "2024-01-22T10:30:00Z" };

        // Act
        var emailSchema = _engine.InferFromData(emailData);
        var uriSchema = _engine.InferFromData(uriData);
        var dateSchema = _engine.InferFromData(dateData);

        // Assert
        var emailObj = (IObjectSchemaType)emailSchema.RootType;
        var emailProp = (IStringSchemaType)emailObj.Properties["email"];
        Assert.AreEqual("email", emailProp.Format);

        var uriObj = (IObjectSchemaType)uriSchema.RootType;
        var uriProp = (IStringSchemaType)uriObj.Properties["uri"];
        Assert.AreEqual("uri", uriProp.Format);

        var dateObj = (IObjectSchemaType)dateSchema.RootType;
        var dateProp = (IStringSchemaType)dateObj.Properties["date"];
        Assert.AreEqual("date-time", dateProp.Format);
    }

    [TestMethod]
    public void InferFromData_NumberValue_InfersRange()
    {
        // Arrange
        var data = new { count = 42, price = 99.99m };

        // Act
        var schema = _engine.InferFromData(data);

        // Assert
        var objType = (IObjectSchemaType)schema.RootType;
        var countProp = (IIntegerSchemaType)objType.Properties["count"];
        Assert.AreEqual(42, countProp.Minimum);
        Assert.AreEqual(42, countProp.Maximum);

        var priceProp = (INumberSchemaType)objType.Properties["price"];
        Assert.AreEqual(99.99m, priceProp.Minimum);
        Assert.AreEqual(99.99m, priceProp.Maximum);
    }

    [TestMethod]
    public void InferFromData_Array_InfersItemSchema()
    {
        // Arrange
        var data = new
        {
            numbers = new[] { 1, 2, 3, 4, 5 },
            names = new[] { "Alice", "Bob", "Charlie" }
        };

        // Act
        var schema = _engine.InferFromData(data);

        // Assert
        var objType = (IObjectSchemaType)schema.RootType;

        var numbersArray = (IArraySchemaType)objType.Properties["numbers"];
        Assert.AreEqual(SchemaTypeKind.Integer, numbersArray.ItemsSchema.Kind);
        Assert.AreEqual(5, numbersArray.MinItems);
        Assert.AreEqual(5, numbersArray.MaxItems);

        var namesArray = (IArraySchemaType)objType.Properties["names"];
        Assert.AreEqual(SchemaTypeKind.String, namesArray.ItemsSchema.Kind);
    }

    [TestMethod]
    public void InferFromData_NullValue_CreatesNullableType()
    {
        // Arrange
        var data = new { value = (string?)null };

        // Act
        var schema = _engine.InferFromData(data);

        // Assert
        var objType = (IObjectSchemaType)schema.RootType;
        Assert.IsTrue(objType.Properties["value"].IsNullable);
    }
}

public class SimpleCustomer
{
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public int Age { get; set; }
}
```

---

### Category 2: JSON Schema Provider Tests (15 tests)

```csharp
[TestClass]
public class JsonSchemaProviderTests
{
    private JsonSchemaProvider _provider = null!;

    [TestInitialize]
    public void Setup()
    {
        _provider = new JsonSchemaProvider();
    }

    [TestMethod]
    public void Parse_SimpleObjectSchema_CreatesCanonicalSchema()
    {
        // Arrange
        var jsonSchema = @"{
  ""$schema"": ""http://json-schema.org/draft-07/schema#"",
  ""type"": ""object"",
  ""properties"": {
    ""name"": { ""type"": ""string"" },
    ""age"": { ""type"": ""integer"" }
  },
  ""required"": [""name""]
}";

        // Act
        var schema = _provider.Parse(jsonSchema);

        // Assert
        Assert.AreEqual(SchemaTypeKind.Object, schema.RootType.Kind);

        var objType = (IObjectSchemaType)schema.RootType;
        Assert.AreEqual(2, objType.Properties.Count);
        Assert.IsTrue(objType.Properties.ContainsKey("name"));
        Assert.IsTrue(objType.Properties.ContainsKey("age"));
        Assert.AreEqual(1, objType.Required.Count);
        Assert.AreEqual("name", objType.Required[0]);
    }

    [TestMethod]
    public void Parse_StringWithConstraints_CreatesStringSchema()
    {
        // Arrange
        var jsonSchema = @"{
  ""type"": ""string"",
  ""minLength"": 5,
  ""maxLength"": 50,
  ""pattern"": ""^[A-Za-z]+$"",
  ""format"": ""email""
}";

        // Act
        var schema = _provider.Parse(jsonSchema);

        // Assert
        var stringType = (IStringSchemaType)schema.RootType;
        Assert.AreEqual(5, stringType.MinLength);
        Assert.AreEqual(50, stringType.MaxLength);
        Assert.AreEqual("^[A-Za-z]+$", stringType.Pattern);
        Assert.AreEqual("email", stringType.Format);
    }

    [TestMethod]
    public void Parse_ArraySchema_CreatesArrayType()
    {
        // Arrange
        var jsonSchema = @"{
  ""type"": ""array"",
  ""items"": { ""type"": ""string"" },
  ""minItems"": 1,
  ""maxItems"": 10,
  ""uniqueItems"": true
}";

        // Act
        var schema = _provider.Parse(jsonSchema);

        // Assert
        var arrayType = (IArraySchemaType)schema.RootType;
        Assert.AreEqual(SchemaTypeKind.String, arrayType.ItemsSchema.Kind);
        Assert.AreEqual(1, arrayType.MinItems);
        Assert.AreEqual(10, arrayType.MaxItems);
        Assert.IsTrue(arrayType.UniqueItems);
    }

    [TestMethod]
    public void Parse_NumberWithConstraints_CreatesNumberSchema()
    {
        // Arrange
        var jsonSchema = @"{
  ""type"": ""number"",
  ""minimum"": 0,
  ""maximum"": 100,
  ""multipleOf"": 0.5
}";

        // Act
        var schema = _provider.Parse(jsonSchema);

        // Assert
        var numberType = (INumberSchemaType)schema.RootType;
        Assert.AreEqual(0m, numberType.Minimum);
        Assert.AreEqual(100m, numberType.Maximum);
        Assert.AreEqual(0.5m, numberType.MultipleOf);
    }

    [TestMethod]
    public void Parse_WithDefinitions_CreatesDefinitions()
    {
        // Arrange
        var jsonSchema = @"{
  ""type"": ""object"",
  ""properties"": {
    ""address"": { ""$ref"": ""#/definitions/Address"" }
  },
  ""definitions"": {
    ""Address"": {
      ""type"": ""object"",
      ""properties"": {
        ""street"": { ""type"": ""string"" },
        ""city"": { ""type"": ""string"" }
      }
    }
  }
}";

        // Act
        var schema = _provider.Parse(jsonSchema);

        // Assert
        Assert.AreEqual(1, schema.Definitions.Count);
        Assert.IsTrue(schema.Definitions.ContainsKey("Address"));

        var addressDef = (IObjectSchemaType)schema.Definitions["Address"];
        Assert.IsTrue(addressDef.Properties.ContainsKey("street"));
        Assert.IsTrue(addressDef.Properties.ContainsKey("city"));
    }

    [TestMethod]
    public void Format_ObjectSchema_CreatesJsonSchema()
    {
        // Arrange
        var schema = new CanonicalSchema
        {
            Title = "Person",
            RootType = new ObjectSchemaType
            {
                Properties = new Dictionary<string, ISchemaType>
                {
                    ["name"] = new StringSchemaType(),
                    ["age"] = new IntegerSchemaType()
                },
                Required = new[] { "name" }
            }
        };

        // Act
        var jsonSchema = _provider.Format(schema);

        // Assert
        Assert.IsTrue(jsonSchema.Contains("\"$schema\""));
        Assert.IsTrue(jsonSchema.Contains("\"title\": \"Person\""));
        Assert.IsTrue(jsonSchema.Contains("\"type\": \"object\""));
        Assert.IsTrue(jsonSchema.Contains("\"name\""));
        Assert.IsTrue(jsonSchema.Contains("\"age\""));
        Assert.IsTrue(jsonSchema.Contains("\"required\""));
    }

    [TestMethod]
    public void Validate_ValidData_ReturnsSuccess()
    {
        // Arrange
        var schema = new CanonicalSchema
        {
            RootType = new ObjectSchemaType
            {
                Properties = new Dictionary<string, ISchemaType>
                {
                    ["name"] = new StringSchemaType { MinLength = 1 },
                    ["age"] = new IntegerSchemaType { Minimum = 0, Maximum = 120 }
                },
                Required = new[] { "name" }
            }
        };

        var data = new { name = "John", age = 30 };

        // Act
        var result = _provider.Validate(data, schema);

        // Assert
        Assert.IsTrue(result.IsValid);
        Assert.AreEqual(0, result.Errors.Count);
    }

    [TestMethod]
    public void Validate_MissingRequiredProperty_ReturnsError()
    {
        // Arrange
        var schema = new CanonicalSchema
        {
            RootType = new ObjectSchemaType
            {
                Properties = new Dictionary<string, ISchemaType>
                {
                    ["name"] = new StringSchemaType(),
                    ["age"] = new IntegerSchemaType()
                },
                Required = new[] { "name" }
            }
        };

        var data = new { age = 30 };  // Missing "name"

        // Act
        var result = _provider.Validate(data, schema);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.AreEqual("required_missing", result.Errors[0].ErrorCode);
        Assert.IsTrue(result.Errors[0].Message.Contains("name"));
    }

    [TestMethod]
    public void Validate_ConstraintViolation_ReturnsError()
    {
        // Arrange
        var schema = new CanonicalSchema
        {
            RootType = new ObjectSchemaType
            {
                Properties = new Dictionary<string, ISchemaType>
                {
                    ["age"] = new IntegerSchemaType { Minimum = 0, Maximum = 120 }
                }
            }
        };

        var data = new { age = 150 };  // Exceeds maximum

        // Act
        var result = _provider.Validate(data, schema);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.Errors.Any(e => e.Path.Contains("age")));
    }

    [TestMethod]
    public void CanParse_JsonSchemaDocument_ReturnsTrue()
    {
        // Arrange
        var jsonSchema = @"{
  ""$schema"": ""http://json-schema.org/draft-07/schema#"",
  ""type"": ""object""
}";

        // Act
        var canParse = _provider.CanParse(jsonSchema);

        // Assert
        Assert.IsTrue(canParse);
    }

    [TestMethod]
    public void CanParse_NonJsonSchema_ReturnsFalse()
    {
        // Arrange
        var xmlSchema = @"<?xml version=""1.0""?><xs:schema></xs:schema>";

        // Act
        var canParse = _provider.CanParse(xmlSchema);

        // Assert
        Assert.IsFalse(canParse);
    }
}
```

---

### Category 3: Schema Translation Tests (12 tests)

```csharp
[TestClass]
public class SchemaTranslationTests
{
    private SchemaDiscoveryService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _service = new SchemaDiscoveryService();
        _service.RegisterProvider(new JsonSchemaProvider());
        _service.RegisterProvider(new XsdSchemaProvider());
    }

    [TestMethod]
    public void TranslateSchema_JsonToXsd_ConvertsCorrectly()
    {
        // Arrange
        var jsonSchema = @"{
  ""type"": ""object"",
  ""properties"": {
    ""name"": { ""type"": ""string"" },
    ""age"": { ""type"": ""integer"" }
  }
}";

        // Act
        var xsd = _service.TranslateSchema(jsonSchema, "jsonschema", "xsd");

        // Assert
        Assert.IsTrue(xsd.Contains("<?xml"));
        Assert.IsTrue(xsd.Contains("xs:schema"));
        Assert.IsTrue(xsd.Contains("xs:element"));
        Assert.IsTrue(xsd.Contains("name"));
        Assert.IsTrue(xsd.Contains("age"));
    }

    [TestMethod]
    public void TranslateSchema_XsdToJson_ConvertsCorrectly()
    {
        // Arrange
        var xsd = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<xs:schema xmlns:xs=""http://www.w3.org/2001/XMLSchema"">
  <xs:element name=""person"">
    <xs:complexType>
      <xs:sequence>
        <xs:element name=""name"" type=""xs:string""/>
        <xs:element name=""age"" type=""xs:int""/>
      </xs:sequence>
    </xs:complexType>
  </xs:element>
</xs:schema>";

        // Act
        var jsonSchema = _service.TranslateSchema(xsd, "xsd", "jsonschema");

        // Assert
        Assert.IsTrue(jsonSchema.Contains("$schema"));
        Assert.IsTrue(jsonSchema.Contains("name"));
        Assert.IsTrue(jsonSchema.Contains("age"));
        Assert.IsTrue(jsonSchema.Contains("string"));
        Assert.IsTrue(jsonSchema.Contains("integer"));
    }

    [TestMethod]
    public void TranslateSchema_PreservesConstraints()
    {
        // Arrange
        var jsonSchema = @"{
  ""type"": ""string"",
  ""minLength"": 5,
  ""maxLength"": 50
}";

        // Act - Round-trip translation
        var xsd = _service.TranslateSchema(jsonSchema, "jsonschema", "xsd");
        var backToJson = _service.TranslateSchema(xsd, "xsd", "jsonschema");

        var originalSchema = _service.ParseAny(jsonSchema);
        var roundTripSchema = _service.ParseAny(backToJson);

        // Assert
        var originalStr = (IStringSchemaType)originalSchema.RootType;
        var roundTripStr = (IStringSchemaType)roundTripSchema.RootType;

        Assert.AreEqual(originalStr.MinLength, roundTripStr.MinLength);
        Assert.AreEqual(originalStr.MaxLength, roundTripStr.MaxLength);
    }

    [TestMethod]
    public void TranslateSchema_UnknownSourceFormat_ThrowsException()
    {
        // Arrange
        var schema = "{ \"type\": \"object\" }";

        // Act & Assert
        Assert.ThrowsException<SchemaProviderNotFoundException>(() =>
            _service.TranslateSchema(schema, "unknown", "jsonschema"));
    }

    [TestMethod]
    public void TranslateSchema_UnknownTargetFormat_ThrowsException()
    {
        // Arrange
        var schema = "{ \"type\": \"object\" }";

        // Act & Assert
        Assert.ThrowsException<SchemaProviderNotFoundException>(() =>
            _service.TranslateSchema(schema, "jsonschema", "unknown"));
    }

    [TestMethod]
    public void TranslateSchema_ComplexNesting_PreservesStructure()
    {
        // Arrange
        var jsonSchema = @"{
  ""type"": ""object"",
  ""properties"": {
    ""customer"": {
      ""type"": ""object"",
      ""properties"": {
        ""address"": {
          ""type"": ""object"",
          ""properties"": {
            ""city"": { ""type"": ""string"" }
          }
        }
      }
    }
  }
}";

        // Act
        var canonical = _service.ParseAny(jsonSchema);

        // Assert - Verify 3-level nesting
        var root = (IObjectSchemaType)canonical.RootType;
        var customer = (IObjectSchemaType)root.Properties["customer"];
        var address = (IObjectSchemaType)customer.Properties["address"];
        var city = address.Properties["city"];

        Assert.AreEqual(SchemaTypeKind.String, city.Kind);
    }
}
```

---

### Category 4: Schema Discovery Service Tests (15 tests)

```csharp
[TestClass]
public class SchemaDiscoveryServiceTests
{
    private SchemaDiscoveryService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _service = new SchemaDiscoveryService();
        _service.RegisterProvider(new JsonSchemaProvider());
    }

    [TestMethod]
    public async Task GetSchemaAsync_DataContainer_InfersSchema()
    {
        // Arrange
        var data = new
        {
            Customer = new
            {
                FirstName = "John",
                LastName = "Doe",
                Age = 30
            }
        };
        var container = DataContainerFactory.Create(data);

        // Act
        var schema = await _service.GetSchemaAsync(container);

        // Assert
        Assert.AreEqual(SchemaTypeKind.Object, schema.RootType.Kind);
        var objType = (IObjectSchemaType)schema.RootType;
        Assert.IsTrue(objType.Properties.ContainsKey("Customer"));
    }

    [TestMethod]
    public async Task GetSchemaAsync_WithPath_ReturnsSchemaForPath()
    {
        // Arrange
        var data = new
        {
            Customers = new[]
            {
                new { Id = 1, Name = "Alice" },
                new { Id = 2, Name = "Bob" }
            }
        };
        var container = DataContainerFactory.Create(data);

        // Act
        var schema = await _service.GetSchemaAsync(container, "Customers");

        // Assert
        Assert.AreEqual(SchemaTypeKind.Array, schema.RootType.Kind);
        var arrayType = (IArraySchemaType)schema.RootType;
        Assert.AreEqual(SchemaTypeKind.Object, arrayType.ItemsSchema.Kind);
    }

    [TestMethod]
    public void InferSchema_SimpleData_CreatesSchema()
    {
        // Arrange
        var data = new { name = "Alice", age = 25 };

        // Act
        var schema = _service.InferSchema(data);

        // Assert
        var objType = (IObjectSchemaType)schema.RootType;
        Assert.AreEqual(2, objType.Properties.Count);
        Assert.AreEqual(SchemaTypeKind.String, objType.Properties["name"].Kind);
        Assert.AreEqual(SchemaTypeKind.Integer, objType.Properties["age"].Kind);
    }

    [TestMethod]
    public void InferSchemaFromType_Generic_CreatesSchema()
    {
        // Arrange & Act
        var schema = _service.InferSchemaFromType(typeof(SimpleCustomer));

        // Assert
        Assert.AreEqual("SimpleCustomer", schema.Title);
        var objType = (IObjectSchemaType)schema.RootType;
        Assert.IsTrue(objType.Properties.ContainsKey("FirstName"));
    }

    [TestMethod]
    public void ParseAny_AutoDetectsFormat()
    {
        // Arrange
        var jsonSchema = @"{
  ""$schema"": ""http://json-schema.org/draft-07/schema#"",
  ""type"": ""string""
}";

        // Act
        var schema = _service.ParseAny(jsonSchema);

        // Assert
        Assert.AreEqual(SchemaTypeKind.String, schema.RootType.Kind);
    }

    [TestMethod]
    public void ValidateAgainstSchema_ValidData_ReturnsSuccess()
    {
        // Arrange
        var schema = new CanonicalSchema
        {
            RootType = new ObjectSchemaType
            {
                Properties = new Dictionary<string, ISchemaType>
                {
                    ["name"] = new StringSchemaType { MinLength = 1 }
                },
                Required = new[] { "name" }
            }
        };
        var data = new { name = "Alice" };

        // Act
        var result = _service.ValidateAgainstSchema(data, schema);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void ValidateAgainstSchema_InvalidData_ReturnsErrors()
    {
        // Arrange
        var schema = new CanonicalSchema
        {
            RootType = new ObjectSchemaType
            {
                Properties = new Dictionary<string, ISchemaType>
                {
                    ["age"] = new IntegerSchemaType { Minimum = 0, Maximum = 120 }
                }
            }
        };
        var data = new { age = 200 };

        // Act
        var result = _service.ValidateAgainstSchema(data, schema);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.Errors.Count > 0);
    }

    [TestMethod]
    public void GetProvider_RegisteredFormat_ReturnsProvider()
    {
        // Act
        var provider = _service.GetProvider("jsonschema");

        // Assert
        Assert.IsNotNull(provider);
        Assert.AreEqual("jsonschema", provider.SchemaFormat);
    }

    [TestMethod]
    public void GetProvider_UnregisteredFormat_ReturnsNull()
    {
        // Act
        var provider = _service.GetProvider("unknown");

        // Assert
        Assert.IsNull(provider);
    }
}
```

---

### Category 5: Validation Tests (13 tests)

```csharp
[TestClass]
public class SchemaValidationTests
{
    [TestMethod]
    public void Validate_ObjectType_RequiredProperties()
    {
        // Arrange
        var schema = new ObjectSchemaType
        {
            Properties = new Dictionary<string, ISchemaType>
            {
                ["name"] = new StringSchemaType(),
                ["email"] = new StringSchemaType()
            },
            Required = new[] { "name", "email" }
        };

        var validData = new { name = "Alice", email = "alice@example.com" };
        var invalidData = new { name = "Bob" };  // Missing email

        // Act
        var validResult = schema.Validate(validData);
        var invalidResult = schema.Validate(invalidData);

        // Assert
        Assert.IsTrue(validResult.IsValid);
        Assert.IsFalse(invalidResult.IsValid);
        Assert.AreEqual(1, invalidResult.Errors.Count);
        Assert.AreEqual("required_missing", invalidResult.Errors[0].ErrorCode);
    }

    [TestMethod]
    public void Validate_StringType_LengthConstraints()
    {
        // Arrange
        var schema = new StringSchemaType
        {
            MinLength = 5,
            MaxLength = 10
        };

        // Act
        var tooShort = schema.Validate("abc");
        var tooLong = schema.Validate("this is too long");
        var valid = schema.Validate("valid");

        // Assert
        Assert.IsFalse(tooShort.IsValid);
        Assert.IsTrue(tooShort.Errors.Any(e => e.ErrorCode == "min_length_violated"));

        Assert.IsFalse(tooLong.IsValid);
        Assert.IsTrue(tooLong.Errors.Any(e => e.ErrorCode == "max_length_violated"));

        Assert.IsTrue(valid.IsValid);
    }

    [TestMethod]
    public void Validate_StringType_PatternConstraint()
    {
        // Arrange
        var schema = new StringSchemaType
        {
            Pattern = @"^\d{3}-\d{3}-\d{4}$"  // Phone number pattern
        };

        // Act
        var valid = schema.Validate("555-123-4567");
        var invalid = schema.Validate("not a phone");

        // Assert
        Assert.IsTrue(valid.IsValid);
        Assert.IsFalse(invalid.IsValid);
        Assert.AreEqual("pattern_mismatch", invalid.Errors[0].ErrorCode);
    }

    [TestMethod]
    public void Validate_NumberType_RangeConstraints()
    {
        // Arrange
        var schema = new IntegerSchemaType
        {
            Minimum = 0,
            Maximum = 100
        };

        // Act
        var tooLow = schema.Validate(-10);
        var tooHigh = schema.Validate(150);
        var valid = schema.Validate(50);

        // Assert
        Assert.IsFalse(tooLow.IsValid);
        Assert.IsFalse(tooHigh.IsValid);
        Assert.IsTrue(valid.IsValid);
    }

    [TestMethod]
    public void Validate_ArrayType_ItemsConstraint()
    {
        // Arrange
        var schema = new ArraySchemaType
        {
            ItemsSchema = new IntegerSchemaType { Minimum = 0 },
            MinItems = 2,
            MaxItems = 5
        };

        // Act
        var tooFew = schema.Validate(new[] { 1 });
        var tooMany = schema.Validate(new[] { 1, 2, 3, 4, 5, 6 });
        var invalidItem = schema.Validate(new[] { 1, -5, 3 });
        var valid = schema.Validate(new[] { 1, 2, 3 });

        // Assert
        Assert.IsFalse(tooFew.IsValid);
        Assert.IsFalse(tooMany.IsValid);
        Assert.IsFalse(invalidItem.IsValid);
        Assert.IsTrue(valid.IsValid);
    }

    [TestMethod]
    public void Validate_ArrayType_UniqueItems()
    {
        // Arrange
        var schema = new ArraySchemaType
        {
            ItemsSchema = new IntegerSchemaType(),
            UniqueItems = true
        };

        // Act
        var duplicates = schema.Validate(new[] { 1, 2, 3, 2, 4 });
        var unique = schema.Validate(new[] { 1, 2, 3, 4 });

        // Assert
        Assert.IsFalse(duplicates.IsValid);
        Assert.IsTrue(unique.IsValid);
    }

    [TestMethod]
    public void Validate_Nullable_AllowsNull()
    {
        // Arrange
        var nullableSchema = new StringSchemaType { IsNullable = true };
        var nonNullableSchema = new StringSchemaType { IsNullable = false };

        // Act
        var nullableResult = nullableSchema.Validate(null);
        var nonNullableResult = nonNullableSchema.Validate(null);

        // Assert
        Assert.IsTrue(nullableResult.IsValid);
        Assert.IsFalse(nonNullableResult.IsValid);
        Assert.AreEqual("null_not_allowed", nonNullableResult.Errors[0].ErrorCode);
    }

    [TestMethod]
    public void Validate_NestedObject_ValidatesRecursively()
    {
        // Arrange
        var schema = new ObjectSchemaType
        {
            Properties = new Dictionary<string, ISchemaType>
            {
                ["address"] = new ObjectSchemaType
                {
                    Properties = new Dictionary<string, ISchemaType>
                    {
                        ["city"] = new StringSchemaType { MinLength = 1 }
                    },
                    Required = new[] { "city" }
                }
            }
        };

        var validData = new
        {
            address = new { city = "Seattle" }
        };

        var invalidData = new
        {
            address = new { city = "" }  // Too short
        };

        // Act
        var validResult = schema.Validate(validData);
        var invalidResult = schema.Validate(invalidData);

        // Assert
        Assert.IsTrue(validResult.IsValid);
        Assert.IsFalse(invalidResult.IsValid);
        Assert.IsTrue(invalidResult.Errors[0].Path.Contains("/address/city"));
    }
}
```

---

## Integration Tests (20 tests)

### Category 6: DataContainer Integration (8 tests)

```csharp
[TestClass]
public class SchemaDiscoveryDataContainerIntegrationTests
{
    [TestMethod]
    public async Task DataContainer_GetSchema_ReflectsProviderData()
    {
        // Arrange
        var container = DataContainerFactory.Create();
        container.RegisterProvider("Customer", new StaticDataProvider(new
        {
            FirstName = "John",
            LastName = "Doe",
            Age = 30,
            Address = new { City = "Seattle", ZipCode = "98101" }
        }));

        var schemaService = new SchemaDiscoveryService();

        // Act
        var schema = await schemaService.GetSchemaAsync(container, "Customer");

        // Assert
        var objType = (IObjectSchemaType)schema.RootType;
        Assert.IsTrue(objType.Properties.ContainsKey("FirstName"));
        Assert.IsTrue(objType.Properties.ContainsKey("Address"));

        var addressType = (IObjectSchemaType)objType.Properties["Address"];
        Assert.IsTrue(addressType.Properties.ContainsKey("City"));
    }

    [TestMethod]
    public async Task DataContainer_GetSchemaAsJsonSchema_ReturnsJsonFormat()
    {
        // Arrange
        var data = new { name = "Test", value = 42 };
        var container = DataContainerFactory.Create(data);

        var schemaService = new SchemaDiscoveryService();
        schemaService.RegisterProvider(new JsonSchemaProvider());

        // Act
        var jsonSchema = await schemaService.GetSchemaAsAsync(container, "jsonschema");

        // Assert
        Assert.IsTrue(jsonSchema.Contains("$schema"));
        Assert.IsTrue(jsonSchema.Contains("name"));
        Assert.IsTrue(jsonSchema.Contains("value"));
    }

    [TestMethod]
    public async Task DataContainer_ValidateAgainstInferredSchema_Works()
    {
        // Arrange
        var template Data = new { name = "Alice", age = 25 };
        var container = DataContainerFactory.Create(templateData);

        var schemaService = new SchemaDiscoveryService();
        var schema = await schemaService.GetSchemaAsync(container);

        // Valid data
        var validData = new { name = "Bob", age = 30 };

        // Invalid data (age is string instead of int)
        var invalidData = new { name = "Charlie", age = "thirty" };

        // Act
        var validResult = schemaService.ValidateAgainstSchema(validData, schema);
        var invalidResult = schemaService.ValidateAgainstSchema(invalidData, schema);

        // Assert
        Assert.IsTrue(validResult.IsValid);
        Assert.IsFalse(invalidResult.IsValid);
    }
}
```

---

## Performance Benchmarks (5 tests)

```csharp
[TestClass]
[TestCategory(TestCategories.Performance)]
public class SchemaDiscoveryPerformanceTests
{
    [TestMethod]
    public void InferSchema_1000Types_CompletesUnder500ms()
    {
        // Arrange
        var engine = new SchemaInferenceEngine();
        var stopwatch = Stopwatch.StartNew();

        // Act
        for (int i = 0; i < 1000; i++)
        {
            engine.InferFromType(typeof(SimpleCustomer));
        }
        stopwatch.Stop();

        // Assert
        Assert.IsTrue(stopwatch.ElapsedMilliseconds < 500,
            $"Expected < 500ms, actual: {stopwatch.ElapsedMilliseconds}ms");
    }

    [TestMethod]
    public void ParseJsonSchema_1000Schemas_CompletesUnder1000ms()
    {
        // Arrange
        var provider = new JsonSchemaProvider();
        var jsonSchema = @"{""type"": ""object"", ""properties"": {""name"": {""type"": ""string""}}}";
        var stopwatch = Stopwatch.StartNew();

        // Act
        for (int i = 0; i < 1000; i++)
        {
            provider.Parse(jsonSchema);
        }
        stopwatch.Stop();

        // Assert
        Assert.IsTrue(stopwatch.ElapsedMilliseconds < 1000,
            $"Expected < 1000ms, actual: {stopwatch.ElapsedMilliseconds}ms");
    }

    [TestMethod]
    public void ValidateData_1000Validations_CompletesUnder500ms()
    {
        // Arrange
        var schema = new ObjectSchemaType
        {
            Properties = new Dictionary<string, ISchemaType>
            {
                ["name"] = new StringSchemaType { MinLength = 1 },
                ["age"] = new IntegerSchemaType { Minimum = 0, Maximum = 120 }
            },
            Required = new[] { "name" }
        };

        var data = new { name = "Alice", age = 30 };
        var stopwatch = Stopwatch.StartNew();

        // Act
        for (int i = 0; i < 1000; i++)
        {
            schema.Validate(data);
        }
        stopwatch.Stop();

        // Assert
        Assert.IsTrue(stopwatch.ElapsedMilliseconds < 500,
            $"Expected < 500ms, actual: {stopwatch.ElapsedMilliseconds}ms");
    }

    [TestMethod]
    public void TranslateSchema_100Translations_CompletesUnder1000ms()
    {
        // Arrange
        var service = new SchemaDiscoveryService();
        service.RegisterProvider(new JsonSchemaProvider());
        service.RegisterProvider(new XsdSchemaProvider());

        var jsonSchema = @"{""type"": ""object"", ""properties"": {""name"": {""type"": ""string""}}}";
        var stopwatch = Stopwatch.StartNew();

        // Act
        for (int i = 0; i < 100; i++)
        {
            service.TranslateSchema(jsonSchema, "jsonschema", "xsd");
        }
        stopwatch.Stop();

        // Assert
        Assert.IsTrue(stopwatch.ElapsedMilliseconds < 1000,
            $"Expected < 1000ms, actual: {stopwatch.ElapsedMilliseconds}ms");
    }

    [TestMethod]
    public void SchemaCache_ImprovesPerformance()
    {
        // Arrange
        var service = new CachingSchemaDiscoveryService(new SchemaDiscoveryService());
        var type = typeof(SimpleCustomer);

        // Warm-up
        service.InferSchemaFromType(type);

        // Act - First run (may hit cache)
        var stopwatch1 = Stopwatch.StartNew();
        for (int i = 0; i < 1000; i++)
        {
            service.InferSchemaFromType(type);
        }
        stopwatch1.Stop();

        // Assert - Cached run should be significantly faster
        Assert.IsTrue(stopwatch1.ElapsedMilliseconds < 100,
            $"Cached inference should be < 100ms, actual: {stopwatch1.ElapsedMilliseconds}ms");
    }
}
```

---

## Test Coverage Report

### Target Coverage by Component

| Component | Target Coverage | Priority |
|-----------|----------------|----------|
| ISchemaProvider implementations | 90% | HIGH |
| ICanonicalSchema | 95% | HIGH |
| SchemaDiscoveryService | 90% | HIGH |
| Schema inference engine | 85% | HIGH |
| Validation logic | 90% | HIGH |
| Schema types | 85% | MEDIUM |

---

## Success Criteria

- ✅ 70+ unit tests implemented
- ✅ 20+ integration tests implemented
- ✅ 5 performance benchmarks implemented
- ✅ 85%+ overall code coverage
- ✅ All schema providers have parse/format/validate tests
- ✅ Translation between all format pairs tested
- ✅ Schema inference from types and data tested
- ✅ Validation covers all constraint types
- ✅ Performance requirements met (< 500ms inference, < 1s translation)

---

## Related Documents

- [Requirements](./requirements.md)
- [Architecture](./architecture.md)
- [API Design](./api-design.md)
- [Core Container Testing](../CoreContainer/testing-strategy.md)
- [Path Translation Testing](../PathTranslation/testing-strategy.md)
