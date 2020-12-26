using System;
using System.Collections.Specialized;
using System.Reflection;

namespace zoft.TinyMvvmExtensions.Core.WeakSubscription
{
    public class NotifyCollectionChangedEventSubscription
        : WeakEventSubscription<INotifyCollectionChanged, NotifyCollectionChangedEventArgs>
    {
        private static readonly EventInfo EventInfo = typeof(INotifyCollectionChanged).GetEvent("CollectionChanged");

        // This code ensures the CollectionChanged event is not stripped by Xamarin linker
        public static void LinkerPleaseInclude(INotifyCollectionChanged iNotifyCollectionChanged)
        {
            iNotifyCollectionChanged.CollectionChanged += (sender, e) => { };
        }

        public NotifyCollectionChangedEventSubscription(INotifyCollectionChanged source,
                                                        EventHandler<NotifyCollectionChangedEventArgs> targetEventHandler)
            : base(source, EventInfo, targetEventHandler)
        {
        }

        protected override Delegate CreateEventHandler()
        {
            return new NotifyCollectionChangedEventHandler(OnSourceEvent);
        }
    }
}