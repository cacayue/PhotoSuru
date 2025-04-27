//-----------------------------------------------------------------------
// <copyright file="GetXmlDocumentCompletedEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     Event arguments accompanying delegate for GetXmlDocument event.
// </summary>
//-----------------------------------------------------------------------

namespace ScePhoto.Feed
{
    using System;
    using System.ComponentModel;
    using System.Xml.XPath;

    /// <summary>
    /// Event arguments accompanying delegate for GetXmlDocument event.
    /// </summary>
    public class GetXmlDocumentCompletedEventArgs : AsyncCompletedEventArgs
    {
        /// <summary>
        /// The Xml document representing data from the Internet resource.
        /// </summary>
        private XPathDocument document;

        /// <summary>
        /// Initializes a new instance of arguments for successful completion.
        /// </summary>
        /// <param name="document">The Xml document representing data from the Internet resource.</param>
        /// <param name="userState">The user-supplied state object.</param>
        public GetXmlDocumentCompletedEventArgs(XPathDocument document, object userState)
            : base(null, false, userState)
        {
            this.document = document;
        }

        /// <summary>
        /// Initializes a new instance of arguments for an error or a cancellation.
        /// </summary>
        /// <param name="error">Any error that occurred during the asynchronous operation.</param>
        /// <param name="cancelled">A value indicating whether the asynchronous operation was canceled.</param>
        /// <param name="userState">The user-supplied state object.</param>
        public GetXmlDocumentCompletedEventArgs(Exception error, bool cancelled, object userState)
            : base(error, cancelled, userState)
        {
        }

        /// <summary>
        /// Gets the Xml document representing data from the Internet resource.
        /// </summary>
        public XPathDocument Document
        {
            get
            {
                RaiseExceptionIfNecessary();
                return this.document;
            }
        }
    }
}
