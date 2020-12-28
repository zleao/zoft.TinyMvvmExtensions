using System.Globalization;

namespace ClassicForms.Models
{
    public class SupportedLanguage
    {
        public string DisplayName { get; }
        public CultureInfo Culture { get; }

        public SupportedLanguage(string displayName, CultureInfo culture)
        {
            DisplayName = displayName;
            Culture = culture;
        }

        public override string ToString()
        {
            return DisplayName ?? string.Empty;
        }
    }
}
