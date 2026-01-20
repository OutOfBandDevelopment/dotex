namespace OoBDev.Caching.Abstractions
{
    public interface ICacheableFactory
    {
        TInterface Create<TInterface, TImplemention>() where TImplemention : class, TInterface;
    }
}
