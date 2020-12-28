using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using zoft.NotificationService;
using zoft.NotificationService.Core;
using zoft.NotificationService.Core.Async.Subscriptions;
using zoft.NotificationService.Messages;
using zoft.NotificationService.Messages.Base;
using zoft.NotificationService.Messages.OneWay;
using zoft.NotificationService.Messages.TwoWay;
using zoft.NotificationService.Messages.TwoWay.Result;
using zoft.TinyMvvmExtensions.Core.Extensions;
using zoft.TinyMvvmExtensions.Core.Localization;
using zoft.TinyMvvmExtensions.Core.ViewModels;
using zoft.TinyMvvmExtensions.Models;

namespace zoft.TinyMvvmExtensions.ViewModels
{
    public class ExtendedViewModel : CoreViewModel
    {
        #region Fields

        private volatile List<NotificationGenericMessage> _initialGenericMessages = new List<NotificationGenericMessage>();
        private volatile IList<SubscriptionToken> _messageTokens = new List<SubscriptionToken>();
        private volatile IList<LongRunningSubscriptionToken> _longRunningMessageTokens = new List<LongRunningSubscriptionToken>();

        #endregion

        #region Properties

        /// <summary>
        /// Plugin used for notifications propagation.
        /// </summary>
        protected INotificationService NotificationManager { get; }

        /// <summary>
        /// Gets this view model specific context.
        /// Can be used to subscribe/publish messages that are only intended for this context
        /// </summary>
        public string ViewModelContext => _viewModelContext ??= GetType().Name;
        private string _viewModelContext;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewModel" /> class.
        /// </summary>
        /// <param name="textSource">The text source.</param>
        /// <param name="textSourceCommon">The text source common.</param>
        /// <param name="jsonConverter">The json converter.</param>
        /// <param name="notificationManager">The notification manager.</param>
        /// <param name="logProvider">The log provider.</param>
        /// <exception cref="System.NullReferenceException">IMvxJsonConverter
        /// or
        /// INotificationService
        /// or
        /// IMvxLanguageBinder</exception>
        protected ExtendedViewModel(INotificationService notificationManager, ILocalizationService localizationService)
        {
            NotificationManager = notificationManager.ThrowIfNull(nameof(notificationManager));
            LocalizationService = localizationService.ThrowIfNull(nameof(localizationService));
        }

        #endregion

        #region Notification Management

        #region Subscription/Unsubscription

        /// <summary>
        /// Subscribes an one-way event.
        /// This subscription will at best be valid until the 'OnViewHidden' is called
        /// </summary>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <param name="asyncDeliveryAction">The asynchronous delivery action.</param>
        /// <param name="context">The context.</param>
        protected void SubscribeEvent<TMessage>(Func<TMessage, Task> asyncDeliveryAction, string context = AsyncSubscription.DefaultContext)
            where TMessage : INotificationMessage
        {
            var token = NotificationManager.Subscribe(asyncDeliveryAction, context);
            _messageTokens.Add(token);
        }

        /// <summary>
        /// Unsubscribes the message events.
        /// </summary>
        protected virtual void UnsubscribeMessageEvents()
        {
            foreach (var item in _messageTokens)
            {
                NotificationManager.Unsubscribe(item);
            }
            _messageTokens.Clear();
        }

        /// <summary>
        /// Subscribes the long running message events.
        /// This Method is called in the viewmodel base constructor
        /// </summary>
        protected virtual void SubscribeLongRunningMessageEvents() { }

        /// <summary>
        /// Subscribes an one-way long running event.
        /// This subscription will at best be valid until the destructor of this class is called
        /// </summary>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <param name="asyncDeliveryAction">The asynchronous delivery action.</param>
        /// <param name="asyncDeliveryActionName">Name of the asynchronous delivery action.</param>
        /// <param name="unsubscribeOnMessageArrival">if set to <c>true</c> [unsubscribe on message arrival].</param>
        /// <param name="context">The context.</param>
        protected void SubscribeLongRunningEvent<TMessage>(Func<TMessage, Task> asyncDeliveryAction, string asyncDeliveryActionName, bool unsubscribeOnMessageArrival = false, string context = AsyncSubscription.DefaultContext)
            where TMessage : NotificationOneWayMessage
        {
            var messageType = typeof(TMessage);
            Task CastedAsyncDeliveryAction(INotificationOneWayMessage msg) => asyncDeliveryAction((TMessage)msg);
            SubscribeLongRunningEvent(messageType, CastedAsyncDeliveryAction, asyncDeliveryActionName, unsubscribeOnMessageArrival, context);
        }

