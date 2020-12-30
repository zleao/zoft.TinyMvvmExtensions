using Acr.UserDialogs;
using System;
using System.Windows.Input;
using zoft.TinyMvvmExtensions.Core.Attributes;
using zoft.TinyMvvmExtensions.Core.Commands;
using zoft.TinyMvvmExtensions.Core.Validation;
using zoft.TinyMvvmExtensions.Core.Validation.Rules;
using zoft.TinyMvvmExtensions.Core.ViewModels;

namespace ClassicForms.ViewModels
{
    public class ValidationsViewModel : CoreViewModel
    {
        public ValidatableObject<string> FirstName { get; } = new ValidatableObject<string>();
        public ValidatableObject<string> LastName { get; } = new ValidatableObject<string>();
        public ValidatableObject<string> FullNameValidatable { get; } = new ValidatableObject<string>();
        public ValidatableObject<DateTime> BirthDay { get; } = new ValidatableObject<DateTime>() { Value = DateTime.Now };
        public ValidatableObject<string> PhoneNumber { get; } = new ValidatableObject<string>();
        public ValidatablePair<string> Email { get; } = new ValidatablePair<string>();
        public ValidatablePair<string> Password { get; } = new ValidatablePair<string>();
        public ValidatableObject<bool> TermsAndCondition { get; } = new ValidatableObject<bool>();

        [DependsOn(nameof(FirstName))]
        [DependsOn(nameof(LastName))]
        public string FullName => $"{FirstName} {LastName}";

        private ICommand _validateCommand;
        public ICommand ValidateCommand => _validateCommand ??= new SyncCommand(() =>
        {
            if (AreFieldsValid())
            {
                UserDialogs.Instance.Alert("All fields valid!");
            }
        });

        private ICommand _setDefaultValuesCommand;
        public ICommand SetDefaultValuesCommand => _setDefaultValuesCommand ??= new SyncCommand(() =>
        {
            FirstName.Value = "John";
            LastName.Value = "Doe";
            BirthDay.Value = DateTime.Now;
            PhoneNumber.Value = "123456789";
            Email.Item1.Value = "email@email.com";
            Email.Item2.Value = "email@email.com";
            Password.Item1.Value = "pass1";
            Password.Item2.Value = "pass2";
            TermsAndCondition.Value = true;
        });

        public ValidationsViewModel()
        {
            AddValidationRules();
        }

        private void AddValidationRules()
        {
            FirstName.Validations.Add(new IsNotNullOrEmptyRule<string> { ValidationMessage = "First Name Required" });
            LastName.Validations.Add(new IsNotNullOrEmptyRule<string> { ValidationMessage = "Last Name Required" });
            BirthDay.Validations.Add(new HasValidAgeRule<DateTime> { ValidationMessage = "You must be 18 years of age or older" });
            PhoneNumber.Validations.Add(new IsNotNullOrEmptyRule<string> { ValidationMessage = "Phone Number Required" });
            PhoneNumber.Validations.Add(new IsLenghtValidRule<string> { ValidationMessage = "Phone Number should have a maximun of 10 digits and minimun of 8", MaximumLenght = 10, MinimumLenght = 8 });

            //Email Validation Rules
            Email.Item1.Validations.Add(new IsNotNullOrEmptyRule<string> { ValidationMessage = "Email Required" });
            Email.Item1.Validations.Add(new IsValidEmailRule<string> { ValidationMessage = "Invalid Email" });
            Email.Item2.Validations.Add(new IsNotNullOrEmptyRule<string> { ValidationMessage = "Confirm Email Required" });
            Email.Validations.Add(new MatchPairValidationRule<string> { ValidationMessage = "Email and confirm email don't match" });

            //Password Validation Rules
            Password.Item1.Validations.Add(new IsNotNullOrEmptyRule<string> { ValidationMessage = "Password Required" });
            Password.Item1.Validations.Add(new IsValidPasswordRule<string> { ValidationMessage = "Password between 8-20 characters; must contain at least one lowercase letter, one uppercase letter, one numeric digit, and one special character" });
            Password.Item2.Validations.Add(new IsNotNullOrEmptyRule<string> { ValidationMessage = "Confirm password required" });
            Password.Validations.Add(new MatchPairValidationRule<string> { ValidationMessage = "Password and confirm password don't match" });

            TermsAndCondition.Validations.Add(new IsValueTrueRule<bool> { ValidationMessage = "Please accept tems and condition" });
        }

        [DependsOn(nameof(FirstName))]
        [DependsOn(nameof(LastName))]
        private void UpdateFullNameValidatable()
        {
            FullNameValidatable.Value = $"{FirstName} {LastName}";
        }

        private bool AreFieldsValid()
        {
            bool isFirstNameValid = FirstName.Validate();
            bool isLastNameValid = LastName.Validate();
            bool isBirthDayValid = BirthDay.Validate();
            bool isPhoneNumberValid = PhoneNumber.Validate();
            bool isEmailValid = Email.Validate();
            bool isPasswordValid = Password.Validate();
            bool isTermsAndConditionValid = TermsAndCondition.Validate();

            return isFirstNameValid && isLastNameValid && isBirthDayValid &&
                   isPhoneNumberValid && isEmailValid && isPasswordValid &&
                   isTermsAndConditionValid;
        }
    }
}
