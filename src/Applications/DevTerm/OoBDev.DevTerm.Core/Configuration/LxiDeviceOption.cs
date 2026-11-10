using System.Linq;
using OoBDev.System.IO.Tcp;
namespace OoBDev.DevTerm.Configuration;

/// <summary>
/// One entry in <see cref="ConnectionEditorViewModel.LxiDeviceOptions"/>: a LAN instrument found by a front end's own scan
/// (<see cref="LxiDeviceScanner.Scan"/>), with the host and raw-SCPI port that get written into the TCP fields when picked.
/// </summary>
public sealed record LxiDeviceOption(string Display, string Host, int Port, string Transport = "tcp")
{
    /// <summary>
    /// A hit from any probe, carrying the transport it suggests: <c>vxi11</c> and <c>mqtt</c> hits switch the editor to that
    /// transport; anything else is a plain <c>tcp</c> hit (a VXI-11-only hit's RPC port is not a raw-SCPI port, so none is kept).
    /// </summary>
    public static LxiDeviceOption FromHit(Discovery.NetworkDeviceHit hit) => hit.Transport switch
    {
        "vxi11" => new(hit.Display, hit.Address, 0, "vxi11"),
        "mqtt" => new(hit.Display, hit.Address, hit.Port, "mqtt"),
        "tcp" => new(hit.Display, hit.Address, hit.Port),
        _ => new(hit.Display, hit.Address, hit.Port),
    };

    /// <summary>
    /// One row for a whole device (decided 2026-10-13): the label lists its services as badges, and picking the row fills the
    /// service most likely to carry data: a raw tcp port first, then vxi11, then mqtt, then whatever else was found.
    /// </summary>
    public static LxiDeviceOption FromDevice(Discovery.NetworkDevice device)
    {
        global::System.ArgumentNullException.ThrowIfNull(device);
        var primary = device.Services.FirstOrDefault(s => s.Transport == "tcp" && s.Port > 0)
            ?? device.Services.FirstOrDefault(s => s.Transport == "vxi11")
            ?? device.Services.FirstOrDefault(s => s.Transport == "mqtt")
            ?? device.Services[0];
        var option = FromHit(primary);
        return option with { Display = $"{device.Address}  {device.Name}  [{device.Badges}]" };
    }

    public static LxiDeviceOption FromDevice(LxiDevice device) =>
        new(device.Display, device.Host, device.ScpiPort > 0 ? device.ScpiPort : 5025);
}