        /// <summary>
        /// Subscribes an one-way long running event.
        /// This subscription will at best be valid until the destructor of this class is called
        /// </summary>
        /// <param name="messageType">Type of the message.</param>
        /// <param name="asyncDeliveryAction">The asynchronous delivery action.</param>
        /// <param name="asyncDeliveryActionName">Name of the asynchronous delivery action.</param>
        /// <param name="unsubscribeOnMessageArrival">if set to <c>true</c> [unsubscribe on message arrival].</param>
        /// <param name="context">The context.</param>
        protected void SubscribeLongRunningEvent(Type messageType, Func<INotificationOneWayMessage, Task> asyncDeliveryAction, string asyncDeliveryActionName, bool unsubscribeOnMessageArrival = false, string context = AsyncSubscription.DefaultContext)
        {
            var equalLongSubscription = _longRunningMessageTokens?.FirstOrDefault(l => l.Token.MessageType == messageType);
            if (equalLongSubscription != null)
            {
                NotificationManager.Unsubscribe(equalLongSubscription.Token);
                _longRunningMessageTokens.Remove(equalLongSubscription);
            }

            var token = NotificationManager.Subscribe(messageType, OnLongRunningNotificationAsync, context);

            _longRunningMessageTokens.Add(new LongRunningSubscriptionToken(token, asyncDeliveryAction, asyncDeliveryActionName, unsubscribeOnMessageArrival));
        }

        private async Task OnLongRunningNotificationAsync(INotificationMessage msg)
        {
            var messageType = msg.GetType();

            var longSubscription = _longRunningMessageTokens?.FirstOrDefault(l => l.Token.MessageType == messageType);
            if (longSubscription != null)
            {
                if (longSubscription.AsyncDeliveryAction is Func<INotificationOneWayMessage, Task> asyncDeliveryAction)
                {
                    await asyncDeliveryAction.Invoke(msg as INotificationOneWayMessage).ConfigureAwait(false);
                }

                if (longSubscription.UnsubscribeOnArrival)
                {
                    NotificationManager.Unsubscribe(longSubscription.Token);
                    _longRunningMessageTokens.Remove(longSubscription);
                }
            }
        }

        /// <summary>
        /// Unsubscribes the long running message events.
        /// </summary>
        protected virtual void UnsubscribeLongRunningMessageEvents()
        {
            foreach (var item in _longRunningMessageTokens)
            {
                NotificationManager.Unsubscribe(item.Token);
            }
            _longRunningMessageTokens.Clear();
        }

        #endregion

        #region Publish

        /// <summary>
        /// Publishes an one-way under construction message.
        /// The assumes that the text resource 'Message_Info_UnderConstruction' is correctly defined for the current language
        /// </summary>
        protected virtual Task PublishUnderConstructionNotificationAsync()
        {
            return NotificationManager.PublishInfoNotificationAsync(LocalizationService.GetTextForKey("Message_Info_UnderConstruction"));
        }

        /// <summary>
        /// Publishes a two way input dialog notification. Used to get a string input from the user
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        protected virtual Task<NotificationInputDialogResult> PublishInputDialogNotificationAsync(string message)
        {
            return NotificationManager.PublishAsync<NotificationInputDialogResult>(new NotificationInputDialogMessage(this, message));
        }

        #endregion

        #endregion

        #region TextSource

        public ILocalizationService LocalizationService { get; }

        #endregion

        #region ViewModel Lifecycle

        public override Task Initialize()
        {
            SubscribeLongRunningMessageEvents();

            return base.Initialize();
        }

        public override async Task OnAppearing()
        {
            SubscribeLongRunningMessageEvents();

            await base.OnAppearing();

            await DoWorkAsync(OnViewShownAsync, isSilent: true).ConfigureAwait(false);
        }

        protected virtual async Task OnViewShownAsync()
        {
            if (_initialGenericMessages.Count > 0)
            {
                foreach (var message in _initialGenericMessages)
                {
                    await NotificationManager.PublishAsync(message).ConfigureAwait(false);
                }
                _initialGenericMessages.Clear();
            }

            if (NotificationManager != null)
            {
                await NotificationManager.PublishPendingNotificationsAsync(this, ViewModelContext).ConfigureAwait(false);
                await NotificationManager.PublishPendingNotificationsAsync(this).ConfigureAwait(false);
            }
        }

        public override Task OnDisappearing()
        {
            UnsubscribeMessageEvents();

            UnsubscribeLongRunningMessageEvents();

            return base.OnDisappearing();
        }

        #endregion

        #region Generic Methods

        /// <summary>
        /// Adds a new message to the initial messages buffer.
        /// </summary>
        /// <param name="newMessage">The new message.</param>
        protected void AddInitialGenericMessage(NotificationGenericMessage newMessage)
        {
            if (!_initialGenericMessages.Contains(newMessage))
                _initialGenericMessages.Add(newMessage);
        }

        /// <summary>
        /// Adds a new info message to the initial messages buffer.
        /// </summary>
        /// <param name="newMessage">The new message.</param>
        protected void AddInitialGenericInfoMessage(string newMessage)
        {
            if (!newMessage.IsNullOrEmpty())
                AddInitialGenericMessage(new NotificationGenericMessage(this, newMessage, NotificationModeEnum.Default, NotificationSeverityEnum.Info));
        }

        #endregion
    }
}
