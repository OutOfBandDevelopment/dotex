﻿using System.Collections.Generic;

namespace OoBDev.System.IO;

public interface IDeviceFactory
{
    bool CanGetDevice(object? definition);
    IEnumerable<string> GetDeviceNames();
    IDeviceAdapter? GetDevice(string devicePath, object? definition);
}
