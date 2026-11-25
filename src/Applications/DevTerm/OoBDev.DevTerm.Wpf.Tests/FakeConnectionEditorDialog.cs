using System;
using OoBDev.DevTerm.Configuration;

namespace OoBDev.DevTerm.Wpf.Tests;

internal sealed class FakeConnectionEditorDialog(Action<ConnectionEditorViewModel> interact) : IConnectionEditorDialog
{
    public CliOptions? Show(ConnectionEditorViewModel editor)
    {
        interact(editor);
        return editor.Result;
    }
}
