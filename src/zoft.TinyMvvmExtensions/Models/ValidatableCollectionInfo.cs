using zoft.TinyMvvmExtensions.Validation;
using zoft.TinyMvvmExtensions.WeakSubscription;

namespace zoft.TinyMvvmExtensions.Models
{
    internal class ValidatableCollectionInfo
    {
        public IValidatable ValidatableObject { get; set; }
        public NotifyPropertyChangedEventSubscription PropertyChangedSubscription { get; set; }
    }
}
