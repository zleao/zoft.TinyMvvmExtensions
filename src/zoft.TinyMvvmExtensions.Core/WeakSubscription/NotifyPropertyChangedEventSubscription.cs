using System;
using System.ComponentModel;
using System.Reflection;

namespace zoft.TinyMvvmExtensions.Core.WeakSubscription
{
    public class NotifyPropertyChangedEventSubscription
        : WeakEventSubscription<INotifyPropertyChanged, PropertyChangedEventArgs>
    {
        private static readonly EventInfo PropertyChangedEventInfo = typeof(INotifyPropertyChanged).GetEvent("PropertyChanged");

        // This code ensures the PropertyChanged event is not stripped by Xamarin linker
        public static void LinkerPleaseInclude(INotifyPropertyChanged iNotifyPropertyChanged)
        {
            iNotifyPropertyChanged.PropertyChanged += (sender, e) => { };
        }

        public NotifyPropertyChangedEventSubscription(INotifyPropertyChanged source,
                                                      EventHandler<PropertyChangedEventArgs> targetEventHandler)
            : base(source, PropertyChangedEventInfo, targetEventHandler)
        {
        }

        protected override Delegate CreateEventHandler()
        {
            return new PropertyChangedEventHandler(OnSourceEvent);
        }
    }
}