using Acr.UserDialogs;
using System.Linq;
using System.Windows.Input;
using zoft.TinyMvvmExtensions.Core.Attributes;
using zoft.TinyMvvmExtensions.Core.Commands;
using zoft.TinyMvvmExtensions.Core.Extensions;
using zoft.TinyMvvmExtensions.Core.ViewModels;

namespace ClassicForms.ViewModels
{
    public class BindingsTestViewModel : CoreViewModel
    {
        #region Properties

        private string _firstName;
        public string FirstName
        {
            get => _firstName;
            set => Set(ref _firstName, value);
        }

        private string _lastName;
        public string LastName
        {
            get => _lastName;
            set => Set(ref _lastName, value);
        }

        [DependsOn(nameof(FirstName))]
        [DependsOn(nameof(LastName))]
        public string FullName
        {
            get
            {
                return $"{FirstName} {LastName}";
            }
        }

        private string _text;
        public string Text
        {
            get => _text;
            set => Set(ref _text, value);
        }

        private string _textForStrokes;
        public string TextForStrokes
        {
            get => _textForStrokes;
            set => Set(ref _textForStrokes, value);
        }

        private ICommand _showTextCommand;
        [DependsOn(nameof(Text))]
        public ICommand ShowTextCommand => _showTextCommand ??= new SyncCommand(ShowValue, CanShowValue);

        private bool CanShowValue()
        {
            return !Text.IsNullOrWhiteSpace();
        }

        private async void ShowValue()
        {
            await UserDialogs.Instance.AlertAsync(Text);
        }

        [DependsOn(nameof(TextForStrokes))]
        private async void ShowLastCharacter()
        {
            if(!TextForStrokes.IsNullOrWhiteSpace())
            {
                await UserDialogs.Instance.AlertAsync(TextForStrokes.Last().ToString(), "Last Char");
            }
        }

        #endregion
    }
}
