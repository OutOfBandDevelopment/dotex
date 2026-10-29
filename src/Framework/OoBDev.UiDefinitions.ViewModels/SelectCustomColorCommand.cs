using System;

namespace OoBDev.UiDefinitions.ViewModels;

/// <summary>Selects the option that stands for a picked colour: brings the last colour back, or opens the picker when none was picked yet.</summary>
/// <param name="pick">The colour picker button's command.</param>
public sealed class SelectCustomColorCommand(PickColorCommand pick) : CommandBase
{
    private readonly PickColorCommand _pick = pick ?? throw new ArgumentNullException(nameof(pick));

    /// <inheritdoc />
    public override void Execute(object? parameter)
    {
        if (!_pick.Reapply())
        {
            _pick.Execute(null);
        }
    }
}
