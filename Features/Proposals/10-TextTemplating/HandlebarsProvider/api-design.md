# Handlebars Template Provider - API Design

**Epic:** 10 - Text Templating Extensions
**Feature:** Handlebars Template Provider
**Last Updated:** 2026-01-22

---

## API Overview

The Handlebars Template Provider API provides three primary components:
1. **HandlebarsTemplateProvider** - Main template provider implementation
2. **IDataContainerAdapter** - Bridge between IDataContainer and Handlebars
3. **Helper Registration** - Custom helper and partial registration

---

## Core Interfaces

### HandlebarsTemplateProvider

**Purpose:** Handlebars.NET implementation of ITemplateProvider.

```csharp
namespace OoBDev.Extensions.Templates.Handlebars;

/// <summary>
/// Handlebars.NET template provider with IDataContainer integration.
/// </summary>
public class HandlebarsTemplateProvider : ITemplateProvider
{
    /// <summary>
    /// Gets the provider name.
    /// </summary>
    public string ProviderName => "handlebars";

    /// <summary>
    /// Renders template with data.
    /// </summary>
    /// <param name="template">Handlebars template string</param>
    /// <param name="data">Data object (IDataContainer or POCO)</param>
    /// <param name="context">Optional rendering context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Rendered text</returns>
    public async Task<string> RenderAsync(
        string template,
        object data,
        IDictionary<string, object>? context = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Compiles template for reuse.
    /// </summary>
    /// <param name="template">Handlebars template string</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Compiled template</returns>
    public async Task<ICompiledTemplate> CompileAsync(
        string template,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Registers custom Handlebars helper.
    /// </summary>
    /// <param name="name">Helper name</param>
    /// <param name="helper">Helper delegate</param>
    public void RegisterHelper(string name, HandlebarsHelper helper);

    /// <summary>
    /// Registers custom Handlebars block helper.
    /// </summary>
    /// <param name="name">Block helper name</param>
    /// <param name="helper">Block helper delegate</param>
    public void RegisterBlockHelper(string name, HandlebarsBlockHelper helper);

    /// <summary>
    /// Registers Handlebars partial template.
    /// </summary>
    /// <param name="name">Partial name</param>
    /// <param name="template">Partial template string</param>
    public void RegisterPartial(string name, string template);
}
```

---

### IDataContainerAdapter

**Purpose:** Adapter interface for bridging IDataContainer to Handlebars data model.

```csharp
namespace OoBDev.Extensions.Templates.Handlebars;

/// <summary>
/// Adapts IDataContainer to Handlebars-compatible data object.
/// </summary>
public interface IDataContainerAdapter
{
    /// <summary>
    /// Adapts container to dynamic object for Handlebars rendering.
    /// </summary>
    /// <param name="container">Data container</param>
    /// <returns>Handlebars-compatible data object</returns>
    object Adapt(IDataContainer container);
}

/// <summary>
/// Default adapter using dynamic proxy.
/// </summary>
public class DefaultDataContainerAdapter : IDataContainerAdapter
{
    public object Adapt(IDataContainer container)
    {
        return new DataContainerProxy(container);
    }
}
```

---

### HandlebarsCompiledTemplate

**Purpose:** Wrapper for compiled Handlebars template.

```csharp
namespace OoBDev.Extensions.Templates.Handlebars;

/// <summary>
/// Compiled Handlebars template.
/// </summary>
public class HandlebarsCompiledTemplate : ICompiledTemplate
{
    private readonly HandlebarsTemplate<object, object> _compiledTemplate;
    private readonly IDataContainerAdapter _dataAdapter;

    public HandlebarsCompiledTemplate(
        HandlebarsTemplate<object, object> compiledTemplate,
        IDataContainerAdapter dataAdapter)
    {
        _compiledTemplate = compiledTemplate;
        _dataAdapter = dataAdapter;
    }

    /// <summary>
    /// Renders compiled template with data.
    /// </summary>
    public async Task<string> RenderAsync(
        object data,
        IDictionary<string, object>? context = null,
        CancellationToken cancellationToken = default)
    {
        var adaptedData = data is IDataContainer container
            ? _dataAdapter.Adapt(container)
            : data;

        var result = _compiledTemplate(adaptedData);
        return await Task.FromResult(result);
    }
}
```

