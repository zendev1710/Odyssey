using AvaloniaEdit.TextMate.Grammars;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using TextMateSharp.Internal.Types;
using TextMateSharp.Registry;
using TextMateSharp.Themes;


namespace Odyssea.TextMate;

public class EresseaOrdersRegistryOptions : TextMateSharp.Grammars.RegistryOptions /*IRegistryOptions*/
{
    // Path to your grammar and theme files
    private const string GrammarResourceName = "Odyssey.Resources.Grammars.eressea-orders.tmLanguage.json";
    private const string ThemeResourceName = "Odyssey.Resources.Themes.dark_plus.tmTheme.json";

    public EresseaOrdersRegistryOptions() : base(TextMateSharp.Grammars.ThemeName.DarkPlus)
    {
        base.LoadFromLocalDir("", true);
        string extension = ".txt";
        List<TextMateSharp.Grammars.Language> languages = GetAvailableLanguages();
        IEnumerable<TextMateSharp.Grammars.GrammarDefinition> grammarDefinitions = GetAvailableGrammarDefinitions();
        TextMateSharp.Grammars.Language language = GetLanguageByExtension(extension);
        string scope = GetScopeByExtension(extension);
        string scopeByLanguageId = GetScopeByLanguageId("eresseaorders");
    }

    public new IRawGrammar GetGrammar(string scopeName)
    {
        return base.GetGrammar(scopeName);
        /*
        //Here we are getting the language by the extension and right after that we are initializing grammar with this language.
        //And that's all 😀, you are ready to use AvaloniaEdit with syntax highlighting!
        //_textMateInstallation.SetGrammar(_registryOptions.GetScopeByLanguageId(_registryOptions.GetLanguageByExtension(".cs").Id));
        if (scopeName == "source.eresseaorders")
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream(GrammarResourceName);
            using var reader = new StreamReader(stream);
            var grammarJson = reader.ReadToEnd();
            return Grammar.FromJson(grammarJson);
        }
        return null;
        */
    }

    public new IRawTheme GetTheme(string scopeName)
    {
        return base.GetTheme(scopeName);
        /*
        // Always return the default theme for simplicity
        return GetDefaultTheme();
        */
    }

    public new ICollection<string> GetInjections(string scopeName)
    {
        // No injections for this grammar
        return null;
    }

    public new IRawTheme GetDefaultTheme()
    {
        /*
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(ThemeResourceName);
        using var reader = new StreamReader(stream);
        var themeJson = reader.ReadToEnd();
        return RawTheme.FromJson(themeJson);
        */
        return base.GetDefaultTheme();
    }
}