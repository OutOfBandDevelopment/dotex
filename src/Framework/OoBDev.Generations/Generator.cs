using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace OoBDev.Generations;

/// <summary>Seeded generator driven by attributes; unattributed properties get a default per type.</summary>
public sealed class Generator : IGenerator
{
    private static readonly string[] FirstNames = ["Ada", "Ben", "Cora", "Dev", "Eli", "Fay", "Gus", "Hana"];
    private static readonly string[] LastNames = ["Archer", "Brook", "Clarke", "Dunn", "Ellis", "Frost", "Grant", "Hale"];
    private static readonly string[] Streets = ["Oak", "Maple", "Cedar", "Pine", "Elm", "Birch"];

    private readonly Random _random;
    private readonly TimeProvider _time;
    private readonly int _maxDepth;

    /// <summary>Creates a generator.</summary>
    /// <param name="seed">Same seed, same output.</param>
    /// <param name="time">Supplies the end of generated date windows.</param>
    /// <param name="maxDepth">How deep a graph goes before nested objects become null.</param>
    public Generator(int seed, TimeProvider time, int maxDepth = 3)
    {
        _random = new Random(seed);
        _time = time ?? throw new ArgumentNullException(nameof(time));
        _maxDepth = maxDepth;
    }

    /// <inheritdoc />
    public T Create<T>() => (T)Create(typeof(T))!;

    /// <inheritdoc />
    public object? Create(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);
        return Build(type, null, type.Name, 0);
    }

    private object? Build(Type type, PropertyInfo? attributes, string path, int depth)
    {
        var underlying = Nullable.GetUnderlyingType(type) ?? type;
        if (underlying == typeof(string)) return MakeString(attributes?.GetCustomAttributes(typeof(GenerateStringAttribute), true) is [GenerateStringAttribute s, ..] ? s : new GenerateStringAttribute());
        if (underlying == typeof(bool)) return _random.Next(2) == 1;
        if (underlying == typeof(Guid)) return new Guid(_random.NextInt64().ToString("X16", CultureInfo.InvariantCulture).PadLeft(32, '0'));
        if (underlying.IsEnum) return Enum.GetValues(underlying).GetValue(_random.Next(Enum.GetValues(underlying).Length));
        if (underlying == typeof(DateTime) || underlying == typeof(DateTimeOffset)) return MakeDate(underlying, attributes);
        if (IsNumber(underlying)) return MakeNumber(underlying, attributes);

        if (depth >= _maxDepth) return type.IsValueType ? Activator.CreateInstance(type) : null;

        if (type.IsArray) return MakeArray(type.GetElementType()!, attributes, path, depth);
        if (type.IsGenericType && type.GetGenericTypeDefinition() is var d && (d == typeof(List<>) || d == typeof(IList<>) || d == typeof(IEnumerable<>) || d == typeof(IReadOnlyList<>) || d == typeof(ICollection<>)))
            return MakeList(type.GetGenericArguments()[0], attributes, path, depth);
        if (type.IsInterface) return MakeProxy(type, path, depth);
        if (type.IsClass && !type.IsAbstract && type.GetConstructor(Type.EmptyTypes) is not null) return MakeObject(type, path, depth);

        throw new NotSupportedException($"Cannot generate \"{path}\" of type {type.FullName}");
    }

    private static bool IsNumber(Type t) => t == typeof(int) || t == typeof(long) || t == typeof(short) || t == typeof(byte) || t == typeof(decimal) || t == typeof(double) || t == typeof(float);

    private object MakeNumber(Type type, PropertyInfo? attributes)
    {
        var a = attributes?.GetCustomAttributes(typeof(GenerateNumberAttribute), true) is [GenerateNumberAttribute n, ..] ? n : new GenerateNumberAttribute();
        var value = _random.NextInt64(a.Min, a.Max + 1);
        return Convert.ChangeType(value, type, CultureInfo.InvariantCulture);
    }

    private object MakeDate(Type type, PropertyInfo? attributes)
    {
        var a = attributes?.GetCustomAttributes(typeof(GenerateDateTimeAttribute), true) is [GenerateDateTimeAttribute d, ..] ? d : new GenerateDateTimeAttribute();
        var now = _time.GetUtcNow();
        var date = now.AddSeconds(-_random.NextInt64(0, (long)a.DaysBack * 86400 + 1));
        if (type == typeof(DateTimeOffset)) return date;
        return date.UtcDateTime;
    }

    private string MakeString(GenerateStringAttribute a) => a.Preset switch
    {
        StringPreset.Name => $"{Pick(FirstNames)} {Pick(LastNames)}",
        StringPreset.Email => $"{Pick(FirstNames).ToLowerInvariant()}.{Pick(LastNames).ToLowerInvariant()}{_random.Next(100)}@example.test",
        StringPreset.Phone => $"555-01{_random.Next(100):00}",
        StringPreset.Address => $"{_random.Next(1, 999)} {Pick(Streets)} Street",
        _ => Word(_random.Next(a.MinLength, a.MaxLength + 1)),
    };

    private string Pick(string[] list) => list[_random.Next(list.Length)];

    private string Word(int length)
    {
        var chars = new char[length];
        for (var i = 0; i < length; i++) chars[i] = (char)('a' + _random.Next(26));
        return new string(chars);
    }

    private int Count(PropertyInfo? attributes)
    {
        var a = attributes?.GetCustomAttributes(typeof(GenerateCollectionAttribute), true) is [GenerateCollectionAttribute c, ..] ? c : new GenerateCollectionAttribute();
        return _random.Next(a.MinCount, a.MaxCount + 1);
    }

    private Array MakeArray(Type element, PropertyInfo? attributes, string path, int depth)
    {
        var count = Count(attributes);
        var array = Array.CreateInstance(element, count);
        for (var i = 0; i < count; i++) array.SetValue(Build(element, null, $"{path}[{i}]", depth + 1), i);
        return array;
    }

    private IList MakeList(Type element, PropertyInfo? attributes, string path, int depth)
    {
        var count = Count(attributes);
        var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(element))!;
        for (var i = 0; i < count; i++) list.Add(Build(element, null, $"{path}[{i}]", depth + 1));
        return list;
    }

    private object MakeObject(Type type, string path, int depth)
    {
        var instance = Activator.CreateInstance(type)!;
        foreach (var p in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!p.CanWrite || p.GetIndexParameters().Length > 0) continue;
            p.SetValue(instance, Build(p.PropertyType, p, $"{path}.{p.Name}", depth + 1));
        }

        return instance;
    }

    private object MakeProxy(Type type, string path, int depth)
    {
        var proxy = DispatchProxy.Create(type, typeof(PropertyProxy));
        ((PropertyProxy)proxy).Factory = (propertyType, name) =>
        {
            var property = type.GetProperty(name);
            return Build(propertyType, property, $"{path}.{name}", depth + 1);
        };
        return proxy;
    }
}
