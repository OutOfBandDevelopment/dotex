using System;
using System.Collections.Generic;
using OoBDev.System.IO.UsbTmc;

namespace OoBDev.System.IO.Usb.Bridge.Tests;

internal sealed class FakeUsbTmcDevice : IUsbTmcDevice
{
    public List<byte[]> Written { get; } = [];
    public byte[] Reply { get; set; } = [];
    public bool Stalled { get; set; }
    public bool TimeOutOnRead { get; set; }
    public List<bool> RemoteCalls { get; } = [];
    public List<byte> AbortedIn { get; } = [];
    public int ClearCount { get; private set; }
    public bool Disposed { get; private set; }
    public bool IsOpen { get; private set; }
    public int MaxTransferSize => 4096;
    public int MaxPacketSize => 64;

    public void Open() => IsOpen = true;
    public void Close() => IsOpen = false;
    public void WriteBulkOut(byte[] transfer) => Written.Add(transfer);

    public int ReadBulkIn(byte[] buffer, out bool stalled)
    {
        if (TimeOutOnRead)
            throw new TimeoutException("fake timeout");
        stalled = Stalled;
        Array.Copy(Reply, buffer, Reply.Length);
        return Reply.Length;
    }

    public void AbortBulkIn(byte bTag) => AbortedIn.Add(bTag);
    public void AbortBulkOut(byte bTag) { }
    public void Clear() => ClearCount++;
    public void SetRemote(bool remote) => RemoteCalls.Add(remote);

    public void Dispose()
    {
        Disposed = true;
        IsOpen = false;
    }
}
