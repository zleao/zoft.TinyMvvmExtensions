using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TinyMvvm;
using TinyMvvm.Forms;
using TinyMvvm.IoC;
using Xamarin.Essentials;
using zoft.NotificationService;
using zoft.NotificationService.Core;
using zoft.NotificationService.Core.Async.Subscriptions;
using zoft.NotificationService.Messages;
using zoft.NotificationService.Messages.Base;
using zoft.NotificationService.Messages.OneWay;
using zoft.NotificationService.Messages.TwoWay.Question;
using zoft.NotificationService.Messages.TwoWay.Result;
using zoft.TinyMvvmExtensions.Core.Extensions;
using zoft.TinyMvvmExtensions.Core.WeakSubscription;
using zoft.TinyMvvmExtensions.ViewModels;

namespace zoft.TinyMvvmExtensions.Forms
{
    /// <summary>
    /// Base page with that matches the <see cref="ExtendedViewModel"/>, providing out-of-the-box functionality for Notifications and Localization
    /// </summary>
    /// <seealso cref="TinyMvvm.Forms.ViewBase" />
    public class ExtendedPage : ViewBase
    {
        #region Fields

        private readonly IList<SubscriptionToken> _messageTokens = new List<SubscriptionToken>();

        private volatile NotifyPropertyChangedEventSubscription _propertyChangedSubscription;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the notification manager.
        /// </summary>
        /// <value>
        /// The notification manager.
        /// </value>
        protected INotificationService NotificationManager => _notificationManager ??= Resolver.Resolve<INotificationService>();
        private INotificationService _notificationManager;

        #endregion

        #region Lifecycle Methods

        /// <inheritdoc/>
        protected override void OnAppearing()
        {
            SubscribeMessageEvents();

            base.OnAppearing();
        }

        /// <inheritdoc/>
        protected override void OnDisappearing()
        {
            UnsubscribeMessageEvents();

            base.OnDisappearing();
        }

        #endregion

        #region Notification Management

        /// <summary>
        /// Gets a value indicating whether subscribe generic messages.
        /// </summary>
        /// <value>
        /// <c>true</c> if subscribe generic messages; otherwise, <c>false</c>.
        /// </value>
        public virtual bool SubscribeGenericMessages => true;

        /// <summary>
        /// Gets a value indicating whether to subscribe generic question messages.
        /// </summary>
        /// <value>
        /// <c>true</c> if subscribe generic question messages; otherwise, <c>false</c>.
        /// </value>
        public virtual bool SubscribeGenericQuestionMessages => true;

        /// <summary>
        /// Gets a value indicating whether to subscribe question with custom answer messages.
        /// </summary>
        /// <value>
        /// <c>true</c> if subscribe question with custom answer messages; otherwise, <c>false</c>.
        /// </value>
        public virtual bool SubscribeQuestionWithCustomAnswerMessages => true;

        /// <summary>
        /// Gets a value indicating whether to subscribe terminate application message.
        /// </summary>
        /// <value>
        /// <c>true</c> if [subscribe terminate application message]; otherwise, <c>false</c>.
        /// </value>
        public virtual bool SubscribeTerminateApplicationMessage => true;

        /// <summary>
        /// Subscribes the event
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
        /// Subscribes two way events
        /// </summary>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="asyncDeliveryAction">The asynchronous delivery action.</param>
        /// <param name="context">The context.</param>
        protected void SubscribeEvent<TMessage, TResult>(Func<TMessage, Task<TResult>> asyncDeliveryAction, string context = AsyncSubscription.DefaultContext)
            where TMessage : INotificationTwoWayMessage
            where TResult : INotificationResult
        {
            var token = NotificationManager.Subscribe(asyncDeliveryAction, context);
            _messageTokens.Add(token);
        }

