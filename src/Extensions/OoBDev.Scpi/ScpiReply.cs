using System;
using System.Runtime.InteropServices;

namespace OoBDev.Scpi;

/// <summary>
/// One reply from the instrument: a text line with the line ending removed, or the data of a definite-length block.
/// </summary>
/// <param name="Text">The reply text; empty for a block.</param>
/// <param name="Block">The block data (for example a bitmap) with its <c>#&lt;n&gt;&lt;length&gt;</c> prefix removed; empty for a text reply.</param>
[StructLayout(LayoutKind.Auto)]
public readonly record struct ScpiReply(string Text, ReadOnlyMemory<byte> Block = default);
