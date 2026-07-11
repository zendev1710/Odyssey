using System;
using System.Collections.Generic;
using System.Text;

namespace Odyssey.Models.Localization
{
    public static class GameLanguageExtensions
    {
        public static string ToIsoCode(this GameLanguage lang) =>
            lang switch
            {
                GameLanguage.English => "en",
                GameLanguage.German => "de",
                _ => "en" // fallback
            };
        public static SupportedUILanguage ToUILanguage(this GameLanguage lang) =>
            lang switch
            {
                GameLanguage.English => SupportedUILanguage.English,
                GameLanguage.German => SupportedUILanguage.German,
                _ => SupportedUILanguage.English
            };

    }
}
