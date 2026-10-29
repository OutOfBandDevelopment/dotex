namespace OoBDev.DevTerm.Wpf;

/// <summary>Builds one device panel for the shell; a device registers one of these to appear in the panel list.</summary>
public interface IPanelFactory
{
    /// <summary>The title shown in the panel list.</summary>
    string Title { get; }

    /// <summary>Creates a new panel.</summary>
    PanelViewModel Create();
}
