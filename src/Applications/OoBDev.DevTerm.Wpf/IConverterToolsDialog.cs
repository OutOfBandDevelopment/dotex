namespace OoBDev.DevTerm.Wpf;

/// <summary>Shows the converter tools window.</summary>
public interface IConverterToolsDialog
{
    /// <summary>Shows the window modally.</summary>
    /// <param name="tools">The window view model.</param>
    /// <returns>True when the owner accepted the tools.</returns>
    bool Edit(ConverterToolsDialogViewModel tools);
}