        /// <summary>
        /// Subscribes the message events.
        /// </summary>
        protected virtual void SubscribeMessageEvents()
        {
            if (SubscribeGenericMessages)
                SubscribeEvent<NotificationGenericMessage>(OnNotificationGenericMessageAsync);

            if (SubscribeGenericQuestionMessages)
                SubscribeEvent<NotificationGenericQuestionMessage, NotificationGenericQuestionResult>(OnNotificationGenericQuestionMessageAsync);

            if (SubscribeQuestionWithCustomAnswerMessages)
                SubscribeEvent<NotificationQuestionWithCustomAnswerMessage, NotificationQuestionCustomAnswerResult>(OnNotificationQuestionWithCustomAnswerMessageAsync);
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

            if (_propertyChangedSubscription != null)
            {
                _propertyChangedSubscription.Dispose();
                _propertyChangedSubscription = null;
            }
        }

        /// <summary>
        /// Called when notification error message.
        /// </summary>
        /// <param name="message">The obj.</param>
        protected virtual async Task OnNotificationGenericMessageAsync(NotificationGenericMessage message)
        {
            if (message != null)
            {
                switch (message.Mode)
                {
                    case NotificationModeEnum.MessageBox:
                        await ShowMessageBoxAsync(message.Message, message.Severity);
                        break;

                    case NotificationModeEnum.Default:
                    case NotificationModeEnum.Toast:
                        //TODO: Implement Toast service
                        await ShowMessageBoxAsync(message.Message, message.Severity);
                        break;
                    default:
                        throw new NotSupportedException($"Message mode not supported ({message.Mode})");
                }
            }
        }

        /// <summary>
        /// Called when notification generic question message asynchronous.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        protected virtual async Task<NotificationGenericQuestionResult> OnNotificationGenericQuestionMessageAsync(NotificationGenericQuestionMessage message)
        {
            var buttons = GetButtonsName(message.PossibleAnswers);

            var result = await ShowGenericQuestionDialogAsync(string.Empty, message.Question, buttons[0], buttons[1]);

            return new NotificationGenericQuestionResult(ConvertBool2NotificationTwoWayAnswersEnum(result, message.PossibleAnswers));
        }

        /// <summary>
        /// Called when notification question width custom answer asynchronous.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        protected virtual async Task<NotificationQuestionCustomAnswerResult> OnNotificationQuestionWithCustomAnswerMessageAsync(NotificationQuestionWithCustomAnswerMessage message)
        {
            var selectedIndex = await Task.Run(() => ShowSimpleSelectionDialogAsync(message.Question, message.PossibleAnswers));
            if (selectedIndex < 0 || selectedIndex >= message.PossibleAnswers?.Count())
                return new NotificationQuestionCustomAnswerResult(null, selectedIndex);

            return new NotificationQuestionCustomAnswerResult(message.PossibleAnswers[selectedIndex], selectedIndex);
        }

        #endregion

        #region Generic Methods

        /// <summary>
        /// Converts answer of a TwoWay notification message, based on the bool value returned.
        /// </summary>
        /// <param name="value">if set to <c>true</c> [value].</param>
        /// <param name="possibleAnswers">The possible answers.</param>
        /// <returns></returns>
        protected NotificationTwoWayAnswersEnum ConvertBool2NotificationTwoWayAnswersEnum(bool value, NotificationTwoWayAnswersGroupEnum possibleAnswers)
        {
            return possibleAnswers switch
            {
                NotificationTwoWayAnswersGroupEnum.Ok => value ? NotificationTwoWayAnswersEnum.Ok : NotificationTwoWayAnswersEnum.Unknown,
                NotificationTwoWayAnswersGroupEnum.YesNo => value ? NotificationTwoWayAnswersEnum.Yes : NotificationTwoWayAnswersEnum.No,
                NotificationTwoWayAnswersGroupEnum.OkCancel => value ? NotificationTwoWayAnswersEnum.Ok : NotificationTwoWayAnswersEnum.Cancel,
                _ => NotificationTwoWayAnswersEnum.Unknown,
            };
        }

