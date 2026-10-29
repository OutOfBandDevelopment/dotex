using System.Collections.Generic;

namespace OoBDev.ZWave;

/// <summary>
/// Names for the Serial API function ids, command classes and device classes the package understands. Anything else prints as hex.
/// </summary>
public static class ZWaveNames
{
    private static readonly Dictionary<byte, string> _functions = new()
    {
        [ZWaveFunctions.GetInitData] = "SerialApiGetInitData",
        [0x03] = "SerialApiApplNodeInformation",
        [ZWaveFunctions.ApplicationCommandHandler] = "ApplicationCommandHandler",
        [ZWaveFunctions.GetControllerCapabilities] = "GetControllerCapabilities",
        [ZWaveFunctions.SetTimeouts] = "SerialApiSetTimeouts",
        [ZWaveFunctions.GetCapabilities] = "SerialApiGetCapabilities",
        [ZWaveFunctions.SoftReset] = "SerialApiSoftReset",
        [ZWaveFunctions.SendData] = "SendData",
        [0x14] = "SendDataMulti",
        [ZWaveFunctions.GetVersion] = "GetVersion",
        [ZWaveFunctions.MemoryGetId] = "MemoryGetId",
        [ZWaveFunctions.GetNodeProtocolInfo] = "GetNodeProtocolInfo",
        [ZWaveFunctions.ApplicationUpdate] = "ApplicationUpdate",
        [ZWaveFunctions.GetSucNodeId] = "GetSucNodeId",
        [ZWaveFunctions.SetDefault] = "SetDefault",
        [ZWaveFunctions.RequestNodeNeighborUpdate] = "RequestNodeNeighborUpdate",
        [ZWaveFunctions.AddNodeToNetwork] = "AddNodeToNetwork",
        [ZWaveFunctions.RemoveNodeFromNetwork] = "RemoveNodeFromNetwork",
        [ZWaveFunctions.SetLearnMode] = "SetLearnMode",
        [ZWaveFunctions.RequestNodeInfo] = "RequestNodeInfo",
        [ZWaveFunctions.RemoveFailedNode] = "RemoveFailedNode",
        [ZWaveFunctions.IsFailedNode] = "IsFailedNode",
    };

    private static readonly Dictionary<byte, string> _commandClasses = new()
    {
        [0x20] = "Basic",
        [0x25] = "Binary Switch",
        [0x26] = "Multilevel Switch",
        [0x30] = "Binary Sensor",
        [0x31] = "Multilevel Sensor",
        [0x32] = "Meter",
        [0x40] = "Thermostat Mode",
        [0x43] = "Thermostat Setpoint",
        [0x5B] = "Central Scene",
        [0x5E] = "Z-Wave Plus Info",
        [0x60] = "Multi Channel",
        [0x62] = "Door Lock",
        [0x63] = "User Code",
        [0x70] = "Configuration",
        [0x71] = "Notification",
        [0x72] = "Manufacturer Specific",
        [0x73] = "Powerlevel",
        [0x80] = "Battery",
        [0x84] = "Wake Up",
        [0x85] = "Association",
        [0x86] = "Version",
        [0x98] = "Security",
        [0x9F] = "Security 2",
    };

    private static readonly Dictionary<byte, string> _basicDeviceClasses = new()
    {
        [0x01] = "Controller",
        [0x02] = "Static Controller",
        [0x03] = "Slave",
        [0x04] = "Routing Slave",
    };

    private static readonly Dictionary<byte, string> _genericDeviceClasses = new()
    {
        [0x01] = "Remote Controller",
        [0x02] = "Static Controller",
        [0x07] = "Notification Sensor",
        [0x08] = "Thermostat",
        [0x10] = "Binary Switch",
        [0x11] = "Multilevel Switch",
        [0x20] = "Binary Sensor",
        [0x21] = "Multilevel Sensor",
        [0x31] = "Meter",
        [0x40] = "Entry Control",
    };

