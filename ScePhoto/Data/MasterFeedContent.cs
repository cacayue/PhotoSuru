//-----------------------------------------------------------------------
// <copyright file="MasterFeedContent.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     Contains all information associated with the top-level ScePhoto feed.
// </summary>
//-----------------------------------------------------------------------

namespace ScePhoto.Data
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    /// <summary>
    /// Contains all information associated with the top-level ScePhoto feed.
    /// </summary>
    public class MasterFeedContent : Channel
    {
        /// <summary>
        /// Collection of photo galleries in the master feed.
        /// </summary>
        private MasterPhotoGalleryCollection photoGalleries;

        /// <summary>
        /// Collection of feeds representing the photo galleries in the master feed.
        /// </summary>
        private ReadOnlyCollection<FeedItem> photoGalleryFeeds;

        /// <summary>
        /// Initializes the MasterFeedContent instance.
        /// </summary>
        public MasterFeedContent()
        {
            this.photoGalleries = new MasterPhotoGalleryCollection();
            this.photoGalleryFeeds = new ReadOnlyCollection<FeedItem>(new List<FeedItem>());
            Guid = "[master_feed]";
        }

        /// <summary>
        /// Initializes the MasterFeedContent instance.
        /// </summary>
        /// <param name="title">The name of the channel.</param>
        /// <param name="description">The description of the channel.</param>
        /// <param name="publishDate">The publication date for the content of the channel.</param>
        /// <param name="changeDate">The last time the content of the channel changed.</param>
        /// <param name="guidStore">The guid repository that provides mapping from a guid to associated with it object.</param>
        /// <param name="photoGalleryFeeds">The collection of items representing gallery feeds location and properties.</param>
        public MasterFeedContent(string title, string description, DateTime publishDate, DateTime changeDate, GuidStore guidStore, IList<FeedItem> photoGalleryFeeds)
            : base(title, description, publishDate, changeDate, guidStore)
        {
            if (photoGalleryFeeds == null)
            {
                throw new ArgumentNullException("photoGalleryFeeds");
            }

            this.photoGalleries = new MasterPhotoGalleryCollection();
            this.photoGalleryFeeds = new ReadOnlyCollection<FeedItem>(photoGalleryFeeds);
            Guid = "[master_feed]";
        }

        /// <summary>
        /// Gets the collection of photo galleries in the master feed.
        /// </summary>
        public PhotoGalleryCollection PhotoGalleries
        {
            get { return this.photoGalleries; }
        }

        /// <summary>
        /// Gets the collection of photo gallery feeds in the master feed.
        /// </summary>
        public ReadOnlyCollection<FeedItem> PhotoGalleryFeeds
        {
            get { return this.photoGalleryFeeds; }
        }

        /// <summary>
        /// Merges data stored in another Channel into the current data.
        /// </summary>
        /// <param name="channel">The Channel to be merged with the current data.</param>
        public override void Merge(Channel channel)
        {
            MasterFeedContent masterFeedContent = channel as MasterFeedContent;
            if (masterFeedContent != null)
            {
                base.Merge(masterFeedContent);
                this.photoGalleryFeeds = masterFeedContent.photoGalleryFeeds;

                // Iterate though collection of new photo galleries and merge them into the current collection. 
                // As we progress through merge process both collections up to ‘index’ position are synchronized.
                int index, newCount;
                for (index = 0, newCount = this.photoGalleryFeeds.Count; index < newCount; index++)
                {
                    FeedItem nestedFeed = this.photoGalleryFeeds[index];
                    this.photoGalleries.MergeFeedItem(index, nestedFeed);
                }

                // All new photo galleries have been merged. If the current collection contains obsolete galleries, 
                // they are located at the end of collection (starting from ‘index’ position). 
                // Remove those galleries from the current collection.
                if (index < this.photoGalleries.Count)
                {
                    this.photoGalleries.RemoveObsoleteItems(index);
                }

                this.photoGalleries.RaiseCollectionChanged();

                // Notify property changed for properties set during Merge.
                OnNotifyPropertyChanged("PhotoGalleryFeeds");
            }
        }

        /// <summary>
        /// Resets IsNew flag on all items.
        /// </summary>
        public void ResetIsNew()
        {
            GuidStore.ResetIsNew();
            foreach (PhotoGallery photoGallery in this.photoGalleries)
            {
                photoGallery.ResetIsNew();
            }
        }

        /// <summary>
        /// A PhotoGalleryCollection providing methods to merge feeds to form a collection of photo galleries.
        /// </summary>
        private class MasterPhotoGalleryCollection : PhotoGalleryCollection
        {
            /// <summary>
            /// Merges a FeedItem into the collection at the specified index.
            /// </summary>
            /// <param name="index">Index of item after merge.</param>
            /// <param name="feedItem">FeedItem to be merged.</param>
            public void MergeFeedItem(int index, FeedItem feedItem)
            {
                // There are 3 categories of items:
                // 1) Existing item in the same position (guid and index is matching). No work needs to be done.
                // 2) Existing item at new position (guid is matching, but index is not). In this case reposition 
                //    the item and treat it as #1.
                // 3) New item. In this case insert it in appropriate position.
                int count = Items.Count;
                if (index < count)
                {
                    PhotoGallery dataFeed = Items[index];
                    if (String.Compare(feedItem.Guid, dataFeed.Guid, StringComparison.OrdinalIgnoreCase) != 0)
                    {
                        int oldIndex;
                        for (oldIndex = index + 1; oldIndex < count; oldIndex++)
                        {
                            if (String.Compare(feedItem.Guid, Items[oldIndex].Guid, StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                break;
                            }
                        }

                        if (oldIndex < count)
                        {
                            // Existing item at new position
                            dataFeed = Items[oldIndex];
                            Items.RemoveAt(oldIndex);
                            Items.Insert(index, dataFeed);
                        }
                        else
                        {
                            // New item which needs to be inserted at 'index' position
                            dataFeed = new PhotoGallery(feedItem.Guid);
                            Items.Insert(index, dataFeed);
                        }
                    }

                    // else - existing item in the same position
                }
                else
                {
                    // New item which needs to be inserted at 'index' position
                    PhotoGallery dataFeed = new PhotoGallery(feedItem.Guid);
                    Items.Insert(index, dataFeed);
                }
            }

            /// <summary>
            /// Removes items past a certain index.
            /// </summary>
            /// <param name="index">Index of last valid item.</param>
            public void RemoveObsoleteItems(int index)
            {
                int count = this.Count - index;
                if (count > 0)
                {
                    ((List<PhotoGallery>)Items).RemoveRange(index, count);
                }
            }

            /// <summary>
            /// Raises collection changed event for this collection.
            /// </summary>
            public void RaiseCollectionChanged()
            {
                OnCollectionChanged();
            }
        }
    }
}