---

## Helper Registration

### Built-in Helpers

#### formatDate
```csharp
/// <summary>
/// Formats date with specified format string.
/// Usage: {{formatDate dateValue "yyyy-MM-dd"}}
/// </summary>
_handlebars.RegisterHelper("formatDate", (writer, context, parameters) =>
{
    if (parameters.Length >= 1 && parameters[0] is DateTime date)
    {
        var format = parameters.Length >= 2
            ? parameters[1]?.ToString() ?? "yyyy-MM-dd"
            : "yyyy-MM-dd";

        writer.WriteSafeString(date.ToString(format));
    }
    else
    {
        writer.WriteSafeString(string.Empty);
    }
});
```

#### formatNumber
```csharp
/// <summary>
/// Formats number with specified format string.
/// Usage: {{formatNumber value "N2"}}
/// </summary>
_handlebars.RegisterHelper("formatNumber", (writer, context, parameters) =>
{
    if (parameters.Length >= 1)
    {
        try
        {
            var number = Convert.ToDecimal(parameters[0]);
            var format = parameters.Length >= 2
                ? parameters[1]?.ToString() ?? "N2"
                : "N2";

            writer.WriteSafeString(number.ToString(format));
        }
        catch
        {
            writer.WriteSafeString(parameters[0]?.ToString() ?? string.Empty);
        }
    }
});
```

#### currency
```csharp
/// <summary>
/// Formats number as currency.
/// Usage: {{currency totalAmount}}
/// </summary>
_handlebars.RegisterHelper("currency", (writer, context, parameters) =>
{
    if (parameters.Length >= 1)
    {
        try
        {
            var amount = Convert.ToDecimal(parameters[0]);
            writer.WriteSafeString(amount.ToString("C"));
        }
        catch
        {
            writer.WriteSafeString("$0.00");
        }
    }
});
```

#### uppercase / lowercase
```csharp
/// <summary>
/// Converts text to uppercase.
/// Usage: {{uppercase name}}
/// </summary>
_handlebars.RegisterHelper("uppercase", (writer, context, parameters) =>
{
    if (parameters.Length >= 1 && parameters[0] is string text)
    {
        writer.WriteSafeString(text.ToUpperInvariant());
    }
});

/// <summary>
/// Converts text to lowercase.
/// Usage: {{lowercase name}}
/// </summary>
_handlebars.RegisterHelper("lowercase", (writer, context, parameters) =>
{
    if (parameters.Length >= 1 && parameters[0] is string text)
    {
        writer.WriteSafeString(text.ToLowerInvariant());
    }
});
```

#### json
```csharp
/// <summary>
/// Serializes object to JSON.
/// Usage: {{json dataObject}}
/// </summary>
_handlebars.RegisterHelper("json", (writer, context, parameters) =>
{
    if (parameters.Length >= 1)
    {
        try
        {
            var json = JsonSerializer.Serialize(parameters[0], new JsonSerializerOptions
            {
                WriteIndented = parameters.Length >= 2 && Convert.ToBoolean(parameters[1])
            });
            writer.WriteSafeString(json);
        }
        catch (Exception ex)
        {
            writer.WriteSafeString($"{{\"error\": \"{ex.Message}\"}}");
        }
    }
});
```

---

## Dependency Injection Extensions

