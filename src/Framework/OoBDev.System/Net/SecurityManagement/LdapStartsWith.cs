// Ignore Spelling: Ldap

namespace OoBDev.System.Net.SecurityManagement;

public record LdapStartsWith : LdapSimpleFilter
{
    public LdapStartsWith(string attributeName, string value)
        : base(attributeName, LdapFilterTypes.Equals, value, "*")
    {
    }
}
