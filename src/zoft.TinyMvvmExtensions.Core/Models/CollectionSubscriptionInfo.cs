using System.Collections.Specialized;
using zoft.TinyMvvmExtensions.Core.WeakSubscription;

namespace zoft.TinyMvvmExtensions.Core.Models
{
    internal class CollectionSubscriptionInfo
    {
        public INotifyCollectionChanged Collection { get; set; }
        public NotifyCollectionChangedEventSubscription CollectionChangedSubscription { get; set; }
    }
}
