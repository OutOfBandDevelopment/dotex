using System;

namespace OoBDev.Generations;

/// <summary>Builds object graphs from a seed; the same seed and type always give the same graph.</summary>
public interface IGenerator
{
    /// <summary>Creates a value of <typeparamref name="T"/>.</summary>
    /// <typeparam name="T">A class with a public parameterless constructor, an interface, or a supported primitive.</typeparam>
    T Create<T>();

    /// <summary>Creates a value of the given type.</summary>
    /// <param name="type">The type.</param>
    object? Create(Type type);
}
