using System;
using System.Collections.Specialized;
using System.ComponentModel;

namespace zoft.TinyMvvmExtensions.Core.WeakSubscription
{
    public static class WeakSubscriptionExtensions
    {
        public static NotifyPropertyChangedEventSubscription WeakSubscribe(this INotifyPropertyChanged source,
                                                                           EventHandler<PropertyChangedEventArgs> eventHandler)
        {
            return new NotifyPropertyChangedEventSubscription(source, eventHandler);
        }

        public static NotifyCollectionChangedEventSubscription WeakSubscribe(this INotifyCollectionChanged source,
                                                                             EventHandler<NotifyCollectionChangedEventArgs> eventHandler)
        {
            return new NotifyCollectionChangedEventSubscription(source, eventHandler);
        }

        public static WeakEventSubscription<TSource> WeakSubscribe<TSource>(this TSource source, string eventName, EventHandler eventHandler)
            where TSource : class
        {
            return new WeakEventSubscription<TSource>(source, eventName, eventHandler);
        }

        public static WeakEventSubscription<TSource, TEventArgs> WeakSubscribe<TSource, TEventArgs>(this TSource source, string eventName, EventHandler<TEventArgs> eventHandler)
            where TSource : class
        {
            return new WeakEventSubscription<TSource, TEventArgs>(source, eventName, eventHandler);
        }
    }
}
