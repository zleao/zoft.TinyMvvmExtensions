using ClassicForms.Models;
using ClassicForms.Views;
using System.Collections.ObjectModel;
using zoft.TinyMvvmExtensions.ViewModels;

namespace ClassicForms.ViewModels
{
    public class MainMenuViewModel : CoreViewModel
    {
        private ObservableCollection<MenuOption> _menuOptions = new ObservableCollection<MenuOption>();
        public ObservableCollection<MenuOption> MenuOptions
        {
            get => _menuOptions;
            set => Set(ref _menuOptions, value);
        }

        public MainMenuViewModel()
        {
            Title = "Menu";

            MenuOptions = new ObservableCollection<MenuOption>(new[]
            {
                new MenuOption ("Home", nameof(HomePage)),
                new MenuOption ("Bindings Test", nameof(BindingsTestPage)),
                new MenuOption ("Validations", nameof(ValidationsPage)),
                new MenuOption ("Notifications", nameof(NotificationsPage)),
            });
        }

        public void SelectMenuOption(MenuOption menuOption)
        {
            if (menuOption != null)
            {
                if(menuOption.PageKey == nameof(HomePage))
                {
                    Navigation.ResetStackWith(menuOption.PageKey);
                }
                if (menuOption.PageKey == nameof(NotificationsPage))
                {
                    Navigation.NavigateToAsync(menuOption.PageKey, typeof(HomeViewModel).Name);
                }
                else
                {
                    Navigation.NavigateToAsync(menuOption.PageKey);
                }
            }
        }
    }
}
