namespace Odyssey.Core.Theme;

/// <summary>
/// Specifies a UI theme that should be used for individual UIElement parts of an app UI.
/// TODO: remove this enum in favor of ApplicationThemeMode.
/// </summary>
public enum AvailableApplicationTheme
{
    /// <summary>
    /// Use the Application.RequestedTheme value for the element. This is the default.
    /// </summary>
    Default,

    /// <summary>
    /// Use the **Light** default theme.
    /// </summary>
    Light,

    /// <summary>
    /// Use the **Dark** default theme.
    /// </summary>
    Dark
}