```csharp
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Dependency injection extensions for Handlebars template provider.
/// </summary>
public static class HandlebarsTemplateProviderServiceCollectionExtensions
{
    /// <summary>
    /// Adds Handlebars template provider to service collection.
    /// </summary>
    public static IServiceCollection AddHandlebarsTemplateProvider(
        this IServiceCollection services)
    {
        services.TryAddSingleton<IDataContainerAdapter, DefaultDataContainerAdapter>();
        services.TryAddSingleton<ITemplateProvider, HandlebarsTemplateProvider>();

        return services;
    }

    /// <summary>
    /// Adds Handlebars template provider with configuration.
    /// </summary>
    public static IServiceCollection AddHandlebarsTemplateProvider(
        this IServiceCollection services,
        Action<HandlebarsTemplateProviderOptions> configure)
    {
        services.Configure(configure);
        services.TryAddSingleton<IDataContainerAdapter, DefaultDataContainerAdapter>();
        services.TryAddSingleton<ITemplateProvider, HandlebarsTemplateProvider>();

        return services;
    }
}

/// <summary>
/// Configuration options for Handlebars template provider.
/// </summary>
public class HandlebarsTemplateProviderOptions
{
    /// <summary>
    /// Enable template compilation caching (default: true).
    /// </summary>
    public bool EnableCompilationCache { get; set; } = true;

    /// <summary>
    /// Maximum number of compiled templates to cache (default: 100).
    /// </summary>
    public int MaxCachedTemplates { get; set; } = 100;

    /// <summary>
    /// Custom helpers to register.
    /// </summary>
    public Dictionary<string, HandlebarsHelper> CustomHelpers { get; set; } = new();

    /// <summary>
    /// Custom block helpers to register.
    /// </summary>
    public Dictionary<string, HandlebarsBlockHelper> CustomBlockHelpers { get; set; } = new();

    /// <summary>
    /// Partials to register.
    /// </summary>
    public Dictionary<string, string> Partials { get; set; } = new();
}
```

---

## Usage Examples

### Example 1: Basic Template Rendering

```csharp
using OoBDev.Extensions.Templates.Handlebars;
using OoBDev.System.Data.Enhancement;

// Create provider
var provider = new HandlebarsTemplateProvider();

// Static data
var data = new
{
    Customer = new
    {
        FirstName = "John",
        LastName = "Doe",
        Email = "john@example.com"
    }
};

// Render template
var template = "Hello {{Customer.FirstName}} {{Customer.LastName}}!";
var result = await provider.RenderAsync(template, data);

Console.WriteLine(result);  // "Hello John Doe!"
```

---

### Example 2: IDataContainer Integration

```csharp
// Create data container with lazy providers
var container = DataContainerFactory.Create();

container.RegisterProvider("Customer", new DelegateDataProvider(async () =>
{
    // Fetch from database (executed ONLY if template accesses Customer)
    return await _customerRepo.GetByIdAsync(customerId);
}));

container.RegisterProvider("Order", new DelegateDataProvider(async () =>
{
    // Fetch from API (executed ONLY if template accesses Order)
    return await _orderService.GetOrderAsync(orderId);
}));

// Template uses ONLY Customer data
var template = @"
Dear {{Customer.FirstName}},

Thank you for your business!

Best regards,
The Team
";

// Render (ONLY Customer provider executes, Order provider never called)
var provider = new HandlebarsTemplateProvider();
var result = await provider.RenderAsync(template, container);
```

---

### Example 3: Conditionals and Loops

```csharp
var data = new
{
    Order = new
    {
        OrderNumber = "12345",
        IsActive = true,
        LineItems = new[]
        {
            new { ProductName = "Widget", Quantity = 2, UnitPrice = 19.99m },
            new { ProductName = "Gadget", Quantity = 1, UnitPrice = 29.99m }
        }
    }
};

var template = @"
Order #{{Order.OrderNumber}}

{{#if Order.IsActive}}
Status: Active
{{else}}
Status: Inactive
{{/if}}

Line Items:
{{#each Order.LineItems}}
- {{ProductName}}: {{Quantity}} x ${{UnitPrice}} = ${{multiply Quantity UnitPrice}}
{{/each}}
";

var provider = new HandlebarsTemplateProvider();
var result = await provider.RenderAsync(template, data);
```

---

### Example 4: Built-in Helpers

```csharp
var data = new
{
    Order = new
    {
        OrderDate = DateTime.UtcNow,
        Total = 99.99m,
        CustomerName = "john doe"
    }
};

var template = @"
Order Date: {{formatDate Order.OrderDate "MMM dd, yyyy"}}
Total: {{currency Order.Total}}
Customer: {{uppercase Order.CustomerName}}
";

var provider = new HandlebarsTemplateProvider();
var result = await provider.RenderAsync(template, data);

// Output:
// Order Date: Jan 22, 2026
// Total: $99.99
// Customer: JOHN DOE
```

