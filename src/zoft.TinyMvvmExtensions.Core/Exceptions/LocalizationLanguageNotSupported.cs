using System;

namespace zoft.TinyMvvmExtensions.Core.Exceptions
{
    public class LocalizationLanguageNotSupported : Exception
    {
        public LocalizationLanguageNotSupported() : base()
        {
        }

        public LocalizationLanguageNotSupported(string message) : base(message)
        {
        }

        public LocalizationLanguageNotSupported(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
