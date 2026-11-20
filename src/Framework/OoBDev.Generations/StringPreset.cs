namespace OoBDev.Generations;

/// <summary>Kinds of generated text; all data is made up from small built-in lists.</summary>
public enum StringPreset
{
    /// <summary>Random lower-case letters.</summary>
    Word,

    /// <summary>A first and last name.</summary>
    Name,

    /// <summary>An address at example.test.</summary>
    Email,

    /// <summary>A phone number in the 555-01xx range.</summary>
    Phone,

    /// <summary>A street address line.</summary>
    Address,
}