        /// <summary>
        /// Gets the name of the buttons to apply to a TwoWay message type.
        /// </summary>
        /// <param name="possibleAnswers">The possible answers.</param>
        /// <returns></returns>
        protected List<string> GetButtonsName(NotificationTwoWayAnswersGroupEnum possibleAnswers)
        {
            if (BindingContext is ExtendedViewModel myVm)
            {
                var posBtnName = myVm.LocalizationService.GetTextForKey("Common_Label_Ok");
                var negBtnName = myVm.LocalizationService.GetTextForKey("Common_Label_Cancel");
                if (possibleAnswers == NotificationTwoWayAnswersGroupEnum.YesNo)
                {
                    posBtnName = myVm.LocalizationService.GetTextForKey("Common_Label_Yes");
                    negBtnName = myVm.LocalizationService.GetTextForKey("Common_Label_No");
                }

                return new List<string>() { posBtnName, negBtnName };
            }

            return null;
        }

        /// <summary>
        /// Gets the title from severity level.
        /// </summary>
        /// <param name="severity">The severity level.</param>
        /// <returns></returns>
        protected virtual string GetTitleFromSeverity(NotificationSeverityEnum severity)
        {
            if (BindingContext is ExtendedViewModel myVm)
            {
                return severity switch
                {
                    NotificationSeverityEnum.Error => myVm.LocalizationService.GetTextForKey("Common_Label_Dialog_Title_Error"),
                    NotificationSeverityEnum.Success => myVm.LocalizationService.GetTextForKey("Common_Label_Dialog_Title_Success"),
                    NotificationSeverityEnum.Info => myVm.LocalizationService.GetTextForKey("Common_Label_Dialog_Title_Information"),
                    NotificationSeverityEnum.Warning => myVm.LocalizationService.GetTextForKey("Common_Label_Dialog_Title_Warning"),
                    _ => myVm.LocalizationService.GetTextForKey("Common_Label_Dialog_Title_Message"),
                };
            }

            return null;
        }

        /// <summary>
        /// Shows a question dialog.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="message">The message.</param>
        /// <param name="positiveButtonName">Name of the positive button.</param>
        /// <param name="negativeButtonName">Name of the negative button.</param>
        /// <returns></returns>
        /// <exception cref="System.NotSupportedException">ShowBlockingQuestionMessage must be called in a background thread!</exception>
        protected virtual async Task<bool> ShowGenericQuestionDialogAsync(string title, string message, string positiveButtonName, string negativeButtonName)
        {
            return await MainThread.InvokeOnMainThreadAsync(async () => await DisplayAlert(title, message, positiveButtonName, negativeButtonName)).ConfigureAwait(false);
        }

        /// <summary>
        /// Shows a simple selection dialog and waits for user interaction
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="options">The options.</param>
        /// <param name="indexIfCancel">The index if cancel.</param>
        protected virtual async Task<int> ShowSimpleSelectionDialogAsync(string title, IList<string> options, int indexIfCancel = -1)
        {
            var answer = await MainThread.InvokeOnMainThreadAsync(async () =>
                await DisplayActionSheet(title,
                                         (BindingContext as ExtendedViewModel)?.LocalizationService.GetTextForKey("Common_Label_Cancel"),
                                         null,
                                         options.ToArray())).ConfigureAwait(false);
            var selectedIndex = indexIfCancel;

            if (!answer.IsNullOrEmpty())
            {
                selectedIndex = options.IndexOf(answer);
            }

            return selectedIndex;
        }

