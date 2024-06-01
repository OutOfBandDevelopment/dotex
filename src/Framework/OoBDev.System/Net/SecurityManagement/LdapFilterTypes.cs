namespace OoBDev.System.Net.SecurityManagement;

public enum LdapFilterTypes
{
    /// <remarks>=</remarks>
    Equals,

    /// <remarks>~=</remarks>
    Approximate,
    /// <remarks>&gt;=</remarks>
    GreaterThanOrEqualTo,
    /// <remarks>&lt;=</remarks>
    LessThanOrEqualTo,
}
