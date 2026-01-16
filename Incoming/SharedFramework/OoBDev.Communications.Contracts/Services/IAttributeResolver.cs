
namespace OoBDev.Communications.Contracts.Services
{
    public interface IAttributeResolver
    {
        string GetMessageType<T>();
        RequestPriorities GetPriority<T>();
    }
}
