using System;
using System.Threading;
using System.Threading.Tasks;
using OoBDev.UiDefinitions;

namespace OoBDev.DevTerm.Panels;

internal static class SurfaceSender
{
    public static void Send(IControlSurface surface, string commandId, string? value, Action<Exception>? onError)
    {
        Task task;
        try
        {
            task = surface.InvokeAsync(commandId, value);
        }
        catch (Exception ex) when (onError is not null)
        {
            onError(ex);
            return;
        }

        if (onError is not null)
        {
            _ = task.ContinueWith(t => onError(t.Exception!.GetBaseException()), CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted, TaskScheduler.Default);
        }
    }
}
