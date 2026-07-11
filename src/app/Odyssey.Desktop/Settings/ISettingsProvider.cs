using System;

namespace Odyssey.Settings;

/// <summary>
/// Provides a set of methods to manage the application's settings.
/// Inspired from https://github.com/DevToys-app/DevToys.
/// </summary>
public interface ISettingsProvider
{
    /// <summary>
    /// Raised when a setting value has changed.
    /// </summary>
    event EventHandler<SettingChangedEventArgs>? SettingChanged;

    /// <summary>
    /// Gets the value of a defined setting.
    /// </summary>
    /// <typeparam name="T">The type of value that will be retrieved.</typeparam>
    /// <param name="settingName">The name of the targeted setting.</param>
    /// <returns>Return the value of the setting or its default value.</returns>
    T GetSetting<T>(string settingName);
}
