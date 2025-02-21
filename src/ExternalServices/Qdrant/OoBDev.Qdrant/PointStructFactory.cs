using OoBDev.System.IO;
using Qdrant.Client.Grpc;
using System;
using System.IO;

namespace OoBDev.Qdrant;

/// <summary>
/// Factory for creating instances of <see cref="PointStruct"/>.
/// </summary>
public class PointStructFactory : IPointStructFactory
{
    /// <summary>
    /// Represents the constant value indicating service instance type.
    /// </summary>
    public const string ServiceInstanceType = "service-instance";

    /// <summary>
    /// Creates a <see cref="PointStruct"/> instance representing a file chunk.
    /// </summary>
    /// <param name="metaData">The metadata of the file.</param>
    /// <param name="chunk">The content chunk.</param>
    /// <param name="fileInfo">Information about the file.</param>
    /// <param name="vector">The vector representing the file chunk.</param>
    /// <returns>The created <see cref="PointStruct"/>.</returns>
    public virtual PointStruct CreateFileChunk(FileMetaData metaData, ContentChunk chunk, FileInfo fileInfo, ReadOnlyMemory<float> vector) =>
        new()
        {
            Id = new() { Uuid = metaData.Uuid },
            Payload = {
                [nameof(metaData.Path)] = metaData.Path,
                [nameof(metaData.Path) + nameof(metaData.Hash)] = Convert.ToBase64String(metaData.Hash),
                [nameof(metaData.Path) + nameof(metaData.Hash) + "Id"] =new Guid(metaData.Hash).ToString(),
                [nameof(metaData.BasePath)] = metaData.BasePath,

                [$"chunk_{nameof(chunk.Data)}"] =chunk.Data,
                [$"chunk_{nameof(chunk.Length)}"] =chunk.Length,
                [$"chunk_{nameof(chunk.Sequence)}"] =chunk.Sequence,
                [$"chunk_{nameof(chunk.Start)}"] =chunk.Start,

                [$"file_{nameof(fileInfo.Length)}"] =fileInfo.Length,
                [$"file_{nameof(fileInfo.LastAccessTime)}"] =fileInfo.LastAccessTime.ToString("yyyyMMddHHmmss"),
                [$"file_{nameof(fileInfo.LastWriteTime)}"] =fileInfo.LastWriteTime.ToString("yyyyMMddHHmmss"),
                [$"file_{nameof(fileInfo.CreationTime)}"] =fileInfo.CreationTime.ToString("yyyyMMddHHmmss"),
                [$"file_{nameof(fileInfo.Extension)}"] =fileInfo.Extension,
            },

            Vectors = new Vectors { Vector = vector.ToArray(), }
        };

    /// <summary>
    /// Creates a <see cref="PointStruct"/> instance representing a service reference.
    /// </summary>
    /// <param name="uuid">The UUID of the service reference.</param>
    /// <param name="serviceType">The type of the service.</param>
    /// <param name="description">The description of the service reference.</param>
    /// <param name="vector">The vector representing the service reference.</param>
    /// <returns>The created <see cref="PointStruct"/>.</returns>
    public virtual PointStruct CreateServiceReference(
        Guid uuid,
        Type serviceType,
        string description,
        ReadOnlyMemory<float> vector
        ) =>
        new()
        {
            Id = new() { Uuid = uuid.ToString() },

            Payload =
            {
                    [nameof( serviceType.Name)] = serviceType.Name,
                    [nameof( serviceType.FullName)] = serviceType.FullName ?? serviceType.Name,
                    [nameof( serviceType.AssemblyQualifiedName)] = serviceType.AssemblyQualifiedName ?? serviceType.Name,
                    [nameof( serviceType.Namespace)] = serviceType.Namespace ?? serviceType.Name,
                    [nameof( description)] = description,

                    ["type"]= ServiceInstanceType,
            },

            Vectors = new Vectors { Vector = vector.ToArray(), }
        };

    /// <summary>
    /// Creates a <see cref="PointStruct"/> instance representing a question.
    /// </summary>
    /// <param name="uuid">The UUID of the question.</param>
    /// <param name="question">The question text.</param>
    /// <param name="vector">The vector representing the question.</param>
    /// <param name="type">The type of the question.</param>
    /// <returns>The created <see cref="PointStruct"/>.</returns>
    public virtual PointStruct CreateQuestion(
        Guid uuid,
        string question,
        ReadOnlyMemory<float> vector,
        string? type = null
        ) =>
        new()
        {
            Id = new() { Uuid = uuid.ToString() },

            Payload =
            {
                [nameof( question)] = question,

                ["type"]= type ?? nameof(question),
            },

            Vectors = new Vectors { Vector = vector.ToArray(), }
        };
}
