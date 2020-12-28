using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Resources;
using zoft.TinyMvvmExtensions.Core.Exceptions;
using zoft.TinyMvvmExtensions.Core.Extensions;

namespace zoft.TinyMvvmExtensions.Core.Localization
{
    public class LocalizationService : ILocalizationService
    {
        public event PropertyChangedEventHandler PropertyChanged;
        
        private readonly ResourceManager _resourceManager;
        
        private CultureInfo _currentCulture;
        public CultureInfo CurrentCulture => _currentCulture;

        public LocalizationService(string resourceFileNamespace, Assembly resourceAssembly)
            : this(new ResourceManager(resourceFileNamespace.ThrowIfNull(nameof(resourceFileNamespace)),
                                       resourceAssembly.ThrowIfNull(nameof(resourceAssembly))))
        {
        }

        public LocalizationService(string resourceFileNamespace, Assembly resourceAssembly, string languageCultureName)
            : this(new ResourceManager(resourceFileNamespace.ThrowIfNull(nameof(resourceFileNamespace)),
                                       resourceAssembly.ThrowIfNull(nameof(resourceAssembly))),
                   languageCultureName)
        {
        }

        public LocalizationService(string resourceFileNamespace, Assembly resourceAssembly, CultureInfo languageCultureInfo)
            : this(new ResourceManager(resourceFileNamespace.ThrowIfNull(nameof(resourceFileNamespace)),
                                       resourceAssembly.ThrowIfNull(nameof(resourceAssembly))),
                   languageCultureInfo)
        {
        }

        public LocalizationService(ResourceManager resourceManager)
            : this(resourceManager, CultureInfo.CurrentUICulture)
        {
        }

        public LocalizationService(ResourceManager resourceManager, string languageCultureName)
        {
            _resourceManager = resourceManager.ThrowIfNull(nameof(resourceManager));

            SetLanguage(languageCultureName.ThrowIfNull(nameof(languageCultureName)));
        }

        public LocalizationService(ResourceManager resourceManager, CultureInfo languageCultureInfo)
        {
            _resourceManager = resourceManager.ThrowIfNull(nameof(resourceManager));

            SetLanguage(languageCultureInfo.ThrowIfNull(nameof(languageCultureInfo)));
        }

        public string GetTextForKey(string key)
        {
            return _resourceManager.GetString(key, _currentCulture);
        }

        public void SetLanguage(string cultureName, bool throwIfFail = false)
        {
            CultureInfo newLanguageCultureInfo;
            try
            {
                newLanguageCultureInfo = CultureInfo.GetCultureInfo(cultureName);
                if (throwIfFail && newLanguageCultureInfo == null)
                {
                    throw new LocalizationLanguageNotSupported($"Language not supported ({cultureName})");
                }
            }
            catch (Exception ex)
            {
                if (throwIfFail)
                {
                    throw new LocalizationLanguageNotSupported($"Language not supported ({cultureName})", ex);
                }

                newLanguageCultureInfo = default;
            }

            SetLanguage(newLanguageCultureInfo);
        }

        public void SetLanguage(CultureInfo languageCultureInfo, bool throwIfFail = false)
        {
            _currentCulture = languageCultureInfo;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        }

        public string this[string key] => GetTextForKey(key);
    }
}
