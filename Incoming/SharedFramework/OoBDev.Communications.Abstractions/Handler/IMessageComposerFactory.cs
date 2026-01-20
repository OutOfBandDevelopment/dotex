using OoBDev.Communications.Contracts.Channels;

namespace OoBDev.Communications.Abstractions.Handler
{
    public interface IMessageComposerFactory
    {
        IMessageComposer GetComposer(string channel);
    }
}
