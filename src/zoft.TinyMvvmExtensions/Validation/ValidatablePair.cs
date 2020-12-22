﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace zoft.TinyMvvmExtensions.Validation
{
    public class ValidatablePair<T> : IValidatable<ValidatablePair<T>>
    {
        public List<IValidationRule<ValidatablePair<T>>> Validations { get; } = new List<IValidationRule<ValidatablePair<T>>>();

        public bool IsValid { get; set; } = true;

        public List<string> Errors { get; set; } = new List<string>();

        public ValidatableObject<T> Item1 { get; set; } = new ValidatableObject<T>();

        public ValidatableObject<T> Item2 { get; set; } = new ValidatableObject<T>();

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged()
        {
            Item1?.RaisePropertyChanged();
            Item2?.RaisePropertyChanged();
        }

        public bool Validate()
        {
            var item1IsValid = Item1.Validate();
            var item2IsValid = Item2.Validate();
            if (item1IsValid && item2IsValid)
            {
                Errors.Clear();

                IEnumerable<string> errors = Validations.Where(v => !v.Check(this))
                    .Select(v => v.ValidationMessage);

                Errors = errors.ToList();
                Item2.Errors = Errors;
                Item2.IsValid = Errors.Count == 0;
            }

            IsValid = Item1.Errors.Count == 0 && Item2.Errors.Count == 0;

            return this.IsValid;
        }
    }
}
