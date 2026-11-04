using Avalonia;
using Odyssey.Core.Theme;

namespace Odyssey.Themes;

public interface IThemeModeManager
{
    void Switch();

    void SetMode(ApplicationThemeMode mode);
}
