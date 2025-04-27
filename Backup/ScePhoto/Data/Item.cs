//-----------------------------------------------------------------------
// <copyright file="Item.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     Base class for all items in the feed.
// </summary>
//-----------------------------------------------------------------------

namespace ScePhoto.Data
{
    using System;
    using Microsoft.SubscriptionCenter.Sync;
    using System.Collections.Generic;

    /// <summary>
    /// Base class for all items in the feed.
    /// </summary>
    public abstract class Item : NavigatableObject
    {
        /// <summary>
        /// The Uri to the website corresponding to the item.
        /// </summary>
        private readonly string webLink;

        /// <summary>
        /// The date and time when the item was published.
        /// </summary>
        private readonly DateTime publishDate;

        /// <summary>
        /// The revision information for the item.
        /// </summary>
        private readonly CsxRevision revision;

        /// <summary>
        /// Property bag, which is used to store custom item's properties.
        /// </summary>
        private readonly IDictionary<string, string> properties;

        /// <summary>
        /// Flag indicating whether or not the item is new.
        /// </summary>
        private bool newItem;
        
        /// <summary>
        /// Initializes the Item instace.
        /// </summary>
        /// <param name="title">The title of the item.</param>
        /// <param name="guid">A string that uniquely identifies the item.</param>
        /// <param name="webLink">The Uri to the website corresponding to the item.</param>
        /// <param name="publishDate">The date and time when the item was published.</param>
        /// <param name="revision">The revision information for the item.</param>
        /// <param name="properties">Property bag, which is used to store custom item's properties.</param>
        protected Item(string title, string guid, string webLink, DateTime publishDate, CsxRevision revision, IDictionary<string, string> properties)
            : base(title, guid)
        {
            if (title == null)
            {
                throw new ArgumentNullException("title");
            }

            if (String.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid");
            }

            if (webLink == null)
            {
                throw new ArgumentNullException("webLink");
            }

            if (revision == null)
            {
                throw new ArgumentNullException("revision");
            }

            this.webLink = webLink;
            this.publishDate = publishDate;
            this.revision = revision;
            this.properties = properties;
            this.newItem = true;
        }

        /// <summary>
        /// Gets the Uri to the website corresponding to the item.
        /// </summary>
        public string WebLink
        {
            get { return this.webLink; }
        }

        /// <summary>
        /// Gets the date and tiem when the item was published.
        /// </summary>
        public DateTime PublishDate
        {
            get { return this.publishDate; }
        }

        /// <summary>
        /// Gets the revision information for the item.
        /// </summary>
        public CsxRevision Revision
        {
            get { return this.revision; }
        }

        /// <summary>
        /// Gets the property bag, which is used to store custom item's properties.
        /// </summary>
        public IDictionary<string, string> Properties
        {
            get { return this.properties; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not the item is new.
        /// </summary>
        public bool IsNew
        {
            get { return this.newItem; }
            set { this.newItem = value; }
        }

        /// <summary>
        /// Returns a String that represents the Item.
        /// </summary>
        /// <returns>A String that represents the Item.</returns>
        public override string ToString()
        {
            return this.Title;
        }

        /// <summary>
        /// Merges data stored in another Item into the current data. Called during e.g. GuidStore merge operations.
        /// </summary>
        /// <param name="otherItem">The other item to be merged with the current data.</param>
        public virtual void Merge(Item otherItem)
        {
            if (otherItem == null)
            {
                throw new ArgumentNullException("otherItem");
            }

            this.newItem = false;
        }
    }
}
