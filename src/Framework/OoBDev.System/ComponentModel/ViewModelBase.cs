using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace OoBDev.System.ComponentModel;

/// <summary>
/// Base class for view models implementing INotifyPropertyChanged with dispatcher support.
/// </summary>
public abstract class ViewModelBase : INotifyPropertyChanged
{
    /*
    * WeakEventManager
	        * http://www.codeproject.com/Articles/786606/WeakEventManager-for-WinRT
	        * http://reedcopsey.com/2009/08/06/preventing-event-based-memory-leaks-weakeventmanager/
	        * https://msdn.microsoft.com/en-us/library/system.windows.weakeventmanager.aspx
	        * http://www.jonathanantoine.com/2011/09/19/wpf-4-5-part-2-improved-weakeventmanager/
	    */
    /// <summary>
    /// Initializes a new instance of the <see cref="ViewModelBase"/> class.
    /// </summary>
    /// <param name="dispatched">The action to use for dispatching property change notifications to the UI thread.</param>
    protected ViewModelBase(Action<Action> dispatched)
    {
        Dispatched = dispatched;
    }

    /// <summary>
    /// Gets the action used to dispatch work to the UI thread.
    /// </summary>
    public Action<Action> Dispatched { get; }

    /// <summary>
    /// Dispatches work to the UI thread if a dispatcher is configured, otherwise executes immediately.
    /// </summary>
    /// <param name="work">The work to dispatch.</param>
    public void DispatchWork(Action work)
    {
        if (Dispatched == null)
            work();
        else
            Dispatched(work);
    }

    /// <summary>
    /// Occurs when a property value changes. This event is raised on the UI thread via the dispatcher.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Raises the PropertyChanged event for the specified property.
    /// </summary>
    /// <param name="propertyName">The name of the property that changed (automatically set by CallerMemberName).</param>
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) => DispatchWork(() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)));
}
