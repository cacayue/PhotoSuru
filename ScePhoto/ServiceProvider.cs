//-----------------------------------------------------------------------
// <copyright file="ServiceProvider.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     The ServiceProvider class hosts and provides access to all 
//     ScePhoto services.
// </summary>
//-----------------------------------------------------------------------

namespace ScePhoto
{
    using System;
    using Microsoft.SubscriptionCenter.Sync;
    using ScePhoto.Data;
    using ScePhoto.Feed;
    using ScePhoto.View;
    using System.Windows.Threading;

    /// <summary>
    /// Hosts and provides access to all ScePhoto services.
    /// </summary>
    public class ServiceProvider : IDisposable
    {
        #region Fields
        /// <summary>
        /// The current instance of the ServiceProvider.
        /// </summary>
        private static ServiceProvider instance;

        /// <summary>
        /// The DataManager that centralizes management of data represented by data feed.
        /// </summary>
        private DataManager dataManager;

        /// <summary>
        /// The DataFeedSource providing access to data associated with data feeds.
        /// </summary>
        private DataFeedSource dataFeedSource;

        /// <summary>
        /// The Logger that provides error, warning and information logging for the application.
        /// </summary>
        private ScePhotoLogger logger;

        /// <summary>
        /// The WebCredentials that handles the authentication process required to retrieve data from remote location.
        /// </summary>
        private WebCredentials webCredentials;

        /// <summary>
        /// The PhotoFeedConverter that handles CSX RSS document conversion to photo data objects.
        /// </summary>
        private PhotoFeedConverter photoFeedConverter;

        /// <summary>
        /// The ViewManager that provides services to the UI layer.
        /// </summary>
        private ViewManager viewManager;

        /// <summary>
        /// A value indicating whether this type has been disposed or not.
        /// </summary>
        private bool disposed;
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes the instance of ServiceProvider.
        /// </summary>
        /// <remarks>Protected to prevent explicit instantiation.</remarks>
        protected ServiceProvider()
        {
        } 
        #endregion

        #region Destructor
        /// <summary>
        /// Destructor, for completness in the IDisposable implementation.
        /// </summary>
        ~ServiceProvider()
        {
            this.Dispose(false);
        } 
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets the current instance of the ServiceProvider.
        /// </summary>
        public static ServiceProvider Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ServiceProvider();
                    instance.Initialize();
                }

                return instance;
            }
        }

        /// <summary>
        /// Gets the DataManager that centralizes management of data represented by data feed.
        /// </summary>
        public static DataManager DataManager
        {
            get { return Instance.DataManagerCore; }
        }

        /// <summary>
        /// Gets the DataFeedSource providing access to data associated with data feeds.
        /// </summary>
        public static DataFeedSource DataFeedSource
        {
            get { return Instance.DataFeedSourceCore; }
        }

        /// <summary>
        /// Gets the Logger that provides error, warning and information logging for the application.
        /// </summary>
        public static ScePhotoLogger Logger
        {
            get { return Instance.LoggerCore; }
        }

        /// <summary>
        /// Gets the WebCredentials object that handles the authentication process required to retrieve data from remote location.
        /// </summary>
        public static WebCredentials WebCredentials
        {
            get { return Instance.WebCredentialsCore; }
        }

        /// <summary>
        /// Gets the PhotoFeedConverter that handles CSX RSS document conversion to photo data objects.
        /// </summary>
        public static PhotoFeedConverter PhotoFeedConverter
        {
            get { return Instance.PhotoFeedConverterCore; }
        }

        /// <summary>
        /// Gets the ViewManager that provides services to the UI layer.
        /// </summary>
        public static ViewManager ViewManager
        {
            get { return Instance.ViewManagerCore; }
        } 
        #endregion

        #region Protected Properties
        /// <summary>
        /// Gets or sets the DataManager that centralizes management of data represented by data feed.
        /// </summary>
        protected DataManager DataManagerCore
        {
            get { return this.dataManager; }
            set { this.dataManager = value; }
        }

        /// <summary>
        /// Gets or sets the DataFeedSource providing access to data associated with data feeds.
        /// </summary>
        protected DataFeedSource DataFeedSourceCore
        {
            get { return this.dataFeedSource; }
            set { this.dataFeedSource = value; }
        }

        /// <summary>
        /// Gets or sets the Logger that provides error, warning and information logging for the application.
        /// </summary>
        protected ScePhotoLogger LoggerCore
        {
            get { return this.logger; }
            set { this.logger = value; }
        }

        /// <summary>
        /// Gets or sets the WebCredentials object that handles the authentication process required to retrieve data from remote location.
        /// </summary>
        protected WebCredentials WebCredentialsCore
        {
            get { return this.webCredentials; }
            set { this.webCredentials = value; }
        }

        /// <summary>
        /// Gets or sets the PhotoFeedConverter that handles CSX RSS document conversion to photo data objects.
        /// </summary>
        protected PhotoFeedConverter PhotoFeedConverterCore
        {
            get { return this.photoFeedConverter; }
            set { this.photoFeedConverter = value; }
        }

        /// <summary>
        /// Gets or sets the ViewManager that provides services to the UI layer.
        /// </summary>
        protected ViewManager ViewManagerCore
        {
            get { return this.viewManager; }
            set { this.viewManager = value; }
        } 
        #endregion

        #region Methods
        /// <summary>
        /// Initialization using customized ServiceProvider.
        /// </summary>
        /// <param name="newInstance">Explicit instance of ServiceProvider.</param>
        public static void Initialize(ServiceProvider newInstance)
        {
            if (newInstance == null)
            {
                throw new ArgumentNullException("newInstance");
            }

            instance = newInstance;
            instance.Initialize();
        }

        /// <summary>
        /// Shuts down the service provider.
        /// </summary>
        public static void Shutdown()
        {
            instance.Dispose();
        }

        /// <summary>
        /// Implements IDisposable to dispose of the IDisposable types it creates.
        /// <remarks>Since this object has already done its own disposal, it no longer needs to be finalized by the framework.</remarks>
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Initializes settings values.
        /// </summary>
        protected virtual void Initialize()
        {
            if (!this.disposed)
            {
                this.logger = new ScePhotoLogger();
                this.dataFeedSource = new LocalDataFeedSource(Dispatcher.CurrentDispatcher);
                this.dataManager = new DataManager(Dispatcher.CurrentDispatcher);
                this.webCredentials = null;
                this.photoFeedConverter = new PhotoFeedConverter();
                this.viewManager = new ViewManager();
            }
            else
            {
                throw new ObjectDisposedException("ServiceProvider");
            }
        }

        /// <summary>
        /// Implements IDisposable to dispose of the IDisposable types the ServiceProvider creates.
        /// </summary>
        /// <remarks>Dispose(bool disposing) is called in two situations.  When disposing is true, it has been called from
        /// user code and can dispose of other managed objects; when false, it has been called automatically from the runtime
        /// finalizer and other managed resources may or may not have already been finalized.</remarks>
        /// <param name="disposing">Whether this method is being called by user code.</param>
        private void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    this.dataManager.Dispose();
                    IDisposable disposable = this.dataFeedSource as IDisposable;
                    if (disposable != null)
                    {
                        disposable.Dispose();
                    }
                }

                this.disposed = true;
            }
        }
        #endregion
    }
}
