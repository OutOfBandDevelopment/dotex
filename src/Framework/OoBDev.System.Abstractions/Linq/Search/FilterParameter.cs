using System.Text;
using System.Text.Json.Serialization;

namespace OoBDev.System.Linq.Search;

/// <summary>
/// Filter parameter
/// </summary>
public record FilterParameter
{
    /// <summary>
    /// `Equal To`: pass in the value to match for a given property  
    /// 
    /// If you are using string values you may also use wild cards  
    /// \*bc -> Ends with  
    /// \*b\* -> Contains  
    /// ab\* -> Starts with
    /// </summary>
    [JsonPropertyName("eq")]
    public object? EqualTo { get; set; }

    /// <summary>
    /// `Not Equal To`: pass in the value to match for a given property  
    /// 
    /// If you are using string values you may also use wild cards  
    /// \*bc -> Ends with  
    /// \*b\* -> Contains  
    /// ab\* -> Starts with
    /// </summary>
    [JsonPropertyName("neq")]
    public object? NotEqualTo { get; set; }

    /// <summary>
    /// This allows for providing a set of values where the value from the queries data must match at least 
    /// one of provided values
    /// </summary>
    [JsonPropertyName("in")]
    public object?[]? InSet { get; set; }

    /// <summary>
    /// `Greater than`
    /// </summary>
    [JsonPropertyName("gt")]
    public object? GreaterThan { get; set; }
    /// <summary>
    /// `Greater than or equal to`
    /// </summary>
    [JsonPropertyName("gte")]
    public object? GreaterThanOrEqualTo { get; set; }

    /// <summary>
    /// `Less than`
    /// </summary>
    [JsonPropertyName("lt")]
    public object? LessThan { get; set; }
    /// <summary>
    /// `Less than or equal to`
    /// </summary>
    [JsonPropertyName("lte")]
    public object? LessThanOrEqualTo { get; set; }

    ///// <summary>
    ///// build `or` predicate chain
    ///// </summary>
    //[JsonPropertyName("or")]
    //public FilterParameter[] Ors { get; set; }
    ///// <summary>
    ///// build `and` predicate chain
    ///// </summary>
    //[JsonPropertyName("and")]
    //public FilterParameter[] Ands { get; set; }

    ///// <summary>
    ///// include null values if true
    ///// </summary>
    //[JsonPropertyName("isnull")]
    //public bool OrNull { get; set; }

    /// <summary>
    /// convert FilterParameter to string
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        var sb = new StringBuilder();
        if (EqualTo != null) sb.AppendLine($"{nameof(EqualTo)}: {EqualTo} ");
        if (NotEqualTo != null) sb.AppendLine($"{nameof(NotEqualTo)}: {NotEqualTo} ");
        if (InSet != null) sb.AppendLine($"{nameof(InSet)}: {string.Join("; ", InSet)} ");
        if (GreaterThan != null) sb.AppendLine($"{nameof(GreaterThan)}: {GreaterThan} ");
        if (GreaterThanOrEqualTo != null) sb.AppendLine($"{nameof(GreaterThanOrEqualTo)}: {GreaterThanOrEqualTo} ");
        if (LessThan != null) sb.AppendLine($"{nameof(LessThan)}: {LessThan} ");
        if (LessThanOrEqualTo != null) sb.AppendLine($"{nameof(LessThanOrEqualTo)}: {LessThanOrEqualTo} ");
        //if (Ors != null) sb.AppendLine($"{nameof(Ors)}: ({string.Join(" | ", (Ors ?? Array.Empty<FilterParameter>()).AsEnumerable())}) ");
        //if (Ands != null) sb.AppendLine($"{nameof(Ands)}: ({string.Join(" | ", (Ands ?? Array.Empty<FilterParameter>()).AsEnumerable())}) ");
        //if (OrNull != null) sb.AppendLine($"{nameof(OrNull)}: {OrNull} ");
        return sb.ToString().Trim();
    }
}
