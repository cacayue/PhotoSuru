//-----------------------------------------------------------------------
// <copyright file="GetTextDocumentCompletedEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     Event arguments accompanying delegate for GetTextDocument event.
// </summary>
//-----------------------------------------------------------------------

namespace ScePhoto.Feed
{
    using System;
    using System.ComponentModel;
    using System.IO;

    /// <summary>
    /// Event arguments accompanying delegate for GetTextDocument event.
    /// </summary>
    public class GetTextDocumentCompletedEventArgs : AsyncCompletedEventArgs
    {
        /// <summary>
        /// The Xml document representing data from the Internet resource.
        /// </summary>
        private string documentText = String.Empty;

        /// <summary>
        /// The URI representing the location of document.
        /// </summary>
        private Uri link;

        /// <summary>
        /// Initializes a new instance of arguments for successful completion.
        /// </summary>
        /// <param name="link">The URI representing the location of document.</param>
        /// <param name="documentText">The StreamReader representing text data from the Internet resource.</param>
        /// <param name="userState">The user-supplied state object.</param>
        public GetTextDocumentCompletedEventArgs(Uri link, string documentText, object userState)
            : base(null, false, userState)
        {
            this.documentText = documentText;
            this.link = link;
        }

        /// <summary>
        /// Initializes a new instance of arguments for an error or a cancellation.
        /// </summary>
        /// <param name="error">Any error that occurred during the asynchronous operation.</param>
        /// <param name="cancelled">A value indicating whether the asynchronous operation was canceled.</param>
        /// <param name="userState">The user-supplied state object.</param>
        public GetTextDocumentCompletedEventArgs(Exception error, bool cancelled, object userState)
            : base(error, cancelled, userState)
        {
        }

        /// <summary>
        /// Gets the StreamReader representing text data from the Internet resource.
        /// </summary>
        public string DocumentText
        {
            get
            {
                RaiseExceptionIfNecessary();
                return this.documentText;
            }
        }

        /// <summary>
        /// Gets the URI representing the location of document.
        /// </summary>
        public Uri Link
        {
            get
            {
                RaiseExceptionIfNecessary();
                return this.link;
            }
        }
    }
}
