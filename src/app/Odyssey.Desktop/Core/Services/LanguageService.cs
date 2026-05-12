using CommunityToolkit.Mvvm.ComponentModel;
using Odyssey.Models.Localization;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Odyssey.Core.Services
{
    public class LanguageService : ObservableObject, ILanguageService
    {
        public SupportedUILanguage CurrentUILanguage { get; }
        public string CurrentUILanguageCode { get; }

        private GameLanguage _currentGameLanguage = GameLanguage.English;

        public GameLanguage CurrentGameLanguage
        {
            get => _currentGameLanguage;
            set => SetProperty(ref _currentGameLanguage, value);
        }

        public string CurrentGameLanguageCode =>
            CurrentGameLanguage == GameLanguage.German ? "de" : "en";

        public LanguageService()
        {
            var iso = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

            // Fallback UI → en
            var parsed = SupportedUILanguageExtensions.FromIsoCode(iso);

            CurrentUILanguage = parsed == SupportedUILanguage.Unknown
                ? SupportedUILanguage.English
                : parsed;

            // Code ISO final (toujours en, fr ou de)
            CurrentUILanguageCode = CurrentUILanguage.ToIsoCode();

            // Game language by default
            CurrentGameLanguage = CurrentUILanguage switch
            {
                SupportedUILanguage.German => GameLanguage.German,
                _ => GameLanguage.English
            };
        }
    }

}
