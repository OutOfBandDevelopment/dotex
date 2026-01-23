using System;

namespace OoBDev.ToolKit.Numerics;

public interface INumeric : IFormattable
{
    double AsDouble();
    float AsSingle();
    decimal AsDecimal();
    short AsInt16();
    int AsInt32();
    long AsInt64();
}
