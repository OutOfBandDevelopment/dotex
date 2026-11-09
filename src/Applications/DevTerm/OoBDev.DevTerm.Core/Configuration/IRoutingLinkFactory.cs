namespace OoBDev.DevTerm.Configuration;

public interface IRoutingLinkFactory
{
    IRoutingLink Create(RoutingOptions options);
}
