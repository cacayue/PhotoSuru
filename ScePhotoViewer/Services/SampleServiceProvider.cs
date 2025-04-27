//-----------------------------------------------------------------------
// <copyright file="SampleServiceProvider.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     Sample application service provider.
// </summary>
//-----------------------------------------------------------------------

namespace ScePhotoViewer
{
    using ScePhoto;

    /// <summary>
    /// Sample service provider.
    /// </summary>
    public class SampleServiceProvider : ServiceProvider
    {
        #region Private Fields

        /// <summary>
        /// The subscription manager.
        /// </summary>
        private SubscriptionServiceManager subscriptionServiceManager;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the SubscriptionServiceManager which manages connections and synchronization with the subscription service.
        /// </summary>
        public static SubscriptionServiceManager SubscriptionServiceManager
        {
            get
            {
                if (((SampleServiceProvider)Instance).SubscriptionServiceManagerInternal == null)
                {
                    ((SampleServiceProvider)Instance).SubscriptionServiceManagerInternal = new SubscriptionServiceManager();
                }

                return ((SampleServiceProvider)Instance).SubscriptionServiceManagerInternal;
            }
        }

        /// <summary>
        /// Gets the DataManager that stores the application content.
        /// </summary>
        public static ScePhoto.Data.DataManager PhotoDataManager
        {
            get
            {
                return ScePhoto.ServiceProvider.DataManager;
            }
        }

        #endregion

        #region Private Properties

        /// <summary>
        /// Gets or sets the SubscriptionServiceManager which manages connections and synchronization with the subscription service.
        /// </summary>
        private SubscriptionServiceManager SubscriptionServiceManagerInternal
        {
            get { return this.subscriptionServiceManager; }
            set { this.subscriptionServiceManager = value; }
        }

        #endregion

        /// <summary>
        /// Initializes the ServiceProvider and SampleViewManager.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.ViewManagerCore = new SampleViewManager();
        }
    }
}
