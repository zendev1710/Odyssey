using System.Diagnostics;

namespace Odyssey.Models.Data
{
    public enum GameLanguage
    {
        EN,
        DE,
        UNKNOWN,
    }

    /// <summary>
    /// Extension methods for LocaleType.
    /// </summary>
    public static class GameLanguageMethods
    {
        public static string ToString(this GameLanguage l)
        {
            if (l == GameLanguage.EN)
            {
                return Localization.Language.en.ToString();
            }
            if (l == GameLanguage.DE)
            {
                return Localization.Language.de.ToString();
            }
            return "unknown";
        }
        public static GameLanguage ToLocaleType(this string s)
        {
            string? localeValue = s?.ToLower();
            if (string.IsNullOrWhiteSpace(localeValue))
            {
                return GameLanguage.UNKNOWN;
            }
            return localeValue switch
            {
                "en" => GameLanguage.EN,
                "de" => GameLanguage.DE,
                _ => GameLanguage.UNKNOWN,
            };
        }
    }
    
}
