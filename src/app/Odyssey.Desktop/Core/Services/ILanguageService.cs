using Odyssey.Models.Localization;
using System.ComponentModel;

namespace Odyssey.Core.Services
{
    public interface ILanguageService: INotifyPropertyChanged
    {
        SupportedUILanguage CurrentUILanguage { get; }
        string CurrentUILanguageCode { get; }
        GameLanguage CurrentGameLanguage { get; set; }
        string CurrentGameLanguageCode { get; }
    }
}
