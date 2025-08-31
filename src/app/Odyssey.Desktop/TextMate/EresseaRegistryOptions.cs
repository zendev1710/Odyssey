using AvaloniaEdit.TextMate;
using TextMateSharp.Grammars;
using System.IO;
using System.Reflection;
using TextMateSharp.Registry;

namespace Odyssea.Desktop.TextMate;

public class EresseaRegistryOptions : IRegistryOptions
{
    public string GetScopeByLanguageId(int languageId)
    {
        // Use a custom language id, e.g., 1000 for Eressea Orders
        if (languageId == 1000)
            return "source.eressea-orders";
        return null;
    }

    public int? GetLanguageIdByScope(string scopeName)
    {
        if (scopeName == "source.eressea-orders")
            return 1000;
        return null;
    }

    public Grammar GetGrammar(string scopeName)
    {
        if (scopeName == "source.eressea-orders")
        {
            // Load the grammar from embedded resource or file
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream("Odyssey.Desktop.Resources.Grammars.eressea-orders.tmLanguage.json");
            using var reader = new StreamReader(stream);
            var grammarJson = reader.ReadToEnd();
            return Grammar.FromJson(grammarJson);
        }
        return null;
    }

    public string GetMimeTypeByLanguageId(int languageId)
    {
        if (languageId == 1000)
            return "text/eressea-orders";
        return null;
    }
}