using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace LScape.Wpf
{
    /// <summary>
    /// Observable collection of notifying items, collection emits change event when child items notify change
    /// </summary>
    /// <typeparam name="T">The type of items</typeparam>
    public class FullObservableCollection<T> : ObservableCollection<T> where T : INotifyPropertyChanged
    {
        /// <summary>
        /// Constructs an empty collection
        /// </summary>
        public FullObservableCollection()
        {
            CollectionChanged += Full_CollectionChanged;
        }

        /// <summary>
        /// Constructs a collection from the list of items
        /// </summary>
        /// <param name="items">The items to add</param>
        public FullObservableCollection(IEnumerable<T> items) : base(items)
        {
            CollectionChanged += Full_CollectionChanged;
        }

        private void Full_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (T item in e.OldItems)
                    item.PropertyChanged -= Item_PropertyChanged;
            }

            if (e.NewItems != null)
            {
                foreach (T item in e.NewItems)
                    item.PropertyChanged += Item_PropertyChanged;
            }
        }

        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, sender, sender, IndexOf((T)sender));
            OnCollectionChanged(args);
        }
    }
}
