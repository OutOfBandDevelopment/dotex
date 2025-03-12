// Ignore Spelling: Ldap

namespace OoBDev.System.Net.SecurityManagement;

public record LdapPresentFilter : LdapSimpleFilter
{
    public LdapPresentFilter(string attributeName)
        : base(attributeName, LdapFilterTypes.Equals, "", "*")
    {
    }
}
