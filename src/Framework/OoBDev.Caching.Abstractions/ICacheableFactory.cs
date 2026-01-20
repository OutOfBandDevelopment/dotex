namespace OoBDev.Caching;

public interface ICacheableFactory
{
    TInterface Create<TInterface, TImplemention>() where TImplemention : class, TInterface;
}
