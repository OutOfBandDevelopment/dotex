using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace OoBDev.System.Net.Protocols;

/// <summary>
/// Provides Wake-on-LAN functionality to wake up computers over the network.
/// Implements the Wake-on-LAN protocol by sending magic packets containing the target MAC address.
/// </summary>
public class WakeOnLan
{
    /// <summary>
    /// Builds a Wake-on-LAN magic packet for the specified MAC address.
    /// The magic packet consists of 6 bytes of 0xFF followed by 16 repetitions of the target MAC address (102 bytes total).
    /// </summary>
    /// <param name="macAddress">The MAC address of the computer to wake (format: XX:XX:XX:XX:XX:XX or XX-XX-XX-XX-XX-XX).</param>
    /// <returns>A byte array containing the magic packet payload.</returns>
    /// <exception cref="InvalidMacAddressException">Thrown when the MAC address format is invalid.</exception>
    public static byte[] BuildMagicPacket(string macAddress)
    {
        var clientMac = MacAddressEx.Parse(macAddress);
        var message = new[]{
            [0xff,0xff,0xff,0xff,0xff,0xff,],
            clientMac,
            clientMac,
            clientMac,
            clientMac,
            clientMac,
            clientMac,
            clientMac,
            clientMac,
            clientMac,
            clientMac,
            clientMac,
            clientMac,
            clientMac,
            clientMac,
            clientMac,
            clientMac,
        };
        var payload = message.SelectMany(b => b).ToArray();
        return payload;
    }

    /// <summary>
    /// Sends a Wake-on-LAN magic packet to wake up a computer with the specified MAC address.
    /// Broadcasts the packet over UDP to ensure it reaches the target computer even if it's powered off.
    /// </summary>
    /// <param name="macAddress">The MAC address of the computer to wake (format: XX:XX:XX:XX:XX:XX or XX-XX-XX-XX-XX-XX).</param>
    /// <param name="ipAddress">The broadcast IP address to send to. Defaults to "255.255.255.255" (global broadcast).</param>
    /// <returns>True if the magic packet (102 bytes) was successfully sent; otherwise, false.</returns>
    /// <exception cref="InvalidMacAddressException">Thrown when the MAC address format is invalid.</exception>
    public static async Task<bool> Wake(string macAddress, string ipAddress = "255.255.255.255")
    {
        var clientAddress = IPAddress.Parse(ipAddress);
        var magicPacket = BuildMagicPacket(macAddress);

        // http://en.wikipedia.org/wiki/Wake-on-LAN#Principle_of_operation
        using var client = new UdpClient();
        client.Connect(clientAddress, 65535);
        client.EnableBroadcast = true;
        client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 0);

        var result = await client.SendAsync(magicPacket, magicPacket.Length);

        return result == 102; /* MagicPacket length should be broadcast MAC + target MAX x 16 */
    }
}
