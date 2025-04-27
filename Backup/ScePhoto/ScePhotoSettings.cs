//-----------------------------------------------------------------------
// <copyright file="ScePhotoSettings.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     The ScePhotoSettings class provides an abstract base class for 
//     specifying settings information to ScePhoto services.
// </summary>
//-----------------------------------------------------------------------

namespace ScePhoto
{
    using System;
    using System.IO;

    /// <summary>
    /// Abstract base class for specifying settings information to ScePhoto services,
    /// so that services can easily access it and UI can bind to it in XAML.
    /// </summary>
    public abstract class ScePhotoSettings
    {
        /// <summary>
        /// The current instance of the application’s settings.
        /// </summary>
        private static ScePhotoSettings instance;

        /// <summary>
        /// The path to the root folder of the application’s local cache.
        /// </summary>
        private string localCacheFolder;

        /// <summary>
        /// The path to the application's log file.
        /// </summary>
        private string logFilePath;

        /// <summary>
        /// Gets the current instance of the application’s settings.
        /// </summary>
        public static ScePhotoSettings Instance
        {
            get
            {
                if (instance == null)
                {
                    throw new InvalidOperationException(Strings.SettingsNotInitialized);
                }

                return instance;
            }
        }

        /// <summary>
        /// Gets the name of the application.
        /// </summary>
        public static string ApplicationName
        {
            get { return Instance.ApplicationNameCore; }
        }

        /// <summary>
        /// Gets the name of the company.
        /// </summary>
        public static string CompanyName
        {
            get { return Instance.CompanyNameCore; }
        }

        /// <summary>
        /// Gets the URI of the data feed.
        /// </summary>
        public static Uri DataFeedUri
        {
            get { return Instance.DataFeedUriCore; }
        }

        /// <summary>
        /// Gets the path to the root folder of the application’s local cache.
        /// </summary>
        public static string LocalCacheFolder
        {
            get { return Instance.LocalCacheFolderCore; }
        }

        /// <summary>
        /// Gets the path to the application's log file.
        /// </summary>
        public static string LogFilePath
        {
            get { return Instance.LogFilePathCore; }
        }

        /// <summary>
        /// Gets the name of the application.
        /// </summary>
        protected abstract string ApplicationNameCore
        {
            get;
        }

        /// <summary>
        /// Gets the name of the company.
        /// </summary>
        protected abstract string CompanyNameCore
        {
            get;
        }

        /// <summary>
        /// Gets the URI of the data feed.
        /// </summary>
        protected abstract Uri DataFeedUriCore
        {
            get;
        }

        /// <summary>
        /// Gets or sets the path to the root folder of the application’s local cache.
        /// </summary>
        protected string LocalCacheFolderCore
        {
            get { return this.localCacheFolder; }
            set { this.localCacheFolder = value; }
        }

        /// <summary>
        /// Gets or sets the path to the application's log file.
        /// </summary>
        protected string LogFilePathCore
        {
            get { return this.logFilePath; }
            set { this.logFilePath = value; }
        }

        /// <summary>
        /// Initialization using customized ScePhotoSettings.
        /// </summary>
        /// <param name="newInstance">Explicit instance of ScePhotoSettings.</param>
        public static void Initialize(ScePhotoSettings newInstance)
        {
            if (newInstance == null)
            {
                throw new ArgumentNullException("newInstance");
            }

            instance = newInstance;
            instance.Initialize();
        }

        /// <summary>
        /// Initializes settings values.
        /// </summary>
        protected virtual void Initialize()
        {
            string applicationName = this.ApplicationNameCore;
            string companyName = this.CompanyNameCore;
            string localAppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), companyName);
            localAppDataPath = Path.Combine(localAppDataPath, applicationName);

            // Initialize local cache folder.
            this.localCacheFolder = Path.Combine(localAppDataPath, "Cache");
            if (!Directory.Exists(this.localCacheFolder))
            {
                Directory.CreateDirectory(this.localCacheFolder);
            }

            this.logFilePath = Path.Combine(localAppDataPath, "log.txt");
        }
    }
}
