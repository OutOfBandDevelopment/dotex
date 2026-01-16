using OoBDev.Communications.Contracts.Channels;

namespace OoBDev.Communications.Contracts.Handler
{
    public interface IMessageComposerFactory
    {
        IMessageComposer GetComposer(string channel);
    }
}
