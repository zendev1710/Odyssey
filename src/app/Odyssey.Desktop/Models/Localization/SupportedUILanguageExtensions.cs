using System;
using System.Collections.Generic;
using System.Text;

namespace Odyssey.Models.Localization
{
    public static class SupportedUILanguageExtensions
    {
        public static string ToIsoCode(this SupportedUILanguage lang) =>
            lang switch
            {
                SupportedUILanguage.English => "en",
                SupportedUILanguage.German => "de",
                SupportedUILanguage.French => "fr",
                _ => "en"
            };

        public static SupportedUILanguage FromIsoCode(string? iso)
        {
            if (string.IsNullOrWhiteSpace(iso))
                return SupportedUILanguage.Unknown;

            return iso.Trim().ToLowerInvariant() switch
            {
                "en" => SupportedUILanguage.English,
                "de" => SupportedUILanguage.German,
                "fr" => SupportedUILanguage.French,
                _ => SupportedUILanguage.Unknown
            };
        }
    }
}
