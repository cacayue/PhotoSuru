//-----------------------------------------------------------------------
// <copyright file="PhotoSlideShow.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     Represents a single image, along with all of its associated attributes.
// </summary>
//-----------------------------------------------------------------------

namespace ScePhoto.Data
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using Microsoft.SubscriptionCenter.Sync;
    using ScePhoto.View;
    
    /// <summary>
    /// Represents a slideshow of images.
    /// </summary>
    public class PhotoSlideShow : NavigatableObject
    {
        #region Private Fields

        /// <summary>
        /// Navigators for all photos in the slide show.
        /// </summary>
        private IList<PhotoNavigator> photoNavigators;
        
        /// <summary>
        /// Index of currently viewed Photo.
        /// </summary>
        private int currentPhotoIndex;

        /// <summary>
        /// Navigator for album used in this slide show.
        /// </summary>
        private PhotoAlbumNavigator albumNavigator;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor for PhotoSlideShow - creates slide show object for a photo album.
        /// </summary>
        /// <param name="albumNavigator">Navigator for PhotoAlbum in the slide show.</param>
        public PhotoSlideShow(PhotoAlbumNavigator albumNavigator)
            : base()
        {
            if (albumNavigator == null)
            {
                throw new ArgumentNullException("albumNavigator");
            }

            this.albumNavigator = albumNavigator;
            this.photoNavigators = albumNavigator.GetPhotos();
            this.Title = "Slideshow: " + ((PhotoAlbum)albumNavigator.Content).Title;
            this.Guid = ((PhotoAlbum)albumNavigator.Content).Guid;
        }

        /// <summary>
        /// Constructor for PhotoSlideShow - creates slide show object for a photo album.
        /// </summary>
        /// <param name="photoNavigator">Navigator for a Photo object, slide show is created for the album containing this Photo.</param>
        public PhotoSlideShow(PhotoNavigator photoNavigator)
            : base()
        {
            if (photoNavigator == null)
            {
                throw new ArgumentNullException("photoNavigator");
            }

            PhotoAlbumNavigator albumNavigator = photoNavigator.GetParent() as PhotoAlbumNavigator;
            if (albumNavigator == null)
            {
                throw new InvalidOperationException(Strings.SlideShowMissingAlbum);
            }

            IList<PhotoNavigator> photos = albumNavigator.GetPhotos();
            int index = photos.IndexOf(photoNavigator);
            index = (index >= 0) ? index : 0;

            this.albumNavigator = albumNavigator;
            this.photoNavigators = photos;
            this.currentPhotoIndex = index;
            this.Title = "Slideshow: " + ((PhotoAlbum)albumNavigator.Content).Title;
            this.Guid = ((PhotoAlbum)albumNavigator.Content).Guid;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets current photo displayed in the slide show.
        /// </summary>
        public PhotoNavigator CurrentPhoto
        {
            get
            {
                if (this.photoNavigators == null)
                {
                    return null;
                }

                return this.photoNavigators[this.currentPhotoIndex];
            }
        }

        /// <summary>
        /// Gets next photo in the slide show.
        /// </summary>
        public PhotoNavigator NextPhoto
        {
            get
            {
                if (this.photoNavigators == null)
                {
                    return null;
                }

                int nextPhotoIndex = this.currentPhotoIndex + 1;
                nextPhotoIndex %= this.photoNavigators.Count;
                return this.photoNavigators[nextPhotoIndex];
            }
        }

        /// <summary>
        /// Gets previous photo in the slide show.
        /// </summary>
        public PhotoNavigator PreviousPhoto
        {
            get
            {
                if (this.photoNavigators == null)
                {
                    return null;
                }

                int prevPhotoIndex = this.currentPhotoIndex - 1;
                if (this.currentPhotoIndex == 0)
                {
                    prevPhotoIndex = this.photoNavigators.Count - 1;
                }

                return this.photoNavigators[prevPhotoIndex];
            }
        }

        /// <summary>
        /// Gets the Navigator to PhotoAlbum for this slide show.
        /// </summary>
        public PhotoAlbumNavigator Album
        {
            get
            {
                return this.albumNavigator;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Advance to next photo.
        /// </summary>
        public void MoveNext()
        {
            this.currentPhotoIndex += 1;
            this.currentPhotoIndex %= this.photoNavigators.Count;
        }

        /// <summary>
        /// Advance backwards to previous photo.
        /// </summary>
        public void MovePrevious()
        {
            if (this.currentPhotoIndex == 0)
            {
                this.currentPhotoIndex = this.photoNavigators.Count - 1;
            }
            else
            {
                this.currentPhotoIndex -= 1;
            }
        }

        #endregion
    }
}
