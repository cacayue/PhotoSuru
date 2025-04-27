//-----------------------------------------------------------------------
// <copyright file="ScePhotoViewerApplication.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     Code behind file for the ScePhotoViewer Application XAML.
// </summary>
//-----------------------------------------------------------------------

namespace ScePhotoViewer
{
    using System;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Interop;
    using System.Windows.Navigation;
    using System.Windows.Threading;
    using ScePhoto;
    using System.Windows.Media;

    /// <summary>
    /// Code behind file for the ScePhotoViewer Application XAML.
    /// </summary>
    public partial class ScePhotoViewerApplication : Application
    {
        #region Private Fields

        /// <summary>
        /// Indicates whether an update takes place after a cached load.
        /// </summary>
        private bool updateAfterCachedLoad;

        #endregion

        #region Public Properties
        
        /// <summary>
        /// Gets a value indicating whether the client is 
        /// Tier 2 capable which is required for hardware-accelerated
        /// Effects. 
        /// </summary>
        public static bool IsShaderEffectSupported
        {
            get
            {
                return RenderCapability.Tier == 0x00020000 && RenderCapability.IsPixelShaderVersionSupported(2, 0);
            }
        }
        
        #endregion

        #region Protected Methods

        /// <summary>
        /// On startup, perform initialization of services and settings, and register for appropriate notifications from
        /// ScePhoto services.
        /// </summary>
        /// <param name="e">Event arguments describing the event.</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            Initialize();
            ProcessCommandLineArgs();
            this.StartDataLoad();
            base.OnStartup(e);
        }

        /// <summary>
        /// On application exit, saves current settings to use for next instance. Shuts down subscription service manager so
        /// communication channels between the application and subscription sync service can be shut down and resources freed.
        /// </summary>
        /// <param name="e">Event arguments describing the event.</param>
        protected override void OnExit(ExitEventArgs e)
        {
            ScePhotoViewer.Properties.Settings.Default.Save();

            // Dispose resources associated with DataManager.
            SampleServiceProvider.PhotoDataManager.Dispose();

            // Shut down remoting services with subscription center
            SampleServiceProvider.SubscriptionServiceManager.Shutdown();

            base.OnExit(e);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Initializes settings and sample service provider. Initializes subscription service manager which initiates communication
        /// with the subscription sync service.
        /// </summary>
        private static void Initialize()
        {
            ScePhotoSettings.Initialize(new ScePhotoViewerSettings());
            ServiceProvider.Initialize(new SampleServiceProvider());

            // Initialize communication with subscription service
            SampleServiceProvider.SubscriptionServiceManager.Initialize();
        }

        /// <summary>
        /// Processes command line args passed to the application.
        /// </summary>
        private static void ProcessCommandLineArgs()
        {
            ServiceProvider.ViewManager.ProcessCommandLineArgs(SingleInstance.GetCommandLineArgs());
        }

        /// <summary>
        /// Starts data load from cache or from design data, if enabled.
        /// </summary>
        private void StartDataLoad()
        {
            // Begin by loading data from the cache
            // Initiate auto-sync after first data update is completed.
            // Upload log after any update is completed
            this.updateAfterCachedLoad = true;
            SampleServiceProvider.PhotoDataManager.LoadCachedDataCompleted += this.OnDataManagerLoadCachedDataCompleted;
            Dispatcher.BeginInvoke(DispatcherPriority.Background, new DispatcherOperationCallback(this.StartCacheLoadCallback), null);
        }

        /// <summary>
        /// Event handler for DataManager's LoadCachedDataCompleted event. After cached load completes, if the application is not
        /// displaying design mode data, a full data update is initiated.
        /// After the first cached load initiate automatic data update process.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event arguments describing the event.</param>
        private void OnDataManagerLoadCachedDataCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (this.updateAfterCachedLoad)
            {
                // On startup the applicaiton automatically loads from cache.  After the first cached load, 
                // a full sync is initiated to get most recent data. Subsequent cached loads should not repeat this
                this.updateAfterCachedLoad = false;
                this.InitiateUpdate();
            }
        }

        /// <summary>
        /// Queue dispatcher item to start data update process.
        /// </summary>
        private void InitiateUpdate()
        {
            Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new DispatcherOperationCallback(this.StartDataUpdateCallback), null);
        }

        /// <summary>
        /// Callback worker that initiates async data update.
        /// </summary>
        /// <param name="arg">The callback argument.</param>
        /// <returns>Always null.</returns>
        private object StartDataUpdateCallback(object arg)
        {
            // Execute sample's start sync command, which checks that no subscription service update is in progress before starting
            // data update
            if (((SampleViewManager)SampleServiceProvider.ViewManager).SampleCommands.SampleStartSyncCommand.CanExecute(null))
            {
                ((SampleViewManager)SampleServiceProvider.ViewManager).SampleCommands.SampleStartSyncCommand.Execute(null);
            }

            return null;
        }

        /// <summary>
        /// Callback worker that starts the async data load from cache.
        /// </summary>
        /// <param name="arg">The callback argument.</param>
        /// <returns>Always null.</returns>
        private object StartCacheLoadCallback(object arg)
        {
            SampleServiceProvider.PhotoDataManager.LoadCachedDataAsync();
            return null;
        }

        #endregion       
    }
}