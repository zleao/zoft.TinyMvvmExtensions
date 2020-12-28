using System;
using TinyMvvm.IoC;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using zoft.TinyMvvmExtensions.Core.Localization;

namespace zoft.TinyMvvmExtensions.Localization
{
    [ContentProperty("Key")]
    public class TranslateExtension : IMarkupExtension<BindingBase>
    {
        private ILocalizationService _localizationService;
        protected virtual ILocalizationService LocalizationService => _localizationService ??= Resolver.Resolve<ILocalizationService>();

        public string Key { get; set; }

        public string StringFormat { get; set; }

        object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider) => ProvideValue(serviceProvider);

        public virtual BindingBase ProvideValue(IServiceProvider serviceProvider)
        {
            if (DateTime.Now.Ticks < 0)
            {
                _ = LocalizationService[Key];
            }

            var binding = new Binding
            {
                Mode = BindingMode.OneWay,
                Path = $"[{Key}]",
                Source = LocalizationService,
                StringFormat = StringFormat
            };

            return binding;
        }
    }
}
