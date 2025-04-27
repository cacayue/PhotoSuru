//-----------------------------------------------------------------------
// <copyright file="FeedItem.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     Contains information associated with embedded feed.
// </summary>
//-----------------------------------------------------------------------

namespace ScePhoto.Data
{
    using System;
    using Microsoft.SubscriptionCenter.Sync;

    /// <summary>
    /// Contains information associated with embedded feed.
    /// </summary>
    public class FeedItem : Item
    {
        /// <summary>
        /// The URI representing the location of embedded feed.
        /// </summary>
        private Uri link;

        /// <summary>
        /// Initializes the FeedItem instance.
        /// </summary>
        /// <param name="title">The title of the item.</param>
        /// <param name="guid">A string that uniquely identifies the item.</param>
        /// <param name="webLink">The Uri to the website corresponding to the item.</param>
        /// <param name="publishDate">Indicates when the item was published.</param>
        /// <param name="revision">The revision information for the item.</param>
        /// <param name="link">The URI representing the location of associated resource.</param>
        public FeedItem(string title, string guid, string webLink, DateTime publishDate, CsxRevision revision, Uri link)
            : base(title, guid, webLink, publishDate, revision, null)
        {
            if (link == null)
            {
                throw new ArgumentNullException("link");
            }

            this.link = link;
        }

        /// <summary>
        /// Gets the URI representing the location of embedded feed.
        /// </summary>
        public Uri Link
        {
            get { return this.link; }
        }
    }
}
