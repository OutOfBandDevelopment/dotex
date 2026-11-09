namespace OoBDev.System.IO.Ports.Bridge;

internal sealed class ActionDisposable : IDisposable
{
    private Action? _action;

    public ActionDisposable(Action action)
    {
        _action = action;
    }

    public void Dispose() => Interlocked.Exchange(ref _action, null)?.Invoke();
}
