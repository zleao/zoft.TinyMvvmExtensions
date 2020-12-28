using System.ComponentModel;
using System.Globalization;

namespace zoft.TinyMvvmExtensions.Core.Localization
{
    public interface ILocalizationService : INotifyPropertyChanged
    {
        CultureInfo CurrentCulture { get; }
        void SetLanguage(string cultureName, bool throwIfFail = false);
        void SetLanguage(CultureInfo culture, bool throwIfFail = false);
        string GetTextForKey(string key);
        string this[string key] { get; }
    }
}
