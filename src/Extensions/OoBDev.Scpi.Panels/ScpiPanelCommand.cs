using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using OoBDev.UiDefinitions;
using OoBDev.UiDefinitions.Forms;

namespace OoBDev.Scpi.Panels;

/// <summary>Sends one profile command with the values a panel button packs into its parameter, and shows a query's reply.</summary>
/// <param name="profile">The instrument profile that builds the request.</param>
/// <param name="command">The command this button runs.</param>
/// <param name="execute">Sends the request; returns the reply text for a query, or null.</param>
public sealed class ScpiPanelCommand(ScpiInstrumentProfile profile, ScpiCommandDefinition command, Func<ScpiRequest, Task<string?>> execute) : ICommand
{
    private readonly ScpiInstrumentProfile _profile = profile ?? throw new ArgumentNullException(nameof(profile));
    private readonly ScpiCommandDefinition _command = command ?? throw new ArgumentNullException(nameof(command));
    private readonly Func<ScpiRequest, Task<string?>> _execute = execute ?? throw new ArgumentNullException(nameof(execute));

    /// <summary>Where the reply or the error is written; set once the panel's binding exists.</summary>
    internal FormBinding? Binding { get; set; }

    /// <inheritdoc />
    public event EventHandler? CanExecuteChanged
    {
        add { }
        remove { }
    }

    /// <inheritdoc />
    public bool CanExecute(object? parameter) => true;

    /// <inheritdoc />
    public async void Execute(object? parameter) => await ExecuteAsync(parameter as string).ConfigureAwait(false);

    /// <summary>Builds and sends the request; a failure is shown in the reply field instead of thrown.</summary>
    /// <param name="parameter">The parameter values joined by <c>,</c>, or null for a command without parameters.</param>
    /// <returns>A task that completes when the reply is shown.</returns>
    public async Task ExecuteAsync(string? parameter)
    {
        string? shown;
        try
        {
            IReadOnlyList<string> values = parameter is null ? [] : parameter.Split(',');
            shown = await _execute(_profile.Build(_command.Id, values)).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or FormatException or TimeoutException)
        {
            shown = "Error: " + ex.Message;
        }

        if (_command.IsQuery || shown is not null)
        {
            Binding?.SetText(new TextFieldControl { Id = ScpiPanelIds.Reply(_command.Id), Label = _command.Label }, shown ?? string.Empty);
        }
    }
}
