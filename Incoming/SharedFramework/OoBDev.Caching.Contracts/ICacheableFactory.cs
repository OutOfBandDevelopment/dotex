namespace OoBDev.Caching.Contracts
{
    public interface ICacheableFactory
    {
        TInterface Create<TInterface, TImplemention>() where TImplemention : class, TInterface;
    }
}
