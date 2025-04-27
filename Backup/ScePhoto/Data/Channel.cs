//-----------------------------------------------------------------------
// <copyright file="Channel.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     Base class for all channels in the feed.
// </summary>
//-----------------------------------------------------------------------

namespace ScePhoto.Data
{
    using System;
    using System.ComponentModel;

    /// <summary>
    /// Base class for all channels in the feed.
    /// </summary>
    public abstract class Channel : NavigatableObject, INotifyPropertyChanged
    {
        /// <summary>
        /// The description of the channel.
        /// </summary>
        private string description;

        /// <summary>
        /// The publication date for the content of the channel.
        /// </summary>
        private DateTime publishDate;

        /// <summary>
        /// The last time the content of the channel changed.
        /// </summary>
        private DateTime changeDate;

        /// <summary>
        /// The guid repository that provides mapping from a guid to associated with it object.
        /// </summary>
        private GuidStore guidStore;

        /// <summary>
        /// The base URI for all resources referenced by this channel.
        /// </summary>
        private Uri baseUri;

        /// <summary>
        /// Initializes the Channel instance.
        /// </summary>
        protected Channel()
            : base()
        {
            this.description = String.Empty;
            this.guidStore = new GuidStore();
        }

        /// <summary>
        /// Initializes the Channel instance.
        /// </summary>
        /// <param name="title">The name of the channel.</param>
        /// <param name="description">The description of the channel.</param>
        /// <param name="publishDate">The publication date for the content of the channel.</param>
        /// <param name="changeDate">The last time the content of the channel changed.</param>
        /// <param name="guidStore">The guid repository that provides mapping from a guid to associated with it object.</param>
        protected Channel(string title, string description, DateTime publishDate, DateTime changeDate, GuidStore guidStore)
        {
            if (String.IsNullOrEmpty(title))
            {
                throw new ArgumentNullException("title");
            }

            if (description == null)
            {
                throw new ArgumentNullException("description");
            }

            if (guidStore == null)
            {
                throw new ArgumentNullException("guidStore");
            }

            this.Title = title;
            this.description = description;
            this.publishDate = publishDate;
            this.changeDate = changeDate;
            this.guidStore = guidStore;
            this.Guid = String.Empty;
        }

        /// <summary>
        /// PropertyChanged event notifies listeners if a property value on this element has changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets the description of the channel.
        /// </summary>
        public string Description
        {
            get { return this.description; }
        }

        /// <summary>
        /// Gets the publication date for the content of the channel.
        /// </summary>
        public DateTime PublishDate
        {
            get { return this.publishDate; }
        }

        /// <summary>
        /// Gets the last time the content of the channel changed.
        /// </summary>
        public DateTime ChangeDate
        {
            get { return this.changeDate; }
        }

        /// <summary>
        /// Gets the guid repository that provides mapping from a guid to associated with it object.
        /// </summary>
        public GuidStore GuidStore
        {
            get { return this.guidStore; }
        }

        /// <summary>
        /// Gets or sets the base URI for all resources referenced by this channel.
        /// </summary>
        public Uri BaseUri
        {
            get { return this.baseUri; }
            set { this.baseUri = value; }
        }

        /// <summary>
        /// Merges data stored in another Channel into the current data.
        /// </summary>
        /// <param name="channel">The Channel to be merged with the current data.</param>
        public virtual void Merge(Channel channel)
        {
            if (channel == null)
            {
                throw new ArgumentNullException("channel");
            }

            this.Title = channel.Title;
            this.description = channel.description;
            this.publishDate = channel.publishDate;
            this.changeDate = channel.changeDate;

            channel.guidStore.Merge(this.guidStore);
            this.guidStore = channel.guidStore;

            // Notify property changed for properties set during Merge.
            // NOTE: Guid and BaseUri are not changed during the merge process, because
            // those values are set independently.
            this.OnNotifyPropertyChanged("Title");
            this.OnNotifyPropertyChanged("Description");
            this.OnNotifyPropertyChanged("PublishDate");
            this.OnNotifyPropertyChanged("ChangeDate");
            this.OnNotifyPropertyChanged("GuidStore");
        }

        /// <summary>
        /// Notifies about specified property value change.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        protected void OnNotifyPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
