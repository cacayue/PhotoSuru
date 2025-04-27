//-----------------------------------------------------------------------
// <copyright file="PhotoFlowDescriptionViewer.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Subclass of page viewer that loads/saves it's zoom from / to 
//      application settings
// </summary>
//-----------------------------------------------------------------------

namespace ScePhotoViewer
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Windows.Controls;
    using System.Windows;

    /// <summary>
    /// Subclass of page viewer that loads/saves it's zoom from / to application settings
    /// </summary>
    public class PhotoFlowDescriptionViewer : FlowDocumentPageViewer
    {
        /// <summary>
        /// Static constructor overrides Zoom property metadata to add callback for Zoom changes
        /// </summary>
        static PhotoFlowDescriptionViewer()
        {
            ZoomProperty.OverrideMetadata(typeof(PhotoFlowDescriptionViewer), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnZoomChanged)));
        }

        /// <summary>
        /// Called when the element is initialized.
        /// </summary>
        /// <param name="e">Arguments of the event.</param>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            Zoom = Properties.Settings.Default.PhotoFlowDescriptionZoom;
        }

        /// <summary>
        /// Zoom changed callback worker
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments describing the event.</param>
        private static void OnZoomChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            PhotoFlowDescriptionViewer viewer = sender as PhotoFlowDescriptionViewer;
            viewer.OnZoomChanged();
        }

        /// <summary>
        /// Virtual handler for ZoomChanged
        /// </summary>
        private void OnZoomChanged()
        {
            Properties.Settings.Default.PhotoFlowDescriptionZoom = this.Zoom;
        }
    }
}
