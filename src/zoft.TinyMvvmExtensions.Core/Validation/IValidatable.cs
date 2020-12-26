using System.Collections.Generic;
using System.ComponentModel;

namespace zoft.TinyMvvmExtensions.Core.Validation
{
    public interface IValidatable : INotifyPropertyChanged
    {
        List<string> Errors { get; set; }

        bool Validate();

        bool IsValid { get; set; }

        void RaisePropertyChanged();
    }
    public interface IValidatable<T> : IValidatable
    {
        List<IValidationRule<T>> Validations { get; }
    }
}
