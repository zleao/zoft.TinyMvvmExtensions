using System.Collections.Specialized;
using zoft.TinyMvvmExtensions.WeakSubscription;

namespace zoft.TinyMvvmExtensions.Models
{
    internal class CollectionSubscriptionInfo
    {
        public INotifyCollectionChanged Collection { get; set; }
        public NotifyCollectionChangedEventSubscription CollectionChangedSubscription { get; set; }
    }
}
