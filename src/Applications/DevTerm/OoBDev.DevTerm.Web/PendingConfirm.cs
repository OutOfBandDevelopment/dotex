using System.Threading.Tasks;
namespace OoBDev.DevTerm.Web;

/// <summary>A broker-to-device message awaiting the viewer's decision.</summary>
public sealed class PendingConfirm(int id, string topic, string text)
{
    public int Id { get; } = id;

    public string Topic { get; } = topic;

    public string Text { get; } = text;

    internal TaskCompletionSource<RoutingConfirmChoice> Completion { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
}
