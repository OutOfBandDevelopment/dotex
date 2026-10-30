using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    private string _sampleText = string.Empty;

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
        AddRuleCommand = new AddRoutingRuleCommand(this);
        RemoveRuleCommand = new RemoveRoutingRuleCommand(this);
        MoveRuleUp = new MoveRoutingRuleUpCommand(this);
        MoveRuleDown = new MoveRoutingRuleDownCommand(this);
        TestRulesCommand = new TestRoutingRulesCommand(this);
        RefreshCommand = new RefreshRoutingCommand(this);
        Reload();
    }

    /// <summary>The routing section being edited.</summary>
    public RoutingOptions Draft => _routing.Draft;

    /// <summary>The draft rules, in the order the router tries them.</summary>
    public ObservableCollection<RoutingRuleRow> Rules { get; } = [];

    /// <summary>The recent messages the router routed, newest last.</summary>
    public ObservableCollection<string> History { get; } = [];

    /// <summary>The device line to try against the rules.</summary>
    public string SampleText
    {
        get => _sampleText;
        set => SetProperty(ref _sampleText, value);
    }

    /// <summary>One line per rule saying what the sample line would do.</summary>
    public ObservableCollection<string> TestResults { get; } = [];

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

    /// <summary>Adds an empty rule.</summary>
    public ICommand AddRuleCommand { get; }

    /// <summary>Removes the rule row passed as the parameter.</summary>
    public ICommand RemoveRuleCommand { get; }

    /// <summary>Moves the rule row passed as the parameter one place earlier.</summary>
    public ICommand MoveRuleUp { get; }

    /// <summary>Moves the rule row passed as the parameter one place later.</summary>
    public ICommand MoveRuleDown { get; }

    /// <summary>Runs <see cref="SampleText"/> through the rules.</summary>
    public ICommand TestRulesCommand { get; }

    /// <summary>Reloads the hit counts and the history from the running router.</summary>
    public ICommand RefreshCommand { get; }

    /// <summary>Applies the draft to the session.</summary>
    public ICommand Apply { get; }

    /// <summary>Starts routing on the open session.</summary>
    public ICommand Start { get; }

    /// <summary>Stops routing.</summary>
    public ICommand Stop { get; }

    /// <summary>Saves the applied routing section into the saved profile.</summary>
    public ICommand SaveToProfile { get; }

    internal void AddRule()
    {
        _routing.AddRule();
        Reload();
    }

    internal void RemoveRule(RoutingRuleRow? row)
    {
        if (row is not null)
        {
            _routing.RemoveRule(Rules.IndexOf(row));
            Reload();
        }
    }

    internal void MoveRule(RoutingRuleRow? row, int delta)
    {
        if (row is not null)
        {
            _routing.MoveRule(Rules.IndexOf(row), delta);
            Reload();
        }
    }

    internal void TestRules()
    {
        TestResults.Clear();
        foreach (var line in _routing.Test(SampleText))
        {
            TestResults.Add(line);
        }
    }

    internal void Refresh()
    {
        for (var i = 0; i < Rules.Count; i++)
        {
            Rules[i].Hits = _routing.Hits(i);
        }

        History.Clear();
        foreach (var message in _routing.History)
        {
            History.Add($"{message.Timestamp:HH:mm:ss} {(message.Direction == RoutingDirection.DeviceToBroker ? "->" : "<-")} {message.Topic} {message.Payload}");
        }

        OnPropertyChanged(nameof(StatusText));
    }

    private void Reload()
    {
        Rules.Clear();
        foreach (var rule in _routing.Draft.Rules)
        {
            Rules.Add(new RoutingRuleRow(rule));
        }

        Refresh();
    }

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
        Refresh();
    }
}
