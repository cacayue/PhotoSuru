//-----------------------------------------------------------------------
// <copyright file="DataManager.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     Manages the data used by the application and provides methods to retrieve and synchronize data.
// </summary>
//-----------------------------------------------------------------------

namespace ScePhoto.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.Threading;
    using System.Windows.Threading;
    using Microsoft.SubscriptionCenter.Sync;
    using ScePhoto.Feed;

    /// <summary>
    /// Manages the data used by the application and provides methods to retrieve and synchronize data.
    /// </summary>
    public sealed class DataManager : IDisposable
    {
        #region Fields

        /// <summary>
        /// The dispatcher associated with the UI thread.
        /// </summary>
        private readonly Dispatcher dispatcher;

        /// <summary>
        /// A pool of low priority threads that can be used to post work items.
        /// </summary>
        private readonly WorkerThreadPool threadPool;

        /// <summary>
        /// Locally cached MasterFeedContent.
        /// </summary>
        private readonly MasterFeedContent masterFeedContent;

        /// <summary>
        /// Collection of pending asynchronous requests.
        /// </summary>
        private readonly IList<DataRequest> pendingDataRequests;

        /// <summary>
        /// Holds state associated with update session.
        /// </summary>
        private UpdateSession updateSession;

        /// <summary>
        /// The date and time of the last update.
        /// </summary>
        private DateTime lastUpdateDate;

        /// <summary>
        /// Flag indicating whether or not the last update process was successfully completed.
        /// </summary>
        private bool successfulUpdate;

        /// <summary>
        /// The dictionary of tags used to tag photos in the application.
        /// </summary>
        private TagStore tagStore;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes the instance of DataManager.
        /// </summary>
        /// <param name="dispatcher">The dispatcher associated with the UI thread.</param>
        public DataManager(Dispatcher dispatcher)
        {
            if (dispatcher == null)
            {
                throw new ArgumentNullException("dispatcher");
            }

            this.dispatcher = dispatcher;
            this.threadPool = new WorkerThreadPool(ServiceProvider.Logger, 0, 10, ThreadPriority.Lowest);
            this.lastUpdateDate = DateTime.MinValue;
            this.successfulUpdate = true;
            this.masterFeedContent = new MasterFeedContent();
            this.masterFeedContent.BaseUri = ScePhotoSettings.DataFeedUri;
            this.pendingDataRequests = new List<DataRequest>();
            this.tagStore = new TagStore();

            // Schedule data initialization process. 
            DataFeedSource dataFeedSource = ServiceProvider.DataFeedSource;
            dataFeedSource.GetXmlDocumentCompleted += new EventHandler<GetXmlDocumentCompletedEventArgs>(this.DataFeedSource_GetXmlDocumentCompleted);
            dataFeedSource.GetImageSourceCompleted += new EventHandler<GetImageSourceCompletedEventArgs>(this.DataFeedSource_GetImageSourceCompleted);
            dataFeedSource.GetTextDocumentCompleted += new EventHandler<GetTextDocumentCompletedEventArgs>(this.DataFeedSource_GetTextDocumentCompleted);
        }

        #endregion

        #region Events

        /// <summary>
        /// Event raised on successful completion, an error, or a cancellation of data load from cache.
        /// </summary>
        public event EventHandler<AsyncCompletedEventArgs> LoadCachedDataCompleted;

        /// <summary>
        /// Event raised on start of data load from cache.
        /// </summary>
        public event EventHandler<EventArgs> LoadCachedDataStarted;

        /// <summary>
        /// Event raised on successful completion, an error, or a cancellation of data update process.
        /// </summary>
        public event EventHandler<AsyncCompletedEventArgs> UpdateCompleted;

        /// <summary>
        /// Event raised on start of data update process.
        /// </summary>
        public event EventHandler<EventArgs> UpdateStarted;

        /// <summary>
        /// Event raised to track progress of data update process.
        /// </summary>
        public event EventHandler<ProgressChangedEventArgs> UpdateProgressChanged;

        /// <summary>
        /// Event raised during update process when all feeds are updated.
        /// </summary>
        public event EventHandler<EventArgs> FeedsUpdated;

        /// <summary>
        /// Event raised on successful completion, an error, or a cancellation of GetImageSource request.
        /// </summary>
        public event EventHandler<GetImageSourceCompletedEventArgs> GetImageSourceCompleted;

        /// <summary>
        /// Event raised on successful completion, an error, or a cancellation of GetTextDocument request.
        /// </summary>
        public event EventHandler<GetTextDocumentCompletedEventArgs> GetTextDocumentCompleted;

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether or not update process is in progress.
        /// </summary>
        public bool IsUpdateInProgress
        {
            get { return (this.updateSession != null); }
        }

        /// <summary>
        /// Gets the date and time of the last update.
        /// </summary>
        public DateTime LastUpdateDate
        {
            get { return this.lastUpdateDate; }
        }

        /// <summary>
        /// Gets the collection of cached PhotoGalleries.
        /// </summary>
        public PhotoGalleryCollection PhotoGalleries
        {
            get { return this.masterFeedContent.PhotoGalleries; }
        }

        /// <summary>
        /// Gets the dictionary of tags used to tag photos in the application.
        /// </summary>
        public TagStore TagStore
        {
            get { return this.tagStore; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Loads data from cache, if local cache exists. If not, this method does nothing.
        /// </summary>
        public void LoadCachedDataAsync()
        {
            // Multiple update processes are not supported.
            if (this.IsUpdateInProgress)
            {
                throw new InvalidOperationException(Strings.DataManagerConcurrentUpdateNotSupported);
            }

            this.updateSession = new UpdateSession(true);
            ServiceProvider.Logger.Information(Strings.DataManagerLoadFromCacheStarted);
            this.OnLoadCachedDataStarted(EventArgs.Empty);

            // Make asynchronous request to get MasterFeedContent object.
            this.GetFeedDocumentAsync(new DataRequest(this.masterFeedContent, this.masterFeedContent.BaseUri, this));
        }

        /// <summary>
        /// Start asynchronous data update process. 
        /// </summary>
        public void UpdateAsync()
        {
            // Multiple update processes are not supported.
            if (this.IsUpdateInProgress)
            {
                throw new InvalidOperationException(Strings.DataManagerConcurrentUpdateNotSupported);
            }

            // Start the update process.
            this.updateSession = new UpdateSession(false);
            ServiceProvider.Logger.Information(Strings.DataManagerUpdateStarted);
            this.OnUpdateStarted(EventArgs.Empty);

            LocalDataFeedSource localDataFeedSource = ServiceProvider.DataFeedSource as LocalDataFeedSource;
            if (localDataFeedSource != null)
            {
                FeedConfiguration config = new FeedConfiguration(ScePhotoSettings.ApplicationName, ScePhotoSettings.LocalCacheFolder, ScePhotoSettings.DataFeedUri);
                this.updateSession.SubscriptionCache = new SubscriptionCache(config, this.threadPool, ServiceProvider.Logger, ServiceProvider.WebCredentials, localDataFeedSource.SyncItemCache);
                this.updateSession.SubscriptionCache.FeedsUpdated += new EventHandler<EventArgs>(this.SubscriptionCache_FeedsUpdated);
                this.updateSession.SubscriptionCache.UpdateProgressChanged += new EventHandler<ProgressChangedEventArgs>(this.SubscriptionCache_UpdateProgressChanged);
                this.updateSession.SubscriptionCache.UpdateCompleted += new EventHandler<UpdateCompletedEventArgs>(this.SubscriptionCache_UpdateCompleted);
                this.updateSession.SubscriptionCache.UpdateAsync();
            }
            else
            {
                this.GetFeedDocumentAsync(new DataRequest(this.masterFeedContent, this.masterFeedContent.BaseUri, this));
            }
        }

        /// <summary>
        /// Cancel asynchronous operation before it gets completed.
        /// </summary>
        /// <param name="userState">A user-defined object that uniquely identifies the request.</param>
        public void CancelAsync(object userState)
        {
            DataFeedSource dataFeedSource = ServiceProvider.DataFeedSource;

            if (userState != null)
            {
                foreach (DataRequest dataRequest in this.pendingDataRequests)
                {
                    if (object.ReferenceEquals(dataRequest.UserState, userState))
                    {
                        dataFeedSource.CancelAsync(dataRequest);
                    }
                }
            }
            else if (this.updateSession != null)
            {
                this.updateSession.Cancel();
                dataFeedSource.CancelAsync(this);
            }
        }

        /// <summary>
        /// Releases the managed resources and waits for database shutdown process.
        /// </summary>
        public void Dispose()
        {
            DataFeedSource dataFeedSource = ServiceProvider.DataFeedSource;

            if (this.updateSession != null)
            {
                this.updateSession.Cancel();
                dataFeedSource.CancelAsync(this);
            }

            foreach (DataRequest dataRequest in this.pendingDataRequests)
            {
                dataFeedSource.CancelAsync(dataRequest);
            }

            IDisposable disposable = dataFeedSource as IDisposable;
            if (disposable != null)
            {
                disposable.Dispose();
            }

            this.threadPool.Dispose();
        }

        /// <summary>
        /// Make asynchronous request to get ImageSource from an Uri.
        /// </summary>
        /// <param name="uri">The Uri representing an image.</param>
        /// <param name="userState">A user-defined object that uniquely identifies the request.</param>
        public void GetImageSourceAsync(Uri uri, object userState)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }

            if (userState == null)
            {
                throw new ArgumentNullException("userState");
            }

            if (!uri.IsAbsoluteUri)
            {
                throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, Strings.DataManagerAbsoluteUriRequired, uri.ToString()));
            }

            // Make asynchronous request to get ImageSource object from the data feed.
            DataRequest imageRequest = new DataRequest(null, uri, userState);
            this.pendingDataRequests.Add(imageRequest);
            ServiceProvider.DataFeedSource.GetImageSourceAsync(uri, false, imageRequest);
        }

        /// <summary>
        /// Make asynchronous request to get text document from an Uri.
        /// </summary>
        /// <param name="uri">The Uri representing text document.</param>
        /// <param name="userState">A user-defined object that uniquely identifies the request.</param>
        public void GetTextDocumentAsync(Uri uri, object userState)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }

            if (userState == null)
            {
                throw new ArgumentNullException("userState");
            }

            if (!uri.IsAbsoluteUri)
            {
                throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, Strings.DataManagerAbsoluteUriRequired, uri.ToString()));
            }

            // Make asynchronous request to get text document from from the data feed.
            DataRequest descriptionRequest = new DataRequest(null, uri, userState);
            this.pendingDataRequests.Add(descriptionRequest);
            ServiceProvider.DataFeedSource.GetTextDocumentAsync(uri, false, descriptionRequest);
        }

        /// <summary>
        /// Raises LoadCachedDataStarted event.
        /// </summary>
        /// <param name="e">Arguments describing the event.</param>
        private void OnLoadCachedDataStarted(EventArgs e)
        {
            if (this.LoadCachedDataStarted != null)
            {
                this.LoadCachedDataStarted(this, e);
            }
        }

        /// <summary>
        /// Raises OnUpdateStarted event.
        /// </summary>
        /// <param name="e">Arguments describing the event.</param>
        private void OnUpdateStarted(EventArgs e)
        {
            if (this.UpdateStarted != null)
            {
                this.UpdateStarted(this, e);
            }
        }

        /// <summary>
        /// Raises UpdateProgressChanged event.
        /// </summary>
        /// <param name="e">Arguments describing the event.</param>
        private void OnUpdateProgressChanged(ProgressChangedEventArgs e)
        {
            if (this.UpdateProgressChanged != null)
            {
                this.UpdateProgressChanged(this, e);
            }
        }

        /// <summary>
        /// Raises FeedsUpdated event.
        /// </summary>
        /// <param name="e">Arguments describing the event.</param>
        private void OnFeedsUpdated(EventArgs e)
        {
            if (this.FeedsUpdated != null)
            {
                this.FeedsUpdated(this, e);
            }
        }

        /// <summary>
        /// Raises GetImageSourceCompleted event.
        /// </summary>
        /// <param name="e">Arguments describing the event.</param>
        private void OnGetImageSourceCompleted(GetImageSourceCompletedEventArgs e)
        {
            if (this.GetImageSourceCompleted != null)
            {
                this.GetImageSourceCompleted(this, e);
            }
        }

        /// <summary>
        /// Raises GetTextDocumentCompleted event.
        /// </summary>
        /// <param name="e">Arguments describing the event.</param>
        private void OnGetTextDocumentCompleted(GetTextDocumentCompletedEventArgs e)
        {
            if (this.GetTextDocumentCompleted != null)
            {
                this.GetTextDocumentCompleted(this, e);
            }
        }

        /// <summary>
        /// Raise UpdateCompleted event.
        /// </summary>
        /// <remarks>
        /// If update is from local cache only, no update completed event is raised
        /// </remarks>
        private void OnUpdateCompleted()
        {
            bool loadFromCache = this.updateSession.LoadFromCache;
            this.successfulUpdate = (this.updateSession.Error == null && !this.updateSession.Canceled);
            if (!this.updateSession.Canceled)
            {
                this.lastUpdateDate = loadFromCache ? this.masterFeedContent.ChangeDate : this.updateSession.UpdateTime;
            }

            AsyncCompletedEventArgs args = new AsyncCompletedEventArgs(this.updateSession.Error, this.updateSession.Canceled, null);
            if (!loadFromCache)
            {
                if (this.updateSession.Canceled)
                {
                    ServiceProvider.Logger.Information(Strings.DataManagerUpdateCanceled);
                }
                else
                {
                    ServiceProvider.Logger.Information(Strings.DataManagerUpdateCompleted);
                }

                if (this.UpdateCompleted != null)
                {
                    this.UpdateCompleted(this, args);
                }
            }
            else
            {
                // Cached load completed
                if (this.updateSession.Canceled)
                {
                    ServiceProvider.Logger.Information(Strings.DataManagerLoadFromCacheCanceled);
                }
                else
                {
                    ServiceProvider.Logger.Information(Strings.DataManagerLoadFromCacheCompleted);
                }

                if (this.LoadCachedDataCompleted != null)
                {
                    this.LoadCachedDataCompleted(this, args);
                }
            }

            this.updateSession = null;
        }

        /// <summary>
        /// Event handler for DataFeedSource.GetXmlDocumentCompleted event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">Event arguments accompanying the event.</param>
        private void DataFeedSource_GetXmlDocumentCompleted(object sender, GetXmlDocumentCompletedEventArgs e)
        {
            DataRequest dataRequest = e.UserState as DataRequest;
            if (dataRequest != null)
            {
                if (dataRequest.Context is MasterFeedContent)
                {
                    this.OnMasterFeedContentXmlDocumentReady(e, dataRequest);
                }
                else if (dataRequest.Context is PhotoGallery)
                {
                    this.OnPhotoGalleryXmlDocumentReady(e, dataRequest);
                }
                else if (dataRequest.Context is PhotoAlbum)
                {
                    this.OnPhotoAlbumXmlDocumentReady(e, dataRequest);
                }
            }
        }

        /// <summary>
        /// Event handler for DataFeedSource.GetImageSourceCompleted event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">Event arguments accompanying the event.</param>
        private void DataFeedSource_GetImageSourceCompleted(object sender, GetImageSourceCompletedEventArgs e)
        {
            DataRequest dataRequest = e.UserState as DataRequest;
            if (dataRequest != null && this.pendingDataRequests.Contains(dataRequest))
            {
                this.dispatcher.BeginInvoke(DispatcherPriority.Input, new DispatcherOperationCallback(this.GetImageCompletedCallback), e);
            }
        }

        /// <summary>
        /// Event handler for DataFeedSource.GetTextDocumentCompleted event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">Event arguments accompanying the event.</param>
        private void DataFeedSource_GetTextDocumentCompleted(object sender, GetTextDocumentCompletedEventArgs e)
        {
            DataRequest dataRequest = e.UserState as DataRequest;
            if (dataRequest != null && this.pendingDataRequests.Contains(dataRequest))
            {
                this.dispatcher.BeginInvoke(DispatcherPriority.Input, new DispatcherOperationCallback(this.GetTextDocumentCompletedCallback), e);
            }
        }

        /// <summary>
        /// Callback responsible for asynchronous handling of DataFeedSource.GetImageSourceCompleted event.
        /// </summary>
        /// <param name="arg">Callback argument.</param>
        /// <returns>Always returns null.</returns>
        private object GetImageCompletedCallback(object arg)
        {
            GetImageSourceCompletedEventArgs e = (GetImageSourceCompletedEventArgs)arg;
            DataRequest dataRequest = e.UserState as DataRequest;
            if (dataRequest != null && this.pendingDataRequests.Contains(dataRequest))
            {
                this.pendingDataRequests.Remove(dataRequest);
                GetImageSourceCompletedEventArgs args;
                if (e.Error == null && !e.Cancelled)
                {
                    args = new GetImageSourceCompletedEventArgs(e.ImageSource, dataRequest.UserState);
                }
                else
                {
                    args = new GetImageSourceCompletedEventArgs(e.Error, e.Cancelled, dataRequest.UserState);
                }

                this.OnGetImageSourceCompleted(args);
            }

            return null;
        }

        /// <summary>
        /// Callback responsible for asynchronous handling of DataFeedSource.GetTextDocumentCompleted event.
        /// </summary>
        /// <param name="arg">Callback argument.</param>
        /// <returns>Always returns null.</returns>
        private object GetTextDocumentCompletedCallback(object arg)
        {
            GetTextDocumentCompletedEventArgs e = (GetTextDocumentCompletedEventArgs)arg;
            DataRequest dataRequest = e.UserState as DataRequest;
            if (dataRequest != null && this.pendingDataRequests.Contains(dataRequest))
            {
                this.pendingDataRequests.Remove(dataRequest);
                GetTextDocumentCompletedEventArgs args;
                if (e.Error == null && !e.Cancelled)
                {
                    args = new GetTextDocumentCompletedEventArgs(e.Link, e.DocumentText, dataRequest.UserState);
                }
                else
                {
                    args = new GetTextDocumentCompletedEventArgs(e.Error, e.Cancelled, dataRequest.UserState);
                }

                this.OnGetTextDocumentCompleted(args);
            }

            return null;
        }

        /// <summary>
        /// Handler for SubscriptionCache.UpdateCompleted event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">Event arguments.</param>
        private void SubscriptionCache_UpdateCompleted(object sender, UpdateCompletedEventArgs e)
        {
            this.dispatcher.BeginInvoke(DispatcherPriority.Input, new DispatcherOperationCallback(this.OnCacheUpdateCompleted), e);
        }

        /// <summary>
        /// Handler for SubscriptionCache.UpdateProgressChanged event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">Event arguments.</param>
        private void SubscriptionCache_UpdateProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.dispatcher.BeginInvoke(DispatcherPriority.Input, new DispatcherOperationCallback(this.OnCacheUpdateProgressChanged), e);
        }

        /// <summary>
        /// Handler for SubscriptionCache.FeedsUpdated event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">Event arguments.</param>
        private void SubscriptionCache_FeedsUpdated(object sender, EventArgs e)
        {
            this.dispatcher.BeginInvoke(DispatcherPriority.Input, new DispatcherOperationCallback(this.OnCacheFeedsUpdated), e);
        }

        /// <summary>
        /// Called when XmlDocument representing MasterFeedContent is ready for processing.
        /// </summary>
        /// <param name="e">Provides detailed information about asynchronous operation completion.</param>
        /// <param name="dataRequest">The data request that has been just completed.</param>
        private void OnMasterFeedContentXmlDocumentReady(GetXmlDocumentCompletedEventArgs e, DataRequest dataRequest)
        {
            if (e.Error == null && !e.Cancelled && !this.updateSession.Canceled)
            {
                MasterFeedContent masterFeedContent = (MasterFeedContent)dataRequest.Context;

                // Convert the Xml data to MasterFeedContent.
                MasterFeedContent newMasterFeedContent = ServiceProvider.PhotoFeedConverter.ConvertToMasterFeedContent(e.Document, Path.GetFileName(dataRequest.Uri.LocalPath));
                if (newMasterFeedContent != null)
                {
                    // If just retrieved MasterFeedContent is newer than the current MasterFeedContent, 
                    // merge new data into the current data.
                    if (DateTime.Compare(newMasterFeedContent.ChangeDate, masterFeedContent.ChangeDate) > 0 ||
                        !this.successfulUpdate)
                    {
                        masterFeedContent.Merge(newMasterFeedContent);

                        int index, newCount;
                        for (index = 0, newCount = masterFeedContent.PhotoGalleryFeeds.Count; index < newCount; index++)
                        {
                            FeedItem nestedFeed = masterFeedContent.PhotoGalleryFeeds[index];
                            PhotoGallery photoGallery = masterFeedContent.PhotoGalleries[index];
                            if (nestedFeed.IsNew || !this.successfulUpdate)
                            {
                                // BaseUri may be changed, hence update it before requesting feed's content.
                                photoGallery.BaseUri = new Uri(masterFeedContent.BaseUri, nestedFeed.Link);
                                this.GetFeedDocumentAsync(new DataRequest(photoGallery, photoGallery.BaseUri, this));
                            }
                            else
                            {
                                // Reset guid store so no items are marked New
                                photoGallery.GuidStore.ResetIsNew();
                            }
                        }
                    }
                    else
                    {
                        // Reset IsNew flag.
                        masterFeedContent.ResetIsNew();
                    }
                }
                else
                {
                    ServiceProvider.Logger.Error(Strings.ConvertersXPathDocumentToMasterFeedFailed);
                }
            }

            // Mark the request as completed.
            this.OnFeedRequestCompleted(e.Error, e.Cancelled);
        }

        /// <summary>
        /// Called when XmlDocument representing PhotoGallery is ready for processing.
        /// </summary>
        /// <param name="e">Provides detailed information about asynchronous operation completion.</param>
        /// <param name="dataRequest">The data request that has been just completed.</param>
        private void OnPhotoGalleryXmlDocumentReady(GetXmlDocumentCompletedEventArgs e, DataRequest dataRequest)
        {
            if (e.Error == null && !e.Cancelled && !this.updateSession.Canceled)
            {
                PhotoGallery photoGallery = (PhotoGallery)dataRequest.Context;

                // Convert the Xml data to MasterFeedContent.
                PhotoGallery newPhotoGallery = ServiceProvider.PhotoFeedConverter.ConvertToPhotoGallery(e.Document, Path.GetFileName(dataRequest.Uri.LocalPath));
                if (newPhotoGallery != null)
                {
                    // If just retrieved PhotoGallery is newer than the current PhotoGallery, 
                    // merge new data into the current data.
                    if (DateTime.Compare(newPhotoGallery.ChangeDate, photoGallery.ChangeDate) > 0 ||
                        !this.successfulUpdate)
                    {
                        photoGallery.Merge(newPhotoGallery);

                        int index, newCount;
                        for (index = 0, newCount = photoGallery.PhotoAlbumFeeds.Count; index < newCount; index++)
                        {
                            FeedItem nestedFeed = photoGallery.PhotoAlbumFeeds[index];
                            PhotoAlbum photoAlbum = photoGallery.PhotoAlbums[index];
                            if (nestedFeed.IsNew || !this.successfulUpdate)
                            {
                                // BaseUri may be changed, hence update it before requesting feed's content.
                                photoAlbum.BaseUri = new Uri(photoGallery.BaseUri, nestedFeed.Link);
                                this.GetFeedDocumentAsync(new DataRequest(photoAlbum, photoAlbum.BaseUri, this));
                            }
                            else
                            {
                                // Reset guid store so no items are marked New
                                photoAlbum.GuidStore.ResetIsNew();
                            }
                        }
                    }
                    else
                    {
                        // Reset IsNew flag.
                        photoGallery.ResetIsNew();
                    }
                }
                else
                {
                    ServiceProvider.Logger.Error(Strings.ConvertersXPathDocumentToPhotoGalleryFailed);
                }
            }

            // Mark the request as completed.
            this.OnFeedRequestCompleted(e.Error, e.Cancelled);
        }

        /// <summary>
        /// Called when XmlDocument representing PhotoAlbum is ready for processing.
        /// </summary>
        /// <param name="e">Provides detailed information about asynchronous operation completion.</param>
        /// <param name="dataRequest">The data request that has been just completed.</param>
        private void OnPhotoAlbumXmlDocumentReady(GetXmlDocumentCompletedEventArgs e, DataRequest dataRequest)
        {
            if (e.Error == null && !e.Cancelled && !this.updateSession.Canceled)
            {
                PhotoAlbum photoAlbum = (PhotoAlbum)dataRequest.Context;

                // Convert the Xml data to MasterFeedContent.
                PhotoAlbum newPhotoAlbum = ServiceProvider.PhotoFeedConverter.ConvertToPhotoAlbum(e.Document, Path.GetFileName(dataRequest.Uri.LocalPath));
                if (newPhotoAlbum != null)
                {
                    // If just retrieved PhotoAlbum is newer than the current PhotoAlbum, 
                    // merge new data into the current data.
                    if (DateTime.Compare(newPhotoAlbum.ChangeDate, photoAlbum.ChangeDate) > 0 ||
                        !this.successfulUpdate)
                    {
                        photoAlbum.Merge(newPhotoAlbum);
                    }
                    else
                    {
                        // Reset IsNew flag.
                        photoAlbum.GuidStore.ResetIsNew();
                    }
                }
                else
                {
                    ServiceProvider.Logger.Error(Strings.ConvertersXPathDocumentToPhotoAlbumFailed);
                }
            }

            // Mark the request as completed.
            this.OnFeedRequestCompleted(e.Error, e.Cancelled);
        }

        /// <summary>
        /// Marks the request as completed and raises appropriate events (UpdateComplted and FeedsUpdated), if necessary.
        /// </summary>
        /// <param name="error">Indicates error occurred during an asynchronous operation, if any.</param>
        /// <param name="canceled">Whether or not an asynchronous operation has been canceled.</param>
        private void OnFeedRequestCompleted(Exception error, bool canceled)
        {
            // Mark the request as completed.
            this.updateSession.RequestCompleted(error, canceled, true);

            // If there are no other pending data feed requests, raise EditionsUpdated event.
            if (this.updateSession.RaiseFeedsUpdatedEvent)
            {
                this.updateSession.RaiseFeedsUpdatedEvent = false;
                this.OnFeedsUpdated(EventArgs.Empty);
            }

            // If there are no other pending requests, raise UpdateCompleted event.
            if (this.updateSession.RaiseUpdateCompletedEvent)
            {
                this.OnUpdateCompleted();
            }
        }

        /// <summary>
        /// Make asynchronous request to get XmlDocument from an Uri.
        /// </summary>
        /// <param name="dataRequest">Data associated with a request processed by DataManager.</param>
        private void GetFeedDocumentAsync(DataRequest dataRequest)
        {
            this.updateSession.FeedRequestsAdded(1);
            ServiceProvider.DataFeedSource.GetXmlDocumentAsync(dataRequest.Uri, this.updateSession.LoadFromCache, dataRequest);
        }

        /// <summary>
        /// Responds to SubscriptionCache.UpdateCompleted event in UI thread.
        /// </summary>
        /// <param name="arg">Callback argument.</param>
        /// <returns>Always returns null.</returns>
        private object OnCacheUpdateCompleted(object arg)
        {
            UpdateCompletedEventArgs e = (UpdateCompletedEventArgs)arg;
           
            this.updateSession.SubscriptionCache.FeedsUpdated -= new EventHandler<EventArgs>(this.SubscriptionCache_FeedsUpdated);
            this.updateSession.SubscriptionCache.UpdateProgressChanged -= new EventHandler<ProgressChangedEventArgs>(this.SubscriptionCache_UpdateProgressChanged);
            this.updateSession.SubscriptionCache.UpdateCompleted -= new EventHandler<UpdateCompletedEventArgs>(this.SubscriptionCache_UpdateCompleted);
            this.updateSession.SubscriptionCache = null;

            if (e.Error == null && !e.Cancelled && e.FailedItemCount != 0)
            {
                this.updateSession.Error = new ScePhotoException(String.Format(CultureInfo.InvariantCulture, Strings.DataManagerSyncIncomplete, e.FailedItemCount));
            }

            // If there are no other pending requests, raise UpdateCompleted event.
            if (this.updateSession.RaiseUpdateCompletedEvent)
            {
                this.OnUpdateCompleted();
            }

            return null;
        }

        /// <summary>
        /// Responds to SubscriptionCache.UpdateProgressChanged event in UI thread.
        /// </summary>
        /// <param name="arg">Callback argument.</param>
        /// <returns>Always returns null.</returns>
        private object OnCacheUpdateProgressChanged(object arg)
        {
            if (!this.updateSession.Canceled)
            {
                this.OnUpdateProgressChanged((ProgressChangedEventArgs)arg);
            }

            return null;
        }

        /// <summary>
        /// Responds to SubscriptionCache.FeedsUpdated event in UI thread.
        /// </summary>
        /// <param name="arg">Callback argument.</param>
        /// <returns>Always returns null.</returns>
        private object OnCacheFeedsUpdated(object arg)
        {
            this.GetFeedDocumentAsync(new DataRequest(this.masterFeedContent, this.masterFeedContent.BaseUri, this));
            return null;
        }

        #endregion

        #region Sub-classes

        /// <summary>
        /// Holds state associated with update session.
        /// </summary>
        private sealed class UpdateSession
        {
            /// <summary>
            /// The date and time of the update session start.
            /// </summary>
            private readonly DateTime updateTime;

            /// <summary>
            /// Flag indicating whether or not the session was created to load data from cache.
            /// </summary>
            private readonly bool loadFromCache;

            /// <summary>
            /// The object managing subscription's synchronization services.
            /// </summary>
            private SubscriptionCache subscriptionCache;

            /// <summary>
            /// Flag indicating whether or not the session was canceled.
            /// </summary>
            private bool canceled;

            /// <summary>
            /// The total number of all requests processed by this session.
            /// </summary>
            private int totalRequestCount;

            /// <summary>
            /// The number of remaining requests waiting for processing for this session.
            /// </summary>
            private int pendingRequestCount;

            /// <summary>
            /// The number of remaining feed requests waiting for processing for this session.
            /// </summary>
            private int pendingFeedRequestCount;

            /// <summary>
            /// The number of requests that failed.
            /// </summary>
            private int failedRequestCount;

            /// <summary>
            /// The error associated with most recent data update.
            /// </summary>
            private Exception error;

            /// <summary>
            /// Constructor. Creates and initializes a new update session.
            /// </summary>
            /// <param name="loadFromCache">Flag indicating whether or not the session was created to load data from cache.</param>
            public UpdateSession(bool loadFromCache)
            {
                this.updateTime = DateTime.Now;
                this.loadFromCache = loadFromCache;
            }

            /// <summary>
            /// Gets or sets the object managing subscription's synchronization services.
            /// </summary>
            public SubscriptionCache SubscriptionCache
            {
                get { return this.subscriptionCache; }
                set { this.subscriptionCache = value; }
            }

            /// <summary>
            /// Gets a value indicating whether or not the session was created to load data from cache.
            /// </summary>
            public bool LoadFromCache
            {
                get { return this.loadFromCache; }
            }

            /// <summary>
            /// Gets the date and time of the update session start.
            /// </summary>
            public DateTime UpdateTime
            {
                get { return this.updateTime; }
            }

            /// <summary>
            /// Gets the total number of all requests processed by this session.
            /// </summary>
            public int TotalRequestCount
            {
                get { return this.totalRequestCount; }
            }

            /// <summary>
            /// Gets the number of remaining requests waiting for processing for this session.
            /// </summary>
            public int PendingRequestCount
            {
                get { return this.pendingRequestCount; }
            }

            /// <summary>
            /// Gets or sets a value indicating whether or not FeedsUpdated event needs to be raised.
            /// </summary>
            public bool RaiseFeedsUpdatedEvent
            {
                get { return (this.pendingFeedRequestCount == 0); }
                set { this.pendingFeedRequestCount = value ? 0 : -1; }
            }

            /// <summary>
            /// Gets a value indicating whether or not the entire update process has completed - i.e. 
            /// there are no pending feed requests and the SubscriptionCache used to manage caching is no longer in use.
            /// </summary>
            public bool RaiseUpdateCompletedEvent
            {
                get { return (this.pendingFeedRequestCount <= 0 && this.subscriptionCache == null); }
            }

            /// <summary>
            /// Gets the number of requests that failed.
            /// </summary>
            public int FailedRequestCount
            {
                get { return this.failedRequestCount; }
            }

            /// <summary>
            /// Gets a value indicating whether or not the session was canceled.
            /// </summary>
            public bool Canceled
            {
                get { return this.canceled; }
            }

            /// <summary>
            /// Gets or sets the error associated with most recent data update, if any.
            /// </summary>
            public Exception Error
            {
                get { return this.canceled ? null : this.error; }
                set { this.error = value; }
            }

            /// <summary>
            /// Records addition of new data requests.
            /// </summary>
            /// <param name="requestCount">Number of requests.</param>
            public void RequestsAdded(int requestCount)
            {
                this.pendingRequestCount += requestCount;
                this.totalRequestCount += requestCount;
            }

            /// <summary>
            /// Records addition of new feed requests.
            /// </summary>
            /// <param name="requestCount">Number of requests.</param>
            public void FeedRequestsAdded(int requestCount)
            {
                this.RequestsAdded(requestCount);
                this.pendingFeedRequestCount += requestCount;
            }

            /// <summary>
            /// Records request completion and the cause of the completion.
            /// </summary>
            /// <param name="error">Error associated with the request, if failed.</param>
            /// <param name="canceled">Flag indicating whether or not request was canceled.</param>
            /// <param name="feedRequest">Flag indicating whether or not request was a feed request.</param>
            public void RequestCompleted(Exception error, bool canceled, bool feedRequest)
            {
                this.pendingRequestCount--;
                this.canceled |= canceled;
                if (error != null)
                {
                    this.failedRequestCount++;
                    if (this.error == null)
                    {
                        this.error = error;
                    }
                }

                if (feedRequest)
                {
                    this.pendingFeedRequestCount--;
                }
            }

            /// <summary>
            /// Cancels the current update session.
            /// </summary>
            public void Cancel()
            {
                this.canceled = true;
                if (this.subscriptionCache != null)
                {
                    this.subscriptionCache.CancelAsync();
                }
            }
        }

        /// <summary>
        /// An AsyncDataRequest that saves the context of the request.
        /// </summary>
        private sealed class DataRequest : AsyncDataRequest
        {
            /// <summary>
            /// Context of the request.
            /// </summary>
            private object context;

            /// <summary>
            /// Initializes the DataRequest instance.
            /// </summary>
            /// <param name="context">Context of the request.</param>
            /// <param name="uri">The URI of the resource to retrieve.</param>
            /// <param name="userState">The user-supplied state object.</param>
            public DataRequest(object context, Uri uri, object userState)
                : base(uri, userState)
            {
                this.context = context;
            }

            /// <summary>
            /// Gets the context of the request.
            /// </summary>
            public object Context
            {
                get { return this.context; }
            }
        }

        /// <summary>
        /// A SubscriptionConfiguration that contains the configuration information for a feed.
        /// </summary>
        private class FeedConfiguration : SubscriptionConfiguration
        {
            /// <summary>
            /// Initializes the FeedConfiguration instance.
            /// </summary>
            /// <param name="name">Configuration name.</param>
            /// <param name="cacheFolder">Cache folder for this configuration.</param>
            /// <param name="feedUri">Feed Uri for this configuration.</param>
            public FeedConfiguration(string name, string cacheFolder, Uri feedUri)
            {
                this.Name = name;
                this.CacheFolder = cacheFolder;
                this.FeedUri = feedUri;
            }
        }

        #endregion
    }
}