        /// <summary>
        /// Shows the message box.
        /// </summary>
        /// <param name="message">The message to show.</param>
        /// <param name="severity">The severity of the message.</param>
        /// <returns></returns>
        /// <exception cref="System.NotSupportedException">ShowBlockingQuestionMessage must be called in a background thread!</exception>
        protected virtual async Task ShowMessageBoxAsync(string message, NotificationSeverityEnum severity)
        {
            var title = GetTitleFromSeverity(severity);
            var buttonOk = (BindingContext as ExtendedViewModel)?.LocalizationService.GetTextForKey("Common_Label_Ok");

            await MainThread.InvokeOnMainThreadAsync(async () => await DisplayAlert(title, message, buttonOk)).ConfigureAwait(false);
        }

        #endregion
    }

    /// <summary>
    /// Base page with that matches the <see cref="ExtendedViewModel"/>, providing out-of-the-box functionality for Notifications and Localization
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="TinyMvvm.Forms.ViewBase" />
    public class ExtendedPage<T> : ViewBase<T> where T : IViewModelBase
    {
        #region Fields

        private readonly IList<SubscriptionToken> _messageTokens = new List<SubscriptionToken>();

        private volatile NotifyPropertyChangedEventSubscription _propertyChangedSubscription;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the notification manager.
        /// </summary>
        /// <value>
        /// The notification manager.
        /// </value>
        protected INotificationService NotificationManager => _notificationManager ??= Resolver.Resolve<INotificationService>();
        private INotificationService _notificationManager;

        #endregion

        #region Lifecycle Methods

        /// <inheritdoc/>
        protected override void OnAppearing()
        {
            SubscribeMessageEvents();

            base.OnAppearing();
        }

        /// <inheritdoc/>
        protected override void OnDisappearing()
        {
            UnsubscribeMessageEvents();

            base.OnDisappearing();
        }

        #endregion

        #region Notification Management

        /// <summary>
        /// Gets a value indicating whether subscribe generic messages.
        /// </summary>
        /// <value>
        /// <c>true</c> if subscribe generic messages; otherwise, <c>false</c>.
        /// </value>
        public virtual bool SubscribeGenericMessages => true;

        /// <summary>
        /// Gets a value indicating whether to subscribe generic question messages.
        /// </summary>
        /// <value>
        /// <c>true</c> if subscribe generic question messages; otherwise, <c>false</c>.
        /// </value>
        public virtual bool SubscribeGenericQuestionMessages => true;

        /// <summary>
        /// Gets a value indicating whether to subscribe question with custom answer messages.
        /// </summary>
        /// <value>
        /// <c>true</c> if subscribe question with custom answer messages; otherwise, <c>false</c>.
        /// </value>
        public virtual bool SubscribeQuestionWithCustomAnswerMessages => true;

        /// <summary>
        /// Gets a value indicating whether to subscribe terminate application message.
        /// </summary>
        /// <value>
        /// <c>true</c> if [subscribe terminate application message]; otherwise, <c>false</c>.
        /// </value>
        public virtual bool SubscribeTerminateApplicationMessage => true;

        /// <summary>
        /// Subscribes the event
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
        /// Subscribes two way events
        /// </summary>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="asyncDeliveryAction">The asynchronous delivery action.</param>
        /// <param name="context">The context.</param>
        protected void SubscribeEvent<TMessage, TResult>(Func<TMessage, Task<TResult>> asyncDeliveryAction, string context = AsyncSubscription.DefaultContext)
            where TMessage : INotificationTwoWayMessage
            where TResult : INotificationResult
        {
            var token = NotificationManager.Subscribe(asyncDeliveryAction, context);
            _messageTokens.Add(token);
        }

        /// <summary>
        /// Subscribes the message events.
        /// </summary>
        protected virtual void SubscribeMessageEvents()
        {
            if (SubscribeGenericMessages)
                SubscribeEvent<NotificationGenericMessage>(OnNotificationGenericMessageAsync);

            if (SubscribeGenericQuestionMessages)
                SubscribeEvent<NotificationGenericQuestionMessage, NotificationGenericQuestionResult>(OnNotificationGenericQuestionMessageAsync);

            if (SubscribeQuestionWithCustomAnswerMessages)
                SubscribeEvent<NotificationQuestionWithCustomAnswerMessage, NotificationQuestionCustomAnswerResult>(OnNotificationQuestionWithCustomAnswerMessageAsync);
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

            if (_propertyChangedSubscription != null)
            {
                _propertyChangedSubscription.Dispose();
                _propertyChangedSubscription = null;
            }
        }

