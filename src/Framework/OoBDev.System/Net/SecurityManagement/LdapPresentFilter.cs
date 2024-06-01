namespace OoBDev.System.Net.SecurityManagement;

public class LdapPresentFilter : LdapSimpleFilter
{
    public LdapPresentFilter(string attributeName)
        : base(attributeName, LdapFilterTypes.Equals, "", "*")
    {
    }
}
