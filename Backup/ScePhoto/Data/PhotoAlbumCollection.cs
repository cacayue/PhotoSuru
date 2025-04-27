//-----------------------------------------------------------------------
// <copyright file="PhotoAlbumCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     Read-only collection of PhotoAlbum objects.
// </summary>
//-----------------------------------------------------------------------

namespace ScePhoto.Data
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Read-only collection of PhotoAlbum objects.
    /// </summary>
    public class PhotoAlbumCollection : DataObjectCollection<PhotoAlbum>
    {
        /// <summary>
        /// Initializes the PhotoAlbumCollection instance.
        /// </summary>
        public PhotoAlbumCollection()
        {
        }

        /// <summary>
        /// Initializes the PhotoAlbumCollection instance.
        /// </summary>
        /// <param name="photoAlbums">Collection of PhotoAlbums used to initialize this collection.</param>
        public PhotoAlbumCollection(IList<PhotoAlbum> photoAlbums)
            : base(photoAlbums)
        {
        }
    }
}
