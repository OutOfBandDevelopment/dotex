using System;
using System.ComponentModel.DataAnnotations;

namespace OoBDev.MongoDB.Tests;

public class TestCollection
{
    [Key]
    public string? TestId { get; set; }

    public string? Value1 { get; set; }
    public DateTimeOffset Date { get; internal set; }
    public string? Value2 { get; set; }
}
