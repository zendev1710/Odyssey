using Odyssey.Models.Localization;
using System;
using System.Collections.Generic;
using System.Text;

namespace Odyssey.Core.Parsing
{
    public static class GameLanguageParser
    {
        public static GameLanguage FromCR(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return GameLanguage.Unknown;

            return value.Trim().ToUpperInvariant() switch
            {
                "EN" => GameLanguage.English,
                "DE" => GameLanguage.German,
                _ => GameLanguage.Unknown
            };
        }
    }


}
