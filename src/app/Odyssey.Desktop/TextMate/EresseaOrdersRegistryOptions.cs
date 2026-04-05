using Odyssey.TextMate;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using TextMateSharp.Internal.Grammars.Reader;
using TextMateSharp.Internal.Themes.Reader;
using TextMateSharp.Internal.Types;
using TextMateSharp.Registry;
using TextMateSharp.Themes;


namespace Odyssea.TextMate;

public class EresseaOrdersRegistryOptions : IRegistryOptions
{
    private readonly Assembly _assembly;
    private readonly string _grammarResourceName;

    public string GrammarScopeName { get; } = "source.eressea";
    public string DefaultThemeScopeName { get; } = "dark_plus";

    private readonly Dictionary<string, IRawGrammar> _grammarsCache = new();

    private readonly Dictionary<string, IRawTheme> _themesCache = new();

    private readonly Dictionary<string, string> _themeResources = new()
    {
        { "abyss-color-theme", "abyss-color-theme" },
        { "atom-one-dark-color-theme", "atom-one-dark-color-theme" },
        { "atom-one-light-color-theme", "atom-one-light-color-theme" },
        { "dark_plus", "dark_plus" },
        { "dark_vs", "dark_vs" },
        { "dimmed-monokai-color-theme", "dimmed-monokai-color-theme" },
        { "dracula-color-theme", "dracula-color-theme" },
        { "hc_black", "hc_black" },
        { "hc_light", "hc_light" },
        { "kimbie-dark-color-theme", "kimbie-dark-color-theme" },
        { "light_plus", "light_plus" },
        { "light_vs", "light_vs" },
        { "monokai-color-theme", "monokai-color-theme" },
        { "onedark-color-theme", "onedark-color-theme" },
        { "onedark_color_theme", "onedark_color_theme" },
        { "quietlight-color-theme", "quietlight-color-theme" },
        { "Red-color-theme", "Red-color-theme" },
        { "solarized-dark-color-theme", "solarized-dark-color-theme" },
        { "solarized-light-color-theme", "solarized-light-color-theme" },
        { "tomorrow-night-blue-color-theme", "tomorrow-night-blue-color-theme" },
        { "tomorrow_night", "tomorrow_night" },
        { "tomorrow", "tomorrow" },
        { "visual-studio-dark-theme", "visual-studio-dark-theme" },
        { "visual-studio-light-theme", "visual-studio-light-theme" },
    };

    public EresseaOrdersRegistryOptions(string grammarResourceName)
    {
        _assembly = Assembly.GetExecutingAssembly();
        _grammarResourceName = grammarResourceName;
    }

    public IRawGrammar GetGrammar(string scopeName)
    {
        //if (scopeName != GrammarScopeName)
        //   return null;

        // Si déjà chargé, renvoyer le cache
        if (_grammarsCache.TryGetValue(scopeName, out var cached))
            return cached;

        var stream = _assembly.GetManifestResourceStream(_grammarResourceName);
        if (stream == null)
            return null;

        var reader = new StreamReader(stream);
        var grammar = GrammarReader.ReadGrammarSync(reader);
        _grammarsCache[scopeName] = grammar;
        return grammar;
    }

    public IRawTheme GetTheme(string scopeName)
    {

        if (scopeName.StartsWith("./"))
        {
            // Remove ./ prefix if present
            scopeName = scopeName.Substring(2);
        }

        if (scopeName.EndsWith(".json"))
        {
            // Remove .json suffix if present
            scopeName = scopeName.Substring(0, scopeName.Length - 5);
        }

        // Si le scope ne correspond à aucun thème connu
        if (!_themeResources.TryGetValue(scopeName, out var resourceName))
        {
            // TODO:: afficher une erreur
            return null;
        }

        // Si déjà chargé, renvoyer le cache
        if (_themesCache.TryGetValue(scopeName, out var cached))
            return cached;

        // TODO: get namespace (Odyssey.Resources)
        resourceName = "Odyssey.Resources.Themes." + resourceName + ".json";

        // Charger depuis les ressources embarquées
        using var stream = _assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
            return null;

        using var reader = new StreamReader(stream);
        var theme = ThemeReader.ReadThemeSync(reader);

        _themesCache[scopeName] = theme;
        return theme;
    }

    public IRawTheme GetDefaultTheme() =>
        // TODO: choose default theme according light or dark mode
        GetTheme(DefaultThemeScopeName);

    public ICollection<string> GetInjections(string scopeName)
    {
        // No injections for this grammar
        return new List<string>(); ;
    }

    public IRawTheme LoadTheme(ExtendedThemeName name) => GetTheme(GetThemeFile(name));

    private static string GetThemeFile(ExtendedThemeName name) => name switch
    {
        ExtendedThemeName.Abbys => "abyss-color-theme",
        ExtendedThemeName.Dark => "dark_vs",
        ExtendedThemeName.DarkPlus => "dark_plus",
        ExtendedThemeName.DimmedMonokai => "dimmed-monokai-color-theme",
        ExtendedThemeName.KimbieDark => "kimbie-dark-color-theme",
        ExtendedThemeName.Light => "light_vs",
        ExtendedThemeName.LightPlus => "light_plus",
        ExtendedThemeName.Monokai => "monokai-color-theme",
        ExtendedThemeName.OneDark => "onedark-color-theme",
        ExtendedThemeName.QuietLight => "quietlight-color-theme",
        ExtendedThemeName.Red => "Red-color-theme",
        ExtendedThemeName.SolarizedDark => "solarized-dark-color-theme",
        ExtendedThemeName.SolarizedLight => "solarized-light-color-theme",
        ExtendedThemeName.TomorrowNightBlue => "tomorrow-night-blue-color-theme",
        ExtendedThemeName.HighContrastLight => "hc_light",
        ExtendedThemeName.HighContrastDark => "hc_black",
        ExtendedThemeName.Dracula => "dracula",
        ExtendedThemeName.AtomOneLight => "atom-one-light-color-theme",
        ExtendedThemeName.AtomOneDark => "atom-one-dark-color-theme",
        ExtendedThemeName.VisualStudioLight => "visual-studio-light-theme",
        ExtendedThemeName.VisualStudioDark => "visual-studio-dark-theme",
        ExtendedThemeName.OneDarkOther => "onedark_color_theme",
        ExtendedThemeName.Tomorrow => "tomorrow",
        ExtendedThemeName.TomorrowNight => "tomorrow_night",
        ExtendedThemeName.Magellan => "magellan",
        _ => "dark_plus",
    };
}
