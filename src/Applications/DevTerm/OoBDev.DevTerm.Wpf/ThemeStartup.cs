using System;
using System.Collections.Generic;
using System.Windows.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Win32;
using OoBDev.DevTerm.Configuration;

namespace OoBDev.DevTerm.Wpf;

/// <summary>Start-up wiring for the theme: layered configuration, saved preference and system-theme changes.</summary>
public static class ThemeStartup
{
    /// <summary>
    /// Selects <c>--theme</c> / <c>DEVTERM_THEME</c>, else the saved preference, else the system theme, and
    /// makes later View &gt; Theme choices persist.
    /// </summary>
    /// <param name="args">The command-line arguments.</param>
    /// <param name="preferences">The preference store; the user's own file when null.</param>
    /// <returns>Problems found while loading user themes.</returns>
    public static IReadOnlyList<string> Initialize(string[] args, AppPreferencesStore? preferences = null)
    {
        var builder = new ConfigurationBuilder();
        DevTermConfiguration.Configure(builder, args, Environments.Production);
        return ActiveTheme.Initialize(builder.Build(), preferences: preferences ?? new AppPreferencesStore());
    }

    /// <summary>Re-resolves the "system" theme when Windows changes its colors.</summary>
    /// <param name="dispatcher">The UI dispatcher the refresh runs on.</param>
    /// <returns>The subscription; dispose it to unhook.</returns>
    public static IDisposable FollowSystemChanges(Dispatcher dispatcher)
    {
        void OnChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            if (e.Category is UserPreferenceCategory.General or UserPreferenceCategory.Color)
                dispatcher.BeginInvoke(ActiveTheme.RefreshSystem);
        }

        SystemEvents.UserPreferenceChanged += OnChanged;
        return new Unsubscribe(() => SystemEvents.UserPreferenceChanged -= OnChanged);
    }

    private sealed class Unsubscribe(Action action) : IDisposable
    {
        public void Dispose() => action();
    }
}
