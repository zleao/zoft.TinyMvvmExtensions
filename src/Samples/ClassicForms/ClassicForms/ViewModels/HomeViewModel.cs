using System.Threading.Tasks;
using zoft.NotificationService;
using zoft.NotificationService.Messages;
using zoft.NotificationService.Messages.OneWay;
using zoft.TinyMvvmExtensions.ViewModels;

namespace ClassicForms.ViewModels
{
    public class HomeViewModel : ExtendedViewModel
    {
        public HomeViewModel(INotificationService notificationService)
            : base(notificationService)
        {
            Title = "Home";
        }

        protected override void SubscribeLongRunningMessageEvents()
        {
            base.SubscribeLongRunningMessageEvents();

            SubscribeLongRunningEvent<NotificationGenericMessage>(OnNotificationGenericMessageAsync, nameof(OnNotificationGenericMessageAsync), context: ViewModelContext);
        }

        private Task OnNotificationGenericMessageAsync(NotificationGenericMessage msg)
        {
            return NotificationManager.PublishSuccessNotificationAsync(msg.Message, NotificationModeEnum.MessageBox);
        }
    }
}
