//-----------------------------------------------------------------------
// <copyright file="PhotoViewerHyperlink.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     PhotoViewer hyperlink handles navigate request and opens external
//     browser window.
// </summary>
//-----------------------------------------------------------------------

namespace ScePhotoViewer
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Windows.Documents;
    using System.Windows;
    using System.Windows.Threading;
    using System.Windows.Navigation;

    /// <summary>
    /// PhotoViewer hyperlink handles navigate request and opens external browser window.
    /// </summary>
    public class PhotoViewerHyperlink : Hyperlink
    {
        #region Methods

        /// <summary>
        /// PhotoViewerHyperlink Constructor.
        /// </summary>
        public PhotoViewerHyperlink()
            : base()
        {
            this.RequestNavigate += new System.Windows.Navigation.RequestNavigateEventHandler(this.PhotoViewerHyperlink_RequestNavigate);
        }

        /// <summary>
        /// Handles a navigate request and opens external browser window.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Arguments describing the event.</param>
        private void PhotoViewerHyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            PhotoViewerHyperlink link = sender as PhotoViewerHyperlink;
            if (link != null)
            {
                System.Diagnostics.Process.Start(e.Uri.ToString());
                e.Handled = true;
            }
        }

        #endregion
    }
}
