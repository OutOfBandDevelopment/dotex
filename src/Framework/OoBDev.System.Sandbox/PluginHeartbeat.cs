using System;
using System.Threading.Tasks;

namespace OoBDev.System.Sandbox;

/// <summary>Sends a heartbeat and waits for the plugin's answer; a plugin that does not answer in time is declared hung (protocol-and-manifest.md, Table 8).</summary>
public static class PluginHeartbeat
{
    /// <summary>Returns true when the plugin answered within <paramref name="timeout"/>. On a miss the plugin is disposed, which ends it.</summary>
    public static bool Check(ISandboxedPlugin plugin, TimeSpan timeout)
    {
        ArgumentNullException.ThrowIfNull(plugin);
        var sequence = plugin.Send(FrameType.Heartbeat, "lifecycle.heartbeat", "{}");
        var answer = Task.Run(() =>
        {
            while (true)
            {
                if (plugin.Receive().Ack == sequence)
                {
                    return true;
                }
            }
        });
        if (answer.Wait(timeout))
        {
            return answer.Result;
        }

        plugin.Dispose();
        try
        {
            answer.Wait(TimeSpan.FromSeconds(5));
        }
        catch (AggregateException)
        {
        }

        return false;
    }
}
