// Ignore Spelling: Ldap

namespace OoBDev.System.Net.SecurityManagement;

public interface ILdapFilterBuilder
{
    string? Build(ILdapFilter filter);
}