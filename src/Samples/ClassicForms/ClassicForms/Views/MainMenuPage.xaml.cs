using ClassicForms.Models;
using Xamarin.Forms.Xaml;

namespace ClassicForms.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainMenuPage
    {
        public MainMenuPage()
        {
            InitializeComponent();
        }

        private void TapGestureRecognizer_Tapped(object sender, System.EventArgs e)
        {
            ViewModel?.SelectMenuOption(MenuCollectionView.SelectedItem as MenuOption);
            MenuCollectionView.SelectedItem = null;
        }
    }
}