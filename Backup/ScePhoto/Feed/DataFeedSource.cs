//-----------------------------------------------------------------------
// <copyright file="DataFeedSource.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     Represents a data feed and provides methods for interacting with it.
// </summary>
//-----------------------------------------------------------------------

namespace ScePhoto.Feed
{
    using System;
    using System.Windows.Threading;

    /// <summary>
    /// Represents a data feed and provides methods for interacting with it.
    /// </summary>
    public abstract class DataFeedSource
    {
        /// <summary>
        /// The dispatcher associated with the UI thread.
        /// </summary>
        private readonly Dispatcher dispatcher;

        /// <summary>
        /// Initializes the instance of DataFeedSource.
        /// </summary>
        /// <param name="dispatcher">The dispatcher associated with the UI thread.</param>
        protected DataFeedSource(Dispatcher dispatcher)
        {
            if (dispatcher == null)
            {
                throw new ArgumentNullException("dispatcher");
            }

            this.dispatcher = dispatcher;
        }

        /// <summary>
        /// Event raised on successful completion, an error, or a cancellation of GetXmlDocument request.
        /// </summary>
        public event EventHandler<GetXmlDocumentCompletedEventArgs> GetXmlDocumentCompleted;

        /// <summary>
        /// Event raised on successful completion, an error, or a cancellation of GetImageSource request.
        /// </summary>
        public event EventHandler<GetImageSourceCompletedEventArgs> GetImageSourceCompleted;

        /// <summary>
        /// Event raised on successful completion, an error, or a cancellation of GetTextDocument request.
        /// </summary>
        public event EventHandler<GetTextDocumentCompletedEventArgs> GetTextDocumentCompleted;

        /// <summary>
        /// Gets the dispatcher associated with the UI thread.
        /// </summary>
        protected Dispatcher Dispatcher
        {
            get { return this.dispatcher; }
        }

        /// <summary>
        /// Make asynchronous request to get XmlDocument from an Uri.
        /// </summary>
        /// <param name="uri">The Uri representing XML document.</param>
        /// <param name="localData">Whether or not requesting local only data, if supported.</param>
        /// <param name="userState">The user-supplied state object.</param>
        public abstract void GetXmlDocumentAsync(Uri uri, bool localData, object userState);

        /// <summary>
        /// Make asynchronous request to get ImageSource from an Uri.
        /// </summary>
        /// <param name="uri">The Uri representing an image.</param>
        /// <param name="localData">Whether or not requesting local only data, if supported.</param>
        /// <param name="userState">The user-supplied state object.</param>
        public abstract void GetImageSourceAsync(Uri uri, bool localData, object userState);

        /// <summary>
        /// Make asynchronous request to get text document from a Uri.
        /// </summary>
        /// <param name="uri">The Uri representing the text document.</param>
        /// <param name="localData">Whether or not requesting local only data, if supported.</param>
        /// <param name="userState">The user-supplied state object.</param>
        public abstract void GetTextDocumentAsync(Uri uri, bool localData, object userState);

        /// <summary>
        /// Cancel asynchronous operation before it gets completed.
        /// </summary>
        /// <param name="userState">The user-supplied state object.</param>
        public abstract void CancelAsync(object userState);

        /// <summary>
        /// Raise GetXPathDocumentCompleted event.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        protected virtual void OnGetXmlDocumentCompleted(GetXmlDocumentCompletedEventArgs e)
        {
            if (this.GetXmlDocumentCompleted != null)
            {
                this.GetXmlDocumentCompleted(this, e);
            }
        }

        /// <summary>
        /// Raise GetImageSourceCompleted event.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        protected virtual void OnGetImageSourceCompleted(GetImageSourceCompletedEventArgs e)
        {
            if (this.GetImageSourceCompleted != null)
            {
                this.GetImageSourceCompleted(this, e);
            }
        }

        /// <summary>
        /// Raise GetTextDocumentCompleted event.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        protected virtual void OnGetTextDocumentCompleted(GetTextDocumentCompletedEventArgs e)
        {
            if (this.GetTextDocumentCompleted != null)
            {
                this.GetTextDocumentCompleted(this, e);
            }
        }
    }
}
