//-----------------------------------------------------------------------
// <copyright file="PhotoBaseControl.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     Base class for controls that display images.
// </summary>
//-----------------------------------------------------------------------

namespace ScePhoto.Controls
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Threading;
    using ScePhoto.Data;
    using ScePhoto.Feed;

    /// <summary>
    /// Base class for controls that display images.  Performs an asynchronous postback to get the image
    /// so as not to block the UI thread.
    /// </summary>
    public abstract class PhotoBaseControl : Control
    {
        #region Fields
        /// <summary>
        /// Dependency Property backing store for Photo.
        /// </summary>
        public static readonly DependencyProperty PhotoProperty =
            DependencyProperty.Register("Photo", typeof(Photo), typeof(PhotoBaseControl), new UIPropertyMetadata(OnPhotoChanged));

        /// <summary>
        /// Dependency Property backing store for ImageSource.
        /// </summary>
        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register("ImageSource", typeof(ImageSource), typeof(PhotoBaseControl), new UIPropertyMetadata(null));

        /// <summary>
        /// Indicates whether an image download is in progress.
        /// </summary>
        private bool imageDownloadInProgress;

        /// <summary>
        /// Indicates whether a content update is already pending.
        /// </summary>
        private bool contentUpdatePending;
        #endregion

        #region Properties
        /// <summary>
        /// Gets a value indicating whether an image download is in progress.
        /// </summary>
        public bool ImageDownloadInProgress
        {
            get { return this.imageDownloadInProgress; }
            protected set { this.imageDownloadInProgress = value; }
        }

        /// <summary>
        /// Gets or sets the photo to display.
        /// </summary>
        public Photo Photo
        {
            get { return (Photo)GetValue(PhotoProperty); }
            set { SetValue(PhotoProperty, value); }
        }

        /// <summary>
        /// Gets the actual image content to display.
        /// </summary>
        public ImageSource ImageSource
        {
            get { return (ImageSource)GetValue(ImageSourceProperty); }
            protected set { SetValue(ImageSourceProperty, value); }
        } 
        #endregion

        #region Protected Methods
        /// <summary>
        /// Starts the asynchronous update progress.  Needs to be overriden by child classes.
        /// </summary>
        protected abstract void OnUpdateContent();

        /// <summary>
        /// Invalidates the content of the control and starts an asynchronous content update.
        /// </summary>
        protected virtual void InvalidateContent()
        {
            if (!this.contentUpdatePending)
            {
                if (this.imageDownloadInProgress)
                {
                    ServiceProvider.DataManager.CancelAsync(this);
                }

                this.contentUpdatePending = true;
                Dispatcher.BeginInvoke(DispatcherPriority.Input, new DispatcherOperationCallback(this.UpdateContent), null);
            }
        }

        /// <summary>
        /// Sets the ImageSource of the control as soon as the asynchronous get is completed.
        /// </summary>
        /// <param name="e">Arguments describing the event.</param>
        protected virtual void OnGetImageSourceCompleted(GetImageSourceCompletedEventArgs e)
        {
            this.imageDownloadInProgress = false;
            if (e.Error == null && !e.Cancelled)
            {
                this.ImageSource = e.ImageSource;
            }
            else
            {
                this.ImageSource = null;
            }
        }

        /// <summary>
        /// Attaches the OnGetImageSourceCompleted handler on control load.
        /// </summary>
        protected virtual void OnLoaded()
        {
            ServiceProvider.DataManager.GetImageSourceCompleted += this.OnGetImageSourceCompleted;
        }

        /// <summary>
        /// Detaches the OnGetImageSourceCompleted handler on control load.
        /// </summary>
        protected virtual void OnUnloaded()
        {
            ServiceProvider.DataManager.GetImageSourceCompleted -= this.OnGetImageSourceCompleted;
        }

        /// <summary>
        /// Adds the OnLoaded and OnUnloaded handlers to the appropriate events when the control is initialized.
        /// </summary>
        /// <param name="e">Arguments describing the event.</param>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            this.Loaded += new RoutedEventHandler(this.OnLoaded);
            this.Unloaded += new RoutedEventHandler(this.OnUnloaded);
        } 
        #endregion

        #region Private Methods
        /// <summary>
        /// Invalidates the currently displayed content when the photo is changed.
        /// </summary>
        /// <param name="element">Source of the event.</param>
        /// <param name="e">Arguments describing the event.</param>
        private static void OnPhotoChanged(DependencyObject element, DependencyPropertyChangedEventArgs e)
        {
            ((PhotoBaseControl)element).InvalidateContent();
        }

        /// <summary>
        /// Updates the control content.
        /// </summary>
        /// <param name="arg">Callback argument.</param>
        /// <returns>Always null.</returns>
        private object UpdateContent(object arg)
        {
            this.contentUpdatePending = false;
            this.OnUpdateContent();
            return null;
        }

        /// <summary>
        /// If the async load was requested by this control, updates the image displayed.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Arguments describing the event.</param>
        private void OnGetImageSourceCompleted(object sender, GetImageSourceCompletedEventArgs e)
        {
            if (e.UserState == this)
            {
                this.OnGetImageSourceCompleted(e);
            }
        }

        /// <summary>
        /// Passes the Loaded event through to the parameterless OnLoaded function.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Arguments describing the event.</param>
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.OnLoaded();
        }

        /// <summary>
        /// Passes the Unloaded event through to the parameterless OnUnloaded function.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Arguments describing the event.</param>
        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            this.OnUnloaded();
        } 
        #endregion
    }
}
