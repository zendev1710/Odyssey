using TextMateSharp.Grammars;

namespace Odyssey.ViewModels;

public class TextEditorThemeViewModel
{
    private ThemeName _themeName;

    public ThemeName ThemeName => _themeName;

    public string DisplayName => _themeName.ToString();
    public TextEditorThemeViewModel(ThemeName themeName)
    {
        _themeName = themeName;
    }
}