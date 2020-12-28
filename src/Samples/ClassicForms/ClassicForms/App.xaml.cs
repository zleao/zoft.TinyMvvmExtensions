using Autofac;
using ClassicForms.Resources;
using System.Globalization;
using System.Reflection;
using TinyMvvm;
using TinyMvvm.Autofac;
using TinyMvvm.Forms;
using TinyMvvm.IoC;
using Xamarin.Forms;
using zoft.NotificationService;
using zoft.TinyMvvmExtensions.Core.Localization;
using zoft.TinyMvvmExtensions.Core.ViewModels;

namespace ClassicForms
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            var currentAssembly = Assembly.GetExecutingAssembly();

            var navigationHelper = new FormsNavigationHelper();

            navigationHelper.RegisterViewsInAssembly(currentAssembly);

            var containerBuilder = new ContainerBuilder();

            containerBuilder.RegisterInstance<INavigationHelper>(navigationHelper);

            containerBuilder.RegisterAssemblyTypes(currentAssembly)
                   .Where(x => x.IsSubclassOf(typeof(Page)));

            containerBuilder.RegisterAssemblyTypes(currentAssembly)
                   .Where(x => x.IsSubclassOf(typeof(CoreViewModel)));

            containerBuilder.RegisterType<NotificationManager>().As<INotificationService>().SingleInstance();

            containerBuilder.RegisterInstance<ILocalizationService>(new LocalizationService(AppResource.ResourceManager, CultureInfo.InvariantCulture));

            var container = containerBuilder.Build();

            Resolver.SetResolver(new AutofacResolver(container));

            navigationHelper.SetRootView(nameof(MainPage), false);
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
