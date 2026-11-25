namespace OoBDev.System.IO.Bluetooth;

/// <summary>
/// Creates the <see cref="IBleConnection"/> a <see cref="BleDeviceAdapter"/> connects through. Indirection
/// point that lets tests substitute a fake adapter instead of touching a real Bluetooth stack, and
/// lets a per-OS backend (see <see cref="IBleConnection"/>'s doc comment) register its own factory in
/// place of <see cref="UnsupportedPlatformBleConnectionFactory"/>.
/// </summary>
public interface IBleConnectionFactory
{
    IBleConnection Create(BleDeviceAdapterOptions options);
}