---

### Example 5: Custom Helpers

```csharp
var provider = new HandlebarsTemplateProvider();

// Register custom helper
provider.RegisterHelper("multiply", (writer, context, parameters) =>
{
    if (parameters.Length >= 2)
    {
        var a = Convert.ToDecimal(parameters[0]);
        var b = Convert.ToDecimal(parameters[1]);
        writer.WriteSafeString((a * b).ToString("F2"));
    }
});

provider.RegisterHelper("trim", (writer, context, parameters) =>
{
    if (parameters.Length >= 1 && parameters[0] is string text)
    {
        writer.WriteSafeString(text.Trim());
    }
});

// Use custom helpers in template
var template = "{{multiply 5 10}} | {{trim '  hello  '}}";
var result = await provider.RenderAsync(template, new object());

Console.WriteLine(result);  // "50.00 | hello"
```

---

### Example 6: Block Helpers

```csharp
var provider = new HandlebarsTemplateProvider();

// Register custom block helper
provider.RegisterBlockHelper("section", (writer, options, context, arguments) =>
{
    var title = arguments.Length > 0 ? arguments[0]?.ToString() : "Section";

    writer.WriteSafeString($"<section><h2>{title}</h2>");
    options.Template(writer, context);
    writer.WriteSafeString("</section>");
});

var template = @"
{{#section 'Customer Information'}}
Name: {{Name}}
Email: {{Email}}
{{/section}}
";

var data = new { Name = "John Doe", Email = "john@example.com" };
var result = await provider.RenderAsync(template, data);
```

---

### Example 7: Partials

```csharp
var provider = new HandlebarsTemplateProvider();

// Register partials
provider.RegisterPartial("header", @"
<header>
  <h1>{{CompanyName}}</h1>
  <p>{{CompanySlogan}}</p>
</header>
");

provider.RegisterPartial("footer", @"
<footer>
  <p>&copy; {{Year}} {{CompanyName}}. All rights reserved.</p>
</footer>
");

// Main template uses partials
var template = @"
{{> header}}

<main>
  <p>Dear {{CustomerName}},</p>
  <p>{{MessageBody}}</p>
</main>

{{> footer}}
";

var data = new
{
    CompanyName = "Acme Corp",
    CompanySlogan = "Quality Products Since 1990",
    Year = DateTime.UtcNow.Year,
    CustomerName = "John Doe",
    MessageBody = "Thank you for your order!"
};

var result = await provider.RenderAsync(template, data);
```

---

### Example 8: Compiled Templates

```csharp
var provider = new HandlebarsTemplateProvider();

// Compile template once
var template = "Hello {{FirstName}} {{LastName}}!";
var compiled = await provider.CompileAsync(template);

// Reuse compiled template multiple times
var customers = new[]
{
    new { FirstName = "John", LastName = "Doe" },
    new { FirstName = "Jane", LastName = "Smith" },
    new { FirstName = "Bob", LastName = "Johnson" }
};

foreach (var customer in customers)
{
    var result = await compiled.RenderAsync(customer);
    Console.WriteLine(result);
}

// Output:
// Hello John Doe!
// Hello Jane Smith!
// Hello Bob Johnson!
```

---

### Example 9: Dependency Injection

```csharp
// Startup.cs / Program.cs
services.AddHandlebarsTemplateProvider(options =>
{
    options.EnableCompilationCache = true;
    options.MaxCachedTemplates = 200;

    // Register custom helpers
    options.CustomHelpers["add"] = (writer, context, parameters) =>
    {
        if (parameters.Length >= 2)
        {
            var sum = Convert.ToDecimal(parameters[0]) + Convert.ToDecimal(parameters[1]);
            writer.WriteSafeString(sum.ToString());
        }
    };

    // Register partials
    options.Partials["email-header"] = "<header>{{CompanyName}}</header>";
});

// Usage in controller/service
public class EmailService
{
    private readonly ITemplateProvider _templateProvider;

    public EmailService(ITemplateProvider templateProvider)
    {
        _templateProvider = templateProvider;
    }

    public async Task<string> GenerateEmailAsync(EmailData data)
    {
        var template = await LoadTemplateAsync("welcome-email");
        return await _templateProvider.RenderAsync(template, data);
    }
}
```

