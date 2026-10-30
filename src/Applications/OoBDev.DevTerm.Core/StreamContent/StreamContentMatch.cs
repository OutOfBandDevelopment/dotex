using System;

namespace OoBDev.DevTerm;

/// <summary>Where a <see cref="StreamContentSniffer.Find"/> match starts, and — for content wrapped in an IEEE 488.2 definite-length block (<c>#&lt;n&gt;&lt;length&gt;&lt;payload&gt;</c>) — how long the block header is and how many payload bytes follow it.</summary>
/// <param name="Kind">What the content was recognized as.</param>
/// <param name="Offset">Where the match begins in the scanned data (the <c>#</c> of a block header, otherwise the content's own first byte).</param>
/// <param name="HeaderLength">Bytes of block header to skip before the content itself starts; 0 when the content isn't block-wrapped.</param>
/// <param name="PayloadLength">The block's declared payload length, or <see langword="null"/> when the content isn't block-wrapped (its end has to be found some other way).</param>
public readonly record struct StreamContentMatch(StreamContentKind Kind, int Offset, int HeaderLength, long? PayloadLength);
