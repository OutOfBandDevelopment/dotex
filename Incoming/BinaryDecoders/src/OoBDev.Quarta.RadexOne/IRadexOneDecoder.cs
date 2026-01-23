using OoBDev.IO.Messages;

namespace OoBDev.Quarta.RadexOne;

/// <summary>
/// used to convert buffered data to correct value type
/// </summary>
public interface IRadexOneDecoder : IMessageDecoder<IRadexObject>
{
}