---

## Error Handling

### Exception Types

```csharp
namespace OoBDev.Extensions.Templates.Handlebars;

/// <summary>
/// Base exception for Handlebars template provider errors.
/// </summary>
public class HandlebarsTemplateException : TemplateException
{
    public HandlebarsTemplateException(string message)
        : base(message)
    {
    }

    public HandlebarsTemplateException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>
/// Exception thrown during template compilation.
/// </summary>
public class HandlebarsCompilationException : HandlebarsTemplateException
{
    public string Template { get; }

    public HandlebarsCompilationException(string message, string template, Exception innerException)
        : base(message, innerException)
    {
        Template = template;
    }
}

/// <summary>
/// Exception thrown during template rendering.
/// </summary>
public class HandlebarsRenderException : HandlebarsTemplateException
{
    public object Data { get; }

    public HandlebarsRenderException(string message, object data, Exception innerException)
        : base(message, innerException)
    {
        Data = data;
    }
}
```

### Error Handling Example

```csharp
try
{
    var provider = new HandlebarsTemplateProvider();
    var result = await provider.RenderAsync(template, data);
}
catch (HandlebarsCompilationException ex)
{
    _logger.LogError(ex, "Failed to compile Handlebars template");
    // Handle compilation error
}
catch (HandlebarsRenderException ex)
{
    _logger.LogError(ex, "Failed to render Handlebars template");
    // Handle rendering error
}
catch (HandlebarsTemplateException ex)
{
    _logger.LogError(ex, "Handlebars template error");
    // Handle general template error
}
```

---

## Best Practices

### 1. Template Compilation
```csharp
// ✅ GOOD: Compile once, render many times
var compiled = await provider.CompileAsync(template);
foreach (var data in dataList)
{
    var result = await compiled.RenderAsync(data);
}

// ❌ BAD: Recompile for each render
foreach (var data in dataList)
{
    var result = await provider.RenderAsync(template, data);  // Compiles each time!
}
```

### 2. Helper Safety
```csharp
// ✅ GOOD: Safe helper with error handling
provider.RegisterHelper("safeDivide", (writer, context, parameters) =>
{
    if (parameters.Length >= 2)
    {
        try
        {
            var a = Convert.ToDecimal(parameters[0]);
            var b = Convert.ToDecimal(parameters[1]);

            if (b != 0)
            {
                writer.WriteSafeString((a / b).ToString("F2"));
            }
            else
            {
                writer.WriteSafeString("N/A");
            }
        }
        catch
        {
            writer.WriteSafeString("ERROR");
        }
    }
});

// ❌ BAD: Unsafe helper without error handling
provider.RegisterHelper("unsafeDivide", (writer, context, parameters) =>
{
    var a = Convert.ToDecimal(parameters[0]);  // May throw
    var b = Convert.ToDecimal(parameters[1]);  // May throw
    writer.WriteSafeString((a / b).ToString());  // May throw (divide by zero)
});
```

### 3. IDataContainer Usage
```csharp
// ✅ GOOD: Let Handlebars lazily evaluate paths
var container = DataContainerFactory.Create();
container.RegisterProvider("Customer", expensiveProvider);

var template = "{{#if needsCustomer}}{{Customer.Name}}{{/if}}";
var result = await provider.RenderAsync(template, container);
// Provider executes ONLY if needsCustomer is true

// ❌ BAD: Eagerly evaluate all data
var customer = await expensiveProvider.ProvideAsync(...);  // Always executes
var template = "{{#if needsCustomer}}{{Customer.Name}}{{/if}}";
var result = await provider.RenderAsync(template, new { Customer = customer });
```

---

## Related Documents

- [Requirements](./requirements.md)
- [Architecture](./architecture.md)
- [Testing Strategy](./testing-strategy.md)
- [IDataContainer API](../../11-DataEnhancement/CoreContainer/api-design.md)
