using System;
using System.Collections.Generic;
using System.Text.Json;
using OoBDev.Generations;

namespace OoBDev.WebApi.Models;

/// <summary>Which sample type to generate and the seed that fixes the result.</summary>
/// <param name="SampleType">One of <c>person</c>, <c>customer</c> or <c>order</c>.</param>
/// <param name="Seed">The same seed always gives the same object graph.</param>
public sealed record GenerationRequestModel(string SampleType, int Seed);

/// <summary>The generated object graph.</summary>
/// <param name="SampleType">The sample type that was generated.</param>
/// <param name="Seed">The seed that was used.</param>
/// <param name="Value">The graph as JSON.</param>
public sealed record GenerationResponseModel(string SampleType, int Seed, JsonElement Value);

/// <summary>A generated person; the attributes steer each property.</summary>
public sealed class SamplePerson
{
    /// <summary>Full name.</summary>
    [GenerateString(Preset = StringPreset.Name)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Email address.</summary>
    [GenerateString(Preset = StringPreset.Email)]
    public string Email { get; set; } = string.Empty;

    /// <summary>Phone number.</summary>
    [GenerateString(Preset = StringPreset.Phone)]
    public string Phone { get; set; } = string.Empty;

    /// <summary>Age in years.</summary>
    [GenerateNumber(Min = 18, Max = 90)]
    public int Age { get; set; }

    /// <summary>Date of the last visit.</summary>
    [GenerateDateTime(DaysBack = 30)]
    public DateTime LastVisit { get; set; }

    /// <summary>Whether the person is active.</summary>
    public bool Active { get; set; }
}

/// <summary>A generated order line.</summary>
public sealed class SampleOrderLine
{
    /// <summary>Product name.</summary>
    [GenerateString(MinLength = 4, MaxLength = 10)]
    public string Product { get; set; } = string.Empty;

    /// <summary>Units ordered.</summary>
    [GenerateNumber(Min = 1, Max = 12)]
    public int Quantity { get; set; }
}

/// <summary>A generated order with its lines.</summary>
public sealed class SampleOrder
{
    /// <summary>Order identifier.</summary>
    public Guid Id { get; set; }

    /// <summary>Delivery address.</summary>
    [GenerateString(Preset = StringPreset.Address)]
    public string ShipTo { get; set; } = string.Empty;

    /// <summary>The order lines.</summary>
    [GenerateCollection(MinCount = 1, MaxCount = 4)]
    public List<SampleOrderLine> Lines { get; set; } = [];
}

/// <summary>A generated customer with orders.</summary>
public sealed class SampleCustomer
{
    /// <summary>The customer.</summary>
    public SamplePerson Person { get; set; } = new();

    /// <summary>Orders placed.</summary>
    [GenerateCollection(MinCount = 1, MaxCount = 3)]
    public List<SampleOrder> Orders { get; set; } = [];
}
