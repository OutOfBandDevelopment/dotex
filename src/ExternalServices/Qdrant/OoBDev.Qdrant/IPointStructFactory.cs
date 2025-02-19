using OoBDev.System.IO;
using Qdrant.Client.Grpc;
using System;
using System.IO;

namespace OoBDev.Qdrant;

/// <summary>
/// Interface for a factory that creates instances of <see cref="PointStruct"/>.
/// </summary>
public interface IPointStructFactory
{
    /// <summary>
    /// Creates a new instance of <see cref="PointStruct"/> for a file chunk.
    /// </summary>
    /// <param name="metadata">Metadata associated with the file.</param>
    /// <param name="chunk">Content chunk of the file.</param>
    /// <param name="fileInfo">Information about the file.</param>
    /// <param name="vector">Vector representation of the content chunk.</param>
    /// <returns>A new instance of <see cref="PointStruct"/> representing a file chunk.</returns>
    PointStruct CreateFileChunk(FileMetaData metadata, ContentChunk chunk, FileInfo fileInfo, ReadOnlyMemory<float> vector);

    /// <summary>
    /// Creates a new instance of <see cref="PointStruct"/> for a question.
    /// </summary>
    /// <param name="uuid">Unique identifier of the question.</param>
    /// <param name="question">The question text.</param>
    /// <param name="vector">Vector representation of the question.</param>
    /// <param name="type">Optional type of the question.</param>
    /// <returns>A new instance of <see cref="PointStruct"/> representing a question.</returns>
    PointStruct CreateQuestion(Guid uuid, string question, ReadOnlyMemory<float> vector, string? type = null);

    /// <summary>
    /// Creates a new instance of <see cref="PointStruct"/> for a service reference.
    /// </summary>
    /// <param name="uuid">Unique identifier of the service reference.</param>
    /// <param name="serviceType">Type of the service.</param>
    /// <param name="description">Description of the service.</param>
    /// <param name="vector">Vector representation of the service reference.</param>
    /// <returns>A new instance of <see cref="PointStruct"/> representing a service reference.</returns>
    PointStruct CreateServiceReference(Guid uuid, Type serviceType, string description, ReadOnlyMemory<float> vector);
}
