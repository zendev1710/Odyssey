using Odyssey.TextMate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Odyssey.ViewModels
{
    public class ColorThemeViewModel
    {
        private ExtendedThemeName _themeName;

        public ExtendedThemeName ThemeName => _themeName;

        public string DisplayName => _themeName.ToString();
        public ColorThemeViewModel(ExtendedThemeName themeName)
        {
            _themeName = themeName;
        }
    }
}
