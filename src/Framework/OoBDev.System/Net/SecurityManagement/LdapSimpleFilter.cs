// Ignore Spelling: Ldap

using System;
using System.Linq;
using System.Text;

namespace OoBDev.System.Net.SecurityManagement;

/// <summary>
/// Represents a simple LDAP filter that compares an attribute to a value using a comparison operator.
/// Supports GUID, byte array, and string value comparisons.
/// </summary>
public record LdapSimpleFilter : ILdapFilter
{
    /// <summary>
    /// Initializes a new instance of the LdapSimpleFilter class for comparing a GUID attribute value.
    /// </summary>
    /// <param name="attributeName">The name of the LDAP attribute to filter on.</param>
    /// <param name="operation">The comparison operation to perform.</param>
    /// <param name="value">The GUID value to compare against.</param>
    public LdapSimpleFilter(string attributeName, LdapFilterTypes operation, Guid value)
        : this(attributeName, operation, value.ToByteArray())
    {
    }

    /// <summary>
    /// Initializes a new instance of the LdapSimpleFilter class for comparing a byte array attribute value.
    /// </summary>
    /// <param name="attributeName">The name of the LDAP attribute to filter on.</param>
    /// <param name="operation">The comparison operation to perform.</param>
    /// <param name="value">The byte array value to compare against, which will be hex-encoded.</param>
    public LdapSimpleFilter(string attributeName, LdapFilterTypes operation, byte[] value)
        : this(attributeName, operation, null, (value ?? Enumerable.Empty<byte>())
                                                        .Aggregate(new StringBuilder(),
                                                                   (sb, v) => sb.AppendFormat("\\{0:X}", v),
                                                                   sb => sb.ToString()))
    {
    }

    /// <summary>
    /// Initializes a new instance of the LdapSimpleFilter class for comparing a string attribute value.
    /// </summary>
    /// <param name="attributeName">The name of the LDAP attribute to filter on.</param>
    /// <param name="operation">The comparison operation to perform.</param>
    /// <param name="value">The string value to compare against.</param>
    public LdapSimpleFilter(string attributeName, LdapFilterTypes operation, string value)
        : this(attributeName, operation, value, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the LdapSimpleFilter class with an optional unescaped suffix for wildcard patterns.
    /// </summary>
    /// <param name="attributeName">The name of the LDAP attribute to filter on.</param>
    /// <param name="operation">The comparison operation to perform.</param>
    /// <param name="value">The value to compare against.</param>
    /// <param name="unEscapedSuffix">An optional unescaped suffix to append (used for wildcard patterns like *).</param>
    internal LdapSimpleFilter(string attributeName, LdapFilterTypes operation, string? value, string? unEscapedSuffix)
    {
        AttributeName = attributeName;
        Operation = operation;
        Value = value;
        UnEscapedSuffix = unEscapedSuffix;
    }

    /// <summary>
    /// Gets the name of the LDAP attribute being filtered.
    /// </summary>
    public string AttributeName { get; }

    /// <summary>
    /// Gets the comparison operation being performed.
    /// </summary>
    public LdapFilterTypes Operation { get; }

    /// <summary>
    /// Gets the value to compare the attribute against.
    /// </summary>
    public string? Value { get; }

    /// <summary>
    /// Gets the unescaped suffix appended to the value (used for wildcard patterns).
    /// </summary>
    internal string? UnEscapedSuffix { get; }
}
