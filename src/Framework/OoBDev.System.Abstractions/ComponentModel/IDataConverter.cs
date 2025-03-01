using System;

namespace OoBDev.System.ComponentModel;

public interface IDataConverter
{
    T? ConvertTo<T>(object? value);
    object? ConvertTo(object? value, Type type, object? defaultValue = null);
    bool IsNull(object? value);
}
