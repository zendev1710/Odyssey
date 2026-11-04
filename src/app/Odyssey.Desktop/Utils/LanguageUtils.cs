namespace Odyssey.Utils
{
    /// <summary>
    /// Utilities about languages.
    /// </summary>
    public static class LanguageUtils
    {
        public static void ConvertInGermans(ref string value)
        {
            switch (value)
            {
                case "Schoesslinge":
                case "Baeume":
                    value = ReplaceEnglishDigraphsWithGermanCharacterss(value);
                    break;
                default:
                    // Do nothing if the value does not match any of the cases
                    break;
            }
        }


        /// <summary>
        /// Convertts the English digraphs in the specified value with their German characters equivalents, and returns the result.
        /// For instance, for "Schoesslinge", "Schößlinge" is returned; for "Baeume", "Bäume" is returned.
        /// </summary>
        /// <param name="value">Value to convert. Should be a german term (not a french or english one).</param>
        /// <returns>The converted value</returns>
        public static string ReplaceEnglishDigraphsWithGermanCharacterss(string value)
        {
            // Replace the English digraphs with their German characters equivalents
            return value
                .Replace("ae", "ä")
                .Replace("oe", "ö")
                .Replace("ue", "ü")
                .Replace("ss", "ß")
                .Replace("Ae", "Ä")
                .Replace("Oe", "Ö")
                .Replace("Ue", "Ü");
        }

        /// <summary>
        /// Convert the German characters in the specified value with their English digraph equivalents, and returns the result.
        /// For instance, for "Schößlinge", "Schoesslinge" is returned; for "Bäume", "Baeume" is returned.
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <returns>The converted value</returns>
        public static string ReplaceGermanCharactersWithEnglishDigraphs(string value)
        {
            // Replace the German characters with their English digraph equivalents
            return value
                .Replace("ä", "ae")
                .Replace("ö", "oe")
                .Replace("ü", "ue")
                .Replace("ß", "ss")
                .Replace("Ä", "Ae")
                .Replace("Ö", "Oe")
                .Replace("Ü", "Ue");
        }
    }
}
