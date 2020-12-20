namespace ClassicForms.Models
{
    public sealed class MenuOption
    {
        #region Properties

        public string Text { get; }

        public string PageKey { get; }

        #endregion

        #region Constructor

        public MenuOption(string text, string pageKey)
        {
            Text = text;
            PageKey = pageKey;
        }

        #endregion
    }
}
