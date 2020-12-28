using System;
using TinyMvvm.IoC;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using zoft.TinyMvvmExtensions.Core.Localization;

namespace zoft.TinyMvvmExtensions.Localization
{
    /// <summary>
    /// Markup extension that provides a tranaslation service
    /// Relies on the <see cref="ILocalizationService"/>
    /// </summary>
    [ContentProperty("Key")]
    public class TranslateExtension : IMarkupExtension<BindingBase>
    {
        private ILocalizationService _localizationService;
        /// <summary>
        /// Gets the localization service.
        /// </summary>
        /// <value>
        /// The localization service.
        /// </value>
        protected virtual ILocalizationService LocalizationService => _localizationService ??= Resolver.Resolve<ILocalizationService>();

        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the string format.
        /// </summary>
        /// <value>
        /// The string format.
        /// </value>
        public string StringFormat { get; set; }

        object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider) => ProvideValue(serviceProvider);

        /// <summary>
        /// When implemented in a derived class, returns an object that is provided as the value of the target property for this markup extension.
        /// </summary>
        /// <param name="serviceProvider">The service that provides the value.</param>
        /// <returns>
        /// The object that is provided as the value of the target property for this markup extension.
        /// </returns>
        /// <remarks>
        /// To be added.
        /// </remarks>
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
