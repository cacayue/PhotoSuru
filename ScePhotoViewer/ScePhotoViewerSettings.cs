//-----------------------------------------------------------------------
// <copyright file="ScePhotoViewerSettings.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     Settings class for the ScePhotoViewer.
// </summary>
//-----------------------------------------------------------------------

namespace ScePhotoViewer
{
    using System;
    using System.Windows;
    using ScePhoto;

    /// <summary>
    /// Settings class for the ScePhotoViewer.
    /// </summary>
    public class ScePhotoViewerSettings : ScePhotoSettings
    {
        /// <summary>
        /// The URI of the data feed.
        /// </summary>
        private Uri dataFeedUri;

        /// <summary>
        /// Gets the name of the application.
        /// </summary>
        protected override string ApplicationNameCore
        {
            get { return Properties.Settings.Default.ApplicationName; }
        }

        /// <summary>
        /// Gets the name of the company.
        /// </summary>
        protected override string CompanyNameCore
        {
            get { return Properties.Settings.Default.CompanyName; }
        }

        /// <summary>
        /// Gets the URI of the data feed.
        /// </summary>
        protected override Uri DataFeedUriCore
        {
            get { return this.dataFeedUri; }
        }

        /// <summary>
        /// Initializes settings values.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.dataFeedUri = new Uri(Properties.Settings.Default.DataFeedUri);
        }
    }
    
    /// <summary>
    /// Sample settings for the ScePhotoViewer.
    /// </summary>
    public class SampleScePhotoSettings : ScePhotoSettings
    {
        #region Private Fields

        /// <summary>
        /// The URI of the data feed.
        /// </summary>
        private Uri dataFeedUri;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets a value indicating whether transitions are enabled.
        /// </summary>
        public static bool TransitionsEnabled
        {
            get { return Properties.Settings.Default.TransitionsEnabled; }
            set { Properties.Settings.Default.TransitionsEnabled = value; }
        }

        /// <summary>
        /// Gets the URI for the location to upload log files.
        /// </summary>
        public static Uri LogUploadUri
        {
            get { return Properties.Settings.Default.LogUploadUri; }
        }

        /// <summary>
        /// Gets the frequency to update local cached data.
        /// </summary>
        public static TimeSpan DataUpdateFrequency
        {
            get { return Properties.Settings.Default.DataUpdateFrequency; }
        }

        /// <summary>
        /// Gets the name of the channel exposed by subscription service, which applications must connect to to communicate 
        /// with the service.
        /// </summary>
        public static string SubscriptionServiceChannelName
        {
            get { return Properties.Settings.Default.SubscriptionServiceUri; }
        }

        /// <summary>
        /// Gets the name of the IPC channel port used by this application to receive event notifications from the subscription service.
        /// </summary>
        public static string ChannelPortName
        {
            get { return Properties.Settings.Default.ChannelPortName; }
        }

        /// <summary>
        /// Gets the name of system-wide event that is used by the subscription service to signal channel activation for this user.
        /// Once this event has been signaled, communication with the service may proceed. 
        /// Subscription service events are named by concatenating the service prefix with the current user name.
        /// </summary>
        public static string SubscriptionServiceSignalName
        {
            get { return String.Concat(Properties.Settings.Default.SubscriptionServiceSignalPrefix, Environment.UserName); }
        }

        /// <summary>
        /// Gets a value indicating whether the application consumes navigation journal commands Back and Forward, i.e., the 
        /// BrowseBack and BrowseForward navigation commands behave as they do in the browser, to navigate through journaled history.
        /// <remarks>If set to false, BrowseBack and BrowseForward are used by the application to navigate according to it's own next/previous ordering.</remarks>
        /// </summary>
        public static bool EnableJournalNavigationOnInput
        {
            get { return Properties.Settings.Default.EnableJournalNavigationOnInput; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether size and lcoation of main window should be persisted across instances of the application.
        /// </summary>
        public static bool SaveMainWindowBounds
        {
            get { return Properties.Settings.Default.SaveMainWindowBounds; }
            set { Properties.Settings.Default.SaveMainWindowBounds = value; }
        }

        /// <summary>
        /// Gets or sets the rect with persisted values of MainWindow bounds, so position and size of the MainWindow can be remembered across instances.
        /// </summary>
        public static Rect MainWindowBounds
        {
            get { return Properties.Settings.Default.MainWindowBounds; }
            set { Properties.Settings.Default.MainWindowBounds = value; }
        }

        /// <summary>
        /// Gets or sets the persisted value of WindowState to be restored in subsequent instances.
        /// </summary>
        public static WindowState MainWindowState
        {
            get { return Properties.Settings.Default.MainWindowState; }
            set { Properties.Settings.Default.MainWindowState = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the photo viewer should overlay the description or not.
        /// </summary>
        public static bool PhotoViewerShowsDescription
        {
            get { return Properties.Settings.Default.PhotoViewerShowsDescription; }
            set { Properties.Settings.Default.PhotoViewerShowsDescription = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the gallery home is shown in list view or not.
        /// </summary>
        public static bool GalleryHomeInListView
        {
            get { return Properties.Settings.Default.GalleryHomeInListView; }
            set { Properties.Settings.Default.GalleryHomeInListView = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the photo flow viewer should overlay the description or not.
        /// </summary>
        public static bool PhotoViewerShowsFlowDescription
        {
            get { return Properties.Settings.Default.PhotoViewerShowsFlowDescription; }
            set { Properties.Settings.Default.PhotoViewerShowsFlowDescription = value; }
        }

        #endregion

        #region Protected Properties

        /// <summary>
        /// Gets the name of the application.
        /// </summary>
        protected override string ApplicationNameCore
        {
            get { return Properties.Settings.Default.ApplicationName; }
        }

        /// <summary>
        /// Gets the name of the company.
        /// </summary>
        protected override string CompanyNameCore
        {
            get { return Properties.Settings.Default.CompanyName; }
        }

        /// <summary>
        /// Gets the URI of the data feed.
        /// </summary>
        protected override Uri DataFeedUriCore
        {
            get { return this.dataFeedUri; }
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Initializes settings values.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            this.dataFeedUri = new Uri(Properties.Settings.Default.DataFeedUri);
        }

        #endregion
    }
}
