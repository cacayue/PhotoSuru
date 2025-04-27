//-----------------------------------------------------------------------
// <copyright file="PhotoAlbum.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     Represents a collection of photos, into an "album" of photos.
// </summary>
//-----------------------------------------------------------------------

namespace ScePhoto.Data
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a collection of photos, into an "album" of photos.
    /// </summary>
    public class PhotoAlbum : Channel
    {
        /// <summary>
        /// The collection of photos contained by the album.
        /// </summary>
        private PhotoCollection photos;

        /// <summary>
        /// Initializes the PhotoAlbum instance.
        /// </summary>
        /// <param name="guid">A string that uniquely identifies the item.</param>
        public PhotoAlbum(string guid)
        {
            if (String.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid");
            }

            this.photos = new PhotoCollection();
            Guid = guid;
        }

        /// <summary>
        /// Initializes the PhotoAlbum instance.
        /// </summary>
        /// <param name="title">The name of the channel.</param>
        /// <param name="description">The description of the channel.</param>
        /// <param name="publishDate">The publication date for the content of the channel.</param>
        /// <param name="changeDate">The last time the content of the channel changed.</param>
        /// <param name="guidStore">The guid repository that provides mapping from a guid to associated with it object.</param>
        /// <param name="photos">The collection of photos contained by the album.</param>
        public PhotoAlbum(string title, string description, DateTime publishDate, DateTime changeDate, GuidStore guidStore, PhotoCollection photos)
            : base(title, description, publishDate, changeDate, guidStore)
        {
            if (photos == null)
            {
                throw new ArgumentNullException("photos");
            }

            this.photos = photos;
        }

        /// <summary>
        /// Gets the photos in this album.
        /// </summary>
        public PhotoCollection Photos
        {
            get { return this.photos; }
        }

        /// <summary>
        /// Merges data stored in another PhotoAlbum into the current data.
        /// </summary>
        /// <param name="channel">The Channel to be merged with the current data.</param>
        public override void Merge(Channel channel)
        {
            PhotoAlbum photoAlbum = channel as PhotoAlbum;
            if (photoAlbum != null)
            {
                base.Merge(photoAlbum);
                this.photos.Merge(photoAlbum.photos);
                this.OnNotifyPropertyChanged("Photos");
            }
        }
    }
}
