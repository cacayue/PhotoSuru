//-----------------------------------------------------------------------
// <copyright file="SimplePhotoViewerControl.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     Control used to display only a single full photo.
// </summary>
//-----------------------------------------------------------------------

namespace ScePhotoViewer
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Animation;
    using System.Windows.Threading;
    using EffectControls;
    using EffectLibrary;
    using ScePhoto;
    using ScePhoto.Controls;
    using ScePhoto.Data;
    using ScePhoto.Feed;

    /// <summary>
    /// Control used to display only a single full photo.
    /// </summary>
    public class SimplePhotoViewerControl : LargePhotoThumbnailControl
    {
        #region Constructor

        /// <summary>
        /// SimplePhotoViewerControl constructor
        /// </summary>
        public SimplePhotoViewerControl()
        {
            // force the control to load even if it's not in the visual tree.
            base.OnLoaded();
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Virtual handler for Loaded event.
        /// </summary>
        protected override void OnLoaded()
        {
            // No-op without calling base's virtual.
        }

        /// <summary>
        /// Virtual handler for UnLoaded event.
        /// </summary>
        protected override void OnUnloaded()
        {
            // No-op without calling base's virtual
        }

        #endregion
    }
}
