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
                else
                {
                    Navigation.NavigateToAsync(menuOption.PageKey);
                }
            }
        }
    }
}
