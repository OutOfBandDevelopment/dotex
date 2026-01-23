using OoBDev.System.IO.Messages;
using System;
using System.Linq;

namespace OoBDev.System.IO.Segmenters;

/// <summary>
/// Provides a fluent API for building data segment definitions.
/// </summary>
public static class Segment
{
    /// <summary>
    /// Defines a segment that starts with a specific control character.
    /// </summary>
    /// <param name="start">The control character marking the start of the segment.</param>
    /// <returns>A segment build definition that can be further configured.</returns>
    public static ISegmentBuildDefinition StartsWith(ControlCharacters start) =>
        StartsWith((byte)start);

    /// <summary>
    /// Defines a segment that starts with one of the specified byte values.
    /// </summary>
    /// <param name="starts">The byte values that can mark the start of the segment.</param>
    /// <returns>A segment build definition that can be further configured.</returns>
    public static ISegmentBuildDefinition StartsWith(params byte[] starts) =>
        new SegmentBuildDefinition(starts);

    /// <summary>
    /// Defines a segment that starts with any byte value matching the specified bitmask.
    /// </summary>
    /// <param name="mask">The bitmask to apply when checking for segment start bytes.</param>
    /// <returns>A segment build definition that can be further configured.</returns>
    public static ISegmentBuildDefinition StartsWithMask(byte mask) =>
        new SegmentBuildDefinition(
            [.. Enumerable.Range(0, 255)
                      .Select(b => (byte)(b & mask))
                      .Where(b => b != 0x00)
                      .Distinct()]
            );

    /// <summary>
    /// Defines a pass-through segment that accepts all data without filtering by start bytes.
    /// </summary>
    /// <returns>A segment build definition that can be further configured.</returns>
    public static ISegmentBuildDefinition PassThough() => new SegmentBuildDefinition([]);

    /// <summary>
    /// Configures the segment to end with a specific control character.
    /// </summary>
    /// <param name="builder">The segment build definition to configure.</param>
    /// <param name="end">The control character marking the end of the segment.</param>
    /// <returns>The configured segment build definition.</returns>
    public static ISegmentBuildDefinition AndEndsWith(this ISegmentBuildDefinition builder, ControlCharacters end) =>
        builder.AndEndsWith((byte)end);

    /// <summary>
    /// Configures the segment to end with a specific byte value.
    /// </summary>
    /// <param name="builder">The segment build definition to configure.</param>
    /// <param name="end">The byte value marking the end of the segment.</param>
    /// <returns>The configured segment build definition.</returns>
    /// <exception cref="NotSupportedException">Thrown when the builder type is not supported or when an end byte is specified with a fixed length.</exception>
    public static ISegmentBuildDefinition AndEndsWith(this ISegmentBuildDefinition builder, byte end)
    {
        if (builder is not SegmentBuildDefinition def) throw new NotSupportedException($"{builder.GetType()} is not supported");
        if (def.Length.HasValue) throw new NotSupportedException("May not set end byte if using length");
        def.EndsWith = end;
        return builder;
    }

    /// <summary>
    /// Configures the segment to have its length determined by reading an embedded length field at a specific position.
    /// </summary>
    /// <typeparam name="TOfType">The unmanaged type of the length field (e.g., int, ushort, uint).</typeparam>
    /// <param name="builder">The segment build definition to configure.</param>
    /// <param name="position">The position within the segment where the length field is located.</param>
    /// <param name="endianness">The byte order (endianness) of the length field.</param>
    /// <returns>The configured segment build definition.</returns>
    /// <exception cref="NotSupportedException">Thrown when the builder type is not supported or when the segment does not start with a fixed length.</exception>
    public static ISegmentBuildDefinition ExtendedWithLengthAt<TOfType>(this ISegmentBuildDefinition builder, long position, Endianness endianness)
        where TOfType : unmanaged
    {
        if (builder is not SegmentBuildDefinition def) throw new NotSupportedException($"{builder.GetType()} is not supported");
        if (!def.Length.HasValue) throw new NotSupportedException("Must start with fixed length");

        unsafe
        {
            def.ExtensionDefinition = new SegmentExtensionDefinition(type: typeof(TOfType), length: sizeof(TOfType), postion: position, endianness: endianness);
        }
        return builder;
    }

