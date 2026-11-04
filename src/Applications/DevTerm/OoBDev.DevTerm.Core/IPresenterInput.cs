namespace OoBDev.DevTerm;

/// <summary>Optional companion to <see cref="IPresenter"/> for presenters that can turn typed input back into bytes to send.</summary>
public interface IPresenterInput
{
    /// <summary>Converts user-facing text into the bytes to write.</summary>
    byte[] Parse(string input);
}
