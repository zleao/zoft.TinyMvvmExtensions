using System;
using System.Globalization;
using Xamarin.Forms;

namespace zoft.TinyMvvmExtensions.Forms.Converters
{
    /// <summary>
    /// Converter the inverts a bool value
    /// </summary>
    /// <seealso cref="Xamarin.Forms.IValueConverter" />
    public class InverseBoolConverter : IValueConverter
    {
        /// <summary>
        /// Implement this method to convert <paramref name="value" /> to <paramref name="targetType" /> by using <paramref name="parameter" /> and <paramref name="culture" />.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="targetType">The type to which to convert the value.</param>
        /// <param name="parameter">A parameter to use during the conversion.</param>
        /// <param name="culture">The culture to use during the conversion.</param>
        /// <returns>
        /// To be added.
        /// </returns>
        /// <exception cref="System.InvalidOperationException">The target must be a boolean</exception>
        /// <remarks>
        /// To be added.
        /// </remarks>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is bool))
            {
                throw new InvalidOperationException("The target must be a boolean");
            }

            return !(bool)value;
        }

        /// <summary>
        /// Implement this method to convert <paramref name="value" /> back from <paramref name="targetType" /> by using <paramref name="parameter" /> and <paramref name="culture" />.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="targetType">The type to which to convert the value.</param>
        /// <param name="parameter">A parameter to use during the conversion.</param>
        /// <param name="culture">The culture to use during the conversion.</param>
        /// <returns>
        /// To be added.
        /// </returns>
        /// <remarks>
        /// To be added.
        /// </remarks>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}