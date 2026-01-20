namespace OoBDev.Data.Vectors;

#if NETSTANDARD2_1

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public class SqlFunctionAttribute : Attribute
{
    public SqlFunctionAttribute() { }
    public bool IsDeterministic { get; set; }
    public DataAccessKind DataAccess { get; set; }
    public SystemDataAccessKind SystemDataAccess { get; set; }
    public bool IsPrecise { get; set; }
    public string Name { get; set; }
    public string TableDefinition { get; set; }
    public string FillRowMethodName { get; set; }
}

public enum DataAccessKind
{
    None = 0,
    Read = 1
}

public enum SystemDataAccessKind
{
    None = 0,
    Read = 1
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
public sealed class SqlUserDefinedTypeAttribute : Attribute
{
    public SqlUserDefinedTypeAttribute(Format format)
    {
        Format = format;
    }
    public int MaxByteSize { get; set; }
    public bool IsFixedLength { get; set; }
    public bool IsByteOrdered { get; set; }
    public Format Format { get; private set; }
    public string ValidationMethodName { get; set; }
    public string Name { get; set; }
}

public enum Format
{
    Unknown = 0,
    Native = 1,
    UserDefined = 2
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class SqlMethodAttribute : SqlFunctionAttribute
{

    public SqlMethodAttribute() { }

    public bool OnNullCall { get; set; }

    public bool IsMutator { get; set; }
    public bool InvokeIfReceiverIsNull { get; set; }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
public sealed class SqlUserDefinedAggregateAttribute : Attribute
{

    public const int MaxByteSizeValue = 8000;

    public SqlUserDefinedAggregateAttribute(Format format)
    {
        Format = format;
    }
    public int MaxByteSize { get; set; }
    public bool IsInvariantToDuplicates { get; set; }
    public bool IsInvariantToNulls { get; set; }
    public bool IsInvariantToOrder { get; set; }
    public bool IsNullIfEmpty { get; set; }
    public Format Format { get; private set; }
    public string Name { get; set; }
}

public interface IBinarySerialize
{
    void Read(BinaryReader r);
    void Write(BinaryWriter w);
}
#endif
