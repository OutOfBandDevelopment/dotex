using System;
using System.Threading;
using Terminal.Gui.App;
using Terminal.Gui.ViewBase;

namespace OoBDev.UiDefinitions.Tui.Tests;

/// <summary>
/// Headless Terminal.Gui drivers for the renderer tests: a single-threaded run without a loop, and a run with a real
/// <c>Application.Run()</c> loop on a background thread that the test thread marshals into with <see cref="InvokeOnLoop{T}"/>.
/// </summary>
internal static class TuiTestRunner
{
    private static readonly TimeSpan _startTimeout = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan _stopTimeout = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan _invokeTimeout = TimeSpan.FromSeconds(5);
    private static IApplication? _currentApp;

    /// <summary>Init, run <paramref name="body"/>, dispose; no run loop.</summary>
    internal static void RunHeadlessApp(Action<IApplication> body)
    {
        var app = Application.Create().Init("dotnet");
        _currentApp = app;
        try
        {
            body(app);
        }
        finally
        {
            _currentApp = null;
            app.Dispose();
        }
    }

    /// <summary>Runs <paramref name="build"/> on a loop thread, then <paramref name="body"/> on the test thread.</summary>
    internal static void RunWithLoopApp(Action<IApplication>? beforeBuild, Func<IApplication, View> build, Action<IApplication, View> body)
    {
        View? root = null;
        IApplication? app = null;
        using var ready = new ManualResetEventSlim(false);
        Exception? threadException = null;

        var thread = new Thread(() =>
        {
            try
            {
                app = Application.Create().Init("dotnet");
                _currentApp = app;
                beforeBuild?.Invoke(app);
                root = build(app);
                app.Invoke(() => ready.Set());
                app.Run((IRunnable)root);
            }
            catch (Exception ex)
            {
                threadException = ex;
                ready.Set();
            }
        })
        {
            IsBackground = true,
        };
        thread.Start();

        if (!ready.Wait(_startTimeout))
        {
            throw new TimeoutException("The TUI run loop did not start in time.");
        }

        if (threadException is not null)
        {
            throw new InvalidOperationException("The TUI run loop failed to start.", threadException);
        }

        try
        {
            body(app!, root!);
        }
        finally
        {
            app!.Invoke(() => app.RequestStop((IRunnable)root!));
            thread.Join(_stopTimeout);
            _currentApp = null;
            root!.Dispose();
            app.Dispose();
        }
    }

    /// <summary>Runs <paramref name="func"/> on the loop thread and waits for it.</summary>
    internal static T InvokeOnLoop<T>(Func<T> func)
    {
        using var done = new ManualResetEventSlim(false);
        var result = default(T);
        Exception? exception = null;

        _currentApp!.Invoke(() =>
        {
            try
            {
                result = func();
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            finally
            {
                done.Set();
            }
        });

        if (!done.Wait(_invokeTimeout))
        {
            throw new TimeoutException("Application.Invoke did not run within the timeout.");
        }

        if (exception is not null)
        {
            throw new InvalidOperationException("Action invoked on the TUI loop thread threw.", exception);
        }

        return result!;
    }
}
