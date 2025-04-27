//-----------------------------------------------------------------------
// <copyright file="LocalDataFeedSource.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     Data retrieval class to synchronize data from a data feed.
// </summary>
//-----------------------------------------------------------------------

namespace ScePhoto.Feed
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Threading;
    using System.Windows.Media.Imaging;
    using System.Windows.Threading;
    using System.Xml;
    using System.Xml.XPath;
    using Microsoft.SubscriptionCenter.Sync;

    /// <summary>
    /// Data retrieval class to synchronize data from a data feed.
    /// </summary>
    public sealed class LocalDataFeedSource : DataFeedSource, IDisposable
    {
        #region Fields

        /// <summary>
        /// Thread synchronization object.
        /// </summary>
        private readonly object syncRoot;

        /// <summary>
        /// Provides common methods for receiving data from a resource identified by a URI.  
        /// </summary>
        private readonly WebDataProvider webDataProvider;

        /// <summary>
        /// The LIFO collection of data requests. Those requests are treated as high priority 
        /// requests and are processed in last-in-first-out order.
        /// </summary>
        private readonly Stack<DataFeedRequest> dataFeedRequests;

        /// <summary>
        /// The collection of active request processed by WebDataProvider.
        /// </summary>
        private readonly IList<DataFeedRequest> webDataRequests;

        /// <summary>
        /// Manages the local sync item cache database.
        /// </summary>
        private SyncItemCache syncItemCache;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes the instance of LocalDataFeedSource.
        /// </summary>
        /// <param name="dispatcher">The dispatcher associated with the UI thread.</param>
        public LocalDataFeedSource(Dispatcher dispatcher)
            : base(dispatcher)
        {
            this.syncRoot = new object();
            this.dataFeedRequests = new Stack<DataFeedRequest>();
            this.webDataRequests = new List<DataFeedRequest>();

            WorkerThreadPool threadPool = new WorkerThreadPool(ServiceProvider.Logger, 0, 10, ThreadPriority.Normal);
            this.webDataProvider = new WebDataProvider(threadPool, ServiceProvider.Logger, ServiceProvider.WebCredentials);
            this.webDataProvider.GetWebDataCompleted += new EventHandler<GetWebDataCompletedEventArgs>(this.WebDataProvider_GetWebDataCompleted);

            this.syncItemCache = new SyncItemCache(ScePhotoSettings.LocalCacheFolder, SyncItemCache.DefaultFeedDatabaseName, ServiceProvider.Logger);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the SyncItemCache manages access to the local item cache database.
        /// </summary>
        public SyncItemCache SyncItemCache
        {
            get { return this.syncItemCache; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Make asynchronous request to get XmlDocument from an Uri.
        /// </summary>
        /// <param name="uri">The Uri representing XML document.</param>
        /// <param name="localData">Whether or not requesting local only data, if supported.</param>
        /// <param name="userState">The user-supplied state object.</param>
        public override void GetXmlDocumentAsync(Uri uri, bool localData, object userState)
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
                throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, Strings.DataFeedSourceAbsoluteUriRequired, uri.ToString()));
            }

            bool userStateIsUnique;
            lock (this.syncRoot)
            {
                // Verify that the userState is unique.
                userStateIsUnique = this.VerifyUniqueUserState(userState);
                if (userStateIsUnique)
                {
                    // Queue asynchronous data request.
                    this.dataFeedRequests.Push(new DataFeedRequest(ResourceContentType.Xml, localData, uri, userState));
                }
            }

            if (userStateIsUnique)
            {
                ThreadPool.QueueUserWorkItem(this.ProcessNextRequestCallback);
            }
            else
            {
                Exception exception = new InvalidOperationException(Strings.DataFeedSourceUserStateNotUnique);
                ServiceProvider.Logger.Error(exception.Message);
                OnGetXmlDocumentCompleted(new GetXmlDocumentCompletedEventArgs(exception, false, userState));
            }
        }

        /// <summary>
        /// Make asynchronous request to get ImageSource from an Uri.
        /// </summary>
        /// <param name="uri">The Uri representing an image.</param>
        /// <param name="localData">Whether or not requesting local only data, if supported.</param>
        /// <param name="userState">The user-supplied state object.</param>
        public override void GetImageSourceAsync(Uri uri, bool localData, object userState)
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
                throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, Strings.DataFeedSourceAbsoluteUriRequired, uri.ToString()));
            }

            bool userStateIsUnique;
            lock (this.syncRoot)
            {
                // Verify that the userState is unique.
                userStateIsUnique = this.VerifyUniqueUserState(userState);
                if (userStateIsUnique)
                {
                    // Queue asynchronous data request.
                    this.dataFeedRequests.Push(new DataFeedRequest(ResourceContentType.Image, localData, uri, userState));
                }
            }

            if (userStateIsUnique)
            {
                ThreadPool.QueueUserWorkItem(this.ProcessNextRequestCallback);
            }
            else
            {
                Exception exception = new InvalidOperationException(Strings.DataFeedSourceUserStateNotUnique);
                ServiceProvider.Logger.Error(exception.Message);
                OnGetImageSourceCompleted(new GetImageSourceCompletedEventArgs(exception, false, userState));
            }
        }

        /// <summary>
        /// Make asynchronous request to get Text Document from an Uri.
        /// </summary>
        /// <param name="uri">The Uri representing text document.</param>
        /// <param name="localData">Whether or not requesting local only data, if supported.</param>
        /// <param name="userState">The user-supplied state object.</param>
        public override void GetTextDocumentAsync(Uri uri, bool localData, object userState)
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
                throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, Strings.DataFeedSourceAbsoluteUriRequired, uri.ToString()));
            }

            bool userStateIsUnique;
            lock (this.syncRoot)
            {
                // Verify that the userState is unique.
                userStateIsUnique = this.VerifyUniqueUserState(userState);
                if (userStateIsUnique)
                {
                    // Queue asynchronous data request.
                    this.dataFeedRequests.Push(new DataFeedRequest(ResourceContentType.Text, localData, uri, userState));
                }
            }

            if (userStateIsUnique)
            {
                ThreadPool.QueueUserWorkItem(this.ProcessNextRequestCallback);
            }
            else
            {
                Exception exception = new InvalidOperationException(Strings.DataFeedSourceUserStateNotUnique);
                ServiceProvider.Logger.Error(exception.Message);
                OnGetTextDocumentCompleted(new GetTextDocumentCompletedEventArgs(exception, false, userState));
            }
        }

        /// <summary>
        /// Cancel asynchronous operation before it gets completed.
        /// </summary>
        /// <param name="userState">The user-supplied state object.</param>
        public override void CancelAsync(object userState)
        {
            DataFeedRequest dataFeedRequest = null;
            lock (this.syncRoot)
            {
                // Search for matching data request in the request collection. 
                // If found, cancel the request. Raise completed event to indicate that 
                // the request was cancelled.
                foreach (DataFeedRequest dataRequest in this.dataFeedRequests)
                {
                    if (object.ReferenceEquals(dataRequest.UserState, userState))
                    {
                        dataRequest.Canceled = true;
                        dataFeedRequest = dataRequest;
                        break;
                    }
                }
            }

            // Raise completed events for canceled requests outside of the lock.
            if (dataFeedRequest != null)
            {
                if (dataFeedRequest.ContentType == ResourceContentType.Xml)
                {
                    OnGetXmlDocumentCompleted(new GetXmlDocumentCompletedEventArgs(null, true, userState));
                }
                else if (dataFeedRequest.ContentType == ResourceContentType.Image)
                {
                    OnGetImageSourceCompleted(new GetImageSourceCompletedEventArgs(null, true, userState));
                }
                else if (dataFeedRequest.ContentType == ResourceContentType.Text)
                {
                    OnGetTextDocumentCompleted(new GetTextDocumentCompletedEventArgs(null, true, userState));
                }
            }
        }

        /// <summary>
        /// Releases the managed resources.
        /// </summary>
        public void Dispose()
        {
            lock (this.syncRoot)
            {
                this.syncItemCache.Dispose();
            }
        }

        /// <summary>
        /// Verifies that the 'userState' is unique.
        /// </summary>
        /// <param name="userState">The user-supplied state object.</param>
        /// <returns>Whether or not the 'userState' is unique.</returns>
        /// <remarks>Assumes that lock is done by the caller.</remarks>
        private bool VerifyUniqueUserState(object userState)
        {
            // Search for matching data request in the request collection. 
            bool unique = true;
            foreach (AsyncDataRequest dataRequest in this.dataFeedRequests)
            {
                if (object.ReferenceEquals(dataRequest.UserState, userState))
                {
                    unique = false;
                    break;
                }
            }

            return unique;
        }

        /// <summary>
        /// Called when a thread from the application's thread pool becomes available for next request processing.
        /// </summary>
        /// <param name="arg">Callback argument.</param>
        private void ProcessNextRequestCallback(object arg)
        {
            this.OnProcessNextRequest();
        }

        /// <summary>
        /// Process the next data request.
        /// </summary>
        private void OnProcessNextRequest()
        {
            // Retrieve the next data request for processing.
            DataFeedRequest dataRequest = null;
            lock (this.syncRoot)
            {
                // Try to retrieve the next (not cancelled) data request from the collection of data requests. 
                while (this.dataFeedRequests.Count > 0)
                {
                    DataFeedRequest nextDataRequest = this.dataFeedRequests.Pop();
                    if (!nextDataRequest.Canceled)
                    {
                        dataRequest = nextDataRequest;
                        break;
                    }
                }
            }

            // Process the next data request.
            if (dataRequest != null)
            {
                bool processed = false;

                // If the locally cached file is present, use it to retrieve data. 
                // Otherwise use WebDataProvider to retrieve data from remote location.
                if (this.syncItemCache.IsCached(dataRequest.Uri))
                {
                    processed = true;

                    // Try to open a local file stream. If the operation fails, catch the exception and 
                    // ignore failure. Further processing of this stream will be disabled.
                    Stream localStream = null;
                    Exception error = null;
                    try
                    {
                        DateTime saveDate;
                        string mimeType;
                        localStream = this.syncItemCache.LoadFromCache(dataRequest.Uri, out saveDate, out mimeType);
                    }
                    catch (SystemException exception)
                    {
                        error = exception;
                    }

                    if (localStream != null)
                    {
                        if (dataRequest.ContentType == ResourceContentType.Xml)
                        {
                            this.OnXmlStreamReady(localStream, dataRequest.Uri.ToString(), dataRequest.UserState);
                        }
                        else if (dataRequest.ContentType == ResourceContentType.Image)
                        {
                            this.OnImageStreamReady(localStream, dataRequest.Uri.ToString(), dataRequest.UserState);
                        }
                        else if (dataRequest.ContentType == ResourceContentType.Text)
                        {
                            this.OnTextStreamReady(localStream, dataRequest.Uri, dataRequest.UserState);
                        }

                        localStream.Close();                    
                    }
                    else
                    {
                        Exception exception = new ScePhotoException(String.Format(CultureInfo.InvariantCulture, Strings.DataFeedSourceReadCachedDataFailed, dataRequest.Uri.ToString(), error.Message), error);
                        ServiceProvider.Logger.Error(exception.Message);
                        if (dataRequest.ContentType == ResourceContentType.Xml)
                        {
                            GetXmlDocumentCompletedEventArgs args = new GetXmlDocumentCompletedEventArgs(exception, false, dataRequest.UserState);
                            Dispatcher.BeginInvoke(DispatcherPriority.Input, new DispatcherOperationCallback(this.RaiseGetXmlDocumentCompleted), args);
                        }
                        else if (dataRequest.ContentType == ResourceContentType.Image)
                        {
                            GetImageSourceCompletedEventArgs args = new GetImageSourceCompletedEventArgs(exception, false, dataRequest.UserState);
                            Dispatcher.BeginInvoke(DispatcherPriority.Input, new DispatcherOperationCallback(this.RaiseGetImageSourceCompleted), args);
                        }
                        else if (dataRequest.ContentType == ResourceContentType.Text)
                        {
                            GetTextDocumentCompletedEventArgs args = new GetTextDocumentCompletedEventArgs(exception, false, dataRequest.UserState);
                            Dispatcher.BeginInvoke(DispatcherPriority.Input, new DispatcherOperationCallback(this.RaiseGetTextDocumentCompleted), args);
                        }
                    }
                }
                else if (dataRequest.LocalData)
                {
                    Exception exception = new ScePhotoException(String.Format(CultureInfo.InvariantCulture, Strings.DataFeedSourceCachedDataMissing, dataRequest.Uri.ToString()));
                    ServiceProvider.Logger.Error(exception.Message);
                    if (dataRequest.ContentType == ResourceContentType.Xml)
                    {
                        GetXmlDocumentCompletedEventArgs args = new GetXmlDocumentCompletedEventArgs(exception, false, dataRequest.UserState);
                        Dispatcher.BeginInvoke(DispatcherPriority.Input, new DispatcherOperationCallback(this.RaiseGetXmlDocumentCompleted), args);
                    }
                    else if (dataRequest.ContentType == ResourceContentType.Image)
                    {
                        GetImageSourceCompletedEventArgs args = new GetImageSourceCompletedEventArgs(exception, false, dataRequest.UserState);
                        Dispatcher.BeginInvoke(DispatcherPriority.Input, new DispatcherOperationCallback(this.RaiseGetImageSourceCompleted), args);
                    }
                    else if (dataRequest.ContentType == ResourceContentType.Text)
                    {
                        GetTextDocumentCompletedEventArgs args = new GetTextDocumentCompletedEventArgs(exception, false, dataRequest.UserState);
                        Dispatcher.BeginInvoke(DispatcherPriority.Input, new DispatcherOperationCallback(this.RaiseGetTextDocumentCompleted), args);
                    }
                }

                if (!processed && !dataRequest.LocalData)
                {
                    lock (this.syncRoot)
                    {
                        this.webDataRequests.Add(dataRequest);
                    }

                    this.webDataProvider.GetWebDataAsync(dataRequest.Uri, true, dataRequest);
                }
            }
        }

        /// <summary>
        /// Raises GetXmlDocumentCompleted event in UI thread.
        /// </summary>
        /// <param name="arg">Callback argument.</param>
        /// <returns>Always returns null.</returns>
        private object RaiseGetXmlDocumentCompleted(object arg)
        {
            GetXmlDocumentCompletedEventArgs args = arg as GetXmlDocumentCompletedEventArgs;
            OnGetXmlDocumentCompleted(args);
            return null;
        }

        /// <summary>
        /// Raises GetImageSourceCompleted event in UI thread.
        /// </summary>
        /// <param name="arg">Callback argument.</param>
        /// <returns>Always returns null.</returns>
        private object RaiseGetImageSourceCompleted(object arg)
        {
            GetImageSourceCompletedEventArgs args = arg as GetImageSourceCompletedEventArgs;
            OnGetImageSourceCompleted(args);
            return null;
        }

        /// <summary>
        /// Raises GetTextDocumentCompleted event in UI thread.
        /// </summary>
        /// <param name="arg">Callback argument.</param>
        /// <returns>Always returns null.</returns>
        private object RaiseGetTextDocumentCompleted(object arg)
        {
            GetTextDocumentCompletedEventArgs args = arg as GetTextDocumentCompletedEventArgs;
            OnGetTextDocumentCompleted(args);
            return null;
        }

        /// <summary>
        /// Event handler for WebDataProvider.GetWebDataCompleted event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">Event arguments accompanying GetWebDataCompleted event.</param>
        private void WebDataProvider_GetWebDataCompleted(object sender, GetWebDataCompletedEventArgs e)
        {
            DataFeedRequest dataFeedRequest = e.UserState as DataFeedRequest;
            if (dataFeedRequest != null)
            {
                this.OnGetWebDataCompleted(e, dataFeedRequest);

                bool processNextRequest = false;
                lock (this.syncRoot)
                {
                    this.webDataRequests.Remove(dataFeedRequest);
                    processNextRequest = (this.dataFeedRequests.Count > 0);
                }

                if (processNextRequest)
                {
                    ThreadPool.QueueUserWorkItem(this.ProcessNextRequestCallback);
                }
            }
        }

        /// <summary>
        /// Called when request to get data stream from URI has been completed (successfully or not).
        /// </summary>
        /// <param name="e">Provides detailed information about asynchronous operation completion.</param>
        /// <param name="dataRequest">The data request that has been just completed.</param>
        private void OnGetWebDataCompleted(GetWebDataCompletedEventArgs e, DataFeedRequest dataRequest)
        {
            // When notifying listeners, web error takes precedence over any SaveToCache error.
            Exception error = null;
            if (e.Error == null && !e.Cancelled)
            {
                // If web request has been successfully completed, cache the data in the local cache.
                try
                {
                    this.syncItemCache.SaveToCache(dataRequest.Uri, e.Stream, DateTime.Now, e.ContentMimeType);
                }
                catch (SystemException exception)
                {
                    ServiceProvider.Logger.Error(Strings.DataFeedSourceSaveToCacheFailed, dataRequest.Uri.ToString(), exception.Message);
                    error = exception;
                }

                if (e.Stream.CanSeek)
                {
                    e.Stream.Seek(0, SeekOrigin.Begin);
                }

                if (dataRequest.ContentType == ResourceContentType.Xml)
                {
                    this.OnXmlStreamReady(e.Stream, dataRequest.Uri.ToString(), dataRequest.UserState);
                }
                else if (dataRequest.ContentType == ResourceContentType.Image)
                {
                    this.OnImageStreamReady(e.Stream, dataRequest.Uri.ToString(), dataRequest.UserState);
                }
                else if (dataRequest.ContentType == ResourceContentType.Text)
                {
                    this.OnTextStreamReady(e.Stream, dataRequest.Uri, dataRequest.UserState);
                }

                e.Stream.Close();
            }
            else
            {
                if (e.Error != null)
                {
                    ServiceProvider.Logger.Error(Strings.DataFeedSourceGetWebDataFailed, dataRequest.Uri.ToString(), e.Error.Message);
                    error = e.Error;
                }
            }

            if (error != null)
            {
                if (dataRequest.ContentType == ResourceContentType.Xml)
                {
                    GetXmlDocumentCompletedEventArgs args = new GetXmlDocumentCompletedEventArgs(error, e.Cancelled, dataRequest.UserState);
                    Dispatcher.BeginInvoke(DispatcherPriority.Input, new DispatcherOperationCallback(this.RaiseGetXmlDocumentCompleted), args);
                }
                else if (dataRequest.ContentType == ResourceContentType.Image)
                {
                    GetImageSourceCompletedEventArgs args = new GetImageSourceCompletedEventArgs(error, e.Cancelled, dataRequest.UserState);
                    Dispatcher.BeginInvoke(DispatcherPriority.Input, new DispatcherOperationCallback(this.RaiseGetImageSourceCompleted), args);
                }
                else if (dataRequest.ContentType == ResourceContentType.Text)
                {
                    GetTextDocumentCompletedEventArgs args = new GetTextDocumentCompletedEventArgs(error, e.Cancelled, dataRequest.UserState);
                    Dispatcher.BeginInvoke(DispatcherPriority.Input, new DispatcherOperationCallback(this.RaiseGetTextDocumentCompleted), args);
                }
            }
        }

        /// <summary>
        /// Called when Xml data stream is ready for processing. 
        /// Creates XmlDocument and raises GetXmlDocumentComplted event.
        /// </summary>
        /// <param name="stream">The stream representing requested XML data.</param>
        /// <param name="location">The location of the data stream.</param>
        /// <param name="userState">The user-supplied state object.</param>
        private void OnXmlStreamReady(Stream stream, string location, object userState)
        {
            GetXmlDocumentCompletedEventArgs args;
            try
            {
                XmlNameTable nameTable = new NameTable();
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.ProhibitDtd = false;
                settings.XmlResolver = null;
                settings.NameTable = nameTable;
                XmlReader reader = XmlReader.Create(stream, settings);
                XPathDocument document = new XPathDocument(reader);
                args = new GetXmlDocumentCompletedEventArgs(document, userState);
            }
            catch (SystemException exception)
            {
                ServiceProvider.Logger.Error(Strings.DataFeedSourceCreateXmlDocumentFailed, location, exception.Message);
                args = new GetXmlDocumentCompletedEventArgs(exception, false, userState);
            }

            Dispatcher.BeginInvoke(DispatcherPriority.Input, new DispatcherOperationCallback(this.RaiseGetXmlDocumentCompleted), args);
        }

        /// <summary>
        /// Called when image data stream is ready for processing.
        /// Creates ImageSource from a stream in the UI thread and raises GetImageSourceCompleted event.
        /// </summary>
        /// <param name="stream">The stream representing requested image data.</param>
        /// <param name="location">The location of the data stream.</param>
        /// <param name="userState">The user-supplied state object.</param>
        private void OnImageStreamReady(Stream stream, string location, object userState)
        {
            GetImageSourceCompletedEventArgs args;
            try
            {
                // Create a BitmapImage from memory stream.
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = stream;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
                bitmapImage.EndInit();
                stream.Close();
                bitmapImage.Freeze();
                args = new GetImageSourceCompletedEventArgs(bitmapImage, userState);
            }
            catch (InvalidOperationException exception)
            {
                ServiceProvider.Logger.Error(Strings.DataFeedSourceCreateImageSourceFailed, location, exception.Message);
                args = new GetImageSourceCompletedEventArgs(exception, false, userState);
            }

            Dispatcher.BeginInvoke(DispatcherPriority.Input, new DispatcherOperationCallback(this.RaiseGetImageSourceCompleted), args);
        }

        /// <summary>
        /// Called when text data stream is ready for processing. 
        /// Creates StreamReader for text and raises GetTextDocumentComplted event.
        /// </summary>
        /// <param name="stream">The stream representing requested text data.</param>
        /// <param name="location">The location of the data stream.</param>
        /// <param name="userState">The user-supplied state object.</param>
        private void OnTextStreamReady(Stream stream, Uri location, object userState)
        {
            GetTextDocumentCompletedEventArgs args;
            try
            {
                StreamReader streamReader = new StreamReader(stream);
                string text = streamReader.ReadToEnd();
                streamReader.Close();
                args = new GetTextDocumentCompletedEventArgs(location, text, userState);
            }
            catch (SystemException exception)
            {
                ServiceProvider.Logger.Error(Strings.DataFeedSourceCreateTextDocumentFailed, location.ToString(), exception.Message);
                args = new GetTextDocumentCompletedEventArgs(exception, false, userState);
            }

            Dispatcher.BeginInvoke(DispatcherPriority.Input, new DispatcherOperationCallback(this.RaiseGetTextDocumentCompleted), args);
        }

        #endregion
    }
}
