namespace OoBDev.DevTerm.Configuration;

/// <summary>
/// What to append to a typed line before sending it, when the presenter can encode outgoing
/// text (<see cref="IPresenterInput"/>). Left as <see cref="None"/> by
/// default so numeric-base presenters (hex/decimal/octal/binary), where a typed line is meant to
/// be exact bytes, aren't given a stray extra byte — set this explicitly for text-mode sessions
/// where pressing Enter is expected to send CR/LF like a real terminal.
/// </summary>
public enum LineEnding
{
    None,
    Cr,
    Lf,
    CrLf,
}
