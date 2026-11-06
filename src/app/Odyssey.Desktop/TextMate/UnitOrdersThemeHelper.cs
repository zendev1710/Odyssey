using Avalonia.Media;
using Avalonia.Styling;
using Odyssey.Settings;
using System;

namespace Odyssey.TextMate;

internal static class UnitOrdersThemeHelper
{
    public static ExtendedThemeName GetConfiguredTheme(ThemeVariant applicationTheme)
    {
        bool isDarkMode = applicationTheme == ThemeVariant.Dark;
        string settingName = isDarkMode
            ? GlobalSettings.UNIT_ORDERS_DARK_THEME
            : GlobalSettings.UNIT_ORDERS_LIGHT_THEME;
        int configuredTheme = GlobalSettings.Get<int>(settingName);
        return configuredTheme == GlobalSettings.DOCK_DEFAULT_TEXT_EDITOR_THEME
            ? (isDarkMode ? ExtendedThemeName.DarkPlus : ExtendedThemeName.LightPlus)
            : (ExtendedThemeName)configuredTheme;
    }

    public static Color GetConfirmationBackground(Color themeBackground, bool showUnconfirmedState, bool isDarkMode)
    {
        if (!showUnconfirmedState)
        {
            return themeBackground;
        }

        const double overlayOpacity = 0.10;
        byte overlay = isDarkMode ? byte.MaxValue : (byte)0;
        return Color.FromArgb(
            themeBackground.A,
            BlendChannel(themeBackground.R, overlay, overlayOpacity),
            BlendChannel(themeBackground.G, overlay, overlayOpacity),
            BlendChannel(themeBackground.B, overlay, overlayOpacity));
    }

    private static byte BlendChannel(byte original, byte overlay, double opacity) =>
        (byte)Math.Round(original * (1 - opacity) + overlay * opacity);
}
