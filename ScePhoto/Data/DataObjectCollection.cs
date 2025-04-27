//-----------------------------------------------------------------------
// <copyright file="DataObjectCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     Provides the base class for a generic read-only data object collection.
// </summary>
//-----------------------------------------------------------------------

namespace ScePhoto.Data
{
    using System;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Collections.Generic;
    using System.ComponentModel;

    /// <summary>
    /// Provides the base class for a generic read-only data object collection.
    /// </summary>
    /// <typeparam name="T">Base item type for this collection.</typeparam>
    public abstract class DataObjectCollection<T> : ReadOnlyCollection<T>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        /// <summary>
        /// Initializes the DataObjectCollection instance.
        /// </summary>
        protected DataObjectCollection()
            : base(new List<T>())
        {
        }

        /// <summary>
        /// Initializes the DataObjectCollection instance.
        /// </summary>
        /// <param name="dataObjects">Collection of data objects used to initialize this collection.</param>
        protected DataObjectCollection(IList<T> dataObjects)
            : base(dataObjects)
        {
        }

        /// <summary>
        /// CollectionChanged event notifies listeners if the collection was modified.
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>
        /// PropertyChanged event notifies listeners that this object was modified.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Merges data from another collection.
        /// </summary>
        /// <param name="newCollection">The data object collection that contains new data.</param>
        public virtual void Merge(IList<T> newCollection)
        {
            if (newCollection == null)
            {
                throw new ArgumentNullException("newCollection");
            }

            // Replace entire collection without running merge algorithm. 
            // Merge algorithm has (N^2) complexity. Since currently nobody is using detailed 
            // information about the change, there is no reason to use merge.
            Items.Clear();
            List<T> itemsList = Items as List<T>;
            if (itemsList != null)
            {
                itemsList.AddRange(newCollection);
            }
            else
            {
                foreach (T item in newCollection)
                {
                    Items.Add(item);
                }
            }

            this.OnPropertyChanged("Count");
            this.OnCollectionChanged();
        }

        /// <summary>
        /// Raises CollectionChanged event.
        /// </summary>
        protected virtual void OnCollectionChanged()
        {
            if (this.CollectionChanged != null)
            {
                this.CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        /// <summary>
        /// Raises the PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
