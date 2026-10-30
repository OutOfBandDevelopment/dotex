namespace OoBDev.DevTerm.Wpf.Tests;

internal sealed class FakeSessionLogDialog : ISessionLogDialog
{
    public string? PathToReturn { get; set; }

    public string? NoteToReturn { get; set; }

    public string? SuggestedPath { get; private set; }

    public string? PickLogPath(string suggestedPath)
    {
        SuggestedPath = suggestedPath;
        return PathToReturn;
    }

    public string? PromptNote() => NoteToReturn;
}
