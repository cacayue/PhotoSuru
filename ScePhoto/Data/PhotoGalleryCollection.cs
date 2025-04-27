//-----------------------------------------------------------------------
// <copyright file="PhotoGalleryCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     Read-only collection of PhotoGallery objects.
// </summary>
//-----------------------------------------------------------------------

namespace ScePhoto.Data
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Read-only collection of PhotoGallery objects.
    /// </summary>
    public class PhotoGalleryCollection : DataObjectCollection<PhotoGallery>
    {
        /// <summary>
        /// Initializes the PhotoGalleryCollection instance.
        /// </summary>
        public PhotoGalleryCollection()
        {
        }

        /// <summary>
        /// Initializes the PhotoGalleryCollection instance.
        /// </summary>
        /// <param name="photoGalleries">Collection of PhotoGalleries used to initialize this collection.</param>
        public PhotoGalleryCollection(IList<PhotoGallery> photoGalleries)
            : base(photoGalleries)
        {
        }
    }
}
