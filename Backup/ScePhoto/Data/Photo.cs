//-----------------------------------------------------------------------
// <copyright file="Photo.cs" company="Microsoft">
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
    
    /// <summary>
    /// Represents a single image, along with all of its associated attributes.
    /// </summary>
    public class Photo : Item
    {
        /// <summary>
        /// Short description of the item.
        /// </summary>
        private string description;

        /// <summary>
        /// File Uri for description file if included separately.
        /// </summary>
        private Uri descriptionFileUri;

        /// <summary>
        /// The Uri to the photo image.
        /// </summary>
        private Uri imageUri;

        /// <summary>
        /// The Uri to the thumbnail image.
        /// </summary>
        private Uri thumbnailUri;

        /// <summary>
        /// The collection of short integer IDs of the tags used to classify this photo.
        /// </summary>
        private ObservableCollection<short> photoTagIds;

        /// <summary>
        /// Constructor. Initializes the Item.
        /// </summary>
        /// <param name="guid">A string that uniquely identifies the item.</param>
        /// <param name="title">The title of the item.</param>
        /// <param name="description">Short description of the item.</param>
        /// <param name="webLink">The Uri to the website corresponding to the item.</param>
        /// <param name="publishDate">Indicates when the item was published.</param>
        /// <param name="revision">The revision information for the item.</param>
        /// <param name="properties">Property bag, which is used to store custom item's properties.</param>
        /// <param name="imageUri">The Uri to the photo image.</param>
        /// <param name="thumbnailUri">The Uri to the thumbnail image.</param>
        /// <param name="descriptionFileUri">File Uri for separate description file, if included.</param>
        /// <param name="photoTagIds">The List of tags used to classify this photo.</param>
        public Photo(string guid, string title, string description, string webLink, DateTime publishDate, CsxRevision revision, IDictionary<string, string> properties, Uri imageUri, Uri thumbnailUri, Uri descriptionFileUri, List<short> photoTagIds)
            : base(title, guid, webLink, publishDate, revision, properties)
        {
            if (description == null)
            {
                throw new ArgumentNullException("description");
            }

            this.description = description;
            this.descriptionFileUri = descriptionFileUri;
            this.imageUri = imageUri;
            this.thumbnailUri = thumbnailUri;
            this.photoTagIds = new ObservableCollection<short>(photoTagIds);
        }

        /// <summary>
        /// Gets the photo's description.
        /// </summary>
        public string Description
        {
            get { return this.description; }
        }

        /// <summary>
        /// Gets the photo's description file Uri.
        /// </summary>
        public Uri DescriptionFileUri
        {
            get { return this.descriptionFileUri; }
        }

        /// <summary>
        /// Gets the URI to the photo, large sized.
        /// </summary>
        public Uri ImageUri
        {
            get { return this.imageUri; }
        }

        /// <summary>
        /// Gets the URI to the photo, thumbnail sized.
        /// </summary>
        public Uri ThumbnailUri
        {
            get { return this.thumbnailUri; }
        }

        /// <summary>
        /// Gets or sets the collection of short integer IDs of the tags used to classify this photo.
        /// </summary>
        public ObservableCollection<short> PhotoTagIds
        {
            get { return this.photoTagIds; }
            set { this.photoTagIds = value; }
        }
    }
}
