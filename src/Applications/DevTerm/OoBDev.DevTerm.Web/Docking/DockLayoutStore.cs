using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.JSInterop;
namespace OoBDev.DevTerm.Web.Docking;

/// <summary>Keeps the layout in the browser's <c>localStorage</c>; the server stores nothing.</summary>
/// <param name="js">The JavaScript runtime of the circuit.</param>
public sealed class DockLayoutStore(IJSRuntime js)
{
    /// <summary>The <c>localStorage</c> key.</summary>
    public const string Key = "devterm.layout.v1";

    /// <summary>Reads the saved layout; a missing, corrupt or stale value gives the default layout.</summary>
    /// <param name="registered">The registered panel ids.</param>
    public async Task<DockLayout> LoadAsync(IReadOnlyCollection<string> registered)
    {
        string? json = null;
        try
        {
            json = await js.InvokeAsync<string?>("localStorage.getItem", Key);
        }
        catch (JSException)
        {
            // Storage blocked by the browser: start from the default layout.
        }

        return DockLayout.Load(json, registered);
    }

    /// <summary>Saves the layout.</summary>
    /// <param name="layout">The layout to keep.</param>
    public async Task SaveAsync(DockLayout layout)
    {
        try
        {
            await js.InvokeVoidAsync("localStorage.setItem", Key, layout.ToJson());
        }
        catch (JSException)
        {
            // Storage blocked: the layout lasts for this page only.
        }
    }
}
