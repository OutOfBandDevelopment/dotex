using OoBDev.DevTerm.Configuration;

namespace OoBDev.DevTerm.Wpf;

/// <summary>One colour role in the theme builder list.</summary>
/// <param name="Role">The role.</param>
/// <param name="Row">The text shown: an override marker, the role name and the colour.</param>
public sealed record ThemeRoleRow(ThemeRole Role, string Row);
