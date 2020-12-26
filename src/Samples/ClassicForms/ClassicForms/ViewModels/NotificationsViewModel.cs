using System.Threading.Tasks;
using System.Windows.Input;
using zoft.NotificationService;
using zoft.NotificationService.Messages;
using zoft.NotificationService.Messages.TwoWay.Question;
using zoft.TinyMvvmExtensions.Core.Commands;
using zoft.TinyMvvmExtensions.ViewModels;

namespace ClassicForms.ViewModels
{
    public class NotificationsViewModel : ExtendedViewModel
    {
        #region Fields

        private string _mainViewModelContext = null;

        #endregion

        #region Commands

        public ICommand ErrorNotificationCommand { get; }
        public ICommand QuestionNotificationCommand { get; }
        public ICommand DelayedNotificationCommand { get; }

        #endregion

        #region Constructor

        public NotificationsViewModel(INotificationService notificationManager)
            : base(notificationManager)
        {
            ErrorNotificationCommand = new AsyncCommand(OnErrorNotificationAsync);
            QuestionNotificationCommand = new AsyncCommand(OnQuestionNotificationAsync);
            DelayedNotificationCommand = new AsyncCommand(OnDelayedNotificationAsync);

            Title = "Notifications Sample";
        }

        #endregion

        #region Methods

        public override async Task Initialize()
        {
            await base.Initialize();

            _mainViewModelContext = (string)NavigationParameter;
        }

        private async Task OnErrorNotificationAsync()
        {
            await NotificationManager.PublishErrorNotificationAsync("Error notification", NotificationModeEnum.MessageBox);
        }

        private async Task OnQuestionNotificationAsync()
        {
            var answer = await NotificationManager.PublishGenericQuestionNotificationAsync("Do you feel lucky?", NotificationTwoWayAnswersGroupEnum.YesNo);
            if (answer.Answer == NotificationTwoWayAnswersEnum.Yes)
                await NotificationManager.PublishSuccessNotificationAsync("That's the spirit!");
            else
                await NotificationManager.PublishWarningNotificationAsync("You can do it!");
        }

        private async Task OnDelayedNotificationAsync()
        {
            await NotificationManager.DelayedPublishSuccessNotificationAsync("Delayed notification sent by NotificationsViewModel", NotificationModeEnum.MessageBox, _mainViewModelContext);
            await NotificationManager.PublishInfoNotificationAsync("Go to Home page to be able to see the delayed notification work his/her magic", NotificationModeEnum.MessageBox);
        }

        #endregion
    }
}
