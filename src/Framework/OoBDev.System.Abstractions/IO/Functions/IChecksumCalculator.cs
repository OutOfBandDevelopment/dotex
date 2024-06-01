using System;

namespace OoBDev.System.IO.Functions;

public interface IChecksumCalculator
{
    ushort Simple16(ReadOnlySpan<ushort> buffer);
}