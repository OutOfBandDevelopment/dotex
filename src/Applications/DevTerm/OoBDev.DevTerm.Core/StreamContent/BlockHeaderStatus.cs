namespace OoBDev.DevTerm;

/// <summary>The result of <see cref="StreamContentSniffer.ParseBlockHeader"/>.</summary>
public enum BlockHeaderStatus
{
    /// <summary>The data doesn't start with a definite-length block header.</summary>
    NotABlock,

    /// <summary>The data starts like a block header but ends before the header does.</summary>
    Incomplete,

    /// <summary>A complete, valid block header.</summary>
    Complete,

    /// <summary>
    /// A valid indefinite-length block header (<c>#0</c>) - IEEE 488.2 doesn't give this block a
    /// declared length; its end has to be found some other way (the wrapped content's own
    /// structural end, or failing that, an idle timeout).
    /// </summary>
    Indefinite,
}
