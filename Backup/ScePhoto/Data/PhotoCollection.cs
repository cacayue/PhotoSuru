//-----------------------------------------------------------------------
// <copyright file="PhotoCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     Read-only collection of Photo objects.
// </summary>
//-----------------------------------------------------------------------

namespace ScePhoto.Data
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Read-only collection of Photo objects.
    /// </summary>
    public class PhotoCollection : DataObjectCollection<Photo>
    {
        /// <summary>
        /// Initializes the PhotoCollection instance.
        /// </summary>
        public PhotoCollection()
        {
        }

        /// <summary>
        /// Initializes the PhotoCollection instance.
        /// </summary>
        /// <param name="photos">Collection of photos used to initialize this collection.</param>
        public PhotoCollection(IList<Photo> photos)
            : base(photos)
        {
        }
    }
}
