using ClassicForms.Models;
using System.Collections.Generic;
using System.Globalization;
using zoft.NotificationService;
using zoft.TinyMvvmExtensions.Core.Attributes;
using zoft.TinyMvvmExtensions.Core.Localization;
using zoft.TinyMvvmExtensions.ViewModels;

namespace ClassicForms.ViewModels
{
    public class LocalizationViewModel : ExtendedViewModel
    {
        public List<SupportedLanguage> SupportedLanguages { get; }
        public SupportedLanguage SelectedLanguage
        {
            get => _selectedLanguage;
            set => Set(ref _selectedLanguage, value);
        }
        private SupportedLanguage _selectedLanguage;

        public LocalizationViewModel(INotificationService notificationManager, ILocalizationService localizationService)
            : base(notificationManager, localizationService)
        {
            SupportedLanguages = new List<SupportedLanguage>
            {
                new SupportedLanguage("English", CultureInfo.InvariantCulture),
                new SupportedLanguage("Português (Portugal)", CultureInfo.GetCultureInfo("pt-PT")),
            };

            SelectedLanguage = SupportedLanguages.Find(l => l.Culture == LocalizationService.CurrentCulture) ?? SupportedLanguages[0];
        }

#pragma warning disable IDE0051 // Remove unused private members
        [DependsOn(nameof(SelectedLanguage))]
        private void SetLanguage()

        {
            if (SelectedLanguage != null)
            {
                LocalizationService.SetLanguage(SelectedLanguage.Culture);
            }
        }
#pragma warning restore IDE0051 // Remove unused private members
    }
}
