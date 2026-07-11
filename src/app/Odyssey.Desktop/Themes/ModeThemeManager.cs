using System;
using Avalonia;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;
using Odyssey.Core.Theme;

namespace Odyssey.Themes;

public class ModeThemeManager : IThemeModeManager
{
    /// <summary>
    /// Switch from the current theme mode (light or dark) to its opposite.
    /// Note: DOES NOT WORK when current theme mode is ThemeVariant.Default.
    /// </summary>
    public void Switch()
    {
        if (Application.Current is null)
        {
            return;
        }

        ApplicationThemeMode newMode;
        ThemeVariant actualThemeVariant = Application.Current.ActualThemeVariant;
        if (actualThemeVariant == ThemeVariant.Default)
        {
            //TODO: know how to determine if the system theme mode (ThemeVariant.Default) is really dark or light, ang get the information
        }

        newMode = actualThemeVariant == ThemeVariant.Dark ? ApplicationThemeMode.Light : ApplicationThemeMode.Dark;
        SetMode(newMode);
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
            _ => Application.Current.RequestedThemeVariant
        };
    }
}
