//-----------------------------------------------------------------------
// <copyright file="HomePhotoAlbum.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     Dummy photo album used for navigation to the photo gallery home view.
// </summary>
//-----------------------------------------------------------------------

namespace ScePhoto.Data
{
    /// <summary>
    /// Dummy photo album used for navigation to the photo gallery home view.
    /// </summary>
    public class HomePhotoAlbum : PhotoAlbum
    {
        /// <summary>
        /// HomePhotoAlbum constructor, calls the base PhotoAlbum constructor.
        /// </summary>
        /// <param name="guid">Unique identifier for this album.</param>
        public HomePhotoAlbum(string guid)
            : base(guid)
        {
        }
    }
}