    private static readonly Dictionary<byte, string> _libraryTypes = new()
    {
        [0x01] = "static controller",
        [0x02] = "controller",
        [0x03] = "enhanced slave",
        [0x04] = "slave",
        [0x05] = "installer",
        [0x06] = "routing slave",
        [0x07] = "bridge controller",
        [0x08] = "device under test",
    };

    private static readonly Dictionary<ushort, string> _manufacturers = new()
    {
        [0x0086] = "Aeotec",
    };

    /// <summary>Gets the name of a Serial API function id, or <c>function 0x..</c>.</summary>
    /// <param name="id">The function id.</param>
    /// <returns>The name.</returns>
    public static string Function(byte id) => _functions.TryGetValue(id, out var name) ? name : $"function 0x{id:X2}";

    /// <summary>Gets the name of a command class id, or <c>class 0x..</c>.</summary>
    /// <param name="id">The command class id.</param>
    /// <returns>The name.</returns>
    public static string CommandClass(byte id) =>
        _commandClasses.TryGetValue(id, out var name) ? name : ZWaveRegistry.CommandClass(id) ?? $"class 0x{id:X2}";

    /// <summary>Gets the product name registered for a manufacturer, product type and product id, or <see langword="null"/>.</summary>
    /// <param name="manufacturer">The manufacturer id.</param>
    /// <param name="type">The product type.</param>
    /// <param name="id">The product id.</param>
    /// <returns>The name, or <see langword="null"/> when not registered.</returns>
    public static string? Product(ushort manufacturer, ushort type, ushort id) => ZWaveRegistry.Product(manufacturer, type, id);

    /// <summary>Gets the configuration parameters the manufacturer documents for a product.</summary>
    /// <param name="manufacturer">The manufacturer id.</param>
    /// <param name="type">The product type.</param>
    /// <param name="id">The product id.</param>
    /// <returns>The parameters, or an empty list when the product is not in the table.</returns>
    public static IReadOnlyList<ZWaveConfigParameter> ConfigParameters(ushort manufacturer, ushort type, ushort id) =>
        ZWaveRegistry.ConfigParameters(manufacturer, type, id);

    /// <summary>Gets the registered name of a generic and specific device class pair, or <see langword="null"/>.</summary>
    /// <param name="generic">The generic device class.</param>
    /// <param name="specific">The specific device class.</param>
    /// <returns>The name, or <see langword="null"/> when not registered.</returns>
    public static string? DeviceClass(byte generic, byte specific) => ZWaveRegistry.DeviceClass(generic, specific);

    /// <summary>Gets the name of a basic device class id, or hex.</summary>
    /// <param name="id">The basic device class id.</param>
    /// <returns>The name.</returns>
    public static string BasicDeviceClass(byte id) => _basicDeviceClasses.TryGetValue(id, out var name) ? name : $"0x{id:X2}";

    /// <summary>Gets the name of a generic device class id, or hex.</summary>
    /// <param name="id">The generic device class id.</param>
    /// <returns>The name.</returns>
    public static string GenericDeviceClass(byte id) => _genericDeviceClasses.TryGetValue(id, out var name) ? name : $"0x{id:X2}";

    /// <summary>Gets the name of a Serial API library type, or <c>type 0x..</c>.</summary>
    /// <param name="id">The library type byte.</param>
    /// <returns>The name.</returns>
    public static string LibraryType(byte id) => _libraryTypes.TryGetValue(id, out var name) ? name : $"type 0x{id:X2}";

    /// <summary>Gets the manufacturer name, or the hex id when it is not known.</summary>
    /// <param name="id">The Z-Wave Alliance manufacturer id.</param>
    /// <returns>The name.</returns>
    public static string Manufacturer(ushort id) =>
        _manufacturers.TryGetValue(id, out var name) ? name : ZWaveRegistry.Manufacturer(id) ?? $"0x{id:X4}";
}
