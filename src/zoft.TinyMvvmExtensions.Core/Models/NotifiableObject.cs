using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace zoft.TinyMvvmExtensions.Core.Models
{
    /// <summary>
    /// Base implementation for a <see cref="INotifyPropertyChanged">notifiable</see> object
    /// </summary>
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    public abstract class NotifiableObject : INotifyPropertyChanged
    {
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        /// <returns></returns>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises a PropertyChanged for the specified property
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        public void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Sets the specified field.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field">The field.</param>
        /// <param name="newValue">The new value.</param>
        /// <param name="propertyName">Name of the property.</param>
        protected void Set<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (!EqualityComparer<T>.Default.Equals(field, newValue))
            {
                field = newValue;
                RaisePropertyChanged(propertyName);
            }
        }
    }
}