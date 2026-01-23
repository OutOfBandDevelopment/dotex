namespace OoBDev.System.Archives.Zip;

/// <summary>
/// Specifies the compression method used in a ZIP archive entry.
/// </summary>
public enum CompressionMethodType : short
{
    /// <summary>
    /// No compression - the file is stored as-is (method 0).
    /// </summary>
    None = 0,

    /// <summary>
    /// The file is compressed using the Shrunk algorithm (method 1).
    /// </summary>
    Shrunk = 1,

    /// <summary>
    /// The file is reduced with compression factor 1 (method 2).
    /// </summary>
    Factor1 = 2,

    /// <summary>
    /// The file is reduced with compression factor 2 (method 3).
    /// </summary>
    Factor2 = 3,

    /// <summary>
    /// The file is reduced with compression factor 3 (method 4).
    /// </summary>
    Factor3 = 4,

    /// <summary>
    /// The file is reduced with compression factor 4 (method 5).
    /// </summary>
    Factor4 = 5,

    /// <summary>
    /// The file is compressed using the Implode algorithm (method 6).
    /// </summary>
    Imploded = 6,

    /// <summary>
    /// Reserved for tokenizing compression algorithm (method 7).
    /// </summary>
    Tokenized = 7,

    /// <summary>
    /// The file is compressed using the Deflate algorithm (method 8).
    /// </summary>
    Deflate = 8,

    /// <summary>
    /// Enhanced deflating using Deflate64 (method 9).
    /// </summary>
    Deflate64 = 9,

    /// <summary>
    /// PKWARE Data Compression Library Imploding - old IBM TERSE (method 10).
    /// </summary>
    IbmTerseOld = 10,

    /// <summary>
    /// Reserved by PKWARE (method 11).
    /// </summary>
    Reserved11 = 11,

    /// <summary>
    /// The file is compressed using BZIP2 algorithm (method 12).
    /// </summary>
    BZIP2 = 12,

    /// <summary>
    /// Reserved by PKWARE (method 13).
    /// </summary>
    Reserved13 = 13,

    /// <summary>
    /// The file is compressed using LZMA (EFS) (method 14).
    /// </summary>
    LZMA = 14,

    /// <summary>
    /// Reserved by PKWARE (method 15).
    /// </summary>
    Reserved15 = 15,

    /// <summary>
    /// Reserved by PKWARE (method 16).
    /// </summary>
    Reserved16 = 16,

    /// <summary>
    /// Reserved by PKWARE (method 17).
    /// </summary>
    Reserved17 = 17,

    /// <summary>
    /// The file is compressed using IBM TERSE - new version (method 18).
    /// </summary>
    IbmTerseNew = 18,

    /// <summary>
    /// IBM LZ77 z Architecture (PFS) (method 19).
    /// </summary>
    IbmLZ77z = 19,

    /// <summary>
    /// WavPack compressed data (method 97).
    /// </summary>
    WavPack = 97,

    /// <summary>
    /// PPMd version I, Revision 1 (method 98).
    /// </summary>
    PPMdv1r1 = 98
}
