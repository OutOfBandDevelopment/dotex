using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using OoBDev.DevTerm.Configuration;
using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.DevTerm.Wpf;

/// <summary>The routing window: the editable broker section, the live status and the apply, start, stop and save commands.</summary>
public sealed class RoutingDialogViewModel : ViewModelBase
{
    private readonly RoutingViewModel _routing;
    private string _message = string.Empty;

    /// <summary>Creates the view model over <paramref name="routing"/>.</summary>
    /// <param name="routing">The routing editor of one session tab.</param>
    public RoutingDialogViewModel(RoutingViewModel routing)
    {
        ArgumentNullException.ThrowIfNull(routing);
        _routing = routing;
        Apply = new ApplyRoutingCommand(this);
        Start = new StartRoutingCommand(this);
        Stop = new StopRoutingCommand(this);
        SaveToProfile = new SaveRoutingToProfileCommand(this);
    }

    /// <summary>The routing section being edited.</summary>
    public RoutingOptions Draft => _routing.Draft;

    /// <summary>The protocols the broker link can use.</summary>
    public IReadOnlyList<string> Protocols => RoutingViewModel.Protocols;

    /// <summary>The routing state as text.</summary>
    public string StatusText => _routing.StatusText;

    /// <summary>The outcome of the last command, or empty.</summary>
    public string Message
    {
        get => _message;
        private set => SetProperty(ref _message, value);
    }

    /// <summary>Applies the draft to the session.</summary>
    public ICommand Apply { get; }

    /// <summary>Starts routing on the open session.</summary>
    public ICommand Start { get; }

    /// <summary>Stops routing.</summary>
    public ICommand Stop { get; }

    /// <summary>Saves the applied routing section into the saved profile.</summary>
    public ICommand SaveToProfile { get; }

    internal async Task ApplyAsync() => Report(await _routing.ApplyAsync());

    internal void StartRouting() => Report(_routing.Start());

    internal async Task StopAsync()
    {
        await _routing.StopAsync();
        Report(null);
    }

    internal void Save() => Report(_routing.SaveToProfile());

    private void Report(string? message)
    {
        Message = message ?? string.Empty;
        OnPropertyChanged(nameof(StatusText));
    }
}
