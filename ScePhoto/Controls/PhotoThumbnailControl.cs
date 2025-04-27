//-----------------------------------------------------------------------
// <copyright file="PhotoThumbnailControl.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     Control used to display a photo thumbnail in an album.
// </summary>
//-----------------------------------------------------------------------

namespace ScePhoto.Controls
{
    using System.Windows.Media;
    using ScePhoto.Data;

    /// <summary>
    /// Control used to display a photo thumbnail in an album.
    /// </summary>
    public class PhotoThumbnailControl : PhotoBaseControl
    {
        /// <summary>
        /// Constructor; adds a render transform for animating via XAML styles.
        /// </summary>
        public PhotoThumbnailControl()
        {
            ScaleTransform thumbnailScaleTransform = new ScaleTransform(1.0, 1.0);
            TransformGroup thumbnailTransformGroup = new TransformGroup();
            thumbnailTransformGroup.Children.Add(thumbnailScaleTransform);
            this.RenderTransform = thumbnailTransformGroup;
        }

        /// <summary>
        /// Updates the content of the control to contain the image at Photo.ThumbnailUri.
        /// </summary>
        protected override void OnUpdateContent()
        {
            Photo photo = Photo;
            if (photo != null && photo.ImageUri != null)
            {
                ImageDownloadInProgress = true;
                ServiceProvider.DataManager.GetImageSourceAsync(photo.ThumbnailUri, this);
            }
            else
            {
                ImageSource = null;
            }
        }
    }
}

