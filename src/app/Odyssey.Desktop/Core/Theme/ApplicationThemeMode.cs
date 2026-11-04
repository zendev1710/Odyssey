namespace Odyssey.Core.Theme;

/// <summary>
/// Specifies a UI theme mode (dark or light) that should be used for individual UIElement parts of an app UI.
/// </summary>
public enum ApplicationThemeMode
{
    /// <summary>
    /// Use the default configured system theme mode.
    /// </summary>
    Default = 0,

    /// <summary>
    /// Use the **Light** theme mode.
    /// </summary>
    Light = 1,

    /// <summary>
    /// Use the **Dark** theme mode.
    /// </summary>
    Dark = 2,
}