        /// <summary>
        /// Called when notification error message.
        /// </summary>
        /// <param name="message">The obj.</param>
        protected virtual async Task OnNotificationGenericMessageAsync(NotificationGenericMessage message)
        {
            if (message != null)
            {
                switch (message.Mode)
                {
                    case NotificationModeEnum.MessageBox:
                        await ShowMessageBoxAsync(message.Message, message.Severity);
                        break;

                    case NotificationModeEnum.Default:
                    case NotificationModeEnum.Toast:
                        //TODO: Implement Toast service
                        await ShowMessageBoxAsync(message.Message, message.Severity);
                        break;
                    default:
                        throw new NotSupportedException($"Message mode not supported ({message.Mode})");
                }
            }
        }

        /// <summary>
        /// Called when notification generic question message asynchronous.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        protected virtual async Task<NotificationGenericQuestionResult> OnNotificationGenericQuestionMessageAsync(NotificationGenericQuestionMessage message)
        {
            var buttons = GetButtonsName(message.PossibleAnswers);

            var result = await ShowGenericQuestionDialogAsync(string.Empty, message.Question, buttons[0], buttons[1]);

            return new NotificationGenericQuestionResult(ConvertBool2NotificationTwoWayAnswersEnum(result, message.PossibleAnswers));
        }

        /// <summary>
        /// Called when notification question width custom answer asynchronous.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        protected virtual async Task<NotificationQuestionCustomAnswerResult> OnNotificationQuestionWithCustomAnswerMessageAsync(NotificationQuestionWithCustomAnswerMessage message)
        {
            var selectedIndex = await Task.Run(() => ShowSimpleSelectionDialogAsync(message.Question, message.PossibleAnswers));
            if (selectedIndex < 0 || selectedIndex >= message.PossibleAnswers?.Count())
                return new NotificationQuestionCustomAnswerResult(null, selectedIndex);

            return new NotificationQuestionCustomAnswerResult(message.PossibleAnswers[selectedIndex], selectedIndex);
        }

        #endregion

        #region Generic Methods

        /// <summary>
        /// Converts answer of a TwoWay notification message, based on the bool value returned.
        /// </summary>
        /// <param name="value">if set to <c>true</c> [value].</param>
        /// <param name="possibleAnswers">The possible answers.</param>
        /// <returns></returns>
        protected NotificationTwoWayAnswersEnum ConvertBool2NotificationTwoWayAnswersEnum(bool value, NotificationTwoWayAnswersGroupEnum possibleAnswers)
        {
            return possibleAnswers switch
            {
                NotificationTwoWayAnswersGroupEnum.Ok => value ? NotificationTwoWayAnswersEnum.Ok : NotificationTwoWayAnswersEnum.Unknown,
                NotificationTwoWayAnswersGroupEnum.YesNo => value ? NotificationTwoWayAnswersEnum.Yes : NotificationTwoWayAnswersEnum.No,
                NotificationTwoWayAnswersGroupEnum.OkCancel => value ? NotificationTwoWayAnswersEnum.Ok : NotificationTwoWayAnswersEnum.Cancel,
                _ => NotificationTwoWayAnswersEnum.Unknown,
            };
        }

