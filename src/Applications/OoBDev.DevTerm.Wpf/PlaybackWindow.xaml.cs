using System;
using System.Windows;
using System.Windows.Threading;
using OoBDev.DevTerm.Wpf;

namespace DevTermApp;

/// <summary>Hosts a <see cref="PlaybackDialogViewModel"/> and ticks it while playing.</summary>
public partial class PlaybackWindow : Window
{
    /// <summary>Creates the window over <paramref name="playback"/>.</summary>
    /// <param name="playback">The window view model.</param>
    public PlaybackWindow(PlaybackDialogViewModel playback)
    {
        ArgumentNullException.ThrowIfNull(playback);
        InitializeComponent();
        DataContext = playback;
        var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
        timer.Tick += (_, _) => playback.Tick();
        timer.Start();
        Closed += (_, _) => timer.Stop();
    }
}
