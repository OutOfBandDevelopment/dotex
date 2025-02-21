using OoBDev.System.Linq.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OoBDev.System.Tests.Linq;

public static class TestDataBuilder
{
    private readonly static Dictionary<Type, ConstructorInfo> _cache = [];
    public static ConstructorInfo Constructor<T>()
    {
        if (_cache.TryGetValue(typeof(T), out var constructor)) return constructor;
        _cache.Add(typeof(T), typeof(T).GetConstructor([typeof(int)])
            ?? throw new NotSupportedException($"No Constructor(int) found")
            );
        return _cache.TryGetValue(typeof(T), out constructor) ? constructor : throw new NotSupportedException($"No Constructor(int) found");
    }

    public static T Factory<T>(int index) => (T)Constructor<T>().Invoke([index]);

    public static IQueryable<T> GetTestData<T>(int seed) =>
        Enumerable.Range(seed, QueryBuilder.DefaultPageSize * 100)
                  .Select(Factory<T>)
                  .AsQueryable();

    public static IQueryable GetTestData(Type type, int seed) =>
        (typeof(TestDataBuilder)
        .GetMethod(nameof(GetTestData), 1, [typeof(int)])
        ?.MakeGenericMethod(type)
        ?.Invoke(null, [seed]) as IQueryable)
        ?? throw new NotSupportedException($"No GetTestData<> Found");
}
