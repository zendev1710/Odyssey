using System.ComponentModel;

namespace Odyssey.Models.Localization
{
    public enum SupportedUILanguage
    {
        [Description("Unknown")] Unknown = 0,
        [Description("English")] English = 1,
        [Description("German")] German = 2,
        [Description("French")] French = 3,
    }
}