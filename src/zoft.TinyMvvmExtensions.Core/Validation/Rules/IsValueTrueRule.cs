namespace zoft.TinyMvvmExtensions.Core.Validation.Rules
{
    public class IsValueTrueRule<T> : IValidationRule<T>
    {
        public string ValidationMessage { get; set; }

        public bool Check(T value)
        {
            return bool.Parse($"{value}");
        }
    }
}
