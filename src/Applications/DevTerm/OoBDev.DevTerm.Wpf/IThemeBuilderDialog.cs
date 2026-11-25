namespace OoBDev.DevTerm.Wpf;

/// <summary>Shows the Build/Edit Theme windows.</summary>
public interface IThemeBuilderDialog
{
    /// <summary>Shows the seed picker modally.</summary>
    /// <param name="picker">The picker view model.</param>
    /// <returns>True when a seed was chosen.</returns>
    bool PickSeed(ThemeSeedPickerViewModel picker);

    /// <summary>Shows the builder modally.</summary>
    /// <param name="builder">The builder view model.</param>
    /// <returns>True when the theme was saved.</returns>
    bool Edit(ThemeBuilderDialogViewModel builder);
}
