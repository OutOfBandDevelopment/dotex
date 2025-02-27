// Ignore Spelling: Ldap

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OoBDev.System.Net.SecurityManagement;

public class LdapFilterBuilder
{
    public string Build(ILdapFilter filter)
    {
        ArgumentNullException.ThrowIfNull(filter);

        var result = (Build(filter as LdapSimpleFilter)
                  ?? Build(filter as LdapFilterSetBase)
                  ?? Build(filter as LdapNotFilter)) ?? throw new NotSupportedException(string.Format("LdapFilterType: {0} is not supported", filter.GetType()))
                  ;
        return result;
    }

    private string? Build(LdapNotFilter filter) => filter == null ? null : string.Format("(!{0})", Build(filter.Wrapped));

    private readonly Dictionary<LdapFilterTypes, string> _simpleMap = new()
    {
        { LdapFilterTypes.Equals, "="},
        { LdapFilterTypes.Approximate, "~="},
        { LdapFilterTypes.GreaterThanOrEqualTo, ">="},
        { LdapFilterTypes.LessThanOrEqualTo, "<="},
    };
    private string? Build(LdapSimpleFilter filter)
    {
        if (filter == null)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(filter.AttributeName))
        {
            throw new NullReferenceException("filter.AttributeName must be provided");
        }

        var notAllowed = "()*\0";
        return notAllowed.Any(c => filter.AttributeName.Contains(c))
            ? throw new InvalidOperationException("Invalid character found in filter.AttributeName")
            : string.Format("({0}{1}{2}{3})", filter.AttributeName, _simpleMap[filter.Operation], EscapedValue(filter.Value), filter.UnEscapedSuffix);
    }
    private static string? EscapedValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }
        /*
           *               0x2a
           (               0x28
           )               0x29
           \               0x5c
           NUL             0x00
       */
        return value.Replace("*", @"\2a")
                    .Replace("(", @"\28")
                    .Replace(")", @"\29")
                    .Replace(@"\", @"\5c")
                    .Replace("\0", @"\00")
            ;
    }

    private readonly Dictionary<LdapFilterSetOperations, string> _setMap = new()
    {
        { LdapFilterSetOperations.And, "&"},
        { LdapFilterSetOperations.Or, "|"},
    };
    internal string? Build(LdapFilterSetBase filter)
    {
        return filter == null
            ? null
            : (filter.FilterSet ?? [])
                .Aggregate(new StringBuilder("(" + _setMap[filter.Operation]),
                           (b, v) => b.Append(Build(v)),
                           b => b.Append(')').ToString()
                           );
    }
}
