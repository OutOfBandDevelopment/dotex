using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OoBDev.DevTerm;

namespace OoBDev.DevTerm.Configuration;

public interface IUsbDeviceOption
{
    int VendorId { get; }
    int ProductId { get; }
    string? SerialNumber { get; }
    string? DevicePath { get; }
}