    /// <summary>
    /// Configures the segment to have a maximum length constraint.
    /// </summary>
    /// <param name="builder">The segment build definition to configure.</param>
    /// <param name="maxLength">The maximum length of the segment in bytes. Zero removes the constraint.</param>
    /// <returns>The configured segment build definition.</returns>
    /// <exception cref="NotSupportedException">Thrown when the builder type is not supported or when a maximum length is specified with a fixed length.</exception>
    public static ISegmentBuildDefinition WithMaxLength(this ISegmentBuildDefinition builder, long maxLength)
    {
        if (builder is not SegmentBuildDefinition def) throw new NotSupportedException($"{builder.GetType()} is not supported");
        if (def.Length.HasValue) throw new NotSupportedException("May not set end byte if using length");
        def.MaxLength = maxLength == 0 ? null : maxLength;
        return builder;
    }

    /// <summary>
    /// Configures the segment with specific processing options.
    /// </summary>
    /// <param name="builder">The segment build definition to configure.</param>
    /// <param name="options">The options controlling segmentation behavior.</param>
    /// <returns>The configured segment build definition.</returns>
    /// <exception cref="NotSupportedException">Thrown when the builder type is not supported.</exception>
    public static ISegmentBuildDefinition WithOptions(this ISegmentBuildDefinition builder, SegmentionOptions options)
    {
        if (builder is not SegmentBuildDefinition def) throw new NotSupportedException($"{builder.GetType()} is not supported");
        def.Options = options;
        return builder;
    }

    /// <summary>
    /// Configures the segment to have a fixed length.
    /// </summary>
    /// <param name="builder">The segment build definition to configure.</param>
    /// <param name="length">The fixed length of the segment in bytes.</param>
    /// <returns>The configured segment build definition.</returns>
    /// <exception cref="NotSupportedException">Thrown when the builder type is not supported, or when a fixed length is specified with an end byte or maximum length.</exception>
    public static ISegmentBuildDefinition AndIsLength(this ISegmentBuildDefinition builder, long length)
    {
        if (builder is not SegmentBuildDefinition def) throw new NotSupportedException($"{builder.GetType()} is not supported");
        if (def.EndsWith.HasValue) throw new NotSupportedException("May not set length if using Ends With");
        if (def.MaxLength.HasValue) throw new NotSupportedException("May not set length if using Ends With");
        def.Length = length;
        return builder;
    }

    /// <summary>
    /// Finalizes the segment definition and creates a segmenter with a callback to handle extracted segments.
    /// </summary>
    /// <param name="builder">The segment build definition to finalize.</param>
    /// <param name="onSegmentReceived">The callback to invoke when a segment is successfully extracted.</param>
    /// <returns>A configured segmenter ready to process byte streams.</returns>
    /// <exception cref="NotSupportedException">Thrown when the builder type or configuration is not supported.</exception>
    public static ISegmenter ThenDo(this ISegmentBuildDefinition builder, OnSegmentReceived onSegmentReceived) =>
        builder switch
        {
            SegmentBuildDefinition def => def.EndsWith.HasValue switch
            {
                true when def.StartsWith.Length >= 1 => new BetweenSegmenter(onSegmentReceived, def.StartsWith, def.EndsWith ?? 0, def.MaxLength, def.Options),
                false when def.StartsWith.Length == 0 => new PassThroughSegmenter(onSegmentReceived, def.Length ?? 0L, def.Options),
                false when def.Length.HasValue => new StartAndFixLengthSegmenter(onSegmentReceived, def.StartsWith, def.Length.Value, def.Options, def.ExtensionDefinition),
                _ => throw new NotSupportedException("Unable to Build Segmenter")
            },
            _ => throw new NotSupportedException($"{builder.GetType()} is not supported")
        };

    /// <summary>
    /// Finalizes the segment definition and creates a segmenter that decodes segments into strongly-typed messages.
    /// </summary>
    /// <typeparam name="TMessage">The type of message to decode from the segment.</typeparam>
    /// <param name="builder">The segment build definition to finalize.</param>
    /// <param name="decoder">The message decoder to use for converting segments to messages.</param>
    /// <param name="onMessageReceived">The callback to invoke when a message is successfully decoded.</param>
    /// <returns>A configured segmenter ready to process byte streams and decode messages.</returns>
    public static ISegmenter ThenAs<TMessage>(this ISegmentBuildDefinition builder, IMessageDecoder<TMessage> decoder, OnMessageReceived<TMessage> onMessageReceived) =>
        builder.ThenDo(on => onMessageReceived(decoder.Decode(on)));
}