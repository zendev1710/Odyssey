using System;
using Avalonia;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;
using Odyssey.Core.Theme;
using Odyssey.Settings;

namespace Odyssey.Themes;

public class ModeThemeManager : IThemeModeManager
{
    /// <summary>
    /// Switch from the current theme mode (light or dark) to its opposite and persist the setting.
    /// </summary>
    public void Switch()
    {
        Application? application = Application.Current;
        if (application is null)
        {
            return;
        }

        ApplicationThemeMode currentMode = (ApplicationThemeMode)GlobalSettings.Get<int>(GlobalSettings.THEME_MODE);
        bool isDarkMode = currentMode switch
        {
            ApplicationThemeMode.Default => application.ActualThemeVariant == ThemeVariant.Dark,
            ApplicationThemeMode.Light => false,
            ApplicationThemeMode.Dark => true,
            _ => throw new ArgumentOutOfRangeException(nameof(currentMode), currentMode, "Unknown application theme mode.")
        };

        ApplicationThemeMode newMode = isDarkMode ? ApplicationThemeMode.Light : ApplicationThemeMode.Dark;
        GlobalSettings settings = GlobalSettings.Shared;
        settings.ThemeMode = (int)newMode;
        SetMode(newMode);
        settings.Save();
    }

    public void SetMode(ApplicationThemeMode mode)
    {
        if (Application.Current is null)
        {
            return;
        }

        Application.Current.RequestedThemeVariant = mode switch
        {
            ApplicationThemeMode.Default => ThemeVariant.Default,
            ApplicationThemeMode.Light => ThemeVariant.Light,
            ApplicationThemeMode.Dark => ThemeVariant.Dark,
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, "Unknown application theme mode.")
        };
    }
}
