namespace OoBDev.System.Net.SecurityManagement;

public class LdapNotFilter : ILdapFilter
{
    public LdapNotFilter(ILdapFilter wrapped)
    {
        Wrapped = wrapped;
    }

    public ILdapFilter Wrapped { get; init; }

    public override bool Equals(object obj) => obj switch { LdapNotFilter inner => Wrapped.Equals(inner.Wrapped), _ => false };

    public override int GetHashCode() => new { Wrapped, }.GetHashCode();
}
