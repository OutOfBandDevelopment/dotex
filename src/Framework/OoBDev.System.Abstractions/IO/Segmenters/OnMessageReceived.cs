using System.Threading.Tasks;

namespace OoBDev.System.IO.Segmenters;

public delegate Task OnMessageReceived(object message);
public delegate Task OnMessageReceived<TMessage>(TMessage message);
