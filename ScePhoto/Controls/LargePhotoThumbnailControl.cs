//-----------------------------------------------------------------------
// <copyright file="LargePhotoThumbnailControl.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     Control used to display a photo thumbnail in an album using the full-size image.
// </summary>
//-----------------------------------------------------------------------

namespace ScePhoto.Controls
{
    using System.Windows.Media;
    using ScePhoto.Data;

    /// <summary>
    /// Control used to display a photo thumbnail in an album.
    /// </summary>
    public class LargePhotoThumbnailControl : PhotoThumbnailControl
    {
        /// <summary>
        /// Updates the content of the control to contain the image at Photo.ImageUri.
        /// </summary>
        protected override void OnUpdateContent()
        {
            Photo photo = Photo;
            if (photo != null && photo.ImageUri != null)
            {
                ImageDownloadInProgress = true;
                ServiceProvider.DataManager.GetImageSourceAsync(photo.ImageUri, this);
            }
            else
            {
                ImageSource = null;
            }
        }
    }
}