        /// <summary>
        /// Gets the name of the buttons to apply to a TwoWay message type.
        /// </summary>
        /// <param name="possibleAnswers">The possible answers.</param>
        /// <returns></returns>
        protected List<string> GetButtonsName(NotificationTwoWayAnswersGroupEnum possibleAnswers)
        {
            if (ViewModel is ExtendedViewModel myVm)
            {
                var posBtnName = myVm.LocalizationService.GetTextForKey("Common_Label_Ok");
                var negBtnName = myVm.LocalizationService.GetTextForKey("Common_Label_Cancel");
                if (possibleAnswers == NotificationTwoWayAnswersGroupEnum.YesNo)
                {
                    posBtnName = myVm.LocalizationService.GetTextForKey("Common_Label_Yes");
                    negBtnName = myVm.LocalizationService.GetTextForKey("Common_Label_No");
                }

                return new List<string>() { posBtnName, negBtnName };
            }

            return null;
        }

        /// <summary>
        /// Gets the title from severity level.
        /// </summary>
        /// <param name="severity">The severity level.</param>
        /// <returns></returns>
        protected virtual string GetTitleFromSeverity(NotificationSeverityEnum severity)
        {
            if (ViewModel is ExtendedViewModel myVm)
            {
                return severity switch
                {
                    NotificationSeverityEnum.Error => myVm.LocalizationService.GetTextForKey("Common_Label_Dialog_Title_Error"),
                    NotificationSeverityEnum.Success => myVm.LocalizationService.GetTextForKey("Common_Label_Dialog_Title_Success"),
                    NotificationSeverityEnum.Info => myVm.LocalizationService.GetTextForKey("Common_Label_Dialog_Title_Information"),
                    NotificationSeverityEnum.Warning => myVm.LocalizationService.GetTextForKey("Common_Label_Dialog_Title_Warning"),
                    _ => myVm.LocalizationService.GetTextForKey("Common_Label_Dialog_Title_Message"),
                };
            }

            return null;
        }

        /// <summary>
        /// Shows a question dialog.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="message">The message.</param>
        /// <param name="positiveButtonName">Name of the positive button.</param>
        /// <param name="negativeButtonName">Name of the negative button.</param>
        /// <returns></returns>
        /// <exception cref="System.NotSupportedException">ShowBlockingQuestionMessage must be called in a background thread!</exception>
        protected virtual async Task<bool> ShowGenericQuestionDialogAsync(string title, string message, string positiveButtonName, string negativeButtonName)
        {
            return await MainThread.InvokeOnMainThreadAsync(async () => await DisplayAlert(title, message, positiveButtonName, negativeButtonName)).ConfigureAwait(false);
        }

        /// <summary>
        /// Shows a simple selection dialog and waits for user interaction
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="options">The options.</param>
        /// <param name="indexIfCancel">The index if cancel.</param>
        protected virtual async Task<int> ShowSimpleSelectionDialogAsync(string title, IList<string> options, int indexIfCancel = -1)
        {
            var answer = await MainThread.InvokeOnMainThreadAsync(async () =>
                await DisplayActionSheet(title,
                                         (ViewModel as ExtendedViewModel)?.LocalizationService.GetTextForKey("Common_Label_Cancel"),
                                         null,
                                         options.ToArray())).ConfigureAwait(false);
            var selectedIndex = indexIfCancel;

            if (!answer.IsNullOrEmpty())
            {
                selectedIndex = options.IndexOf(answer);
            }

            return selectedIndex;
        }

        /// <summary>
        /// Shows the message box.
        /// </summary>
        /// <param name="message">The message to show.</param>
        /// <param name="severity">The severity of the message.</param>
        /// <returns></returns>
        /// <exception cref="System.NotSupportedException">ShowBlockingQuestionMessage must be called in a background thread!</exception>
        protected virtual async Task ShowMessageBoxAsync(string message, NotificationSeverityEnum severity)
        {
            var title = GetTitleFromSeverity(severity);
            var buttonOk = (ViewModel as ExtendedViewModel)?.LocalizationService.GetTextForKey("Common_Label_Ok");

            await MainThread.InvokeOnMainThreadAsync(async () => await DisplayAlert(title, message, buttonOk)).ConfigureAwait(false);
        }

        #endregion
    }
}
