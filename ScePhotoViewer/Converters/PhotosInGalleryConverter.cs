//-----------------------------------------------------------------------
// <copyright file="PhotosInGalleryConverter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     Returns the number of photos in a gallery given a PhotoGallery.
// </summary>
//-----------------------------------------------------------------------

namespace ScePhotoViewer
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Data;
    using System.Text;
    using ScePhoto.Data;

    /// <summary>
    /// IValueConverter to convert an angle, in degrees, to a 'simple' angle, that is, an angle which lies between 0 and 360 degrees.
    /// </summary>
    public class PhotosInGalleryConverter : IValueConverter
    {
        /// <summary>
        /// Counts the number of photos in a specific gallery.
        /// </summary>
        /// <param name="value">The source PhotoGallery.</param>
        /// <param name="targetType">The target type of the conversion.</param>
        /// <param name="parameter">The converter parameter.</param>
        /// <param name="culture">The application culture.</param>
        /// <returns>The number of photos in the provided PhotoGallery.</returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int photosInGallery = 0;
            PhotoGallery gallery = value as PhotoGallery;
            if (gallery != null)
            {
                foreach (PhotoAlbum album in gallery.PhotoAlbums)
                {
                    photosInGallery += album.Photos.Count;
                }
            }

            return photosInGallery;
        }

        /// <summary>
        /// Converts an integer number of photos into a gallery. Not implemented.
        /// </summary>
        /// <param name="value">The number of photos.</param>
        /// <param name="targetType">The target type of the conversion.</param>
        /// <param name="parameter">The converter parameter.</param>
        /// <param name="culture">The application culture.</param>
        /// <returns>Throws a NotImplementedException.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
