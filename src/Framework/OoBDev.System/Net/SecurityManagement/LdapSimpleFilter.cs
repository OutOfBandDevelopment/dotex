// Ignore Spelling: Ldap

using System;
using System.Linq;
using System.Text;

namespace OoBDev.System.Net.SecurityManagement;

public record LdapSimpleFilter : ILdapFilter
{
    public LdapSimpleFilter(string attributeName, LdapFilterTypes operation, Guid value)
        : this(attributeName, operation, value.ToByteArray())
    {
    }

    public LdapSimpleFilter(string attributeName, LdapFilterTypes operation, byte[] value)
        : this(attributeName, operation, null, (value ?? Enumerable.Empty<byte>())
                                                        .Aggregate(new StringBuilder(),
                                                                   (sb, v) => sb.AppendFormat("\\{0:X}", v),
                                                                   sb => sb.ToString()))
    {
    }

    public LdapSimpleFilter(string attributeName, LdapFilterTypes operation, string value)
        : this(attributeName, operation, value, null)
    {
    }
    internal LdapSimpleFilter(string attributeName, LdapFilterTypes operation, string? value, string? unEscapedSuffix)
    {
        AttributeName = attributeName;
        Operation = operation;
        Value = value;
        UnEscapedSuffix = unEscapedSuffix;
    }

    public string AttributeName { get; }
    public LdapFilterTypes Operation { get; }
    public string? Value { get; }
    internal string? UnEscapedSuffix { get; }
}
