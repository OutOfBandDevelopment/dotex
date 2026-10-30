using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.DevTerm.Configuration;
using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.DevTerm.Wpf.Tests;

/// <summary>The confirmations behind the profile actions of the connection editor window.</summary>
[TestCategory("Unit")]
[TestClass]
public sealed class ConnectionEditorPromptsTests
{
    private sealed class Recording(bool answer) : IConfirmPrompt
    {
        public List<string> Asked { get; } = [];

        public bool Confirm(string message)
        {
            Asked.Add(message);
            return answer;
        }
    }

    private static ConnectionEditorViewModel Editor() => new(new ConnectionProfileStore(global::System.IO.Path.Combine(global::System.IO.Path.GetTempPath(), "devterm-prompts-" + global::System.Guid.NewGuid().ToString("N"))), new CliOptions());

    [TestMethod]
    public void Wire_DeleteProfiles_AsksWithTheNames()
    {
        // Stage
        var prompt = new Recording(false);
        var editor = Editor();
        ConnectionEditorPrompts.Wire(editor, prompt);

        // Test
        var go = editor.ConfirmDeleteProfiles!(["a", "b"]);

        // Assert
        Assert.IsFalse(go);
        StringAssert.Contains(prompt.Asked[0], "Delete 2 profiles (a, b)");
    }

    [TestMethod]
    public void Wire_ReplaceAll_AsksWithTheCounts()
    {
        // Stage
        var prompt = new Recording(true);
        var editor = Editor();
        ConnectionEditorPrompts.Wire(editor, prompt);

        // Test
        var go = editor.ConfirmReplaceAllProfiles!(3, 5);

        // Assert
        Assert.IsTrue(go);
        StringAssert.Contains(prompt.Asked[0], "Delete all 3 saved profile(s) and import the 5");
    }
}
