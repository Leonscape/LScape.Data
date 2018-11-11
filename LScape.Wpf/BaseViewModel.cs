using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LScape.Wpf
{
    /// <summary>
    /// Base for view models supplying INotifyPropertyChanged support
    /// </summary>
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        /// <inheritdoc />
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Called by properties when value has changed
        /// </summary>
        /// <param name="propertyName">The name of the property, can be left blank if called from the property itself</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Called by a property to set a value in the view model
        /// </summary>
        /// <remarks>
        /// Provides <see cref="INotifyPropertyChanged" /> to properties to remove a lot of boilerplate code
        /// </remarks>
        /// <typeparam name="T">The type of property</typeparam>
        /// <param name="storage">The storage variable</param>
        /// <param name="value">The value to set</param>
        /// <param name="propertyName">The name of the property, can be left blank if called from the property itself</param>
        /// <returns>True if the value changed false if it didn't</returns>
        /// <example>
        /// <code>
        /// private int _id;
        /// public int Id 
        /// {
        ///     get => _id;
        ///     set => SetProperty(ref _id, value);
        /// }
        /// </code>
        /// </example>
        protected virtual bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Comparer<T>.Default.Compare(storage, value) == 0)
                return false;

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
