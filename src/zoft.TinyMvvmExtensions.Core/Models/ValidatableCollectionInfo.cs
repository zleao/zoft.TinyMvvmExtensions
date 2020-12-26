using zoft.TinyMvvmExtensions.Core.Validation;
using zoft.TinyMvvmExtensions.Core.WeakSubscription;

namespace zoft.TinyMvvmExtensions.Core.Models
{
    internal class ValidatableCollectionInfo
    {
        public IValidatable ValidatableObject { get; set; }
        public NotifyPropertyChangedEventSubscription PropertyChangedSubscription { get; set; }
    }
}
