//-----------------------------------------------------------------------
// <copyright file="NoDataAvailableError.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     Error returned when there is no data in the photo gallery that currently populates the view's content.
// </summary>
//-----------------------------------------------------------------------

namespace ScePhoto.Data
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Error returned when there is no data in the photo gallery that currently populates the view's content.
    /// </summary>
    public class NoDataAvailableError
    {
        /// <summary>
        /// The PhotoGallery for which no data is available.
        /// </summary>
        private PhotoGallery photoGallery;

        /// <summary>
        /// Contructor for NoDataAvailableError class.
        /// </summary>
        /// <param name="photoGallery">PhotoGallery for which no data is available.</param>
        public NoDataAvailableError(PhotoGallery photoGallery)
        {
            this.photoGallery = photoGallery;
        }

        /// <summary>
        /// Gets the PhotoGallery for which no data is available.
        /// </summary>
        public PhotoGallery PhotoGallery
        {
            get { return this.photoGallery; }
        }
    }
}
