using System;

namespace OoBDev.System.Security.Cryptography;

public interface IHMACCalculator
{
    ReadOnlySpan<byte> Calculate(string secret, string message);
    string Encode(ReadOnlySpan<byte> bytes);
    string CalculateAndEncode(string secret, string message);
}
