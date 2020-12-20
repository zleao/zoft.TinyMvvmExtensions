using Autofac;
using System.Reflection;
using TinyMvvm;
using TinyMvvm.Autofac;
using TinyMvvm.Forms;
using TinyMvvm.IoC;
using Xamarin.Forms;
using zoft.TinyMvvmExtensions.ViewModels;

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
