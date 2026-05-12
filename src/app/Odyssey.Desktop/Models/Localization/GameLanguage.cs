using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Odyssey.Models.Localization
{
    public enum GameLanguage
    {
        [Description("Unknown")] Unknown = 0,
        [Description("English")] English = 1,
        [Description("German")] German = 2
    }
}
