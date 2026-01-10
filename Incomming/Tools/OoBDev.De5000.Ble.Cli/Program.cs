using InTheHand.Bluetooth;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace OoBDev.De5000.Ble.Cli;

class Program
{
    static async Task Main(string[] args)
    {
        //         //new BluetoothLEScanFilter() { Name = "SH-HC-08"},
        //         //new BluetoothLEScanFilter() { Name = "DE-5000"},

        //var devices = new[] { await BluetoothDevice.FromIdAsync("3403DE435AB5") };

        await ScanDevicesAsync(
            name:"DE-5000"
            );
    }

    public static async Task ScanDevicesAsync(string? id = default, string? name = default)
    {
        Console.WriteLine("Scanning for BLE devices...");

        IEnumerable<BluetoothDevice> devices = (id, name) switch
        {
            (string inputValue, _) => [await BluetoothDevice.FromIdAsync(inputValue)],
            (null, string nameValue) => await Bluetooth.ScanForDevicesAsync(new ()
            {
                Filters =
                {
                    new () { Name = name},
                },
            }),
            (null, null) => await Bluetooth.ScanForDevicesAsync()
        };

        // Use the default adapter to start scanning
        foreach (var device in devices)
        {
            Console.WriteLine($"Device: {device.Name} - {device.Id}");

            foreach (var service in await device.Gatt.GetPrimaryServicesAsync())
            {
                Console.WriteLine($"\t- S:{service.Uuid.Value}");
                foreach (var characteristic in await service.GetCharacteristicsAsync())
                {
                    if (characteristic.Properties.HasFlag(GattCharacteristicProperties.Read))
                    {
                        Console.WriteLine($"\t\t- C:{characteristic.Uuid} = {string.Join("-", characteristic.Value?.Select(i => i.ToString("x2")) ?? Enumerable.Empty<string>())}");
                        foreach (var descriptors in await characteristic.GetDescriptorsAsync())
                        {
                            try
                            {
                                Console.WriteLine($"\t\t\t- D:{descriptors.Uuid} = {string.Join("-", descriptors.Value?.Select(i => i.ToString("x2")) ?? Enumerable.Empty<string>())}");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"\t\t\t- D:{descriptors.Uuid} = ERR:{ex.Message}");
                            }
                        }
                    }
                }
                foreach (var included in await service.GetIncludedServicesAsync())
                {
                    Console.WriteLine($"\t\t- I:{included.Uuid}");
                }
            }
        }

        Console.WriteLine("Scan complete.");

    }
}
