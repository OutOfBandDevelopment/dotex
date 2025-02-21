using OoBDev.System.Text.Json.Serialization;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;

namespace OoBDev.MongoDB.Extensions;

/// <summary>
/// This proxy allow for dynamic creation of wrapper classes to expose MongoDatabase instances
/// </summary>
internal class MongoDispatchProxy : DispatchProxy
{
    private IMongoDatabase _database = null!;
    private IMongoSettings _settings = null!;
    private IJsonSerializer _jsonSerializer = null!;

    private readonly Dictionary<MethodInfo, object> _collections = [];

    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
    {
        ArgumentNullException.ThrowIfNull(targetMethod, nameof(targetMethod));

        if (_collections.TryGetValue(targetMethod, out var ret))
        {
            return ret;
        }

        //TODO: need to simplify this name logic with that on the DataloaderCommandFactory
        var originalName = targetMethod.Name.StartsWith("get_") ? targetMethod.Name[4..] : targetMethod.Name;

        var name = targetMethod.DeclaringType?.GetProperty(originalName)
            ?.GetCustomAttributes()
            ?.OfType<TableAttribute>()
            ?.FirstOrDefault()?.Name;

        if (string.IsNullOrWhiteSpace(name) &&
            targetMethod.ReturnType.IsGenericType)
        {
            var targetType = targetMethod.ReturnType.GetGenericArguments()[0];
            name = targetType
               ?.GetCustomAttributes()
               ?.OfType<TableAttribute>()
               ?.FirstOrDefault()?.Name;
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            name = originalName;
            _jsonSerializer.AsPropertyName(originalName);
        }

        var collectionType = targetMethod.ReturnType.GetGenericArguments()[0];
        var collection = _database.GetType()
            ?.GetMethod(nameof(IMongoDatabase.GetCollection))
            ?.MakeGenericMethod(collectionType)
            ?.Invoke(_database,
            [
                name,
                null
            ])
            ?? throw new NotSupportedException();

        if (_collections.TryGetValue(targetMethod, out ret))
        {
            return ret;
        }

        _collections.Add(targetMethod, collection);
        return collection;
    }

    public static T Create<T>(
        IMongoDatabase database,
        IMongoSettings settings,
        IJsonSerializer jsonSerializer
        )
    {
        object instance = Create<T, MongoDispatchProxy>() ?? throw new NotSupportedException();

        var proxy = (MongoDispatchProxy)instance;
        proxy._database = database;
        proxy._settings = settings;
        proxy._jsonSerializer = jsonSerializer;

        return (T)instance;
    }
}
