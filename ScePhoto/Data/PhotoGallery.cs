//-----------------------------------------------------------------------
// <copyright file="PhotoGallery.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     Represents a collection of photo albums, into a "gallery" of photo albums.
// </summary>
//-----------------------------------------------------------------------

namespace ScePhoto.Data
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    /// <summary>
    /// Represents a collection of photo albums, into a "gallery" of photo albums.
    /// </summary>
    public class PhotoGallery : Channel
    {
        /// <summary>
        /// The photo albums contained within this photo gallery.
        /// </summary>
        private MyPhotoAlbumCollection photoAlbums;

        /// <summary>
        /// The photo album feeds representing the photo albums contained within this photo gallery.
        /// </summary>
        private ReadOnlyCollection<FeedItem> photoAlbumFeeds;

        /// <summary>
        /// Initializes the PhotoGallery instance.
        /// </summary>
        /// <param name="guid">A string that uniquely identifies the item.</param>
        public PhotoGallery(string guid)
        {
            if (String.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid");
            }

            this.photoAlbums = new MyPhotoAlbumCollection();
            this.photoAlbumFeeds = new ReadOnlyCollection<FeedItem>(new List<FeedItem>());
            Guid = guid;
        }

        /// <summary>
        /// Initializes the PhotoGallery instance.
        /// </summary>
        /// <param name="title">The name of the channel.</param>
        /// <param name="description">The description of the channel.</param>
        /// <param name="publishDate">The publication date for the content of the channel.</param>
        /// <param name="changeDate">The last time the content of the channel changed.</param>
        /// <param name="guidStore">The guid repository that provides mapping from a guid to associated with it object.</param>
        /// <param name="photoAlbumFeeds">The collection of items representing album feeds location and properties.</param>
        public PhotoGallery(string title, string description, DateTime publishDate, DateTime changeDate, GuidStore guidStore, IList<FeedItem> photoAlbumFeeds)
            : base(title, description, publishDate, changeDate, guidStore)
        {
            if (photoAlbumFeeds == null)
            {
                throw new ArgumentNullException("photoAlbumFeeds");
            }

            this.photoAlbums = new MyPhotoAlbumCollection();
            this.photoAlbumFeeds = new ReadOnlyCollection<FeedItem>(photoAlbumFeeds);
        }

        /// <summary>
        /// Gets the collection of photo albums in this photo gallery.
        /// </summary>
        public PhotoAlbumCollection PhotoAlbums
        {
            get { return this.photoAlbums; }
        }

        /// <summary>
        /// Gets the collection of photo album feeds representing the photo albums in this photo gallery.
        /// </summary>
        public ReadOnlyCollection<FeedItem> PhotoAlbumFeeds
        {
            get { return this.photoAlbumFeeds; }
        }

        /// <summary>
        /// Merges data stored in another PhotoGallery into the current data.
        /// </summary>
        /// <param name="channel">The Channel to be merged with the current data.</param>
        public override void Merge(Channel channel)
        {
            PhotoGallery photoGallery = channel as PhotoGallery;
            if (photoGallery != null)
            {
                base.Merge(photoGallery);
                this.photoAlbumFeeds = photoGallery.photoAlbumFeeds;

                // Iterate though collection of new photo albums and merge them into the current collection. 
                // As we progress through merge process both collections up to ‘index’ position are synchronized.
                int index, newCount;
                for (index = 0, newCount = this.photoAlbumFeeds.Count; index < newCount; index++)
                {
                    FeedItem nestedFeed = this.photoAlbumFeeds[index];
                    this.photoAlbums.MergeFeedItem(index, nestedFeed);
                }

                // All new photo albums have been merged. If the current collection contains obsolete albums, 
                // they are located at the end of collection (starting from ‘index’ position). 
                // Remove those albums from the current collection.
                if (index < this.photoAlbums.Count)
                {
                    this.photoAlbums.RemoveObsoleteItems(index);
                }

                this.photoAlbums.RaiseCollectionChanged();

                // Notify property changed for properties set during Merge.
                OnNotifyPropertyChanged("PhotoAlbumFeeds");
            }
        }

        /// <summary>
        /// Resets IsNew flag on all items.
        /// </summary>
        public void ResetIsNew()
        {
            GuidStore.ResetIsNew();
            foreach (PhotoAlbum photoAlbum in this.photoAlbums)
            {
                photoAlbum.GuidStore.ResetIsNew();
            }
        }
        
        /// <summary>
        /// A PhotoAlbumCollection providing methods to merge photo album feed items into the photo album collection as PhotoAlbums.
        /// </summary>
        private class MyPhotoAlbumCollection : PhotoAlbumCollection
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
                    PhotoAlbum dataFeed = Items[index];
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
                            dataFeed = new PhotoAlbum(feedItem.Guid);
                            Items.Insert(index, dataFeed);
                        }
                    }

                    // else - existing item in the same position
                }
                else
                {
                    // New item which needs to be inserted at 'index' position
                    PhotoAlbum dataFeed = new PhotoAlbum(feedItem.Guid);
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
                    ((List<PhotoAlbum>)Items).RemoveRange(index, count);
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
